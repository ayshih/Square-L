using Crypto;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Square_L
{
    public class Identity
    {
        public string nickname { get; set; }
        public DateTime lastUsed { get; set; }
        public byte[] masterKey { get; set; }
        public byte[] passwordSalt { get; set; }
        public byte[] passwordHash { get; set; }
        public SCryptParameters parameters { get; set; }

        public Identity()
        {
        }

        public Identity(Identity other)
        {
            nickname = other.nickname;
            lastUsed = other.lastUsed;
            masterKey = other.masterKey;
            passwordSalt = other.passwordSalt;
            passwordHash = other.passwordHash;
            parameters = other.parameters;
        }

        public Identity(byte[] data)
        {
            masterKey = new byte[32];
            passwordSalt = new byte[8];
            passwordHash = new byte[32];

            Buffer.BlockCopy(data, 1, masterKey, 0, 32);
            Buffer.BlockCopy(data, 34, passwordSalt, 0, 8);
            Buffer.BlockCopy(data, 42, passwordHash, 0, 32);
            parameters = new SCryptParameters { log2_N = data[74], r = data[75], p = data[76] + 256 * data[77] };

            Debug.WriteLine("Master key: " + Base64Url.Encode(masterKey));
            Debug.WriteLine("Password salt: " + Base64Url.Encode(passwordSalt));
            Debug.WriteLine("Password hash: " + Base64Url.Encode(passwordHash));
            Debug.WriteLine("SCrypt parameters: N=2^" + parameters.log2_N + ",r=" + parameters.r + ",p=" + parameters.p);
        }

        public async Task<bool> VerifyPassword(string password)
        {
            try
            {
                await GetSCryptResult(password);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public async Task<byte[]> GetSCryptResult(string password)
        {
            var _crypto = new CryptoRuntimeComponent();
            var _SHA256 = new SHA256Managed();

            var passwordInBytes = System.Text.Encoding.UTF8.GetBytes(password);

            Debug.WriteLine("Stored master key: " + Base64Url.Encode(masterKey));
            Debug.WriteLine("Password salt: " + Base64Url.Encode(passwordSalt));

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var scryptResult = await _crypto.SCryptAsync(passwordInBytes, passwordSalt, parameters) as byte[];
            stopwatch.Stop();
            Debug.WriteLine("SCrypt of password+salt: " + Base64Url.Encode(scryptResult) + " (" + stopwatch.ElapsedMilliseconds.ToString() + " ms)");

            var passwordCheck = _SHA256.ComputeHash(scryptResult);
            Debug.WriteLine("Password hash: " + Base64Url.Encode(passwordHash));
            Debug.WriteLine("Password check: " + Base64Url.Encode(passwordCheck));

            if (Base64Url.Encode(passwordCheck).Equals(Base64Url.Encode(passwordHash)))
            {
                return scryptResult;
            }
            else
            {
                throw new Exception("Password does not verify");
            }
        }

        public async Task ChangeSCryptParameters(string password, SCryptParameters newParameters, byte[] oldScryptResult)
        {
            var _crypto = new CryptoRuntimeComponent();
            var _random = new Random();
            var _SHA256 = new SHA256Managed();

            var passwordInBytes = System.Text.Encoding.UTF8.GetBytes(password);

            var newPasswordSalt = new byte[8];
            _random.NextBytes(newPasswordSalt);

            var newScryptResult = await _crypto.SCryptAsync(passwordInBytes, newPasswordSalt, newParameters) as byte[];

            var newMasterKey = Utility.Xor(masterKey, Utility.Xor(oldScryptResult, newScryptResult));
            var newPasswordHash = _SHA256.ComputeHash(newScryptResult);

            masterKey = newMasterKey;
            passwordSalt = newPasswordSalt;
            passwordHash = newPasswordHash;
            parameters = newParameters;
        }

        public async Task ChangeSCryptParameters(string password, SCryptParameters newParameters)
        {
            var _crypto = new CryptoRuntimeComponent();

            var passwordInBytes = System.Text.Encoding.UTF8.GetBytes(password);

            var oldScryptResult = await _crypto.SCryptAsync(passwordInBytes, passwordSalt, parameters) as byte[];

            await ChangeSCryptParameters(password, newParameters, oldScryptResult);
        }

        public System.Windows.Media.Imaging.WriteableBitmap GetQRCode()
        {
            // The specification for the export format has not been decided.  Here it is:
            // byte  0: signature algorithm version
            //       1: encrypted master key (32 bytes)
            //      33: password algorithm version
            //      34: password salt (8 bytes)
            //      42: password hash (32 bytes)
            //      74: scrypt log_2(N) parameter
            //      75: scrypt r parameter
            //      76: scrypt p parameter (2 bytes)
            var export = new byte[78];
            export[0] = 1;
            Buffer.BlockCopy(masterKey, 0, export, 1, 32);
            export[33] = 1;
            Buffer.BlockCopy(passwordSalt, 0, export, 34, 8);
            Buffer.BlockCopy(passwordHash, 0, export, 42, 32);
            export[74] = (byte)parameters.log2_N;
            export[75] = (byte)parameters.r;
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)parameters.p), 0, export, 76, 2);

            var options = new ZXing.QrCode.QrCodeEncodingOptions { Margin = 1, Width = 300, Height = 300, ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.H };
            var writer = new ZXing.BarcodeWriter() { Format = ZXing.BarcodeFormat.QR_CODE, Options = options };
            var qrcode = writer.Write(Base64Url.Encode(export));
            return qrcode;
        }

        public void GetQuery(AssembleUrl assembleUrl, byte[] scryptResult, out string query, out string post)
        {
            var _crypto = new CryptoRuntimeComponent();

            var DomainNameInBytes = System.Text.Encoding.UTF8.GetBytes(assembleUrl.DomainName);
            var _HMACSHA256 = new HMACSHA256(Utility.Xor(masterKey, scryptResult));
            var seed = _HMACSHA256.ComputeHash(DomainNameInBytes);
            Debug.WriteLine("Seed: " + Base64Url.Encode(seed));

            byte[] publicKey, privateKey;
            _crypto.CreateKeyPair(out publicKey, out privateKey, seed);
            Debug.WriteLine("Public key: " + Base64Url.Encode(publicKey));
            Debug.WriteLine("Private key: " + Base64Url.Encode(privateKey));

            assembleUrl.AddParameter("sqrlver", "1");
            assembleUrl.AddParameter("sqrlopt", "");
            assembleUrl.AddParameter("sqrlkey", Base64Url.Encode(publicKey));

            var challenge = System.Text.Encoding.UTF8.GetBytes(assembleUrl.Protocol + "://" + assembleUrl.Buffer);
            query = (assembleUrl.Protocol == "sqrl" ? "https://" : "http://") + assembleUrl.Buffer;
            Debug.WriteLine("Challenge: " + System.Text.Encoding.UTF8.GetString(challenge, 0, challenge.Length));

            var signature = _crypto.CreateSignature(challenge, publicKey, privateKey);
            Debug.WriteLine("Signature: " + Base64Url.Encode(signature));

            post = "sqrlsig=" + Base64Url.Encode(signature);
            Debug.WriteLine("To post: " + post);
        }
    }

    public class Base64Url
    {
        /// <summary>
        /// Encodes a byte array according to the base64url scheme, without trailing padding characters
        /// </summary>
        /// <param name="input">byte array</param>
        /// <returns>encoded string</returns>
        public static string Encode(byte[] input)
        {
            var result = Convert.ToBase64String(input);
            return result.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        /// <summary>
        /// Decodes a string encoded with the base64url scheme into a byte array
        /// </summary>
        /// <param name="input">encoded string</param>
        /// <returns>byte array</returns>
        public static byte[] Decode(string input)
        {
            var result = input.Replace('-', '+').Replace('_', '/').PadRight(4 - (input.Length % 4), '=');
            return Convert.FromBase64String(result);
        }
    }

    public class Utility
    {
        /// <summary>
        /// Element-wise XOR of two byte arrays
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static byte[] Xor(byte[] a, byte[] b)
        {
            int length = (a.Length < b.Length ? a.Length : b.Length);
            byte[] result = new byte[length];
            for (var i = 0; i < length; i++)
                result[i] = (byte)(a[i] ^ b[i]);
            return result;
        }
    }
}

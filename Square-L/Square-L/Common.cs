using System;

namespace Square_L
{
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

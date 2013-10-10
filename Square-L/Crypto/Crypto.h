#pragma once

namespace Crypto
{
    public ref class CryptoRuntimeComponent sealed
    {
    public:
        CryptoRuntimeComponent();

		// The following arrays all have fixed sizes
		// public_key is 32 bytes
		// private_key is 64 bytes
		// seed is 32 bytes
		// signature is 64 bytes
		void CreateKeyPair(Platform::WriteOnlyArray<unsigned char>^ public_key,
		                   Platform::WriteOnlyArray<unsigned char>^ private_key,
						   const Platform::Array<unsigned char>^ seed);
		void CreateSignature(Platform::WriteOnlyArray<unsigned char>^ signature,
		                     const Platform::Array<unsigned char>^ message,
						     int message_length,
		                     const Platform::Array<unsigned char>^ public_key,
						     const Platform::Array<unsigned char>^ private_key);
		bool VerifySignature(const Platform::Array<unsigned char>^ signature,
		                    const Platform::Array<unsigned char>^ message,
						    int message_length,
						    const Platform::Array<unsigned char>^ public_key);

		// The following array has a fixed size
		// output is 32 bytes
		void SHA256(Platform::WriteOnlyArray<unsigned char>^ output,
			        const Platform::Array<unsigned char>^ input,
					int input_length);
		void HMAC_SHA256(Platform::WriteOnlyArray<unsigned char>^ output,
			             const Platform::Array<unsigned char>^ key,
				         int key_length,
			             const Platform::Array<unsigned char>^ input,
					     int input_length);
        void SCrypt(Platform::WriteOnlyArray<unsigned char>^ output,
					const Platform::Array<unsigned char>^ password,
					int password_length,
					const Platform::Array<unsigned char>^ salt,
					int salt_length,
					int N, int r, int p);
    };
}
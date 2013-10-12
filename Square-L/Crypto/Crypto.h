#pragma once

namespace Crypto
{
    public ref class CryptoRuntimeComponent sealed
    {
    public:
        CryptoRuntimeComponent();

		// The following arrays must be allocated with these sizes
		// public_key is 32 bytes
		// private_key is 64 bytes
		// seed is 32 bytes
		// signature is 64 bytes
		void CreateKeyPair(Platform::WriteOnlyArray<unsigned char>^ public_key,
			Platform::WriteOnlyArray<unsigned char>^ private_key,
			const Platform::Array<unsigned char>^ seed);
		void CreateSignature(Platform::WriteOnlyArray<unsigned char>^ signature,
			const Platform::Array<unsigned char>^ message,
			const Platform::Array<unsigned char>^ public_key,
			const Platform::Array<unsigned char>^ private_key);
		bool VerifySignature(const Platform::Array<unsigned char>^ signature,
			const Platform::Array<unsigned char>^ message,
			const Platform::Array<unsigned char>^ public_key);

		// output must be an allocated byte[32] array
		void SCrypt(Platform::WriteOnlyArray<unsigned char>^ output,
			const Platform::Array<unsigned char>^ password,
			const Platform::Array<unsigned char>^ salt,
			int log2_N, int r, int p);

		// output must be an allocated byte[32] array
		void PBKDF2_HMACSHA256(Platform::WriteOnlyArray<unsigned char>^ output,
			const Platform::Array<unsigned char>^ password,
			const Platform::Array<unsigned char>^ salt,
			int iterations);
    };
}
#pragma once

namespace Crypto
{
	public ref class CryptoRuntimeComponent sealed
	{
	public:
		CryptoRuntimeComponent();

		void CreateKeyPair(Platform::Array<unsigned char>^* publicKey,
			Platform::Array<unsigned char>^* privateKey,
			const Platform::Array<unsigned char>^ seed);
		Platform::Array<unsigned char>^ CreateSignature(const Platform::Array<unsigned char>^ message,
			const Platform::Array<unsigned char>^ publicKey,
			const Platform::Array<unsigned char>^ privateKey);
		bool VerifySignature(const Platform::Array<unsigned char>^ signature,
			const Platform::Array<unsigned char>^ message,
			const Platform::Array<unsigned char>^ public_key);

		Platform::Array<unsigned char>^ SCrypt(const Platform::Array<unsigned char>^ password,
			const Platform::Array<unsigned char>^ salt,
			int log2_N, int r, int p);
		Windows::Foundation::IAsyncOperation<Platform::Object^>^ SCryptAsync(const Platform::Array<unsigned char>^ password,
			const Platform::Array<unsigned char>^ salt,
			int log2_N, int r, int p);

		Platform::Array<unsigned char>^ PBKDF2_HMACSHA256(const Platform::Array<unsigned char>^ password,
			const Platform::Array<unsigned char>^ salt,
			int iterations);
	};
}
// Crypto.cpp
#include "Crypto.h"

#include "ed25519\ed25519.h"
#include "scrypt\crypto_scrypt.h"
#include "scrypt\sha256.h"

#include <string.h>

using namespace Crypto;
using namespace Platform;

CryptoRuntimeComponent::CryptoRuntimeComponent()
{
}

void CryptoRuntimeComponent::CreateKeyPair(Platform::WriteOnlyArray<unsigned char>^ public_key,
		                                   Platform::WriteOnlyArray<unsigned char>^ private_key,
						                   const Platform::Array<unsigned char>^ seed)
{
	ed25519_create_keypair(public_key->Data, private_key->Data, seed->Data);
}

void CryptoRuntimeComponent::CreateSignature(Platform::WriteOnlyArray<unsigned char>^ signature,
		                                     const Platform::Array<unsigned char>^ message,
						                     int message_length,
		                                     const Platform::Array<unsigned char>^ public_key,
						                     const Platform::Array<unsigned char>^ private_key)
{
	ed25519_sign(signature->Data, message->Data, message_length, public_key->Data, private_key->Data);
}

bool CryptoRuntimeComponent::VerifySignature(const Platform::Array<unsigned char>^ signature,
		                                    const Platform::Array<unsigned char>^ message,
						                    int message_length,
						                    const Platform::Array<unsigned char>^ public_key)
{
	int result = ed25519_verify(signature->Data, message->Data, message_length, public_key->Data);
	return (result == 1);
}

void CryptoRuntimeComponent::SHA256(Platform::WriteOnlyArray<unsigned char>^ output,
			                        const Platform::Array<unsigned char>^ input,
					                int input_length)
{
	SHA256_CTX ctx;

	SHA256_Init(&ctx);
	SHA256_Update(&ctx, input->Data, input_length);
	SHA256_Final(output->Data, &ctx);
}

void CryptoRuntimeComponent::HMAC_SHA256(Platform::WriteOnlyArray<unsigned char>^ output,
			                             const Platform::Array<unsigned char>^ key,
					                     int key_length,
			                             const Platform::Array<unsigned char>^ input,
					                     int input_length)
{
	HMAC_SHA256_CTX ctx;

	HMAC_SHA256_Init(&ctx, key->Data, key_length);
	HMAC_SHA256_Update(&ctx, input->Data, input_length);
	HMAC_SHA256_Final(output->Data, &ctx);
}

void CryptoRuntimeComponent::SCrypt(Platform::WriteOnlyArray<unsigned char>^ output,
									const Platform::Array<unsigned char>^ password,
									int password_length,
									const Platform::Array<unsigned char>^ salt,
									int salt_length,
									int N, int r, int p)
{
	// Return 0 on success; or -1 on error.
	crypto_scrypt(password->Data, password_length, salt->Data, salt_length, 1 << N, r, p, output->Data, 32);
}

// Crypto.cpp
#include "Crypto.h"

#include "ed25519\ed25519.h"
#include "scrypt\crypto_scrypt.h"
#include "scrypt\sha256.h"

#include <string.h>
#include <errno.h>

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
											 const Platform::Array<unsigned char>^ public_key,
											 const Platform::Array<unsigned char>^ private_key)
{
	ed25519_sign(signature->Data, message->Data, message->Length, public_key->Data, private_key->Data);
}

bool CryptoRuntimeComponent::VerifySignature(const Platform::Array<unsigned char>^ signature,
											 const Platform::Array<unsigned char>^ message,
											 const Platform::Array<unsigned char>^ public_key)
{
	int result = ed25519_verify(signature->Data, message->Data, message->Length, public_key->Data);
	return (result == 1);
}

void CryptoRuntimeComponent::SCrypt(Platform::WriteOnlyArray<unsigned char>^ output,
									const Platform::Array<unsigned char>^ password,
									const Platform::Array<unsigned char>^ salt,
									int log2_N, int r, int p)
{
	errno = 0;
	if (crypto_scrypt(password->Data, password->Length, salt->Data, salt->Length, 1 << log2_N, r, p, output->Data, 32) == -1)
	{
		switch (errno)
		{
		case EFBIG:
			throw ref new Platform::InvalidArgumentException("r*p >= 2^30");
		case EINVAL:
			throw ref new Platform::InvalidArgumentException("N not a power of 2");
		case ENOMEM:
			throw ref new Platform::OutOfMemoryException("Implied memory request will be too large");
		default:
			throw ref new Platform::OutOfMemoryException("Failed to allocate memory");
		}
	}
}

void CryptoRuntimeComponent::PBKDF2_HMACSHA256(Platform::WriteOnlyArray<unsigned char>^ output,
											   const Platform::Array<unsigned char>^ password,
											   const Platform::Array<unsigned char>^ salt,
											   int iterations)
{
	PBKDF2_SHA256(password->Data, password->Length, salt->Data, salt->Length, iterations, output->Data, 32);
}
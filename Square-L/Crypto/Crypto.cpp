// Crypto.cpp
#include "Crypto.h"

#include "ed25519\ed25519.h"
#include "scrypt\crypto_scrypt.h"
#include "scrypt\sha256.h"

#include <string.h>
#include <errno.h>
#include <ppltasks.h>

using namespace Crypto;
using namespace Platform;

CryptoRuntimeComponent::CryptoRuntimeComponent()
{
}

void CryptoRuntimeComponent::CreateKeyPair(Array<unsigned char>^* publicKey,
                                           Array<unsigned char>^* privateKey,
                                           const Array<unsigned char>^ seed)
{
    auto _publicKey = ref new Array<unsigned char>(32);
    auto _privateKey = ref new Array<unsigned char>(64);

    ed25519_create_keypair(_publicKey->Data, _privateKey->Data, seed->Data);

    *publicKey = _publicKey;
    *privateKey = _privateKey;
}

Array<unsigned char>^ CryptoRuntimeComponent::CreateSignature(const Array<unsigned char>^ message,
                                                              const Array<unsigned char>^ publicKey,
                                                              const Array<unsigned char>^ privateKey)
{
    auto _signature = ref new Array<unsigned char>(64);

    ed25519_sign(_signature->Data, message->Data, message->Length, publicKey->Data, privateKey->Data);

    return _signature;
}

bool CryptoRuntimeComponent::VerifySignature(const Platform::Array<unsigned char>^ signature,
                                             const Platform::Array<unsigned char>^ message,
                                             const Platform::Array<unsigned char>^ publicKey)
{
    int result = ed25519_verify(signature->Data, message->Data, message->Length, publicKey->Data);
    return (result == 1);
}

Array<unsigned char>^ CryptoRuntimeComponent::SCrypt(const Array<unsigned char>^ password,
                                                     const Array<unsigned char>^ salt,
                                                     int log2_N, int r, int p)
{
    auto _output = ref new Array<unsigned char>(32);

    errno = 0;
    if (crypto_scrypt(password->Data, password->Length, salt->Data, salt->Length, 1 << log2_N, r, p, _output->Data, 32) == -1)
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

    return _output;
}

Windows::Foundation::IAsyncOperation<Object^>^ CryptoRuntimeComponent::SCryptAsync(const Array<unsigned char>^ password,
                                                                                   const Array<unsigned char>^ salt,
                                                                                   int log2_N, int r, int p)
{
    return concurrency::create_async([this, password, salt, log2_N, r, p] () -> Object^ { return SCrypt(password, salt, log2_N, r, p); });
}

Array<unsigned char>^ CryptoRuntimeComponent::PBKDF2_HMACSHA256(const Array<unsigned char>^ password,
                                                                const Array<unsigned char>^ salt,
                                                                int iterations)
{
    auto _output = ref new Array<unsigned char>(32);

    PBKDF2_SHA256(password->Data, password->Length, salt->Data, salt->Length, iterations, _output->Data, 32);

    return _output;
}

Windows::Foundation::IAsyncOperation<Object^>^ CryptoRuntimeComponent::PBKDF2_HMACSHA256_Async(const Array<unsigned char>^ password,
                                                                const Array<unsigned char>^ salt,
                                                                int iterations)
{
    return concurrency::create_async([this, password, salt, iterations] () -> Object^ { return PBKDF2_HMACSHA256(password, salt, iterations); });
}
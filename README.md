Square-L
--------
A SQRL client for Windows Phone 8

Building
--------
You will need the Windows Phone 8 SDK, and then you will also need to install the following packages using NuGet:

* Windows Phone Toolkit (http://phone.codeplex.com), for ContextMenu and TiltEffect
* ZXing.NET (http://zxingnet.codeplex.com), for QR-code decoding

Notes
-----
This app includes the following C code, appropriately modified:
scrypt 1.1.6 (http://www.tarsnap.com/scrypt.html)
	for scrypt, SHA256, and HMAC-SHA256
	with modifications for compilation
nightcracker's ed25519 (https://github.com/nightcracker/ed25519)
	for ed25519
	replace fixint.h with stdint.h
	fix to fe_cswap (b = -b)
	removed ed25519_create_seed, ed25519_key_exchange, ed25519_add_scalar
	fixed declaration of ed25519_verify

Square-L
--------
A SQRL client for Windows Phone 8, currently at the proof-of-concept stage.  Identity management is not yet implemented; the identity (master key and password) is hard-coded.  You can scan a SQRL QR code, and then, upon entering the correct password, choose to send the SQRL login request.

Also see
--------

* https://www.grc.com/sqrl/sqrl.htm, for the description of the SQRL protocol
* https://github.com/geir54/android-sqrl, for a SQRL client for Android
* https://github.com/TheBigS/SQRL, for a SQRL client in Java
* https://github.com/trianglman/sqrl, for a SQRL server in PHP


To build
--------
You will need the Windows Phone 8 SDK, and the following packages should be downloaded (automatically) through NuGet:

* Windows Phone Toolkit (http://phone.codeplex.com), for ContextMenu and TiltEffect
* ZXing.NET (http://zxingnet.codeplex.com), for QR-code decoding

Notes
-----
This app incorporates the following C code, appropriately modified:
* scrypt 1.1.6 (http://www.tarsnap.com/scrypt.html), for scrypt, SHA256, and HMAC-SHA256
* nightcracker's ed25519 (https://github.com/nightcracker/ed25519), for ed25519 implementation

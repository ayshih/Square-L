Square-L
========
A SQRL client for Windows Phone 8, currently at the proof-of-concept stage.  Identity management is not yet implemented; the identity (master key and password) is hard-coded.  You can scan a SQRL QR code, and then, upon entering the correct password, choose to send the SQRL login request.

See also
--------

* SQRL protocol
  * Description: https://www.grc.com/sqrl/sqrl.htm
  * Newsgroup: https://www.grc.com/groups/sqrl
* SQRL clients
  * https://github.com/geir54/android-sqrl (Android)
  * https://github.com/TheBigS/SQRL (Java)
* SQRL servers
  * https://github.com/trianglman/sqrl (PHP)
  * https://github.com/geir54/php-sqrl (PHP, running at http://sqrl.host56.com)
* ed25519
  * Testing: http://ed25519.herokuapp.com

To build
--------
You will need the Windows Phone 8 SDK, and the following packages need to be downloaded through NuGet:

* ZXing.NET (http://zxingnet.codeplex.com), for QR-code decoding
* Windows Phone Toolkit (http://phone.codeplex.com), for ContextMenu and TiltEffect

Notes
-----
This app incorporates the following C code, appropriately modified:
* scrypt 1.1.6 (http://www.tarsnap.com/scrypt.html)
  * for scrypt, SHA256, and HMAC-SHA256
  * 2-clause BSD license
* nightcracker's ed25519 (https://github.com/nightcracker/ed25519)
  * for ed25519
  * public domain

Square-L
========
A SQRL client for Windows Phone 8, currently as a proof of concept.  As of release 0.2, you can:

* create and delete identities (an identity called "test identity" with a hard-coded master key and password is recreated if deleted)
* scan a SQRL QR code
* verify identity password and generate appropriate keys
* optionally send SQRL authentication query

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
  * 2-clause BSD license
* nightcracker's ed25519 (https://github.com/nightcracker/ed25519)
  * public domain

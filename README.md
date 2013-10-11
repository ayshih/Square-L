Square-L
========
A SQRL client for Windows Phone 8, currently as a proof of concept.  As of release 0.2, you can:

* create and delete identities
  * master key uses randomness from camera images, the clock, and the PRNG
  * an identity called "test identity" with a hard-coded master key and password is recreated if deleted
* scan a SQRL QR code
* verify the identity's password and generate appropriate keys
* optionally send the SQRL authentication query

See also
--------

* SQRL protocol
  * Description: https://www.grc.com/sqrl/sqrl.htm
  * Newsgroup: https://www.grc.com/groups/sqrl
* SQRL clients
  * https://github.com/geir54/android-sqrl (Android)
  * https://github.com/TheBigS/SQRL (Java)
  * https://github.com/dchristensen/sqrl-net (download executable from https://sqrl.apphb.com/)
* SQRL servers
  * https://github.com/trianglman/sqrl (PHP)
  * https://github.com/geir54/php-sqrl (PHP to external signature verification, running at http://sqrl.host56.com)
  * https://github.com/dchristensen/sqrl-net (.NET, running at https://sqrl.apphb.com/Account/Login)
* ed25519
  * Testing: http://ed25519.herokuapp.com

To build
--------
You will need the Windows Phone 8 SDK, and the following packages need to be downloaded through NuGet:

* ZXing.NET (http://zxingnet.codeplex.com), for QR-code decoding
  * [Apache License 2.0](http://opensource.org/licenses/Apache-2.0)
* Windows Phone Toolkit (http://phone.codeplex.com), for ContextMenu and TiltEffect
  * [Microsoft Public License](http://opensource.org/licenses/MS-PL)

Notes
-----
This app incorporates the following C code, appropriately modified:
* scrypt 1.1.6 (http://www.tarsnap.com/scrypt.html)
  * [2-clause BSD license](http://opensource.org/licenses/BSD-2-Clause)
* nightcracker's ed25519 (https://github.com/nightcracker/ed25519)
  * [public domain](http://opensource.org/faq#public-domain)

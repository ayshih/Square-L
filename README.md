Square-L
========
A SQRL client for Windows Phone 8, currently as a proof of concept.  Release 0.4 has:

* authentication following the latest SQRL specification (2013-10-13)
  * scans SQRL QR codes
  * verifies passwords, using scrypt parameters: N=2^14, r=8, p=1
  * sends the SQRL authentication query via GET+POST
* identity management
  * create, using randomness from camera images, the clock, and the PRNG
  * export as a QR code, using scrypt parameters: N=2^14, r=8, p=100 (NB: output format has not been standardized yet)
  * import an exported identity from the QR code

See also
--------

* SQRL protocol
  * Description: https://www.grc.com/sqrl/sqrl.htm
  * Newsgroup: https://www.grc.com/groups/sqrl
  * Wiki: https://sqrlauth.net
* SQRL clients
  * https://github.com/geir54/android-sqrl (Android)
  * https://github.com/TheBigS/SQRL (Java)
  * https://github.com/dchristensen/sqrl-net (download executable from https://sqrl.apphb.com/)
* SQRL servers
  * https://github.com/trianglman/sqrl (PHP)
  * https://github.com/geir54/php-sqrl (PHP to external signature verification, running at http://sqrl.host56.com)
  * https://github.com/dchristensen/sqrl-net (.NET, running at https://sqrl.apphb.com/Account/Login)
* Useful utilities
  * QR code generation: http://zxing.appspot.com/generator
  * ed25519 testing: http://ed25519.herokuapp.com

To build
--------
You will need the Windows Phone 8 SDK, with the following packages downloaded through NuGet:

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

License
-------
[Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0)

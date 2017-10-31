VaultCmd
========

Introduction
------------

A .Net based tool for decrypting Ansible vault files.

Credits
-------

1. This tool is essentially just a C# port of an existing Go based version
called "avtool":

https://github.com/pbthorste/avtool

My Decrypter class is a re-imagining of their code in decrypt.go. 

2. The AES CTR mode implementation was discovered via this Stack Overflow
question:

Can I use AES in CTR mode in .NET?
https://stackoverflow.com/questions/6374437/can-i-use-aes-in-ctr-mode-in-net

The answer by "quadfinity" leads to this Gist of his which I used verbatim:

https://gist.github.com/hanswolff/8809275

3. The HMAC technique comes from this Stack Overflow question:

Rfc2898 / PBKDF2 with SHA256 as digest in c#
https://stackoverflow.com/questions/18648084/rfc2898-pbkdf2-with-sha256-as-digest-in-c-sharp

The answer by "Peter O." includes a code snippet which I used almost verbatim.
(I just lifted it into a static class to keep the code separate.)

https://stackoverflow.com/a/18649357/106119

Contact Details
---------------

Email: gort@cix.co.uk
Web:   http://www.chrisoldwood.com

Chris Oldwood
31st October 2017

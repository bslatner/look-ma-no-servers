I would never include a real private key in a responsitory. That's just nuts.

To get your own private key, 

* Visit https://manage.auth0.com
* Click on "Clients"
* Click on the client for your test app
* Open the "Advanced settings"
* Click the "Certificates" tab
* Download your certificate
* Copy it to `<codeDirectory>/../private-key-certificate.txt`
* PROFIT!

So, for example, if you cloned this repository to C:\Code\ServerlessDemo,
you would want to create the file C:\Code\private-key-certificate.txt.
This ensures that it never gets committed to the repository.

Actually, there's one more step. You need to convert the text-based certificate 
to a binary certificate. You should be able to paste the following directly
into C# interactive in Visual Studio and get what you need. Replace
`C:\your\path` with the directory where you created 
`private-key-certificate.txt`.

```csharp
using System.Security.Cryptography.X509Certificates;

var cert = new X509.X509Certificate2(@"C:\your\path\private-key-certificate.txt");
var b = cert.Export(X509ContentType.Cert);
System.IO.File.WriteAllBytes(@"C:\your\path\private.key", b);

```
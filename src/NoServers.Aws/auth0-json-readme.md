There's nothing actually secret in auth0.json, but I still want to keep
it out of the repository for this demo.

To make this demo run create a file `<codeDirectory>/../auth0.json`
and populate it as follows:

```json
{
    "clientId": "your client ID from auth0",
    "domain": "your auth0 domain, e.g. sct.auth0.com"
}
```

So, for example, if you cloned this repository to C:\Code\ServerlessDemo,
you would want to create the file `C:\Code\auth0.json`. 
This ensures that it never gets committed to the repository.


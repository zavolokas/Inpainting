## What is Serverless Computing?
As Martin Fowler states in his article about Serverless architectures:

> “Serverless can also mean applications where some amount of server-side logic is still written by the application developer but unlike traditional architectures is run in stateless compute containers that are event-triggered, ephemeral (may only last for one invocation), and fully managed by a 3rd party”.

Serverless Computing benefits can be summarized as follows:
- Reduced operational & development costs
- Fast and easy scaling
- Reduced packaging & deployment complexity
- Easy scheduling & event processing
- Reduced time to market / experimentation
For more info, I strongly encourage you to read the following posts:
- Serverless architectures by Martin Fowler
- Serverless architectures: game-changer or a recycled fad?

## What is Azure Functions?
Azure Functions is Microsoft Azure’s solution for running easily small pieces of code, or “functions,” in the cloud. You can write just the code you need for the problem at hand, without having to manage the infrastructure to run it. Key points are:
- You can choose between a variety of languages such as C#, F#, Node.js, Python, PHP, batch, bash, or any executable one.
- You pay only for the time spent running your code.
- They support NuGet and NPM for external libraries, and provide a very easy way to leverage Azure services through triggers, which are ways to start execution of your code, and bindings, which are ways to simplify coding for input and output data.
- Functions scale up or down based on demand.

1. Create `Azure Function` project
![CreateProject]

2. Let's install Durable functions. 
   - Go to Manage NuGet packages update the `Microsoft.NET.SDK.Functions` package to the latest version(v1.0.9).
   - Search for `Microsoft.Azure.WebJobs.Extensions.DurableTask` package and try to install it (current version is in Prerelease v1.2.0-beta3). In a case it is not installing uninstall the `Microsoft.NET.SDK.Functions` package first, but don't forget to install it back after the durable task is installed.

3. Create Azure function
![CreateFunction1]

4. Choose Http trigger, since we want our function read from blob storage when we do a call to a URL
![CreateFunction2]

5. Let's require to pass container and blob names in the query string.
```CSharp
// parse query parameter
var args = req.GetQueryNameValuePairs();
var container= args.FirstOrDefault(q => string.Compare(q.Key, "container", true) == 0)
    .Value;

var blob = args.FirstOrDefault(q => string.Compare(q.Key, "blob", true) == 0)
    .Value;

return string.IsNullOrWhiteSpace(blob) || string.IsNullOrWhiteSpace(container)
    ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a container and blob names on the query string.")
    : req.CreateResponse(HttpStatusCode.OK, $"{container}/{blob}");
```






[CreateProject]:images\001_CreateProj.png
[CreateFunction1]:images\002_AddFunction1.png
[CreateFunction2]:images\002_AddFunction2.png
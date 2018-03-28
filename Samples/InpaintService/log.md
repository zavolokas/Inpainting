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

## What is Azure Durable Functions?
Durable Functions is an extension of Azure Functions and Azure WebJobs that lets you write stateful functions in a serverless environment. The extension manages state, checkpoints, and restarts for you.

The extension lets you define stateful workflows in a new type of function called an orchestrator function. Here are some of the advantages of orchestrator functions:
- They define workflows in code. No JSON schemas or designers are needed.
- They can call other functions synchronously and asynchronously. Output from called functions can be saved to local variables.
- They automatically checkpoint their progress whenever the function awaits. Local state is never lost if the process recycles or the VM reboots.

 Control flow is implemented using normal imperative coding constructs.

 Once an instance is started, the extension exposes webhook HTTP APIs that query the orchestrator function status.

 Behind the scenes, the Durable Functions extension is built on top of the Durable Task Framework, an open-source library on GitHub for building durable task orchestrations. Much like how Azure Functions is the serverless evolution of Azure WebJobs, Durable Functions is the serverless evolution of the Durable Task Framework. The Durable Task Framework is used heavily within Microsoft and outside as well to automate mission-critical processes. It's a natural fit for the serverless Azure Functions environment.

 ### How does it work?
 Each time the code calls await, the Durable Functions framework checkpoints the progress of the current function instance. If the process or VM recycles midway through the execution, the function instance resumes from the previous await call. 

 Orchestrator functions reliably maintain their execution state using a cloud design pattern known as Event Sourcing. Instead of directly storing the current state of an orchestration, the durable extension uses an append-only store to record the full series of actions taken by the function orchestration. This has many benefits, including improving performance, scalability, and responsiveness compared to "dumping" the full runtime state.

 The use of Event Sourcing by this extension is transparent. Under the covers, the await operator in an orchestrator function yields control of the orchestrator thread back to the Durable Task Framework dispatcher. The dispatcher then commits any new actions that the orchestrator function scheduled (such as calling one or more child functions or scheduling a durable timer) to storage. This transparent commit action appends to the execution history of the orchestration instance. The history is stored in a storage table. The commit action then adds messages to a queue to schedule the actual work. At this point, the orchestrator function can be unloaded from memory. Billing for it stops if you're using the Azure Functions Consumption Plan. When there is more work to do, the function is restarted and its state is reconstructed.

Once an orchestration function is given more work to do (for example, a response message is received or a durable timer expires), the orchestrator wakes up again and re-executes the entire function from the start in order to rebuild the local state. If during this replay the code tries to call a function (or do any other async work), the Durable Task Framework consults with the execution history of the current orchestration. If it finds that the activity function has already executed and yielded some result, it replays that function's result, and the orchestrator code continues running. This continues happening until the function code gets to a point where either it is finished or it has scheduled new async work.

The Durable Functions extension uses Azure Storage queues, tables, and blobs to persist execution history state and trigger function execution. The default storage account for the function app can be used, or you can configure a separate storage account. You might want a separate account due to storage throughput limits. The orchestrator code you write does not need to (and should not) interact with the entities in these storage accounts. The entities are managed directly by the Durable Task Framework as an implementation detail.

Orchestrator functions schedule activity functions and receive their responses via internal queue messages. When a function app runs in the Azure Functions Consumption plan, these queues are monitored by the Azure Functions Scale Controller and new compute instances are added as needed. When scaled out to multiple VMs, an orchestrator function may run on one VM while activity functions it calls run on several different VMs. You can find more details on the scale behavior of Durable Functions in Performance and scale.

Table storage is used to store the execution history for orchestrator accounts. Whenever an instance rehydrates on a particular VM, it fetches its execution history from table storage so that it can rebuild its local state. One of the convenient things about having the history available in Table storage is that you can take a look and see the history of your orchestrations using tools such as Microsoft Azure Storage Explorer.

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

6. Create blob storage
We need a blob storage to store the images to process. That is why we need to create a `storage account`. Go to Azure portal and Click 'Create a resource', then choose 'Storage' and finally 'Storage account - blob, file, table, queue'.

![CreateStorageAccount1]

After that you will need to fill in a Name, Resource Group and Location.

![CreateStorageAccount2]

> Question: Resource Group - what is it and what for?

After that we need to get the connection string for the created storage accout.

![StorageConnectionString]

7. Read from blob

> Question: Is it possible to get a binding to a blob from a Http trigger?

Now we are prepared to read a data from a blob. First we need to adjust `local.settings.json` to contain the connection string.

```Json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "",
    "AzureWebJobsDashboard": ""
  },
  "ConnectionStrings": {
    "Storage": "DefaultEndpointsProtocol=https;AccountName=<account name>;AccountKey=<account key>;EndpointSuffix=core.windows.net"
  }
}
```
Now we can read a data from a blob(don't forget to upload one).
```CSharp
var connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.Storage);
var storageAccount = CloudStorageAccount.Parse(connectionString);
var blobClient = storageAccount.CreateCloudBlobClient();
var container = blobClient.GetContainerReference(containerName);
var blob = container.GetBlockBlobReference(blobName);

using (var imageData = new MemoryStream())
{
    await blob.DownloadToStreamAsync(imageData);
}
```

# Issues
- 64K message
- async code inside Orchestration
- not breaking resistent code (nnf stored in nnf.json is scaled one more time after a failure)
- difficult to debug because of saved and restored state.
- get time & timer functionality



[CreateProject]:images\001_CreateProj.png
[CreateFunction1]:images\002_AddFunction1.png
[CreateFunction2]:images\002_AddFunction2.png
[CreateStorageAccount1]:images\003_CreateStorageAccount.png
[CreateStorageAccount2]:images\004_CreateStorageAccount2.png
[StorageConnectionString]:images\005_StorageConnectionString.png
# Azure Durable Functions

Sergey Zavoloka

---

## Problem
bla bla
```CSharp
[FunctionName(Name)]
public static async Task Orchestrate(
    [OrchestrationTrigger] DurableOrchestrationContext ctx)
{
    var inpaintRequest = ctx.GetInput<InpaintRequest>();

    var pyramid = await ctx.CallActivityAsync<CloudPyramid>(PyramidsGenerateActivity.Name, inpaintRequest);
}
```

---

# Q&A
Thank you!

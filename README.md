# Telegram Pipelines
Little C# framework for building stateless telegram bots with support of complex conversations. Its currently very wip but you can already use it.

## Concept
Main purpose of that framework is to provide an ability to make reusable atomic mini conversations that can be used inside another conversations and so on, they called `Pipeline`.

To support complex conversations you need to store state of current conversation but if you would do it in memory it would lead to lost of conversation progress when your app goes down.
To prevent it you can store state of conversation in database (Redis, Mongo and so on). Framework offers support of local storage system for your pipelines, currently there is only Redis provider but you can create your own (I'll tell later).

Its expected that you would nest pipelines for building more complex pipelines, when parent pipeline indicates that its finished or aborted it will clear its storage and storages of all its children pipelines.

## Getting Started
Install:
```
$ dotnet add package TelegramPipelines
```
```
$ dotnet add package TelegramPipelines.Abstractions
```
```
$ dotnet add package TelegramPipelines.RedisLocalStorage
```

Add background service using extension method:
```csharp
services.AddTelegramPipelinesWorker();
```
But you need to provide `IRecursiveLocalStorageFactory` and main pipeline using:
```csharp
services.AddTelegramPipelinesWorker()
            .AddTelegramMainPipeline<MyMainPipeline>()
            .AddRecursiveLocalStorageFactory<RedisRecursiveLocalStorageFactory>();
```

## Pipelines
You can create pipeline using interface `ITelegramPipelineClass<TPipelineReturn>` or just provide a delegate `Task<TPipelineReturn?>TelegramPipelineDelegate<TPipelineReturn>(TelegramPipelineContext context)`.

In pipeline you can access local storage `IRecursiveLocalStorage`. You can use it to save or get some state of current pipeline:
```csharp
public async Task<Accumulator?> Execute(TelegramPipelineContext context)
    {
        // Getting Accumulator instance from local storage under acc key
        Accumulator acc = await context.LocalStorage.Get<Accumulator>("acc") ?? new Accumulator(0);

        // Sending message to user with current Accumulator value
        await context.TelegramBotClient.SendTextMessageAsync(context.Update.Message!.Chat, $"Current number {acc.Value}");

        acc = acc with { Value = acc.Value + 1 };
        
        // Saving updated Accumulator value to acc key        
        await context.LocalStorage.Save("acc", acc);

        return null;
    }
```
You can run nested pipelines inside pipelines, it will create tree dependency between pipelines with parents and children (This time instead of creating class i made a delegate):
```csharp
async context => 
{ 
    await context.NestedPipelineExecutor.Execute("biba", new NumbersAccumulatorPipeline("bibochka")); 
    return null; 
}
```
Nested pipelines access to same instance of `Update` and `TelegramBotClient` but it has different storage. When you nesting pipelines child pipelines getting child storages from parents.

Pipelines has a lifetime. When pipeline returns not null value it means that pipeline finished his work so its storage will be cleared and also all nested pipelines storages will be cleared too. When pipeline throws unhandled exception it will be considered that pipeline finished too.
```csharp
async context => 
{  
    // Pipeline not finished. Storage remain
    return null; 
}
```
```csharp
async context => 
{  
    await context.NestedPipelineExecutor.Execute("biba", new NumbersAccumulatorPipeline("bibochka"));
    // Returned not null so storage will be cleared and all nested pipelines storages will be cleared too
    return ""; 
}
```

Each pipeline has name and identity. Identity consist of `chat id` and `root` root is a full call graph of pipelines names.
If pipeline with name `Tom` called from main pipeline it will have root `[main, Tom]`. Pipelines with different identities has different storages so if you store something under same key in pipelines with different identities it would be overriden. Likewise you can execute two different pipelines but with same name and they would share same storage: 
```csharp
async context => 
{  
    // Two completly different pipeliens under same name share same storage
    await context.NestedPipelineExecutor.Execute("biba", new NumbersAccumulatorPipeline());
    await context.NestedPipelineExecutor.Execute("biba", new PasswordConversation());
    return null; 
}
```

## Custom Local Storage Provider
Currently there are two providers: Redis and InMemory. You can create your own by implementing `IRecursiveLocalStorage` and `IRecursiveLocalStorageFactory` in `TelegramPipelines.Abstractions`.
In `IRecursiveLocalStorage` there are some points that can be confusing. 

`Task AddChildStorage(IRecursiveLocalStorage newChildStorage)` This method should bound child storage to parent and when `Task ClearStorageAndAllItsChildren()` called parent and all child storages must be cleared.

You can check examples in `TelegramPipelines.RedisLocalStorage` and `TelegramPipelines.JObjectLocalStorage`. Also there is a demo project that shows how to use framework `TelegramPipelines.WorkerDemo`.
# LogMagic [![NuGet](https://img.shields.io/nuget/v/LogMagic.svg?maxAge=2592000?style=flat-square)](https://www.nuget.org/packages/LogMagic/)

![logmagic icon](https://alonestore.blob.core.windows.net/nuget/logmagic.png) 

## Why LogMagic

Like many other libraries for .NET, LogMagic provides diagnostic logging into files, the console, and *elsewhere*. It's probably the easiest framework to setup, has a clean API, extremely extensible.

LogMagic also supports a relatively new paradigm of *structured logging*.

## Index

- [Visual Studio Integration (snippets)](doc/vssnippets.md)
- [List of known writers and enrichers](doc/providers.md)

## Installation

### Installing from NuGet

The core logging package is [LogMagic](https://www.nuget.org/packages/LogMagic). Supported frameworks are:

* **.NET 4.5**
* **.NET Standard 1.6**. By supporting .NET Standard we ensure that LogMagic works on .NET Core, Android, iOS or anything else.

> Note to ASP.NET Core users: LogMagic does not integrate with ASP.NET Core logging system, it's a separate logging framework you can use in conjunction with or instead of it. Both system have their pros and cons.

```
PM> Install-Package LogMagic
```

### Setup

Types are in `LogMagic` namespace

```csharp
using LogMagic;
```

An `ILog` instance is the one used to log events and can be created in one of three ways by calling to global static `L` class.

Create using current class type name

```csharp
ILog _log = L.G();
```

Or by specifying type explicitly

```csharp
ILog _log = L.G<T>;			//using generics
ILog _log = L.G(typeof(T));	//passing type
```

Or by specifying name explicitly

```csharp
ILog _log = L.G("instance name");
```

By default LogMagic doesn't write events anywhere and you need to configure it:

```csharp
L.Config.WriteTo.Console();
```

This is typically done once at application startup.

### Example application

The complete example below shows logging in a simple console application, with events sent to the console as well as to file on disk.

1. **Create a new Console Application project**
2. **Install the core LogMagic package**

In Visual Studio, open Package Manager Console and type:

```powershell
Install-Package LogMagic
```

3. **Add the following code to `Program.cs`**

```csharp
using System;
using LogMagic;

namespace LogMagicExample
{
   public class Program
   {
      private readonly ILog _log = L.G();

      public static void Main(string[] args)
      {
         L.Config
            .WriteTo.Console()
            .EnrichWith.ThreadId();

         new Program().Run();

         Console.ReadLine();

         L.Shutdown();
      }

      private void Run()
      {
         _log.I("hello, LogMagic!");

         int a = 10, b = 0;

         try
         {
            _log.D("dividing {a} by {b}", a, b);
            Console.WriteLine(a / b);
         }
         catch(Exception ex)
         {
            _log.E("unexpected error", ex);
         }

         _log.D("attempting to divide by zero");
      }

   }
}
```

4. **Run the program**

## Logging exceptions

LogMagic always check last parameter of log arguments whether it's an exception class and eliminates from the argument list.


## Configuration basics

LogMagic uses C# API to configure logging.

### Log writers

Log event writers generally record log events to some external representation, typically console, file, or external data store. LogMagic has a few built-in writers to get you started. More writers are redistributed via NuGet.

A curated list of available packages are listed below on this page.

Writers are configure using `WriteTo` congiguration object.

```csharp
L.Config.WriteTo.PoshConsole();
```

Multiple writers can be active at the same time.

```csharp
L.Config
	.WriteTo.PoshConsole()
	.WriteTo.Trace();
```

### Formatting

Text-based writers support message formatting. Whenever a `format` parameter appears in writer configuration you can specify your own one:

```csharp
L.Config
	.EnrichWith.ThreadId()
	.WriteTo.Console("{time:H:mm:ss,fff}|{threadId}{level,-7}|{source}|{message}{error}");
```

Enriched properties can also appear in the output. In the example above a `threadId` comes from thread ID ennricher.

#### Formatting syntax

Built-in property names:
- `{time}` - used to reference event time
- `{level}` - prints message severity
- `{source}` - logging source name
- `{message}` - log message
- `{error}` - error

All of the properties support standard .NET formatting used in `string.Format()`.

### Enrichers

Enrichers are simple components that add properties to a log event. This can be used for the purpose of attaching a thread id, machine IP address etc. for example.

```csharp
L.Config.EnrichWith.ThreadId();
```

### Context Information

Sometimes it's useful to add context information to a logging session during a call duration scope. LogMagic achieves it by dynamically adding and removing propeties from the ambient "execution context". For example, all messages during a transaction might carry the id of that transaction, and so on.

The feature does not need any special configuration, and properties can be added an removed using `L.ContextProperty()`:

```csharp
log.D("no properties");

using(L.CP("A", "id1"))
{
	log.D("carries property A=id1");

	using(L.CP("B", "id2"))
	{
		log.D("carries A=id1 and B=id2");
	}

	log.D("carries property A=id1");

	using(L.CP("A", "id3"))
	{
		log.D("carries property A=id3");
	}

	log.D("carries property A=id1");
}

log.D("no properties");
```

Pushing property onto the context will override any existing properties with the same name, until the object returned from `L.CP()` is disposed, as the property A in the example demonstrates.

**Important:** properties must be popped from the context in the precise order in which they were added. Behavior otherwise is undefined.

## Writing log events

Log events are written to writers using the `ILog` interface. Typically you will instantiate it on top of your class definition you want to log from. 

```csharp
private readonly ILog _log = L.G<Class>();

// ...

_log.I("application v{version} started on {date}", "1.0", DateTime.UtcNow);
```

### Message template syntax

the string above `"application v{version} started on {date}"` is a *message template*. Message templates are superset of standard .NET format string, so any format string acceptable to `string.Format()` will also be correctly processed by LogMagic.

- Property names are written between `{` and `}` brackets
- Formats that use numeric property names, like `{0}` and `{1}` exclusively, will be matched with the log method's parameters by treating the property names as indexes; this is identical to `string.Format()`'s behavior
- If any of the property names are non-numeric, then all property names will be matched from left-to-right with the log method's parameters
- Property names, both numeric and named, may be suffixed with an optional format, e.g. `:000` to control how the property is rendered; these format strings behave exactly as their counterparts within the `string.Format()` syntax

### Log event levels

LogMagic uses levels as the primary means for assigning importance to log events. The levels in increasing order of importance are:

1. **Debug** - internal control flow and diagnostic state dumps to facilitate pinpointing of recognised problems
2. **Information** - events of interest or that have relevance to outside observers
3. **Warning** - indicators of possible issues or service degradation
4. **Error** - fa failure within application or connected system, critical errors causing complete failure

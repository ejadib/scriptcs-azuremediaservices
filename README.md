scriptcs-azuremediaservices
===========================

Windows Azure Media Services Script Pack

## What is it?
Script pack for accessing Azure Media Services from scriptcs CSX script and scriptcs REPL.

## Usage
```csharp
var mediaServices = Require<AzureMediaServices>();

var client = mediaServices.CreateClient("mediaServicesAccountName", "mediaServicesAccountKey");

var assets = client.GetAssets(a => a.Name != "Ez");

Console.WriteLine(assets.Count());
```

## What's next
* Add support for more Media Services operations
* Listen to community feedback.

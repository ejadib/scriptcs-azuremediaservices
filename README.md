scriptcs-azuremediaservices
===========================

Windows Azure Media Services Script Pack

## What is it?
Script pack for accessing Azure Media Services from scriptcs CSX script and scriptcs REPL.

## Usage
Install the [nuget package](https://nuget.org/packages/ScriptCs.AzureMediaServices/0.1) by running `scriptcs -install ScriptCs.AzureMediaServices`.

```csharp
var mediaServices = Require<AzureMediaServices>();

var client = mediaServices.CreateClient("mediaServicesAccountName", "mediaServicesAccountKey");

var assets = client.GetAssets(a => a.Name != "Ez");

Console.WriteLine(assets.Count());

var myAsset = client.GetAsset("nb:cid:UUID:8131a85d-5999-555c-a30f-468cb087701c");

Console.WriteLine(myAsset.Name);

```

## What's next
* Add support for more Media Services operations
* Listen to community feedback.

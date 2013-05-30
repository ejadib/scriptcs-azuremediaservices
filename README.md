scriptcs-azuremediaservices
===========================

Windows Azure Media Services Script Pack

## What is it?
Script pack for accessing Azure Media Services from scriptcs CSX script and scriptcs REPL.

## Usage
Install the [nuget package](https://nuget.org/packages/ScriptCs.AzureMediaServices/0.1) by running `scriptcs -install ScriptCs.AzureMediaServices`.

### Creating the Azure Media Services Client
```csharp
var mediaServices = Require<AzureMediaServices>();

var client = mediaServices.CreateClient("mediaServicesAccountName", "mediaServicesAccountKey");
```

### Getting all assets
```csharp
var assets = client.GetAssets();
```

### Getting assets based on a filter condition
```csharp
var assets = client.GetAssets(a => a.AlternateId == "mezzanine");
```

### Getting assets by Id
```csharp
var myAsset = client.GetAsset("nb:cid:UUID:8131a85d-5999-555c-a30f-468cb087701c");
```

### Deleting an asset
```csharp
client.DeleteAsset("nb:cid:UUID:8131a85d-5999-555c-a30f-468cb087701c");
```

### Uploading an asset (*experimental*)
```csharp
var uploader = client.CreateUploader("myAssetName", "d:\media\videos\video.mp4");
uploader.On(
			progress: progressPercentage => Console.WriteLine(progressPercentage),
			completed: assetId => Console.WriteLine(assetId),
			error: exception => Console.WriteLine(exception.Message));
uploader.Start();
```

### Getting all media processors
```csharp
var mediaProcessors = client.GetMediaProcessors();
```

## What's next
* Add support for more Media Services operations
* Listen to community feedback.
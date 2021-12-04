# TelemetryClient Package for Unity

## Lightweight Analytics That's Not Evil

Please visit [TelemetryDeck.com](https://telemetrydeck.com/) to learn more.

## About this repository

This repository contains the TelemetryDeck Unity C# Client Unity Package for Unity Package Manager. For sample code and documentation, please [visit the TelemetryDeck for Unity parent repository](https://github.com/conath/TelemetryDeck-Unity/).

## Dependencies

As TelemetryClient relies on JSON Serialization of Dictionaries, the JsonUtility built into Unity is not sufficient.

Please add either [Json.NET for Unity](https://github.com/jilleJr/Newtonsoft.Json-for-Unity) (preferred) or [Newtonsoft.JSON](https://github.com/JamesNK/Newtonsoft.Json) to your project. We recommend you install Json.NET for Unity via Unity Package Manager. 

<details>
  <summary>How to add Json.Net for Unity via Unity Package Manager</summary> 

  First add the jilleJr Scoped Registry to your Unity Project settings:

  ![Click on Window, Package Manager. Click the Gear icon, then Advanced Project Settings. In the Project Settings window that opens, fill in the details for the jilleJr scoped registry (follows below). Click Save.](https://github.com/conath/TelemetryDeck-Unity/raw/test-upm/HowToAddJilleJRScopedRegistry.jpg)

  The jilleJr Scoped Registry: Name "Packages from jillejr", URL "https://npm.cloudsmith.io/jillejr/newtonsoft-json-for-unity/" and Scopes "jillejr".

  After you've added the registry, you can proceed with the next section, [Installing with Unity Package Manager](#installing-with-unity-package-manager) (Json.NET will automatically be installed).
  
</details>

You can alternatively edit your Project's `Packages/manifest.json` file directly.

<details>
  <summary>How to add Json.Net for Unity by editing manifest.json</summary> 
  Add the following to the end of the dependencies array:

  ```json
    "jillejr.newtonsoft.json-for-unity": "13.0.102"
  ```

  If your manifest doesn't already include a `scopedRegistries` key, add this before the last `}` in the file:

  ```json
  "scopedRegistries": [
    {
      "name": "Packages from jillejr",
      "url": "https://npm.cloudsmith.io/jillejr/newtonsoft-json-for-unity/",
      "scopes": [
        "jillejr"
      ]
    }
  ]
  ```

  Otherwise, add this into the `scopedRegistries` array:
  
  ```json
    ,
    {
      "name": "Packages from jillejr",
      "url": "https://npm.cloudsmith.io/jillejr/newtonsoft-json-for-unity/",
      "scopes": [
        "jillejr"
      ]
    }
  ```

  The Json.NET for Unity Wiki provides [further instructions on installing their package via UPM](https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Installation-via-UPM).
  
</details>

## Installing with Unity Package Manager

TelemetryClient is available with Unity Package Manager. Simply install the [dependencies](#dependencies), then add [this URL](/) to Unity. (TODO is this how it works?)

## Installing (alternative Unity Package)

You may alternatively download the latest [Unity Package release](/releases) and import it into your project.
Note that you also need to add the Json.NET for Unity or Newtonsoft.JSON dependency to your project - see previous section.

## License

TelemetryClient is licensed unter a [variation of The MIT License](/LICENSE), which does not require attribution.

This means you can use the TelemetryClient Unity Package or source code in your projects without including the license text.

Of course, attribution is very much appreciated. <3

## 3rd Party Licenses

TelemetryClient for Unity uses the [Newtonsoft.Json for Unity](https://github.com/jilleJr/Newtonsoft.Json-for-Unity) package (aka Json.Net), which is licensed unter The MIT License.

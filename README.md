# TelemetryDeck SDK for Unity C#

## Lightweight Analytics That's Not Evil

Please visit [TelemetryDeck.com](https://telemetrydeck.com/) to learn more.

## About this repository

This repository contains the TelemetryDeck SDK for Unity C#. You can install the SDK from this repository using Unity Package Manager (instructions below). For sample code and additional documentation, please [visit the TelemetryDeck SDK example repository](https://github.com/TelemetryDeck/UnityCSharpSDK-Example/).

## Dependencies

As the TelemetryDeck SDK relies on JSON Serialization of Dictionaries, the JsonUtility built into Unity is not sufficient.

Please add either [Json.NET for Unity](https://github.com/jilleJr/Newtonsoft.Json-for-Unity) (preferred) or [Newtonsoft.JSON](https://github.com/JamesNK/Newtonsoft.Json) to your project. We recommend you install Json.NET for Unity via Unity Package Manager. 

<details>
  <summary>How to add Json.Net for Unity via Unity Package Manager</summary> 

  First add the jilleJr Scoped Registry to your Unity Project settings:

  ![Click on Window, Package Manager. Click the Gear icon, then Advanced Project Settings. In the Project Settings window that opens, fill in the details for the jilleJr scoped registry (follows below). Click Save.](https://user-images.githubusercontent.com/12073163/144713841-042ddc47-2cca-4f31-8020-d6f0aca36153.jpg)

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

TelemetryDeck SDK is available in Unity Package Manager. Simply install the [dependencies](#dependencies), then add the package to Unity from the git URL of this repository:

  0. Copy [the URL](https://github.com/TelemetryDeck/UnityCSharpSDK.git)
  1. Open Unity Package Managager in your project (menu Window => Package Manager)
  2. Click on the Plus `+` icon in the top left and choose "Add package from git URL…"
  3. Paste [the URL](https://github.com/TelemetryDeck/UnityCSharpSDK.git)
  4. Click "Add"

If you added the [dependencies](#dependencies) correctly, Unity will show "Please wait, installing a GIT package…". The package will appear in the list after it finishes installing.

If you did not add the dependencies correctly, Unity will show a few errors in the Console window:

```log
[Package Manager Window] Unable to add package https://github.com/TelemetryDeck/UnityCSharpSDK.git:
  Package com.telemetrydeck.unitycsharpclient@https://github.com/TelemetryDeck/UnityCSharpSDK.git has invalid dependencies or related test packages:
    jillejr.newtonsoft.json-for-unity (dependency): Package [jillejr.newtonsoft.json-for-unity@13.0.102] cannot be found
```

Please follow the instructions in the [**Dependencies** section](#dependencies) to fix this.

## Installing manually from Unity Package

You may alternatively download the latest [Unity Package release](https://github.com/TelemetryDeck/UnityCSharpSDK/releases) and import it into your project. This is not recommended, as you will not receive updates in Unity. Make sure to watch the releases for this repository to be notified.

The the [dependencies](#dependencies) are required when using the Unity Package method of installing TelemetryClient, however you do not *have to* use Unity Package Manager to do this.

## License

TelemetryDeck SDK for Unity C# is licensed unter a [MIT No Attribution License](/LICENSE.md).

This means you can use the Unity Package or source code in your projects without including the license text.

Of course, attribution is very much appreciated. <3

## 3rd Party Licenses

This project uses the [Newtonsoft.Json for Unity](https://github.com/jilleJr/Newtonsoft.Json-for-Unity) package (aka Json.NET), which is licensed unter The MIT License.

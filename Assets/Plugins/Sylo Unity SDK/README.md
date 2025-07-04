# Unity Sylo SDk

A Unity Engine plugin that provides a simple API for accessing data associated with a Sylo Decentralized Identifier (DID).

---

## 🔍 What is Sylo?

**Sylo** is a protocol for accessing off-chain decentralized data associated with a **DID (Decentralized ID)**. A Sylo DID looks like:

```
did:sylo-data:0xfFFFffFF0000000000000000000000000000052f/ed38c341-a26a-4426-aed9-4f8f362b70bf
```

This structure breaks down as follows:

- `did:` → Declares this is a decentralized identifier.
- `sylo-data:` → Specifies the method used, in this case `sylo-data`.
- `0xfFFFffFF0000000000000000000000000000052f` → The owner address of the data.
- `ed38c341-a26a-4426-aed9-4f8f362b70bf` → A unique ID for the data being accessed.

---

## ✨ Features

- 🔐 **Futurepass Token-based Authentication**
- 💾 **Load Sylo DID data**
  
---

## 🧩 Installation - Package Manager via Git

1. In the Unity Package Manger, click '+' -> "Add package from git URL".
2. Paste the git url for this project and press "add"

## 🧩 Installation - Import .unitypackage

1. In the [Releases](https://github.com/futureversecom/sdk-unity-sylo/releases) area of this repository, download the .unitypackage for your target version
2. Right-click in the Project window in the Unity engine, and select "Import Package -> Custom Package"
3. Navigate to your downloaded .unityproject file
4. Press "Import" on the next window
---

## 📄 API Reference

### SyloUtilities
The primary source of functions and data when interacting with the Sylo SDK
<details>
  <summary>Properties</summary>
  
  ```cs
  static string ResolverURI; // The resolver URI to use when making data requests
  ```

</details>
<details>
  <summary>Methods</summary>
  
  ```cs
  SetResolverURI(string uri); // Used to set the resolver URI property
  ```
  ```cs
  GetBytesFromDID (string did, ISyloAuthDetails authDetails, Action<byte[]> onSuccess, Action<Exception> onError); // Forms a web request using supplied resolver and DID, authenticates and sends request
  ```

</details>

### SyloDebugExecutor
An optional scene object to demonstrate runtime functionality and API usage.<br>
<details>
  <summary>Properties</summary>

  ```cs
  string debug_did; // The Sylo DID for the target data asset
  ```
  ```cs
  string debug_resolverUri; // The URI of the target Sylo Resolver
  ```
  ```cs
  string debug_accessToken; // The access token used to authenticate the request
  ```
</details>
<details>
  <summary>Methods</summary>

  ```cs
  RunDebug(); // Begin the process to retrieve bytes from the debug DID. Prints results to console.
  ```
</details>

### CoroutineSceneObject
A utility object used to run coroutines from the static `FuturePassAuthentication` class
<details>
  <summary>Properties</summary>

  ```cs
  CoroutineSceneObject Instance; // Singleton reference to the scene object
  ```
</details>

### ISyloAuthDetails
Interface definition for providing authentication details to the data provision process
<details>
  <summary>Properties</summary>

  ```cs
  string GetAccessToken(); // Override this to provide access token
  ```

</details>

## 🛠️ Getting Started: Using the Debug Function

<p>Locate and add the Sylo prefab to your game scene</p>
<p>This prefab has inputs for a data id, sylo resolver and access token.</p>
<img src="docs/sc-prefab.png" width=45%>

<br>
<p>For the purposes of debugging you may use the following:</p>

| Var | Data |
| --- | ---- |
| DID | did:sylo-data:0xfFFFffFF0000000000000000000000000000052f/ed38c341-a26a-4426-aed9-4f8f362b70bf |
| Resolver URI | https://sylo-resolver.data.storage-sylo.futureverse.cloud |

<br>
<p>This Sylo resolver is on the staging environment, and as thus needs to be authenticated with a staging FuturePass access token</p>

To get a valid access token, I recommend utilising the [FuturePass Unity SDK](https://github.com/futureversecom/sdk-unity-futurepass)

<p>Once a valid access token has been added to the DebugSyloExecutor component, press the "Run Debug" button.</p>

<img src="docs/sc-console.png">
<p>After a moment, you should see a result packet written to the Console window. The speed of this will depend on your internet connection.</p>

<br>

## 🛠️ Getting Started: Download Test Asset

```cs
using Futureverse.Sylo;

const string resolverUri = "https://sylo-resolver.data.storage-sylo.futureverse.cloud"
const string dataId = "did:sylo-data:0xfFFFffFF0000000000000000000000000000052f/ed38c341-a26a-4426-aed9-4f8f362b70bf";

string GetAccessToken() {
  // return a valid Futurepass access token.
  // Consider utilising the Futureverse Futurepass Unity SDK found at https://github.com/futureversecom/sdk-unity-futurepass
  return "";
}
void Run() {
  SyloUtilities.SetResolverUri(debug_resolverUri);
  CoroutineSceneObject.Instance.StartCoroutine(
      SyloUtilities.GetBytesFromDID(dataId, new DebugAuthDetails(GetAccessToken()), bytes => Debug.Log($"Received {bytes.Length} bytes"), Debug.LogException)
  );
}
```

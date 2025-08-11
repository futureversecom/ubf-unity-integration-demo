# Futureverse UBF Integration Demo – Unity

The **Futureverse UBF Integration Demo (Unity)** showcases the integration of the [**Unity UBF Runtime**](https://github.com/futureversecom/sdk-unity-ubf) with the [**Futureverse UBF Execution Controller**](https://github.com/futureversecom/sdk-unity-execution-controller), within a third-person Unity project and demonstrates an end-to-end implementation of all the tools and systems required to utilize UBF for your project. 

This demo features:
- **Dynamic avatar rendering** based on wallet inventory items retrieved from Futureverse’s **Asset Register** (using the [Unity Asset Register SDK](https://github.com/futureversecom/sdk-unity-asset-register)).
- **Custodial authentication** using **Futurepass** to perform logins and retrieve associated wallet addresses (using the [Unity Futurepass SDK](https://github.com/futureversecom/sdk-unity-futurepass)).
- **Asset management** via **Sylo** to download and manage supported avatar assets (using the [Unity Sylo SDK](https://github.com/futureversecom/sdk-unity-sylo)).  

> For more information about the Universal Blueprint Framework, its authoring tools, and the full ecosystem, visit the [UBF Open Standard](https://ubfstandard.com/) and the [Futureverse Developer Documentation](https://docs.futureverse.com/build-an-asset/asset-creation).

---

## 🔗 Related Links

- [UBF Open Standard](https://ubfstandard.com/)  
- [Unity UBF Runtime](https://github.com/futureversecom/sdk-unity-ubf-runtime)  
- [Unity UBF Execution Controller](https://github.com/futureversecom/sdk-unity-ubf-execution-controller)  
- [Unity Asset Register SDK](https://github.com/futureversecom/sdk-unity-asset-register)  
- [Unity Futurepass SDK](https://github.com/futureversecom/sdk-unity-futurepass)  
- [Unity Sylo SDK](https://github.com/futureversecom/sdk-unity-sylo)  

---

## ✨ Features

- ⚡ **End-to-end UBF integration**
- 🔗 **Examples of Futureverse SDK usage**: including Futurepass, Sylo, and Asset Register
- 🏃 **Character controller**: showcasing UBF runtime animation config
- 🌏 **Runtime environment switching**: across multiple services

---

## 🧩 Installation

1. **Clone** the `main` branch of this repository:
   ```sh
   git clone https://github.com/futureversecom/ubf-unity-integration-demo.git
   ```
2. **Ensure you have [Unity 2022.3.20f1](https://unity3d.com/get-unity/download/archive) installed.**
3. **Open Unity Hub**, select "Add" → "Add local project from disk".
4. **Navigate to the root** of the cloned repository and open it.

---

## ❔ What is UBF? What does it mean to 'run a blueprint'?

**UBF (Universal Blueprint Framework)** is a set of tools for rendering objects across different engines and platforms.

- Blueprints are created using the [UBF Studio](https://github.com/futureversecom/fv-ubf-studio) tool.
- Blueprints are stored with their required resources (models/textures).
- Experiences (like this demo) download a blueprint and its resources, parse them, and render them.

This demo shows an example of how to implement UBF in an experience to consume those blueprints and resources

---

## 🛠️ Getting Started

### Rendering an Asset via Futurepass Wallet

UBF allows users to render compatible blueprints across engines/platforms.

Steps:
1. Open the "Main Scene".
2. Ensure `MonoClient` and `ExperienceController` have their Environment set to `Production`.
3. Press Play in Unity.
4. When prompted, select **"Enter Pass Address"** and use:
   ```
   0xFffffFFF00000000000000000000000000043d70
   ```
5. Press Enter. The program will load the wallet’s assets into a grid on the screen.
   - Note: Some images may be in `.webp` format, which is not supported by Unity 2022.3.
6. Click a 'jacket' item to load and render it. After the jacket has been downloaded, you will see it render in front of the camera.
   
---

### Runtime Animation

UBF supports runtime animation for humanoid models.

Steps:
1. Open the main scene, press Play.
2. Enter wallet: `0xFffffFFF00000000000000000000000000043d70`
3. In the asset grid, select a humanoid asset (bear, fluf, goblin, etc).
4. The asset will appear and animate (idle/run/jump, etc).

**How it works: (Partybear)**
- The bear’s model comes from the 'Partybears' collection.
- A **Mesh Config** data asset contains a 'rig prefab' and a humanoid avatar compatible with the Partybear GLB
- The **Mesh Config** is registered in `ProjectSettings/UBF -> "MeshConfigs"` with a 'key'
- When a graph is run with a `CreateMeshConfig` node that has a matching key, all meshes/models spawned with that config are retargeted to point at the instantiated rig prefab.
- Finally, the avatar from the **Mesh Config** is applied to an animator parented to the `UBFRuntimeController` used to execute the blueprint.
- **Runtime prefab**: Contains skeleton and sockets/game logic.
- **Unity Avatar**: Maps model skeleton to Unity’s humanoid bones.

This demonstration project has mesh configs setup for both Partybears and Gods&Goblins collections

---

### Blueprint Execution

The `ExperienceController` implements a pipeline of requesting assets from the Asset Register via the Asset Register SDK, and then executing them on UI selection. 

The basic experience flow: 
1. Prompt login flow to get user wallet
2. Use wallet to retrieve assets, and show assets in UI
3. Render the asset on selection

<br>

#### 1. Login Flow

Our demonstration retrieves a users wallet by one of two methods:
- Entering the wallet address directly
- Using the custodial authentication pipeline

The demo implementation (found in `ExperienceController.cs`) initiates the custodial pipeline like so: 
```csharp
private void OnLoginClicked()
{
    // Prompt FuturepassAuthentication to begin custodial login
    Auth.StartLogin(
    () => // Register success callback where we store the retrieved wallet
      {
          loginText.text += "Logged in!";
          wallet = Auth.LoadedAuthenticationDetails.DecodedToken.Futurepass;
          loggedIn = true;
      },
    exception => // Register a failure callback
    {
        loginText.text += "Failed to login!\n";
        loginText.text += exception.Message + '\n';
    });
}
```

Here we see a simple call to the Futurepass SDK with a `StartLogin()` method. The wallet is then extracted from the returned token packet. 
Further details: [Futurepass SDK](https://github.com/futureversecom/sdk-unity-futurepass)

<br>

#### 2. Use wallet to retrieve assets, and show assets in UI

Once the wallet is retrieved, it can be used to fetch the users assets using the `ExecutionController` class. 

```cs
    StartCoroutine(executionController.FetchAssetsFromWallet(wallet, OnAssetsLoaded, OnFailure));
```

The onSuccess callback for this method (filled with `OnAssetsLoaded` in this example) is called with an `Asset[]` structure. These Assets contain information provided from the Asset Register for each entry.
In our case, we are going to use that asset data to populate a grid showing each asset, its tokenID and a profile picture (if one is available and valid)

```cs
private void OnAssetsLoaded(Asset[] assets)
    {
        foreach (var asset in assets)
        {
            var ui = Instantiate(assetUI, assetsGrid);
            ui.Load(asset, () => LoadAsset(asset));
        }
    }
```

<br>

#### 3. Render the asset on selection

Previously we created a hook for `LoadAsset(asset)` in the UI grid population, so that an asset may be selected by clicking on its UI element. 

`LoadAsset` is a simple call: 

```
private void LoadAsset(Asset asset)
    {
        StartCoroutine(executionController.LoadUBFAsset(asset));
    }
```

For this we are taking advantage of the UBF Execution Controller package. This allows us access to useful methods, such as rendering a graph from Asset Register data. 
The Execution Controller will fetch data on the entire tree relating to our asset (clothes, accessories, parts, etc), create an `ArtifactProvider` and use the `UBFRuntimeController` to execute it in the scene. 

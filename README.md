# UBF Unity Integration Demo

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [What is UBF?](#what-is-ubf-what-does-it-mean-to-run-a-blueprint)
- [Getting Started](#getting-started)
  - [Rendering an Asset via Futurepass Wallet](#rendering-an-asset-via-futurepass-wallet)
  - [Runtime Animation](#runtime-animation)
  - [Authentication Pipeline](#authentication-pipeline)
  - [Asset Registry Queries](#asset-registry-queries)
- [Further Resources](#further-resources)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

The UBF Integration Demo (Unity) demonstrates an end-to-end implementation of all the tools and systems required to utilize UBF for your project.  
It includes authentication with Futurepass, data retrieval with Sylo, and asset queries with the Asset Registry.

---

## Features

- ⚡ **End-to-end UBF integration**
- 🔗 **Examples of Futureverse SDK usage**: including Futurepass, Sylo, and Asset Registry
- 🏃 **Character controller**: showcasing UBF runtime animation config
- 🌏 **Runtime environment switching**: across multiple services

---

## Installation

1. **Clone** the `main` branch of this repository:
   ```sh
   git clone https://github.com/futureversecom/ubf-unity-integration-demo.git
   ```
2. **Ensure you have [Unity 2022.3.20f1](https://unity3d.com/get-unity/download/archive) installed.**
3. **Open Unity Hub**, select "Add" → "Add local project from disk".
4. **Navigate to the root** of the cloned repository and open it.

---

## What is UBF? What does it mean to 'run a blueprint'?

**UBF (Universal Blueprint Framework)** is a set of tools for rendering objects across different engines and platforms.

- Blueprints are created using the **UBF Studio** tool.
- Blueprints are stored with their required resources (models/textures).
- Experiences (like this demo) download a blueprint and its resources, parse them, and render them.

This demo shows an example of how to implement UBF in an experience to consume those blueprints and resources

---

## Getting Started

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

### Authentication Pipeline

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
Further details: [Futurepass SDK Documentation](https://github.com/futureversecom/sdk-unity-futurepass)

---

### Asset Registry Queries

> _TODO add details of experience implementation of AR SDK_

---

## Further Resources

- [UBF Studio](#)
- [Asset Registry SDK]
- [Sylo SDK]
- [Futurepass SDK](https://github.com/futureversecom/sdk-unity-futurepass)

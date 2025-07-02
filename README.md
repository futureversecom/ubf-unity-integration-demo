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
5. Press Enter. The program will load the wallet’s assets into a grid.
   - Note: Some images may be in `.webp` format (Unity 2022 may require a plugin to view).
6. Click a 'jacket' item to load and render it.

> _Replace with screenshots or a short video walkthrough for clarity._

---

### Runtime Animation

UBF supports runtime animation for humanoid models.

Steps:
1. Open the main scene, press Play.
2. Enter wallet: `0xFffffFFF00000000000000000000000000043d70`
3. In the asset grid, select a bear asset (humanoid).
4. The bear will appear and animate (idle/run/jump, etc).

**How it works:**
- The bear’s model comes from the 'Partybears' collection.
- **Mesh Configs**: Set up data objects to animate GLB models.
- **Runtime prefab**: Contains skeleton and sockets/game logic.
- **Unity Avatar**: Maps model skeleton to Unity’s humanoid bones.

To create a new MeshConfig:
- Duplicate an existing one or right-click in the Project panel: `/Create/UBF/Mesh Config`.
- Register your MeshConfig in Unity Project Settings under UBF.

> _Add example images or annotated screenshots here._

---

### Authentication Pipeline

You can retrieve a user's wallet by:
- Entering the wallet address directly
- Using the custodial authentication pipeline

**Example implementation:**
```csharp
private void OnLoginClicked()
{
    loginText.text = "Connecting...\n";
    Auth.StartLogin(() =>
    {
        loginText.text += "Logged in!";
        wallet = Auth.LoadedAuthenticationDetails.DecodedToken.Futurepass;
        Debug.Log("Logged in with wallet: " + wallet);
        loggedIn = true;
    }, exception =>
    {
        loginText.text += "Failed to login!\n";
        loginText.text += exception.Message + '\n';
    });
}
```
Further details: [Futurepass SDK Documentation](https://github.com/futureversecom/sdk-unity-futurepass)

---

### Asset Registry Queries

> _Add example code and screenshots for performing asset registry queries once implementation is complete._

```csharp
// Example: Query asset registry for available assets
// (Implement and document here)
```

---

## Further Resources

- [UBF Studio](#)
- [Asset Registry SDK]
- [Sylo SDK]
- [Futurepass SDK]

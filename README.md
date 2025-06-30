# ubf-unity-integration-demo

## Overview

The UBF Integration Demo (Unity) has been created to show-case an end-to-end implementation of all the tools and systems required to utilise UBF for your project. 
This includes implementing authentication with Futurepass, data retrieval with Sylo and asset queries with Asset Registry. 

## ✨ Features

- ⚡ **End-to-end UBF integration**
- 🔗 **Examples of Futureverse SDK usage including Futurepass, Sylo and Asset Registry**
- 🏃 **Character controller showcasing UBF runtime animation config**
- 🌏 **Runtime environment switching across multiple services**
  
---

## 🧩 Installation

1. Clone the 'main' branch of this repository into your local folder system
2. Ensure you have Unity 2022.3.20f1 installed
3. Open Unity Hub, and select "Add -> Add local project from disk"
4. Navigate to the root of the cloned repository, and open
---

## 🏡 Scene Layout - Main Scene
The "Main Scene" scene is the primary touchpoint for using the UBF Integration Demo. It contains all the functionality needed to render a game ready UBF asset. 
The "Main Scene" can be found in the Assets/Scenes folder. 

To start, lets go over the key objects and components in the scene: 
<br><br>

<ul>
  
### Environment / Main Camera
Includes the 3D models and colliders for the level geometry.

---
### Player Objects 
Contains a cinemachine camera target for the player, and also a 'Player Armature'. The armature contains an animator, character controller and UBF Runtime Controller; everything we need to render an animated character.

---
### Controllers -> Experience Controller
This component is the primary controller for scene logic, connecting the functionality between UI, UBF, authentication and AR systems.

---
### Controllers -> Execution Controller
This component is the scene object used for retrieving and executing Asset Registry (AR) UBF graphs. It executes queries, builds context trees and forwards them to the Runtime Controller for execution.

---
### UI
This object tree contains all the UI needed for scene interaction, including authentication / wallet entry, an asset grid for object selection and version information text.
</ul>

<br>

---
## 🛠️ Getting Started: Runtime Animation via Futurepass Wallet

One of the great features of UBF is the ability to render and animate humanoid models out of the box. 
To showcase this, we are going to look at rendering an asset from an existing UBF collection inside our demo. 

First, navigate to the "Main Scene" scene. This scene is where the vast majority of UBF functionality can be found. 

   
```cs
// todo
```

---

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

## ❓ What is UBF? What does it mean to 'run a blueprint'?

<p>'UBF' is an acronym for "Universal Blueprint Framework". It encompasses a set of tools used to render objects across different engines and platforms.

  This starts with the creation of a 'blueprint' - a set of instructions to render your asset. You can create blueprints using the 'UBF Studio' tool. 

Blueprints are then stored with whatever resources (such as models or textures) that are required to run the blueprint. 
Finally, an experience (such as this project!) can download that blueprint and its resources, parse them, and run them to render the asset.</p>

<p>This 'demo integration' project will show you how easy it is to begin rendering the wide variety of blueprints and assets that are already present in the Futureverse ecosystem.

  We do this using the tools that have been provided to interact with Futureverse systems. 
- The Futurepass SDK allows us to use a custodial login pipeline to 'log in' to Futureverse and access our assets. 
- The Sylo SDK provides the functionality to access decentralised asset storage.
- The Asset Registry SDK gives the tools to query the Asset Registry, a database of assets and their details such as ownership, storage location and metadata. </p>

---

## 🏡 Scene Layout - Main Scene
The "Main Scene" scene is the primary touchpoint for using the UBF Integration Demo. It contains all the functionality needed to render a game ready UBF asset and can be found in the "Assets/Scenes" folder.

The following is a list of significant objects and components within the scene, and should serve as a touch-point for you to investigate the scene. 
Feel free to skip forward to the 'Getting Started' section if you prefer to experience runtime functionality first!
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
## 🛠️ Getting Started: Rendering an Asset via Futurepass Wallet

// TODO sort staging wallet
// TODO progress pictures

UBF is designed to allow users to render any compatible blueprint across a variety of engines and platforms. 
So, to begin, lets render a simple jacket asset. 

First, ensure you have the "Main Scene" open, as this is where we will run our test. The MonoClient and ExperienceController components should both have their Environment set to Staging.

Start your scene with the play button at the top of the editor, and when presented with the option, select "Enter Pass Address". This allows us to load assets from a specific wallet without the full custodial login process. Enter the wallet as follows: 
`0xFffffFFF00000000000000000000000000043d70`

Press 'Enter' when you have finished entering the wallet and you will now see our program load the wallets assets. 
On the left, you will see a grid begin to populate. Many, if not all, assets have images associated with them for easy viewing. However, some of these are in the .webp format, which will render as white (a known unity issue). 

Using the wallet above, you will see a 'jacket' item. If you click on this the UBF controller will begin to load that jacket for us to see. It will take a variable amount of time to appear depending on your internet connection: the blueprint and its resources are being downloaded from the Asset Registry and pieced together right in front of us. 


```cs
// todo
```

---
## 🛠️ Getting Started: Runtime Animation

One of the great features of UBF is the ability to render and animate humanoid models out of the box. 
To showcase this, we are going to look at rendering an asset from an existing UBF collection inside our demo. 

We are going to follow a very similar process to the above 'Rendering an Asset' tutorial. 
Navigate to the main scene, and play. Enter the wallet: `0xFffffFFF00000000000000000000000000043d70`
Once the assets have loaded in the grid, you will see some are humanoid: bears, rabbits (flufs) and goblins. 
For this demonstration, select one the bear assets to render. 

Once it has loaded, you will notice that it is using an idle animation! Using your mouse and keyboard, you can run around with your bear, showcasing the different animations being blended.

Now that we have seen the end result, lets go over the process. 

The bear belongs to a collection of assets named 'Partybears'. The body of a partybear is a GLB model with a UBF fur shader applied, and that is what you see being rendered. The GLB model of that bear is 'rigged' to a skeleton. This skeleton is made in a fashion to be compatible with unity humanoid systems. However, Unity cannot just detect this out of the box. This is why we use the UBF 'Mesh Config' system. 

Mesh Configs allow us to setup a pre-existing data object in the project that can provide Unity with all the details it needs to animate the incoming GLB. It takes two variables: a runtime prefab, and a humanoid avatar. 

The runtime prefab is a Unity prefab that contains the skeleton to animate. An advantage of using the prefab architecture is that should you wish to add sockets or game logic, this is an easy place to do so. In the test case for our Partybears, the rig prefab just contains our basic bear skeleton. 

A humanoid avatar, or Unity avatar, contains the details of what bones from the bear skeleton should be matched up to the associated Unity humanoid bone. This allows the animator to translate a standard humanoid animation onto our spawned GLB model. 

These elements combined allow a procedurally spawned GLB object to be animated using the Unity humanoid animation system.

To view the MeshConfig for the Partybear collection, navigate to Assets/Collections/Data in the Project panel. Selecting the MC_Partybear asset, you will be able to locate what assets were used for its rig prefab, and humanoid avatar. 

To create a new MeshConfig, you can duplicate an existing one, or right-click in the project panel, /Create/

1. Reference above asset render process
2. Look at player armature (setup of animator, position of runtime controller
3. Look at existing mesh configs (partybears and goblins)
4. Look at AR controller
   
```cs
// todo
```

---


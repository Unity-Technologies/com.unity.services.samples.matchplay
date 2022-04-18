# Matchplay - Matchmaker + Multiplay Sample

_Tested with Unity 2021.2 for PC and OSX clients, and Linux Headless servers._

This sample demonstrates how to create a "Matchmake" button; a basic networked client/server game with a matchmaking feature from end to end using the Unity Engine and Cloud Services SDK.

**Note**: This is not a “drag-and-drop” solution; the Matchplay Sample is not a minimal code sample intended to be completely copied into a full-scale project. Rather, it demonstrates how to use multiple services in a vertical slice with some basic game logic and infrastructure. Use it as a reference to learn how Matchmaker and Multiplay work together to make a common end-user feature.



#### Features:

* **Matchmaking Ticket Config**: Players can set their preferences for the kind of match they want.
* **Matchmaking**: Players can click the matchmaking button to begin looking for a match.
* **Matchmaker Allocation Payload**: Server gets information about the match and configures the server accordingly.
* **Multiplay Server Allocation**: Spin up a dedicated cloud server and pass its information to the player.
* **Basic Client/Server Netcode Experience**: Lightweight server that can be hosted on Multiplay.



### Service organization setup

To use Unity’s multiplayer services, you need a cloud organization ID for your project. If you do not currently have one, follow the **How do I create a new Organization?** article to set up your cloud organization:

[https://support.unity.com/hc/en-us/articles/208592876-How-do-I-create-a-new-Organization-](https://support.unity.com/hc/en-us/articles/208592876-How-do-I-create-a-new-Organization-)

Once you have an ID, link it to your project under **Edit **>** Project Settings **>** Services** and use the Unity Dashboard to manage your project’s services.\




### Service overview


#### **Matchmaker**

The Matchmaker service allows players to search for other players with the same preferences as them and put them in a match together. It is the best way to allow your players a "Find me a good match" button.


The Matchmaker documentation contains code samples and additional information about the service. It includes comprehensive details for using the Matchmaker along with additional code samples, and it might help you better understand the Matchplay: [http://documentation.cloud.unity3d.com/en/collections/3349749-unity-matchmaker](http://documentation.cloud.unity3d.com/en/collections/3349749-unity-matchmaker)

The Lobby service can be managed in the Unity Dashboard:

[https://dashboard.unity3d.com/matchmaker](https://dashboard.unity3d.com/matchmaker)


#### **Multiplay**

The Multiplay Service hosts game servers in the cloud to allow for easy connection between players from around the world with the best ping/performance possible.


The Multiplay documentation contains code samples and additional information about the service. It includes comprehensive details for using Multiplay along with additional code samples, and it might help you better understand the Matchplay Sample: 

[http://documentation.cloud.unity3d.com/en/collections/3254305-multiplay-self-serve](http://documentation.cloud.unity3d.com/en/collections/3254305-multiplay-self-serve)

The Relay service can be managed in the Unity Dashboard:

[https://dashboard.unity3d.com/multiplay](https://dashboard.unity3d.com/multiplay)

In this sample, once players are connected to a server, they are connected through Unity Netcode. Matchmaker and Multiplay both depend on Unity Auth for credentials. This sample uses Unity Auth’s anonymous login feature to create semi-permanent credentials that are unique to each player but do not require developers to maintain a persistent account for them. \



#### **Setup**

The Matchmaker and Multiplay sections of the Unity Dashboard contain their own setup instructions. Select **About & Support **>** Get Started** and follow the provided steps to integrate the services into your project.

With those services set up and your project linked to your cloud organization, open the **bootStrap** scene in the Editor and begin using the Matchplay Sample.


### Running the sample

You will need two “players” to demonstrate the full sample functionality. Create a second build, or use the included Parrelsync plugin to create "clones" of the project to test.

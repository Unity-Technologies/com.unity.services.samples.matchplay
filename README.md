
# Matchplay - Matchmaker + Multiplay Sample

**Note**: This sample was tested with Unity 2021.2 for PC and OSX clients, and Linux Headless servers.

This sample demonstrates how to create a **Matchmake** button: a basic networked client-server game with a matchmaking feature from end to end using the Unity Engine and Cloud Services SDK.

**Note**: This is not a “drag-and-drop” solution; the Matchplay sample is not a minimal code sample intended to be copied into a full-scale project. Rather, it demonstrates how to use multiple services in a vertical slice with some basic game logic and infrastructure. Use it as a reference to learn how Matchmaker and Multiplay work together to make a common end-user feature.

## Table of contents

* [Features](#features)
* [Project Overview](#project-overview)
  * [Scenes](#scenes)
  * [Run as a client](#run-as-a-client)
  * [Run as a server](#run-as-a-server)
  * [Test Netcode locally with ParrelSync](#test-netcode-locally-with-parrelsync)
  * [Cloud Project and Organization](#cloud-project-and-organization)
  * [Services](#services)
    * [Authentication](#authentication)
    * [Matchmaker](#matchmaker)
    * [Multiplay](#multiplay)
* [Sample Setup Guide](#sample-setup-guide)
  * [Unity Matchmaker](#unity-matchmaker)
    * [Match Rules](#match-rules)

## Features



**Matchmaking Ticket Config**: Players set their preferences for the map and game mode they want.
**Matchmaking**: Players click the **Matchmake** button to begin looking for a match.
* **Matchmaker Allocation Payload**: The server gets information about the match and configures the server accordingly.\
* **Multiplay Server Allocation**: Spins up a dedicated cloud server and passes its information to the player.\
* **Client Server Netcode**: A lightweight server that can be hosted on Multiplay.

## Project Overview


The project runs in client or server modes and in local service modes. The server will attempt to find services for 5 seconds, and if it can’t, it starts the server with some default settings for local testing. The client can directly connect to this server using the local host ip.

 
### Scenes



**bootStrap**: You must play from this scene for the sample to work.
* **mainMenu** - Displays the matchmake and local connection buttons to users.\
* **game_lab** - A game scene with a table.\
**game_space**: A game scene which features a sphere.

### Run as a client

The project runs as a client in the following scenarios:

You play the Matchplay project in-editor from the **bootStrap** scene.
* You run a non-server build on your platform of choice.
You play a ParrelSync clone project with “client” in the arguments field from the **bootStrap** scene.

The client can either connect using the Matchmaker or through a local connection with the UI.

Matchmaker won't run without services being set up; follow the [Sample Setup Guide](https://github.com/Unity-Technologies/com.unity.services.samples.matchplay#Sample-Setup-Guide) to set them up.


### Run as a server

The project runs as a server in the following scenarios:



* You run a server build on your platform of choice.
You run a ParrelSync clone project with “server” in the argument field from the **bootStrap** scene.
* Multiplay will run the server automatically when the matchmaker finds a match.

Depending on the context the server is running in, it will either fetch its configuration from the matchmaker or start with the default values.


### Test Netcode locally with ParrelSync

To test Netcode locally with ParrelSync:

1. Go to the top bar and select **ParrelSync** > **Clones Manager** > **Add new Clone**. This duplicates your project folder and synchronizes the contents to allow for iteration.
2. Input "server" in the arguments field so the clone plays in local server mode.
3. Once the server starts, return to the base project and select **Play**.
4. Once the mainMenu scene loads, select local and Play to connect to the local server.

**Note**: The default IP for local servers is (127.0.0.1:7777).



### Cloud Project and Organization

To use Unity’s Multiplayer services, you need a cloud organization ID for your project. Follow the [How do I create a new Organization](https://support.unity.com/hc/en-us/articles/208592876-How-do-I-create-a-new-Organization-) guide to set up your org.

To learn how to connect your project with services, follow the [Setting up Project Services](https://docs.unity3d.com/Manual/SettingUpProjectServices.html) guide.

### Services


#### Authentication

Matchmaker and Multiplay depend on [Unity Authentication](https://docs.unity3d.com/Manual/com.unity.services.authentication.html) 2.0 for credentials. This sample uses Unity Authentication’s anonymous login feature to create semi-permanent credentials unique to each player but do not require developers to maintain a persistent account for them.


#### Matchmaker

The [Matchmaker service](http://docs.unity.com/matchmaker) allows players to search for other players with the same preferences as them and puts them in a match together.

The Matchmaker documentation contains code samples and additional information about the service. It includes comprehensive details for using the Matchmaker along with additional code samples, and it might help you better understand the Matchplay sample. 

The Matchmaker service can be managed in the [Unity Dashboard](https://dashboard.unity3d.com/matchmaker).


#### Multiplay

The [Multiplay service](http://documentation.cloud.unity3d.com/en/collections/3254305-multiplay-self-serve) hosts game servers in the cloud to allow for easy connection between players from around the world with the best ping performance possible.

The Multiplay documentation contains code samples and additional information about the service. It includes comprehensive details for using Multiplay along with additional code samples to help you better understand the Matchplay sample.

The Multiplay service can be managed in the [Unity Dashboard](https://dashboard.unity3d.com/multiplay)


## Sample Setup Guide


1. Link your Editor project to the Cloud Project as described in [Cloud Project and Organization](https://github.com/Unity-Technologies/com.unity.services.samples.matchplay#Cloud-Project-and-Organization).
2. To build the server, go to your Matchplay project, and select BuildTools > Linux Server.
It should automatically build your project as a server build, and output it to:
`<project root>/Builds/Matchplay-<platformBuildType>_<dateTime>`
3. Next, upload the server to Multiplay and configure server hosting. Go to your Unity Dashboard and then go to **Multiplay Setup Guide** > **Create a build**.
4. Fill out the Details fields and move to **Upload Files**. Drag your **Linux Headless Build** into the dropbox and select **Upload Files**.
![image](https://user-images.githubusercontent.com/83710234/183719787-2abcc610-bc0d-47b6-9b6c-e7e1bd5ad2d6.png)
5. Continue to set up the build configuration. Complete the fields and enter `Matchplay.x86_64` in the game server executable field.
6. Select SQP as your query type, and fill in the following as the custom launch parameters:
`-ip 0.0.0.0 -port $$port$$ -queryPort $$query_port$$ -logFile $$server_log_dir$$/matchplaylog.log`
![image](https://user-images.githubusercontent.com/83710234/183719656-c00d57c7-a80a-4d02-a6ca-e7f367cb4ca5.png)
7. To create a fleet, enter a fleet name, and select the previously created build configuration. For the scaling settings, select 1 as the minimum available, and 5 as the maximum. The Multiplay fleet is now ready.
![image](https://user-images.githubusercontent.com/83710234/183719735-ae09c2ee-508f-4c53-b94d-98b76a14ffd3.png)













### Unity Matchmaker

Now that we have our server fleet, we can set up the Matchmaker. Select Matchmaker Setup Guide. You can click through Integrate Matchmaker for this guide, as the sample already has it integrated in the project.

1. Select **Create Queue** and enter a name for the queue. The queue name must be between 1 and 36 characters and contain only alphanumeric or hyphen characters.
2. Name the queue 'casual-queue' and set the maximum players on a ticket to 10.

![Matchmaker Queue](~Documentation/Images/Matchmaker_1.PNG "Matchmaker Queue")

3. The exact string queue name defined in the UDash must match the input string in the following SDK sample:

![Queue string in code](~Documentation/Images/Matchmaker_1b.PNG "Queue String in code")

4. To create a pool, select your previously created Multiplay fleet and build configuration. Set the timeout to 15 seconds.

![Matchmaker Pool](~Documentation/Images/Matchmaker_2.PNG "Matchmaker Pool")

**Note**: If your server fleet scaling settings have 0 minimum servers, you may need to increase the timeout duration to 180 seconds to accommodate the server initial startup duration.

#### Match Rules

The match rules are the filters that we use in the sample to match the players by their preferences. Within the pools and queues, every player’s settings are evaluated against every other player.

To set up the match definition rules:
1. Set up the region you are playing from. This should be the same as your server fleet region. 
2. Set up the basic team definitions, and skip the advanced rules for now.
3. Select **Finish** to finalize the configuration of the matchmaker. 
Now you should be able to play the Project in the Unity Editor and have the **Matchmake** button find you a match, connect you to a Multiplay server, and await more players.

You can use Parrelsync or multiple builds to connect several people to the same server using the **Matchmake** button.
![Match Rules](~Documentation/Images/Matchmaker_4.PNG "Match Rules")

vastan
======

This is a fast-paced, networked multiplayer, bipedal running-and-jumping, grenade lobbing tank game made in Unity3d. 

It's directly influenced by the great game [Avara](https://en.wikipedia.org/wiki/Avara) written in 1996 by Juri Munkki, which has [recently been open sourced](https://github.com/jmunkki/Avara) under the MIT license. Vastan uses several sound and model assets from the game, and several sections of the code influenced internal design as well. 

Building/running this source requires the Unity Engine (version 5.3+ or so). The game's inital scene is "Main Menu Scene" in Assets/Scenes. There's also "walker test scene" and "level test scene" for playing around in the editor. Hopefully this should be self-explanatory to people familiar with unity.

The current version of networking code _requires_ that the project setting "Run in Background" be checked. Without this set you may have issues connecting to a server/hosting clients on a server.

[Come chat](irc://avaraline.net/avaraline) [about the project!](irc://avaraline.net/vastan)
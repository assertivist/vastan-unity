vastan
======

This is a fast-paced, networked multiplayer, bipedal running-and-jumping, grenade lobbing tank game made in Unity3d. 

It's directly influenced by the great game [Avara](https://en.wikipedia.org/wiki/Avara) written in 1996 by Juri Munkki, which has [recently been open sourced](https://github.com/jmunkki/Avara) under the MIT license. Vastan uses several sound and model assets from the game, and several sections of the code influenced internal design as well. 

Building/running this source requires the Unity Engine (vastan targets version 5.4.4f1 specifically). The game's inital scene is "Main Menu Scene" in Assets/Scenes. There's also "walker test scene" and "level test scene" for playing around in the editor. Hopefully this should be self-explanatory to people familiar with Unity.

Development build status: [![Build Status](https://travis-ci.org/assertivist/vastan-unity.svg?branch=development)](https://travis-ci.org/assertivist/vastan-unity)
Release build status: [![Release build status](https://travis-ci.org/assertivist/vastan-unity.svg?branch=master)](https://travis-ci.org/assertivist/vastan-unity)

The current version of networking code _requires_ that the project setting "Run in Background" be checked. Without this set you may have issues connecting to a server/hosting clients on a server.

Come chat about the project on IRC: `avaraline.net` port 6667, `#vastan` & `#avaraline`
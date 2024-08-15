# SCP: Secret Laboratory bot addon

Adds goal-oriented AI controlled players to game server running on Unity Engine.

## Overview

 - GOAP inspired **AI framework featuring strongly-typed states persistance**, actions traversal from goal to find conditions-fulfilling action with least total cost using pathfinding algorithm.
 - Multithreaded perception system using **Unity Jobs System**.
 - Custom **mesh-based navigation system**, pathfinding algorithm based on A*, navigation mesh editor using primitives for navmesh visualization.
 - Introduced escaping the facility AI of human players with action graph set up to reach such goal on top on general action graph (opening doors, picking up items etc.)

![Action finder visited actions graph](https://github.com/user-attachments/assets/19b5a327-be4c-48f6-ac61-2bbb27f04656)
*Action finder visited actions graph (in yellow - action path starting from found action to goal)*

## Project setup

 - **Requires the following enviroment variables** set up:
   - `SL_REFERENCES`: directory where shipped server managed assemblies located of a game.
   - `SL_APPDATA`: directory where application data is located of server.
 - References the following packages:
   - **Lib.Harmony** v2.2.2;
   - **Northwood.PluginAPI** v13.1.1.
 - SDK style project file targeting **.NET Framework 4.8**.
 - **C# 11 language support** enabled.

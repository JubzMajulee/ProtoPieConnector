# ProtoPieConnector

This repository contains the `ProtoPieConnector.cs` script for Unity, which enables communication between ProtoPie Connect and Unity using Socket.IO.

## Features
- Singleton pattern for easy access.
- Mapping of ProtoPie messages to UnityEvents.
- Support for messages with or without values.
- High-performance dictionary lookups for message handling.

## Usage
1. Add `ProtoPieConnector.cs` to your Unity project.
2. Attach the script to a persistent GameObject.
3. Configure the `Server URL` and `Mappings` in the Inspector.

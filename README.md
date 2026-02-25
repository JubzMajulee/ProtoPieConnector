# ProtoPie Unity Connector

This repository provides a robust, reflection-powered bridge between **ProtoPie Connect** and **Unity**. It enables high-performance, bidirectional communication using Socket.IO, allowing you to trigger Unity actions from ProtoPie and send Unity events/data back to ProtoPie with zero coding required in the Inspector.

---

## 🚀 Key Features

- **Bidirectional Communication**: Send and receive messages seamlessly.
- **Dynamic Inspector UI**: A custom property drawer that adapts based on your configuration.
- **No-Code Reflection**: Pick UnityEvents and variables directly from dropdowns—no more manual string typing!
- **Singleton Pattern**: Easy global access to the connector from any script.
- **Smart Data Handling**: Automatically handles various data types (int, float, bool) and converts them to strings for ProtoPie.

---

## 🛠 Setup Guide

### 1. Prerequisites
- **Socket.IO for Unity**: Ensure you have a Socket.IO library installed (e.g., `SocketIOUnity`).
- **Newtonsoft.Json**: Required for JSON parsing.

### 2. Installation
1. Copy the `Protopie_scripts` folder into your Unity `Assets` directory.
2. Ensure the `Editor` folder (containing `MessageMappingDrawer.cs`) is correctly nested within your scripts directory.

### 3. Initial Configuration
1. Create an empty GameObject in your scene named `ProtoPieManager`.
2. Attach the `ProtoPieConnector` script to it.
3. In the Inspector, set the **Server URL** (default is `http://localhost:9981`).

---

## 📦 Tutorial: Interactive 3D Cube

This example demonstrates how to set up a basic interactive cube that talks to ProtoPie.

### Step 1: Create the Cube
1. Right-click in the Hierarchy > **3D Object** > **Cube**.
2. Name it `InteractiveCube`.

### Step 2: Protocol A - Receive (ProtoPie → Unity)
*Goal: Send a message from ProtoPie to make the Cube change color.*

1. Select your `ProtoPieManager` object.
2. In the **Mappings** list, click **(+)** to add a new mapping.
3. **Mapping Label**: `CubeColorChange`.
4. **Message Id**: `ChangeColor` (Matches the message ID in ProtoPie Connect).
5. **Direction**: Set to `Receive`.
6. **Receive Mode**: Set to `WithValue`.
7. **Action To Trigger**:
   - Create a simple script (or use `ProtoPieReceiveTester.cs`) on your Cube.
   - Drag the `InteractiveCube` into the UnityEvent slot.
   - Select the function (e.g., `MeshRenderer.material.color` or a custom `SetColor(string)` function).

### Step 3: Protocol B - Send (Unity → ProtoPie)
*Goal: Trigger an event in ProtoPie whenever the Cube is rotated.*

1. Attach the `DragRotator.cs` utility script to the `InteractiveCube`.
2. Select your `ProtoPieManager` object.
3. Add another mapping.
4. **Mapping Label**: `CubeRotationUpdate`.
5. **Message Id**: `CubeRotated`.
6. **Direction**: Set to `Send`.
7. **Action To Listen To**:
   - **Target Object**: Drag the `InteractiveCube` here.
   - **Event To Listen To**: Pick an event from the dropdown (e.g., a custom `OnRotate` event).
8. **Send Mode**: `WithValue`.
9. **Payload Type**: `DynamicVariable`.
10. **Variable Source Obj**: Drag the `InteractiveCube` here.
11. **Variable Data Source**: Select a variable (e.g., `transform.eulerAngles.y`) to send the rotation value to ProtoPie.

---

## 🔧 Included Utility Scripts

### `DragRotator.cs`
Allows you to rotate any 3D object by dragging the mouse or swiping the screen. Perfect for 3D product viewers.

### `TransparentPieInVuplex.cs`
A specialized utility for users using the **Vuplex Webview**. It handles background transparency for ProtoPie overlays.

### `ProtoPieReceiveTester.cs` & `ProtoPieSendTester.cs`
Handy scripts for debugging. Use these to quickly verify your connection status and message flow without writing custom logic.

---

## 📖 Best Practices

- **Mapping Labels**: Always give your mappings descriptive labels (e.g., "Door_Open", "UI_Refresh") to keep your Inspector organized.
- **Message IDs**: Ensure your Message IDs in Unity match **exactly** with the ones defined in your ProtoPie "Send" and "Receive" triggers.
- **Persistent Objects**: The `ProtoPieConnector` uses `DontDestroyOnLoad`, meaning it will persist across scene changes automatically.

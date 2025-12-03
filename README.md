# Real-World Passthrough Interaction System (Meta Quest 3)

## Overview
This project demonstrates a **mixed reality interface** on the **Meta Quest 3** that captures the user’s real environment through the headset cameras, listens for spoken queries, and generates context-aware responses. The system streams the Quest’s passthrough feed, performs voice capture, sends both audio and an MR frame to a backend, and displays responses via an avatar and in-world billboards.

The prototype explores how MR can support education, robotics, and interactive spatial computing scenarios by blending real-world visuals with AI-driven assistance.

## Features
- **Headset Passthrough Capture** using Uralstech’s QuestCamera API  
- **Real-time Voice Recognition** for hands-free interaction  
- **Continuous Capture Session** that streams headset camera frames  
- **World-Space UI Panels** for visualization and debugging  
- **Avatar State Machine** with Idle, Listening, Processing, Success, and Error modes  
- **Billboard Displays** showing transcription and assistant output  
- **Palm-Up Gesture Triggering** to start and stop interactions  

## Tech Stack
- **Engine:** Unity  
- **SDKs:** Meta XR All-in-One SDK, OpenXR, Uralstech QuestCamera  
- **Language:** C#  
- **Target Device:** Meta Quest 3  

## Project Setup

### 1. Install Unity
Install **Unity 2022 LTS** with:
- Android Build Support  
- Android SDK, NDK, and OpenJDK  

### 2. Clone the Repository
```bash
git clone https://github.com/brnolson/QuestPassthroughInteraction.git
cd QuestPassthroughInteraction
```

### 3. Install Dependencies

Meta XR SDK from the Unity Asset Store

Uralstech QuestCamera package for headset camera access

### 4. Unity Configuration

Open Project Settings → XR Plugin Management → OpenXR
Enable OpenXR
Set:

Asset Serialization → Force Text

Version Control → Visible Meta Files

### 5. Enable Developer Mode

Open Meta Horizon app on your phone

Devices → Headset Settings → Developer Mode → On

Connect Quest 3 to PC via USB-C

Approve Allow USB Debugging inside the headset

### 6. Required Permissions

The app requests these at runtime:

- Camera access

- Microphone access

- Spacial information access

### 7. Backend Setup (Required)

This project includes a Python FastAPI backend used for speech transcription and LLM responses.

To run the backend:

1. Open the `/backend` folder.
2. Follow the instructions in `backend/README.md` to create the Python environment, install dependencies, and start the server.
3. Start ngrok to expose the backend to your Quest device.
4. Update the Unity script `LlmClient.cs` with the provided ngrok HTTPS URL.


### 8. Build and Run

Set build target to Android / Meta Quest

Switch Profile

Build and Run

### How It Works

QuestPassthroughManager opens the headset camera and starts a continuous session.

Frames are streamed to a texture available to Unity’s UI system.

When users raise their palm, VoiceCapture begins recording microphone audio.

After interaction ends, the system:

Converts microphone data to PCM

Captures the current passthrough frame as PNG

Sends both payloads to a backend

The backend response updates the avatar and billboard text inside the headset.

Avatar state transitions provide clear, intuitive feedback.

### Evaluation Plan

- Validate passthrough clarity across lighting conditions

- Measure latency from gesture to final assistant response

- Track speech transcription accuracy in dynamic environments

- Perform user testing on comprehension and comfort

### Future Work

- Add on-device object recognition for labeled environments

- Expand hand gesture vocabulary for non-verbal control

- Introduce spatial anchors for persistent world overlays

- Enable shared multi-user MR sessions

- Improve avatar expressiveness

### Team Members

- Daniel Vu - Passthrough and Voice Integration Lead

- Brenen Olson - Interaction and MR Experience Lead
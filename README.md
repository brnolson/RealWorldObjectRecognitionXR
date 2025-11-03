# Real-World Object Recognition and Interaction (Meta Quest 3)

## Overview
This project explores the use of **mixed reality (MR)** on the **Meta Quest 3** to recognize real-world objects and overlay contextual information directly within the user’s environment.  
The system combines **Vuforia object tracking**, **Unity world-space UI**, and the **Meta XR SDK** to demonstrate how XR can be used for education, maintenance, and training scenarios.

## Features
- **Real-time Object Recognition** using Vuforia Image or Model Targets  
- **World-Space Instruction Overlays** anchored to recognized objects  
- **Passthrough MR Mode** using the Meta XR SDK  
- **Billboarding and Callout System** for intuitive labeling  
- **Spatial Anchoring** to maintain overlay position across sessions  

## Tech Stack
- **Engine:** Unity
- **SDKs:** Meta XR All-in-One SDK, OpenXR, Vuforia Engine  
- **Language:** C#  
- **Target Device:** Meta Quest 3  

## Project Setup
1. Install **Unity 2022 LTS** with Android Build Support, SDK, NDK, and OpenJDK.  
2. Clone the repository:  
   ```bash
   git clone https://github.com/brnolson/RealWorldObjectRecognitionXR.git
   cd RealWorldObjectRecognitionXR
   ```
3. Install dependencies:
   - **Meta XR SDK** (from the Unity Asset Store)  
   - **Vuforia Engine** (from [developer.vuforia.com](https://developer.vuforia.com))

4. In Unity:
   - Go to **Edit → Project Settings → XR Plugin Management → OpenXR** and enable **OpenXR**.  
   - Set **Asset Serialization** to *Force Text*.  
   - Set **Version Control** to *Visible Meta Files*.  

5. Build settings:
   - Platform: **Android**  
   - Architecture: **ARM64**  
   - Target Device: **Meta Quest 3**  

## How It Works
1. The **Vuforia ARCamera** detects a real-world object using an image or 3D model target.  
2. Once detected, Unity spawns a **world-space UI overlay** aligned to the object’s pose.  
3. The overlay includes labels, step-by-step instructions, and interactive buttons.  
4. Users can interact via controller or hand input.  
5. Optional **Meta Anchors** allow the overlay to persist across sessions.

## Evaluation Plan
- Measure recognition accuracy and tracking stability under varied lighting conditions.  
- Assess usability and overlay readability through user feedback sessions.  
- Compare performance and accuracy against baseline Vuforia examples.

## Future Work
- Integrate **TensorFlow Lite** for broader object classification.  
- Add **voice-driven interaction** for accessibility and context-aware queries.  
- Extend the system to **multi-object recognition** and dynamic AR tutorials.
- Add HUD and AI Assistant avatar.

## Team Members
- **Daniel Vu** – Computer Vision & Integration Lead  
- **Brenen Olson** – Interaction & XR Design Lead  


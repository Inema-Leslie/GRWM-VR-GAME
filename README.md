# Glow Up Morning

A pastel-pink VR "Get Ready With Me" game. Your alarm goes off and you have a few minutes to brush your teeth, wash your face, pick an outfit and take a mirror selfie before your big day.

Made in Unity 6 for Meta Quest as an individual AR/VR course project.

## Status

Work in progress. Right now the project has the bedroom and bathroom built and furnished. The routine steps, VR interactions and scoring are next.

## Requirements

- Unity 6 (6000.6.2f1), Universal Render Pipeline
- Blender installed (some props are `.blend` files that Unity imports through Blender)
- XR Interaction Toolkit + OpenXR (for the VR build)
- Meta Quest 2/3, or the XR Device Simulator to test in the editor

## How to run

1. Clone the repo and open the folder in Unity Hub.
2. Open `Assets/Scenes/SampleScene`.
3. If the room is missing, use the menu **Glow Up > Build Apartment** to rebuild it.
4. **Glow Up > View** has quick camera angles for checking the layout.

## Project layout

- `Assets/Editor/ApartmentBuilder.cs` builds the bedroom and bathroom (walls, furniture, teleport points)
- `Assets/Models/` has all 3D assets (bedroom, bathroom, clothes, mannequin, alarm clock, toothbrush set)
- `Assets/Materials/Apartment/` has the pastel wall and floor materials

## Credits

- Minimalist ArchViz Bedroom by Rad-Coders (Unity Asset Store)
- Bathroom Set - Interior (Unity Asset Store)
- Clothing packs: After Dark and Campus Comeback
- Mannequin, alarm clock, toothbrush and toothpaste made in Blender for this project

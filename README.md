# GRWM (Get Ready With Me)

A pastel-pink VR "Get Ready With Me" game. Your alarm goes off and you have a few minutes to brush your teeth, wash your face, pick an outfit and take a mirror selfie before your big day.

Made in Unity 6 for Meta Quest as an individual AR/VR course project.

## Status

The whole morning routine is playable from start to finish:

1. **Wake up**: the alarm clock rings and shakes until you tap it. The 5-minute timer starts then.
2. **Brush your teeth**: put the toothpaste on the brush (it snaps into a socket on the bristles), then scrub near your mouth. Scrubbing faster fills the bar faster, and the controller buzzes while you brush.
3. **Wash your face**: turn on the tap (pink knob), lean over the sink to splash, then dry off with the towel.
4. **Skincare**: pump the moisturiser twice, then pat it on your face.
5. **Get dressed**: pick a top, a bottom and shoes from the pink clothes rail and put them on. You start in pyjamas. The girl in the mirror is you, and you only see her there. There's a full-length mirror next to the rail so you can check the outfit. Picking a new piece sends the old one back to the rail.
6. **Add an accessory**: grab a bow, sun hat or beret from the shelf above the rail and put it on your head.
7. **Selfie**: grab the phone, face a mirror and take the photo.

Mirror, the smart mirror, floats a hint over whatever you need next. At the end a polaroid shows your selfie and 1 to 5 hearts, based on steps finished, time left and how well the outfit matches the day's theme. If time runs out you skip straight to the selfie.

The wardrobe holds the rest of the clothes (dresses and trousers). They're there to look at, you can't wear them yet.

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
5. **Glow Up > Optimize for Quest** reapplies the texture, material and URP settings for the headset (see the Performance section of the GDD).
6. Press Play. With **Desktop Mode** ticked on the XR Origin you can test without a headset:
   - hold the right mouse button to look around (or use the arrow keys)
   - WASD to walk, Shift to go faster
   - left click to press things, pick them up and put them down (they snap into a socket if one is close; clothes go straight on you)
   - hold Space to bring what you're holding to your face, or to lean in with empty hands
   - E to use what you're holding (pump the bottle, take the selfie)
   - 1 to 7 jump to the start, bed, sink, vanity, wardrobe, rail and rail mirror
   - N skips the current step (editor only, for testing)

   Untick Desktop Mode to play in VR.

## Project layout

- `Assets/Editor/QuestOptimizer.cs` applies the Quest performance settings
- `Assets/Editor/ApartmentBuilder.cs` builds the bedroom and bathroom (walls, furniture, teleport points) and wires up the routine
- `Assets/Scripts/Core/` has the routine: `IRoutineStep`, the abstract `RoutineStep`, `RoutineManager` and `GameTimer`
- `Assets/Scripts/Steps/` has one class per step (alarm, teeth, face, skincare, outfit, selfie)
- `Assets/Scripts/Interaction/` has the sockets, pressable and usable items, respawning, sounds and the desktop test hands
- `Assets/Scripts/UI/` has the HUD, Mirror's hints and the score polaroid
- `Assets/Scripts/Score/` has `ScoreManager` with a swappable `IScoreStrategy`
- `Assets/Data/` has the day theme and clothing items (ScriptableObjects)
- `Assets/Models/` has all 3D assets (bedroom, bathroom, clothes, mannequin, alarm clock, toothbrush set)
- `Assets/Materials/Apartment/` has the pastel wall and floor materials

## Credits

- Minimalist ArchViz Bedroom by Rad-Coders (Unity Asset Store)
- Bathroom Set - Interior (Unity Asset Store)
- Clothing packs: After Dark and Campus Comeback by Genies (Unity Asset Store), hung in the wardrobe and on the rail
- The player's body, her fitted clothes, alarm clock, toothbrush and toothpaste made in Blender for this project; hangers, clothes rail, shelf, accessories, bottle and phone made in code

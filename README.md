# GRWM (Get Ready With Me)

A pastel-pink VR "Get Ready With Me" game. Your alarm goes off and you have five minutes to brush your teeth, wash your face, pick an outfit and take a mirror selfie before your big day.

Made in Unity 6 for Meta Quest as an individual AR/VR course project.

### Status

The whole morning routine is playable from start to finish. A landing page explains the morning and lists the steps, and **Let's get ready!** starts the day:

1. **Wake up**: the alarm clock rings and shakes until you tap it. The 5-minute timer starts then.
2. **Brush your teeth**: hold the brush in one hand and touch the toothpaste tube to the bristles, then scrub near your mouth. Scrubbing faster fills the bar faster, and the controller buzzes while you brush.
3. **Wash your face**: turn on the tap (pink knob), lean over the sink to splash, then dry off with the towel.
4. **Skincare**: pump the moisturiser twice, then pat it on your face.
5. **Get dressed**: pick a top, a bottom and shoes from the pink clothes rail. Hold each one against your body and let go to put it on. You start in pyjamas. The girl in the mirror is you, and you only see her there. There's a full-length mirror next to the rail so you can check the outfit. Picking a new piece sends the old one back to the rail.
6. **Add an accessory**: grab a bow, sun hat or beret from the shelf above the rail and put it on your head.
7. **Selfie**: grab the phone, face a mirror and take the photo.

Mirror, the smart mirror, shows a hint card for whatever you need next, and the Glow card keeps the timer and your progress in view. If time runs out you skip straight to the selfie.

At the end, **The Big Reveal** shows your selfie as a polaroid with a glow score out of 100, 1 to 5 hearts and a stylist note. The score is based on how well the outfit matches the day's theme, the steps you finished and the time left. **Another little glow-up** takes you back to the landing page.

The wardrobe holds the rest of the clothes (dresses and trousers). They're there to look at, you can't wear them yet.

### Settings

The gear on the landing page opens settings, where you can turn the music and sound effects on or off. The choice is saved between sessions. During the routine, the home button on the Glow card takes you back to the menu (tap it twice, so you don't leave by accident).

### Comfort

You move by teleporting and snap turning only, with no smooth movement. Menus stay where they are instead of following your head, and only move back in front of you if you look far away. Both changes came from motion sickness in testing.

### Requirements

- Unity 6 (6000.6.2f1), Universal Render Pipeline
- Blender installed (some props are `.blend` files that Unity imports through Blender)
- XR Interaction Toolkit 3.7 + OpenXR
- Meta Quest 2/3, or desktop mode to test in the editor

### How to run

1. Clone the repo and open the folder in Unity Hub.
2. Open `Assets/Scenes/SampleScene`.
3. Press Play. With **Desktop Mode** ticked on the XR Origin you can test without a headset:
   - hold the right mouse button to look around (or use the arrow keys)
   - WASD to walk, Shift to go faster
   - left click to press buttons and things, pick them up and put them down (they snap into a socket if one is close; clothes go straight on you)
   - hold Space to bring what you're holding to your face, or to lean in with empty hands
   - E to use what you're holding (pump the bottle, take the selfie)
   - 1 to 7 jump to the start, bed, sink, vanity, wardrobe, rail and rail mirror
   - N skips the current step (editor only, for testing)

   Untick Desktop Mode to play in VR.

To play on the headset, switch to the **Meta Quest** build profile, plug the Quest in with Developer Mode on, and press **Build And Run**.

### Performance

The build uses IL2CPP (ARM64), static batching, the SRP Batcher, GPU instancing, LOD culling, mipmaps with ASTC compression, multiview rendering and foveated rendering. The two mirrors are the most expensive part of the scene, so each one only renders when it's on screen and you're within 6 m. The full breakdown, with Profiler results, is in the GDD.

### Project layout

- `Assets/Scripts/Core/` has the routine: `IRoutineStep`, the abstract `RoutineStep`, `RoutineManager` and `GameTimer`
- `Assets/Scripts/Steps/` has one class per step (alarm, teeth, face, skincare, outfit, selfie)
- `Assets/Scripts/Interaction/` has the sockets, pressable and usable items, respawning, sounds, ambient audio and the desktop test hands
- `Assets/Scripts/UI/` has the landing page, settings, the Glow card, Mirror's hints and the Big Reveal
- `Assets/Scripts/Score/` has `ScoreManager` with a swappable `IScoreStrategy`
- `Assets/Scripts/PlanarMirror.cs` renders the two real-time mirrors
- `Assets/Data/` has the day theme and clothing items (ScriptableObjects)
- `Assets/Models/` has all 3D assets (bedroom, bathroom, clothes, mannequin, alarm clock, toothbrush set)
- `Assets/UI/` has the icons, card sprites and fonts
- `Assets/Sounds/` has the music and birdsong
- `Assets/Materials/Apartment/` has the pastel wall and floor materials

### Credits

- Minimalist ArchViz Bedroom by Rad-Coders (Unity Asset Store)
- Bathroom Set - Interior (Unity Asset Store)
- Clothing packs: After Dark and Campus Comeback by Genies (Unity Asset Store), hung in the wardrobe and on the rail
- Fredoka and Nunito fonts (Google Fonts)
- Royalty-free ambient music and birdsong recording
- UI designed in my own Figma mood board
- The player's body, her fitted clothes, alarm clock, toothbrush and toothpaste made in Blender for this project; hangers, clothes rail, shelf, accessories, bottle and phone made in code; sound effects made in code

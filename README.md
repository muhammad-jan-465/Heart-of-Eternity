# The Heart of Eternity

## Play the Windows build

Extract the entire Windows ZIP to a folder and double-click HeartOfEternity.exe. Keep the executable, UnityPlayer.dll and the accompanying data folders together. Select Start to play. Press Esc for Resume, Restart or Quit.

| Action | Control |
| --- | --- |
| Move and look | WASD and mouse |
| Run or jump | Shift or Space |
| Fire | Left mouse |
| Collect an egg, place it, use a switch or use the ladder | E |
| Switch first-person and third-person view | V |
| Toggle control hints | H |
| Pause or resume | Esc |

## Demonstrate the game

Defeat the Lizard, Drakonit and Wolf Boss. Each drops an egg. Collect each egg and install it on its matching pedestal to raise the temple. Descend the ladder, pass the zombies and traps, open the hidden reliquary and take the Heart. Taking the Heart starts the top-center countdown and a harmless shower of stone chips in the underground passages. Reach safety before the countdown expires to win. The temple sinks with its stone movement sound after either ending.

## Make a change during evaluation

Open this folder in Unity 6000.3.23f1. Open Assets/Eternity/HeartOfEternity_Temple.unity. In Edit Mode, search the Hierarchy by component (for example, t:TempleCollapseSequence), select the object and edit its Inspector value. Save the scene. Choose Eternity > Build Windows game to regenerate Builds/Windows/HeartOfEternity.exe.

| Example change | Component | Inspector field |
| --- | --- | --- |
| Give the player more escape time | TempleCollapseSequence | Seconds |
| Change walking or running speed | GauntletPlayer | Walk Speed or Run Speed |
| Change temple rise or burial duration | TempleAwakening | Rise Seconds or Sink Seconds |

The Assets, Packages and ProjectSettings folders form the editable Unity project. Use Git LFS when cloning the project so the models and textures are present. The local project includes third-party assets. Review their original redistribution terms before granting repository access or publishing source assets.

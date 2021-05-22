# Planetary Visualizer

Planetary Visualizer is a basic audio visualizer that uses the [CSCore](https://github.com/filoe/cscore)
library to listen to audio devices on Windows, and Unity as the runtime and rendering engine.
The reason CSCore is used is that there is an issues relating to NAudio where it does not work with Unity.

## Build

To build, you will need to have Unity 2020.3 or higher (if that version of Unity supports Universal Render Pipeline).
You will also need Scriptable Render Pipeline and Universal Render Pipeline.
After that, wait for Unity to compile the scripts and that is it.
To fully build to Standalone, make sure it is on Windows and either x86 or x64.

Make sure that Unity has not set the Runtime to IL2CPP.
Since this project relies on things like reflection and some IL/.NET specific abilities.

**Please Note**: This program is not compatible with Linux, Android, iOS, macOS, or any non-Windows Vista or higher OS.
The reason is that this program uses WASAPI (Windows Audio Session API).

## Configuration

Configuring the program is done before building. To change settings, select the Center GameObject in
the Hierarchy.

### Settings

Name | Description
---- | -----------
Bands | The number of objects that react to the audio
Seed | The number to use that can change how the orbiters are setup.
Start Radius | How far the inner-most object is away from the center.
Max Orbit Offset | How much to change the orbit step. Goes from negative to positive of the value.
Orbit Stepping | How much space between each object.
Mass | How much the invisible object at the center has. Effects how fast the other objects orbit.
Scale | How receptive the reactors are to the audio.
Limit | How big the reactors can get, also effects color they display.
Band Prefab | The object (preferable a prefab) to spawn. This object requires a reactor.
Main Gradient | The gradient to use to determine the color.
Use Buffer | For the reactors to use either a buffered audio input or a raw audio input.
Decrease Speed | (Effects the audio buffer) Determines how fast the audio in the buffer decreases from it's highest point.

# palettizer
Generates an indexed palette for a given sprite. Useful for creating a palette-swap character in video games.


Palettizer is a dotnet core implementation of the idea presented [by The Dragonloft over on their blog](http://thedragonloft.blogspot.com/2015/04/sprite-palette-swapping-with-shaders-in.html).

Basically, you can create a more reliable palette by concealing the color index within the last 2 bits of the red, green and blue color channels for a given pixel. This is a much better approach than using the red channel for the index directly as it keeps colors much closer to their intended value.

The downside is that the palettes are limited to 64 slots. But this limitation shouldn't be too hard to work within.

## Running Palettizer

If you have the netcore 2.x SDK installed it is as easy as

`dotnet run --project Palettizer/Palettizer.csproj normalize ./path/to/sprite.png ./new/sprite/path.png ./palette/name.png`


## Roadmap

- Include a unity lit and unlit palette swap shader
- (maybe) Add aseprite support to netcore console app
- (maybe) Add unity editor package to recolor sprites and generate palettes
	- (maybe) Release as a Unity Asset Store package
- (maybe) Release as a NuGet package
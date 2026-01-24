# Moincroft

A C# recreation of Minecraft. Aimed to be an exact visual replica without copying any code.

## Current status

Chunk generation, meshing, and light is working. Lighting is very primitive.

**Next steps:**
- Player movement and collision
- Proper lighting

## Try it yourself

Moincroft uses Minecraft's official assets, but for legal reasons, those cannot be posted. Make sure to have an Minecraft installed before playing.

You MUST have [.NET 10.0](https://dotnet.microsoft.com/en-us/download) installed. If you are unsure, open up a terminal and run `dotnet --version`. It should print "10.x.x".

Steps:
- Download the repo's zip and extract. (Click the big Code button above, then download zip)
- Find the path to `minecraft.jar`, copy
- Add the file `config.cfg` inside the main directory with the contents: `jarPath = PATH/TO/minecraft.jar`
- Open the main directory in a terminal
- Run `dotnet run`
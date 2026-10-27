using Moincroft.Definitions;

namespace Moincroft.World;

public class FlatWorldGenerator : ChunkGenerator {
	public FlatWorldGenerator(World world) : base(world) {
	}

	public override Chunk GenerateChunk(int cx, int cy, int cz) {
		Chunk chunk = new Chunk(this.world, cx, cy, cz);
		this.world.chunks.Add((cx, cy, cz), chunk);
		for (int x = 0; x < 16; x++) {
			for (int z = 0; z < 16; z++) {
				for (int y = 0; y < 16; y++) {
					int worldY = y + cy * 16;

					if (worldY < 8)
						chunk.SetBlock(new BlockPos(x, y, z), new BlockType(Blocks.STONE, 0));
				}
			}
		}
		chunk.loadState = Chunk.LoadState.Data;
		chunk.CalculateLight();

		return chunk;
	}
}
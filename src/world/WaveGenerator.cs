using Moincroft.Definitions;

namespace Moincroft.World;

public class WaveGenerator : ChunkGenerator {
	public WaveGenerator(World world) : base(world) {
	}

	public override Chunk GenerateChunk(int cx, int cy, int cz) {
		Chunk chunk = new Chunk(this.world, cx, cy, cz);
		this.world.chunks.Add((cx, cy, cz), chunk);
		double t = 1.0 / 64.0 * Math.PI;
		for (int x = 0; x < 16; x++) {
			int worldX = x + cx * 16;

			for (int z = 0; z < 16; z++) {
				int worldZ = z + cz * 16;

				int waveHeight = (int) (Math.Sin((worldX + 0.5) * t) * Math.Sin((worldZ + 0.5) * t) * 16.0 + 24.0);

				for (int y = 0; y < 16; y++) {
					int worldY = y + cy * 16;

					if (worldY == waveHeight) {
						chunk.SetBlock(new BlockPos(x, y, z), new BlockType(Blocks.GRASS_BLOCK, 0));
					}
					else if (worldY < waveHeight - 3) {
						chunk.SetBlock(new BlockPos(x, y, z), new BlockType(Blocks.STONE, 0));
					}
					else if (worldY < waveHeight) {
						chunk.SetBlock(new BlockPos(x, y, z), new BlockType(Blocks.DIRT, 0));
					}
				}
			}
		}
		chunk.CalculateLight();

		return chunk;
	}
}
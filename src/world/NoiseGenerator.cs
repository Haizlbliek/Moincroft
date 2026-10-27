using Moincroft.Definitions;

namespace Moincroft.World;

public class NoiseGenerator : ChunkGenerator {
	public FastNoiseLite.FastNoiseLite noise = new FastNoiseLite.FastNoiseLite();
	public FastNoiseLite.FastNoiseLite noise2 = new FastNoiseLite.FastNoiseLite();

	public NoiseGenerator(World world) : base(world) {
		this.noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
		this.noise.SetFrequency(0.02f);

		this.noise2.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		this.noise2.SetFrequency(0.025f);
	}

	public override Chunk GenerateChunk(int cx, int cy, int cz) {
		Chunk chunk = new Chunk(this.world, cx, cy, cz);
		this.world.chunks.Add((cx, cy, cz), chunk);
		for (int x = 0; x < 16; x++) {
			int worldX = x + cx * 16;

			for (int z = 0; z < 16; z++) {
				int worldZ = z + cz * 16;

				for (int y = 0; y < 16; y++) {
					int worldY = y + cy * 16;
					float terrainNoise = this.noise.GetNoise(worldX, worldY * 0.75f, worldZ) - (worldY * 0.03f);

					if (terrainNoise >= 0f) {
						float n1 = this.noise2.GetNoise(worldX, worldY, worldZ);
						float n2 = this.noise2.GetNoise(worldX + 1000, worldY + 1000, worldZ + 1000);
						float cave = (n1 * n1) + (n2 * n2);
						if (cave > 0.02f * this.noise.GetNoise(worldX * 0.6f - 1000, worldY * 0.6f - 1000, worldZ * 0.6f - 1000) * Mathf.Clamp(worldY - 64, 0f, 1f)) {
							float height = this.noise.GetNoise(worldX, (worldY + 3) * 0.75f, worldZ) - ((worldY + 3) * 0.03f);
							if (height >= 0f) {
								chunk.SetBlock(new BlockPos(x, y, z), new BlockType(Blocks.STONE, 0));
							} else {
								chunk.SetBlock(new BlockPos(x, y, z), new BlockType(Blocks.DIRT, 0));
							}
						}
					}
				}
			}
		}
		chunk.CalculateLight();

		return chunk;
	}
}
namespace Moincroft.World;

public class World {
	public Dictionary<(int, int, int), Chunk> chunks = [];

	public static FastNoiseLite.FastNoiseLite noise = new FastNoiseLite.FastNoiseLite();
	public static FastNoiseLite.FastNoiseLite noise2 = new FastNoiseLite.FastNoiseLite();
	public static ChunkData emptyChunk = new ChunkData(null, 0, 0, 0);

	static World() {
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
		noise.SetFrequency(0.02f);

		noise2.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		noise2.SetFrequency(0.025f);
	}

	public Chunk GetChunk(int cx, int cy, int cz) {
		if (this.chunks.TryGetValue((cx, cy, cz), out Chunk chunk)) {
			return chunk;
		}

		return null;
	}

	public Vector3i GetChunkPositionFromBlock(int bx, int by, int bz) {
		return new Vector3i(Mathf.FloorDivide(bx, 16), Mathf.FloorDivide(by, 16), Mathf.FloorDivide(bz, 16));
	}

	public Chunk GetChunkFromBlock(int bx, int by, int bz) {
		return this.GetChunk(Mathf.FloorDivide(bx, 16), Mathf.FloorDivide(by, 16), Mathf.FloorDivide(bz, 16));
	}

	public BlockId GetBlock(int x, int y, int z) {
		if (this.chunks.TryGetValue((x >> 4, y >> 4, z >> 4), out Chunk chunk)) {
			return chunk.GetBlock(x & 15, y & 15, z & 15);
		}

		return 0;
	}

	public void SetBlock(int x, int y, int z, BlockId block) {
		this.GetChunkFromBlock(x, y, z)?.SetBlock(x & 15, y & 15, z & 15, block); // TODO: 
	}

	public Chunk LoadChunk(int cx, int cy, int cz) {
		Chunk chunk = new Chunk(this, cx, cy, cz);
		this.chunks.Add((cx, cy, cz), chunk);
		for (int x = 0; x < 16; x++) {
			int worldX = x + cx * 16;

			for (int z = 0; z < 16; z++) {
				int worldZ = z + cz * 16;
				int xzOffset = (x << 8) | z;

				for (int y = 0; y < 16; y++) {
					int worldY = y + cy * 16;
					float terrainNoise = noise.GetNoise(worldX, worldY * 0.75f, worldZ) - (worldY * 0.03f);

					if (terrainNoise >= 0f) {
						float n1 = noise2.GetNoise(worldX, worldY, worldZ);
						float n2 = noise2.GetNoise(worldX + 1000, worldY + 1000, worldZ + 1000);
						float cave = (n1 * n1) + (n2 * n2);
						if (cave > 0.02f * noise.GetNoise(worldX * 0.6f - 1000, worldY * 0.6f - 1000, worldZ * 0.6f - 1000) * Mathf.Clamp(worldY - 64, 1f, 0f)) {
							float height = noise.GetNoise(worldX, (worldY + 3) * 0.75f, worldZ) - ((worldY + 3) * 0.03f);
							if (height >= 0f) {
								chunk.blocks[xzOffset | (y << 4)] = Blocks.Blocks.STONE.index;
							} else {
								chunk.blocks[xzOffset | (y << 4)] = Blocks.Blocks.DIRT.index;
							}
						}
					}
				}
			}
		}
		chunk.CalculateLight();

		return chunk;
	}

	public Chunk GetOrLoadChunk(int cx, int cy, int cz) {
		if (this.chunks.TryGetValue((cx, cy, cz), out Chunk chunk)) {
			return chunk;
		}

		return this.LoadChunk(cx, cy, cz);
	}

	public void VisibleChunk(int cx, int cy, int cz) {
		Chunk chunk = this.GetOrLoadChunk(cx, cy, cz);
		if (chunk.meshed) return;

		this.GetOrLoadChunk(cx - 1, cy - 1, cz - 1);
		this.GetOrLoadChunk(cx,     cy - 1, cz - 1);
		this.GetOrLoadChunk(cx + 1, cy - 1, cz - 1);
		this.GetOrLoadChunk(cx - 1, cy - 1, cz    );
		this.GetOrLoadChunk(cx,     cy - 1, cz    );
		this.GetOrLoadChunk(cx + 1, cy - 1, cz    );
		this.GetOrLoadChunk(cx - 1, cy - 1, cz + 1);
		this.GetOrLoadChunk(cx,     cy - 1, cz + 1);
		this.GetOrLoadChunk(cx + 1, cy - 1, cz + 1);

		this.GetOrLoadChunk(cx - 1, cy, cz - 1);
		this.GetOrLoadChunk(cx,     cy, cz - 1);
		this.GetOrLoadChunk(cx + 1, cy, cz - 1);
		this.GetOrLoadChunk(cx - 1, cy, cz    );
		this.GetOrLoadChunk(cx + 1, cy, cz    );
		this.GetOrLoadChunk(cx - 1, cy, cz + 1);
		this.GetOrLoadChunk(cx,     cy, cz + 1);
		this.GetOrLoadChunk(cx + 1, cy, cz + 1);

		this.GetOrLoadChunk(cx - 1, cy + 1, cz - 1);
		this.GetOrLoadChunk(cx,     cy + 1, cz - 1);
		this.GetOrLoadChunk(cx + 1, cy + 1, cz - 1);
		this.GetOrLoadChunk(cx - 1, cy + 1, cz    );
		this.GetOrLoadChunk(cx,     cy + 1, cz    );
		this.GetOrLoadChunk(cx + 1, cy + 1, cz    );
		this.GetOrLoadChunk(cx - 1, cy + 1, cz + 1);
		this.GetOrLoadChunk(cx,     cy + 1, cz + 1);
		this.GetOrLoadChunk(cx + 1, cy + 1, cz + 1);

		chunk.GenerateMesh();
	}
}
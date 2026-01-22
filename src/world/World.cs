namespace Moincroft.World;

public class World {
	public Dictionary<(int, int, int), Chunk> chunks = [];

	public Chunk GetChunk(int cx, int cy, int cz) {
		if (this.chunks.TryGetValue((cx, cy, cz), out Chunk chunk)) {
			return chunk;
		}

		return null;
	}

	public Chunk GetChunkFromBlock(int bx, int by, int bz) {
		return this.GetChunk(Mathf.FloorDivide(bx, 16), Mathf.FloorDivide(by, 16), Mathf.FloorDivide(bz, 16));
	}

	public BlockId GetBlock(int x, int y, int z) {
		return this.GetChunkFromBlock(x, y, z)?.GetBlock(Mathf.Mod(x, 16), Mathf.Mod(y, 16), Mathf.Mod(z, 16)) ?? 0; // TODO: 
	}

	public void SetBlock(int x, int y, int z, BlockId block) {
		this.GetChunkFromBlock(x, y, z)?.SetBlock(Mathf.Mod(x, 16), Mathf.Mod(y, 16), Mathf.Mod(z, 16), block); // TODO: 
	}

	public void LoadChunk(int cx, int cy, int cz) {
		Chunk chunk = new Chunk(this, cx, cy, cz);
		this.chunks.Add((cx, cy, cz), chunk);
		float s = 0.06f;
		for (int x = 0; x < 16; x++) {
			int bx = x + cx * 16;
			for (int z = 0; z < 16; z++) {
				int bz = z + cz * 16;
				for (int y = 0; y <= 15; y++) {
					int by = y + cy * 16;
					float v = Noise.Perlin3(bx * s, by * s * 0.5f, bz * s) - (by / 64f);
					
					if (v >= 0f) chunk.SetBlock(x, y, z, 1);
				}
			}
		}
	}
}
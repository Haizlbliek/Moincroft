namespace Moincroft.World;

public class World {
	public Dictionary<(int, int, int), Chunk> chunks = [];

	public Chunk GetChunk(int cx, int cy, int cz) {
		return this.chunks[(cx, cy, cz)];
	}

	public Chunk GetChunkFromBlock(int bx, int by, int bz) {
		return this.GetChunk(Mathf.FloorDivide(bx, 16), Mathf.FloorDivide(by, 16), Mathf.FloorDivide(bz, 16));
	}

	public BlockId GetBlock(int x, int y, int z) {
		return this.GetChunkFromBlock(x, y, z).GetBlock(Mathf.Mod(x, 16), Mathf.Mod(y, 16), Mathf.Mod(z, 16));
	}

	public void SetBlock(int x, int y, int z, BlockId block) {
		this.GetChunkFromBlock(x, y, z).SetBlock(Mathf.Mod(x, 16), Mathf.Mod(y, 16), Mathf.Mod(z, 16), block);
	}

	public void LoadChunk(int cx, int cy, int cz) {
		Chunk chunk = new Chunk(this, cx, cy, cz);
		this.chunks.Add((cx, cy, cz), chunk);
		for (int x = 0; x < 16; x++) {
			for (int z = 0; z < 16; z++) {
				for (int y = 0; y < 8; y++) {
					chunk.SetBlock(x, y, z, 1);
				}
			}
		}
		chunk.GenerateMesh();
	}
}
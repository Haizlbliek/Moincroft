using Moincroft.Definitions;

namespace Moincroft.World;

public class World {
	public ChunkGenerator generator;

	public Dictionary<(int, int, int), Chunk> chunks = [];
	public readonly List<Chunk> chunksToRemesh = [];

	public static ChunkData emptyChunk = new ChunkData(null!, 0, 0, 0);

	public World() {
		this.generator = new WaveGenerator(this);
	}

	public Chunk? GetChunk(int cx, int cy, int cz) {
		if (this.chunks.TryGetValue((cx, cy, cz), out Chunk? chunk)) {
			return chunk;
		}

		return null;
	}

	public Vector3i GetChunkPositionFromBlock(int bx, int by, int bz) {
		return new Vector3i(bx >> 4, by >> 4, bz >> 4);
	}

	public Chunk? GetChunkFromBlock(int bx, int by, int bz) {
		return this.GetChunk(bx >> 4, by >> 4, bz >> 4);
	}

	public BlockType GetBlock(int x, int y, int z) {
		if (this.chunks.TryGetValue((x >> 4, y >> 4, z >> 4), out Chunk? chunk)) {
			return chunk.GetBlock(new BlockPos(x & 15, y & 15, z & 15));
		}

		return default;
	}

	public void SetBlock(int x, int y, int z, BlockType block) {
		this.GetChunkFromBlock(x, y, z)?.SetBlock(new BlockPos(x & 15, y & 15, z & 15), block); // TODO: 
	}

	public Chunk GetOrLoadChunk(int cx, int cy, int cz) {
		if (this.chunks.TryGetValue((cx, cy, cz), out Chunk? chunk)) {
			return chunk;
		}

		return this.generator.GenerateChunk(cx, cy, cz);
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

		chunk.QueueRefresh();
	}

	public void RemeshChunks() {
		ReadOnlySpan<Chunk> chunks = CollectionsMarshal.AsSpan(this.chunksToRemesh);

		for (int i = 0; i < chunks.Length; i++) {
			Chunk chunk = chunks[i];
			chunk.meshNeedsRefresh = false;
			chunk.GenerateMesh();
		}

		this.chunksToRemesh.Clear();
	}

	public void Load() {
		if (!Directory.Exists("chunks"))
			return;

		foreach (string path in Directory.GetFiles("chunks")) {
			if (Path.GetFileName(path) == ".gitignore")
				continue;

			ChunkSerializer.LoadChunk(this, path);
		}

		Queue<Chunk> chunksToLight = [];
		foreach (Chunk chunk in this.chunks.Values) {
			chunksToLight.Enqueue(chunk);
		}

		while (chunksToLight.Count > 0) {
			Chunk chunk = chunksToLight.Dequeue();
			if (this.GetChunk(chunk.cx, chunk.cy + 1, chunk.cz) is Chunk above && above.loadState < Chunk.LoadState.Lit) {
				chunksToLight.Enqueue(chunk);
				continue;
			}

			chunk.CalculateLight();
		}
	}

	public void Save() {
		Directory.CreateDirectory("chunks");

		foreach (Chunk chunk in this.chunks.Values) {
			ChunkSerializer.SaveChunk(chunk, $"chunks/{chunk.cx}-{chunk.cy}-{chunk.cz}");
		}
	}
}
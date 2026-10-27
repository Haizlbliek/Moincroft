namespace Moincroft.World;

public abstract class ChunkGenerator {
	protected readonly World world;

	public ChunkGenerator(World world) {
		this.world = world;
	}

	public abstract Chunk GenerateChunk(int cx, int cy, int cz);
}
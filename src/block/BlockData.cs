namespace Moincroft.Blocks;

public abstract class BlockData {
	public TextureId up;
	public TextureId down;
	public TextureId north;
	public TextureId south;
	public TextureId east;
	public TextureId west;

	public bool opaque;

	public string id;

	public BlockData(string id) {
		this.id = id;
	}
}

public class BlockData<T> : BlockData where T : Block, new() {
	public static T blockInstance = new T();

	public BlockData(string id) : base(id) {
	}

	public T Block => blockInstance;
}
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

public class BlockData<T> : BlockData where T : class {
	public BlockData(string id) : base(id) {
	}
}
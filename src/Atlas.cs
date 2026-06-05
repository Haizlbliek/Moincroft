namespace Moincroft;

public static class Atlas {
	public static Texture atlas = null!;
	public static float AtlasWidth { get; private set; }
	public static float AtlasHeight { get; private set; }
	private static readonly Dictionary<string, FaceUv> order = [];

	public static FaceUv GetFace(string name) {
		if (!order.TryGetValue(name, out FaceUv uv))
			throw new Exception($"Unknown atlas face: {name}");

		return uv;
	}

	public static void Initialize() {
		atlas = Texture.Load("assets/generated/blocks.png");
		AtlasWidth = atlas.width;
		AtlasHeight = atlas.height;

		string[] lines = File.ReadAllLines("assets/generated/blocks.txt");
		foreach (string line in lines) {
			string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			bool added = order.TryAdd(parts[0], new FaceUv(
				int.Parse(parts[1]) / AtlasWidth,
				int.Parse(parts[2]) / AtlasHeight,
				int.Parse(parts[3]) / AtlasWidth,
				int.Parse(parts[4]) / AtlasHeight
			));
			if (!added)
				throw new Exception($"Duplicate atlas entry: {parts[0]}");
		}
	}
}
namespace Moincroft;

public static class Atlas {
	public static Texture atlas = null!;
	public static float atlasWidth;
	public static float atlasHeight;
	private static readonly Dictionary<string, FaceUv> order = [];

	public static FaceUv GetFace(string name) {
		if (!order.TryGetValue(name, out FaceUv uv))
			throw new Exception($"Unknown atlas face: {name}");

		return uv;
	}

	public static void Initialize() {
		atlas = Texture.Load("assets/generated/blocks.png");
		atlasWidth = atlas.width;
		atlasHeight = atlas.height;

		string[] lines = File.ReadAllLines("assets/generated/blocks.txt");
		foreach (string line in lines) {
			string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			bool added = order.TryAdd(parts[0], new FaceUv(
				int.Parse(parts[1]) / atlasWidth,
				int.Parse(parts[2]) / atlasHeight,
				int.Parse(parts[3]) / atlasWidth,
				int.Parse(parts[4]) / atlasHeight
			));
			if (!added)
				throw new Exception($"Duplicate atlas entry: {parts[0]}");
		}
	}
}
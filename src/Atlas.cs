using System.IO;

namespace Moincroft;

public static class Atlas {
	public static Texture atlas;
	public static float atlasWidth;
	public static float atlasHeight;
	public static Dictionary<string, (uint, uint)> order = [];

	public static float SpriteWidth => 1f / atlasWidth;
	public static float SpriteHeight => 1f / atlasHeight;

	public static FaceUv GetFace(string name) {
		(uint, uint) pos = order[name];
		return new FaceUv(pos.Item1 * SpriteWidth, pos.Item2 * SpriteHeight, SpriteWidth, SpriteHeight);
	}

	public static void Initialize() {
		atlas = Texture.Load("assets/texture_atlas.png");
		atlasWidth = atlas.width / 16f;
		atlasHeight = atlas.height / 16f;

		string[] lines = File.ReadAllLines("assets/atlas_order.txt");
		int width = int.Parse(lines[0]);
		uint x = 0;
		uint y = 0;
		for (int i = 1; i < lines.Length; i++) {
			order.Add(lines[i], (x, y));
			x++;
			if (x >= width) {
				x = 0u;
				y++;
			}
		}
	}
}
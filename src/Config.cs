using System.IO;

namespace Moincroft;

public static class Config {
	public static int RenderDistance = 5;
	public static int TicksPerSecond = 20;
	public static string JarPath = "";

	public static void Initialize() {
		if (!File.Exists("config.cfg")) return;

		string[] lines = File.ReadAllLines("config.cfg");
		foreach (string line in lines) {
			if (line.Trim() == "" || line.Trim().StartsWith('#')) continue;

			string key = line[..line.IndexOf('=')].Trim();
			string value = line[(line.IndexOf('=') + 1)..].Trim();

			switch (key.ToLowerInvariant()) {
				case "jarpath": {
					JarPath = value;
					break;
				}

				case "renderdistance": {
					RenderDistance = int.Parse(value);
					break;
				}

				case "tickspersecond": {
					TicksPerSecond = int.Parse(value);
					break;
				}
			}
		}
	}
}
namespace Moincroft;

public static class Config {
	public static int RenderDistance { get; private set; } = 2;
	public static int TicksPerSecond { get; private set; } = 20;
	public static string JarPath { get; private set; } = "";

	public static float MaxInteractionDistance { get; private set; } = 5f;

	public static void Initialize() {
		if (!File.Exists("config.cfg"))
			return;

		string[] lines = File.ReadAllLines("config.cfg");
		foreach (string fullLine in lines) {
			string line = fullLine.Trim();
			if (line.Length == 0 || line.StartsWith('#'))
				continue;

			int separator = line.IndexOf('=');
			if (separator < 0)
				continue;

			string key = line[..separator].Trim().Replace(" ", "");
			string value = line[(separator + 1)..].Trim();

			switch (key.ToLowerInvariant()) {
				case "jarpath": {
					JarPath = Path.GetFullPath(value);

					break;
				}

				case "renderdistance": {
					if (int.TryParse(value, out int v))
						RenderDistance = Math.Max(v, 1);

					break;
				}

				case "tickspersecond": {
					if (int.TryParse(value, out int v))
						TicksPerSecond = Math.Max(v, 1);

					break;
				}

				case "maxinteractiondistance": {
					if (float.TryParse(value, out float v))
						MaxInteractionDistance = v;

					break;
				}
			}
		}
	}
}
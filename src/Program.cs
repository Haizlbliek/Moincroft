using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.IO;

namespace Moincroft;

public static class Program {
	public static IWindow window = null!;
	public static GL gl = null!;
	public static uint _anyVao;

	public static void Main(string[] args) {
		if (args.Contains("-a") || args.Contains("--download-assets")) {
			Assets.Assets.DownloadAssets().GetAwaiter().GetResult();
			return;
		}

		if (args.Contains("-t") || args.Contains("--build-texture-atlas")) {
			Assets.Assets.BuildTextureAtlas();
			return;
		}

		if (!Directory.Exists("assets/generated/")) {
			Console.WriteLine("Generating assets");
			if (!Directory.Exists("tmp")) {
				Assets.Assets.DownloadAssets().GetAwaiter().GetResult();
			}
			Assets.Assets.BuildTextureAtlas();
		}

		WindowOptions options = WindowOptions.Default with {
			Size = new Vector2D<int>(800, 600),
			Title = "Minecraft",
			VideoMode = Monitor.GetMainMonitor(null).VideoMode,
			WindowState = WindowState.Fullscreen,
		};
		window = Window.Create(options);

		window.Load += OnLoad;
		window.Update += OnUpdate;
		window.Render += OnRender;
		window.Resize += Resize;

		window.Run();

		window.Dispose();
	}

	private static void OnLoad() {
		gl = GL.GetApi(window);

		gl.ClearColor(0f, 0f, 0f, 1f);

		window.SetWindowIcon([ Texture.LoadRawImage("assets/icon.png") ]);

		_anyVao = gl.CreateVertexArray();

		Moincroft.Main.Initialize();
	}

	public static double time = 0;
	private static void OnUpdate(double delta) {
		time += delta;
		if (time >= 1.0 / Config.TicksPerSecond) {
			window.Title = "Minecraft " + Math.Round(1.0 / delta);
			Moincroft.Main.Update();
			time -= 1.0 / Config.TicksPerSecond;
		}
	}

	private static void Resize(Vector2D<int> d) {
		gl.Viewport(0, 0, (uint) d.X, (uint) d.Y);
		Moincroft.Main.Resize(d.X, d.Y);
	}

	private static void OnRender(double delta) {
		Moincroft.Main.Render();
	}
}
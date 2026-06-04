using Silk.NET.Windowing;

namespace Moincroft;

public static class Program {
	public static IWindow window = null!;
	public static GL gl = null!;
	public static uint _anyVao;

	public static readonly Vector2D<int> defaultSize = new Vector2D<int>(1280, 720);

	public static void Main(string[] args) {
		Config.Initialize();

		if (args.Contains("-a") || args.Contains("--extract-assets")) {
			Assets.Assets.ExtractAssetsFromJar(Config.JarPath);
			return;
		}

		if (args.Contains("-t") || args.Contains("--build-texture-atlas")) {
			Assets.Assets.BuildTextureAtlas();
			return;
		}

		if (!Directory.Exists("assets/generated/")) {
			Console.WriteLine("Generating assets");
			if (!Directory.Exists("tmp")) {
				Assets.Assets.ExtractAssetsFromJar(Config.JarPath);
			}
			Assets.Assets.BuildTextureAtlas();
		}

		WindowOptions options = WindowOptions.Default with {
			Size = defaultSize,
			Title = "Moincroft",
			VideoMode = Monitor.GetMainMonitor(null).VideoMode,
			// WindowState = WindowState.Fullscreen,
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
		Custom.Custom.Initialize(gl);

		window.SetWindowIcon([ Texture.LoadRawImage("assets/icon.png") ]);

		_anyVao = gl.CreateVertexArray();

		Moincroft.Main.Initialize();
	}

	public static double time = 0;
	private static void OnUpdate(double delta) {
		time += delta;
		if (time >= 1.0 / Config.TicksPerSecond) {
			window.Title = "Moincroft " + Math.Round(1.0 / delta);
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
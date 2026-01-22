using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;

namespace Moincroft;

public static class Program {
	public static IWindow window;
	public static GL gl;

	public static void Main(string[] args) {
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
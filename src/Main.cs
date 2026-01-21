using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Moincroft;

public static class Main {
	private static GL Gl => Program.gl;
	public static Random random = new Random();

	public static Matrix4X4<float> ViewMatrix;
	public static Matrix4X4<float> ProjectionMatrix;

	public static IInputContext input;

	public static World.World world = new World.World();

	public static void Initialize() {
		Console.WriteLine("Initializing...");
		Blocks.Blocks.Initialize();
		Preload.Initialize();

		input = Program.window.CreateInput();
		for (int i = 0; i < input.Keyboards.Count; i++) {
			var keyboard = input.Keyboards[i];
			keyboard.KeyDown += KeyDown;
		}
		for (int i = 0; i < input.Mice.Count; i++) {
			var mouse = input.Mice[i];
			mouse.MouseUp += MouseUp;
			mouse.MouseMove += MouseMove;
			mouse.MouseDown += MouseDown;
		}

		// Audio.Audio.Initialize();

		ViewMatrix = Matrix4X4.CreateTranslation(0f, 0f, -1f);
		float fov = 90f * (Mathf.PI / 180f);
		ProjectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(fov, 800f / 600f, 0.1f, 1000f);

		for (int x = -2; x <= 2; x++) {
			for (int z = -2; z <= 2; z++) {
				world.LoadChunk(x, 0, z);
			}
		}
	}

	public static void Resize(int width, int height) {
		float fov = 90f * (Mathf.PI / 180f);
		ProjectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(fov, width / (float) height, 0.1f, 1000f);
	}

	private static void KeyDown(IKeyboard keyboard, Key key, int arg3) {
		if (key == Key.Escape) {
			Program.window.Close();
		}
	}

	private static void MouseUp(IMouse mouse, MouseButton button) {
	}

	private static void MouseMove(IMouse mouse, System.Numerics.Vector2 position) {
	}

	private static void MouseDown(IMouse mouse, MouseButton button) {
	}

	public static void Update() {
		IKeyboard keyboard = input.Keyboards[0];
		float speed = 0.2f;
		if (keyboard.IsKeyPressed(Key.W)) {
			ViewMatrix *= Matrix4X4.CreateTranslation(0f, 0f, speed);
		}
		if (keyboard.IsKeyPressed(Key.S)) {
			ViewMatrix *= Matrix4X4.CreateTranslation(0f, 0f, -speed);
		}
		if (keyboard.IsKeyPressed(Key.A)) {
			ViewMatrix *= Matrix4X4.CreateTranslation(speed, 0f, 0f);
		}
		if (keyboard.IsKeyPressed(Key.D)) {
			ViewMatrix *= Matrix4X4.CreateTranslation(-speed, 0f, 0f);
		}
		if (keyboard.IsKeyPressed(Key.ShiftLeft)) {
			ViewMatrix *= Matrix4X4.CreateTranslation(0f, speed, 0f);
		}
		if (keyboard.IsKeyPressed(Key.Space)) {
			ViewMatrix *= Matrix4X4.CreateTranslation(0f, -speed, 0f);
		}
	}

	public static void Render() {
		Gl.StencilMask(0xFF);
		Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

		Gl.UseProgram(Preload.Basic);
		Program.gl.UniformMatrix4(Program.gl.GetUniformLocation(Preload.Basic, "view"), false, [ ..Main.ViewMatrix ]);
		Program.gl.UniformMatrix4(Program.gl.GetUniformLocation(Preload.Basic, "projection"), false, [ ..Main.ProjectionMatrix ]);
		Gl.UseProgram(Preload.Basic);

		Gl.Enable(EnableCap.DepthTest);
		foreach (Chunk chunk in world.chunks.Values) {
			chunk.Render();
		}
	}
}
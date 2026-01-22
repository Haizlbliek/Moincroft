using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Moincroft;

public static class Main {
	private static GL Gl => Program.gl;
	public static Random random = new Random();

	public static Matrix4X4<float> ViewMatrix;
	public static Matrix4X4<float> ProjectionMatrix;

	public static Vector3 lastCameraPosition;
	public static Vector3 cameraPosition;
	public static Vector3 cameraRotation;

	public static IInputContext input;
	public static Vector2? lastMousePosition = null;

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
			mouse.Cursor.CursorMode = CursorMode.Disabled;
			mouse.MouseUp += MouseUp;
			mouse.MouseMove += MouseMove;
			mouse.MouseDown += MouseDown;
		}

		// Audio.Audio.Initialize();

		ViewMatrix = Matrix4X4.CreateTranslation(0f, 0f, 0f);
		float fov = 70f * (Mathf.PI / 180f);
		ProjectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(fov, 800f / 600f, 0.1f, 1000f);

		for (int x = -3; x <= 3; x++) {
			for (int z = -3; z <= 3; z++) {
				world.LoadChunk(x, 0, z);
				world.LoadChunk(x, 1, z);
				world.LoadChunk(x, 2, z);
			}
		}
		for (int x = -2; x <= 2; x++) {
			for (int z = -2; z <= 2; z++) {
				world.GetChunk(x, 0, z).GenerateMesh();
				world.GetChunk(x, 1, z).GenerateMesh();
				world.GetChunk(x, 2, z).GenerateMesh();
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
		lastMousePosition ??= position;

		Vector2 delta = (Vector2) position - lastMousePosition.Value;
		lastMousePosition = position;

		cameraRotation.x += delta.y * 0.005f;
		cameraRotation.y += delta.x * 0.005f;
		cameraRotation.x = Mathf.Clamp(cameraRotation.x, -Mathf.PI / 2f, Mathf.PI / 2f);
	}

	private static void MouseDown(IMouse mouse, MouseButton button) {
		
	}

	public static void Update() {
		lastCameraPosition = cameraPosition;

		IKeyboard keyboard = input.Keyboards[0];
		Vector2 movement = Vector2.Zero;
		float speed = keyboard.IsKeyPressed(Key.ControlLeft) ? 0.4f : 0.15f;
		if (keyboard.IsKeyPressed(Key.W)) {
			movement.y -= speed;
		}
		if (keyboard.IsKeyPressed(Key.S)) {
			movement.y += speed;
		}
		if (keyboard.IsKeyPressed(Key.A)) {
			movement.x -= speed;
		}
		if (keyboard.IsKeyPressed(Key.D)) {
			movement.x += speed;
		}
		if (keyboard.IsKeyPressed(Key.ShiftLeft)) {
			cameraPosition.y -= speed;
		}
		if (keyboard.IsKeyPressed(Key.Space)) {
			cameraPosition.y += speed;
		}
		cameraPosition.x += Mathf.Cos(-cameraRotation.y) * movement.x + Mathf.Sin(-cameraRotation.y) * movement.y;
		cameraPosition.z += -Mathf.Sin(-cameraRotation.y) * movement.x + Mathf.Cos(-cameraRotation.y) * movement.y;
	}

	public static void Render() {
		float timeStacker = (float) (Program.time * Config.TicksPerSecond);
		ViewMatrix = Matrix4X4.CreateTranslation(-Mathf.Lerp(lastCameraPosition.x, cameraPosition.x, timeStacker), -Mathf.Lerp(lastCameraPosition.y, cameraPosition.y, timeStacker), -Mathf.Lerp(lastCameraPosition.z, cameraPosition.z, timeStacker));
		ViewMatrix *= Matrix4X4.CreateRotationY(cameraRotation.y);
		ViewMatrix *= Matrix4X4.CreateRotationX(cameraRotation.x);

		Gl.StencilMask(0xFF);
		Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

		Gl.UseProgram(Preload.Basic);
		Program.gl.UniformMatrix4(Program.gl.GetUniformLocation(Preload.Basic, "view"), false, [ ..Main.ViewMatrix ]);
		Program.gl.UniformMatrix4(Program.gl.GetUniformLocation(Preload.Basic, "projection"), false, [ ..Main.ProjectionMatrix ]);
		Gl.UseProgram(Preload.Basic);

		Gl.Enable(EnableCap.DepthTest);
		Gl.Enable(EnableCap.Blend);
		Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		foreach (Chunk chunk in world.chunks.Values) {
			chunk.Render();
		}
	}
}
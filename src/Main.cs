using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Moincroft;

public static class Main {
	public static Random random = new Random();

	public static Matrix4X4<float> ViewMatrix;
	public static Matrix4X4<float> ProjectionMatrix;

	public static Vector3 lastCameraPosition;
	public static Vector3 cameraPosition;
	public static Vector3 cameraRotation;

	public static IInputContext input;
	public static Vector2? lastMousePosition = null;

	public static World.World world = new World.World();
	public static WorldRayResult? rayResult = null;

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
		rayResult = new WorldRay(world, cameraPosition, cameraRotation.AngleToDirection, 1000f).Cast();
		if (rayResult == null) return;

		int x = rayResult.Value.blockPosition.x;
		int y = rayResult.Value.blockPosition.y;
		int z = rayResult.Value.blockPosition.z;
		if (button == MouseButton.Left) {}
		else if (button == MouseButton.Right) {
			x += rayResult.Value.normal.x;
			y += rayResult.Value.normal.y;
			z += rayResult.Value.normal.z;
		} else {
			return;
		}

		Chunk chunk = world.GetChunkFromBlock(x, y, z);
		if (chunk == null) return;
		x = Mathf.Mod(x, 16);
		y = Mathf.Mod(y, 16);
		z = Mathf.Mod(z, 16);
		chunk.SetBlock(x, y, z, button == MouseButton.Left ? 0u : 1u);
		chunk.GenerateMesh();
		if (x == 0 ) world.GetChunk(chunk.cx - 1, chunk.cy, chunk.cz)?.GenerateMesh();
		if (x == 15) world.GetChunk(chunk.cx + 1, chunk.cy, chunk.cz)?.GenerateMesh();
		if (y == 0 ) world.GetChunk(chunk.cx, chunk.cy - 1, chunk.cz)?.GenerateMesh();
		if (y == 15) world.GetChunk(chunk.cx, chunk.cy + 1, chunk.cz)?.GenerateMesh();
		if (z == 0 ) world.GetChunk(chunk.cx, chunk.cy, chunk.cz - 1)?.GenerateMesh();
		if (z == 15) world.GetChunk(chunk.cx, chunk.cy, chunk.cz + 1)?.GenerateMesh();
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

		rayResult = new WorldRay(world, cameraPosition, cameraRotation.AngleToDirection, 1000f).Cast();
	}

	public static void Render() {
		float timeStacker = (float) (Program.time * Config.TicksPerSecond);
		ViewMatrix = Matrix4X4.CreateTranslation(-Mathf.Lerp(lastCameraPosition.x, cameraPosition.x, timeStacker), -Mathf.Lerp(lastCameraPosition.y, cameraPosition.y, timeStacker), -Mathf.Lerp(lastCameraPosition.z, cameraPosition.z, timeStacker));
		ViewMatrix *= Matrix4X4.CreateRotationY(cameraRotation.y);
		ViewMatrix *= Matrix4X4.CreateRotationX(cameraRotation.x);

		Program.gl.StencilMask(0xFF);
		Program.gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

		Program.gl.UseProgram(Preload.Basic);
		Program.gl.UniformMatrix4(Program.gl.GetUniformLocation(Preload.Basic, "view"), false, [ ..Main.ViewMatrix ]);
		Program.gl.UniformMatrix4(Program.gl.GetUniformLocation(Preload.Basic, "projection"), false, [ ..Main.ProjectionMatrix ]);
		Program.gl.UseProgram(Preload.Basic);

		Program.gl.Enable(EnableCap.DepthTest);
		Program.gl.Enable(EnableCap.Blend);
		Program.gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		foreach (Chunk chunk in world.chunks.Values) {
			chunk.Render();
		}

		if (rayResult != null) {
			Program.gl.UseProgram(Preload.Selection);
			Vector3 selectionPos = rayResult.Value.blockPosition;
			Program.gl.Uniform3(Program.gl.GetUniformLocation(Preload.Selection, "offset"), selectionPos.x, selectionPos.y, selectionPos.z);
			Program.gl.UniformMatrix4(Program.gl.GetUniformLocation(Preload.Selection, "view"), false, [ ..Main.ViewMatrix ]);
			Program.gl.UniformMatrix4(Program.gl.GetUniformLocation(Preload.Selection, "projection"), false, [ ..Main.ProjectionMatrix ]);
			Program.gl.BindVertexArray(Program._anyVao);
			Program.gl.DrawArrays(PrimitiveType.Lines, 0, 24);
			Program.gl.BindVertexArray(0);
			Program.gl.UseProgram(0);
		}
	}
}
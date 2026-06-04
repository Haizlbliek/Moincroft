using Silk.NET.Input;

namespace Moincroft;

public static class Main {
	public static Random random = new Random();

	public static Matrix4X4<float> ViewMatrix;
	public static Matrix4X4<float> ProjectionMatrix;

	public static Vector3 lastCameraPosition;
	public static Vector3 cameraPosition = new Vector3(0f, 10.5f, 0f);
	public static Vector3 cameraRotation;

	public static IInputContext input = null!;
	public static Vector2? lastMousePosition = null;

	public static World.World world = new World.World();
	public static WorldRayResult rayResult;
	public static bool rayCollides;
	public static BlockId SelectedBlock = Blocks.REDSTONE_BLOCK;

	public static void Initialize() {
		Console.WriteLine("Initializing...");
		Preload.Initialize();
		Atlas.Initialize();
		Blocks.Initialize();
		// Entities.Initialize();

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
		float fov = 100f * (Mathf.PI / 180f);
		ProjectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(fov, Program.defaultSize.X / (float) Program.defaultSize.Y, 0.1f, 1000f);
	}

	public static void Resize(int width, int height) {
		float fov = 100f * (Mathf.PI / 180f);
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
		rayCollides = WorldRay.Cast(world, cameraPosition, cameraRotation.AngleToDirection, Config.MaxInteractionDistance, out rayResult);
		if (!rayCollides) return;

		int x = rayResult.blockPosition.x;
		int y = rayResult.blockPosition.y;
		int z = rayResult.blockPosition.z;
		if (button == MouseButton.Left) {}
		else if (button == MouseButton.Right) {
			x += rayResult.normal.x;
			y += rayResult.normal.y;
			z += rayResult.normal.z;
		} else {
			return;
		}

		Chunk? chunk = world.GetChunkFromBlock(x, y, z);
		if (chunk == null) return;
		x &= 15;
		y &= 15;
		z &= 15;
		chunk.SetBlock(x, y, z, button == MouseButton.Left ? 0u : SelectedBlock);
		if (!chunk.CalculateLight()) {
			chunk.QueueRefresh();
			if (x == 0 ) world.GetChunk(chunk.cx - 1, chunk.cy, chunk.cz)?.QueueRefresh();
			if (x == 15) world.GetChunk(chunk.cx + 1, chunk.cy, chunk.cz)?.QueueRefresh();
			if (y == 0 ) world.GetChunk(chunk.cx, chunk.cy - 1, chunk.cz)?.QueueRefresh();
			if (y == 15) world.GetChunk(chunk.cx, chunk.cy + 1, chunk.cz)?.QueueRefresh();
			if (z == 0 ) world.GetChunk(chunk.cx, chunk.cy, chunk.cz - 1)?.QueueRefresh();
			if (z == 15) world.GetChunk(chunk.cx, chunk.cy, chunk.cz + 1)?.QueueRefresh();

			if (x == 0 && z == 0) world.GetChunk(chunk.cx - 1, chunk.cy, chunk.cz - 1)?.QueueRefresh();
			if (x == 0 && z == 15) world.GetChunk(chunk.cx - 1, chunk.cy, chunk.cz + 1)?.QueueRefresh();
			if (x == 15 && z == 0) world.GetChunk(chunk.cx + 1, chunk.cy, chunk.cz - 1)?.QueueRefresh();
			if (x == 15 && z == 15) world.GetChunk(chunk.cx + 1, chunk.cy, chunk.cz + 1)?.QueueRefresh();
			if (x == 0 && y == 0) world.GetChunk(chunk.cx - 1, chunk.cy - 1, chunk.cz)?.QueueRefresh();
			if (x == 0 && y == 15) world.GetChunk(chunk.cx - 1, chunk.cy + 1, chunk.cz)?.QueueRefresh();
			if (x == 15 && y == 0) world.GetChunk(chunk.cx + 1, chunk.cy - 1, chunk.cz)?.QueueRefresh();
			if (x == 15 && y == 15) world.GetChunk(chunk.cx + 1, chunk.cy + 1, chunk.cz)?.QueueRefresh();
			if (y == 0 && z == 0) world.GetChunk(chunk.cx, chunk.cy - 1, chunk.cz - 1)?.QueueRefresh();
			if (y == 0 && z == 15) world.GetChunk(chunk.cx, chunk.cy - 1, chunk.cz + 1)?.QueueRefresh();
			if (y == 15 && z == 0) world.GetChunk(chunk.cx, chunk.cy + 1, chunk.cz - 1)?.QueueRefresh();
			if (y == 15 && z == 15) world.GetChunk(chunk.cx, chunk.cy + 1, chunk.cz + 1)?.QueueRefresh();
			if (x == 0 && y == 0 && z == 0) world.GetChunk(chunk.cx - 1, chunk.cy - 1, chunk.cz - 1)?.QueueRefresh();
			if (x == 0 && y == 0 && z == 15) world.GetChunk(chunk.cx - 1, chunk.cy - 1, chunk.cz + 1)?.QueueRefresh();
			if (x == 0 && y == 15 && z == 0) world.GetChunk(chunk.cx - 1, chunk.cy + 1, chunk.cz - 1)?.QueueRefresh();
			if (x == 0 && y == 15 && z == 15) world.GetChunk(chunk.cx - 1, chunk.cy + 1, chunk.cz + 1)?.QueueRefresh();
			if (x == 15 && y == 0 && z == 0) world.GetChunk(chunk.cx + 1, chunk.cy - 1, chunk.cz - 1)?.QueueRefresh();
			if (x == 15 && y == 0 && z == 15) world.GetChunk(chunk.cx + 1, chunk.cy - 1, chunk.cz + 1)?.QueueRefresh();
			if (x == 15 && y == 15 && z == 0) world.GetChunk(chunk.cx + 1, chunk.cy + 1, chunk.cz - 1)?.QueueRefresh();
			if (x == 15 && y == 15 && z == 15) world.GetChunk(chunk.cx + 1, chunk.cy + 1, chunk.cz + 1)?.QueueRefresh();
		} else {
			if (y == 15) world.GetChunk(chunk.cx, chunk.cy + 1, chunk.cz)?.QueueRefresh();

			Chunk? lightChunk = chunk;
			while (true) {
				lightChunk.QueueRefresh();
				if (x == 0 ) world.GetChunk(lightChunk.cx - 1, lightChunk.cy, lightChunk.cz)?.QueueRefresh();
				if (x == 15) world.GetChunk(lightChunk.cx + 1, lightChunk.cy, lightChunk.cz)?.QueueRefresh();
				if (z == 0 ) world.GetChunk(lightChunk.cx, lightChunk.cy, lightChunk.cz - 1)?.QueueRefresh();
				if (z == 15) world.GetChunk(lightChunk.cx, lightChunk.cy, lightChunk.cz + 1)?.QueueRefresh();

				lightChunk = world.GetChunk(lightChunk.cx, lightChunk.cy - 1, lightChunk.cz);
				if (lightChunk == null) break;
				if (!lightChunk.CalculateLight()) {
					lightChunk.QueueRefresh();
					if (x == 0 ) world.GetChunk(lightChunk.cx - 1, lightChunk.cy, lightChunk.cz)?.QueueRefresh();
					if (x == 15) world.GetChunk(lightChunk.cx + 1, lightChunk.cy, lightChunk.cz)?.QueueRefresh();
					if (z == 0 ) world.GetChunk(lightChunk.cx, lightChunk.cy, lightChunk.cz - 1)?.QueueRefresh();
					if (z == 15) world.GetChunk(lightChunk.cx, lightChunk.cy, lightChunk.cz + 1)?.QueueRefresh();

					if (x == 0 && z == 0) world.GetChunk(lightChunk.cx - 1, lightChunk.cy, lightChunk.cz - 1)?.QueueRefresh();
					if (x == 0 && z == 15) world.GetChunk(lightChunk.cx - 1, lightChunk.cy, lightChunk.cz + 1)?.QueueRefresh();
					if (x == 15 && z == 0) world.GetChunk(lightChunk.cx + 1, lightChunk.cy, lightChunk.cz - 1)?.QueueRefresh();
					if (x == 15 && z == 15) world.GetChunk(lightChunk.cx + 1, lightChunk.cy, lightChunk.cz + 1)?.QueueRefresh();

					break;
				}
			}
		}

		rayCollides = WorldRay.Cast(world, cameraPosition, cameraRotation.AngleToDirection, Config.MaxInteractionDistance, out rayResult);
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

		rayCollides = WorldRay.Cast(world, cameraPosition, cameraRotation.AngleToDirection, Config.MaxInteractionDistance, out rayResult);

		Vector3i offset = world.GetChunkPositionFromBlock((int) cameraPosition.x, (int) cameraPosition.y, (int) cameraPosition.z);
		for (int x = -Config.RenderDistance; x <= Config.RenderDistance; x++) {
			for (int z = -Config.RenderDistance; z <= Config.RenderDistance; z++) {
				for (int y = 5; y >= 0; y--) {
					world.VisibleChunk(offset.x + x, y, offset.z + z);
				}
			}
		}

		world.RemeshChunks();
	}

	public static void Render() {
		float timeStacker = (float) (Program.time * Config.TicksPerSecond);
		ViewMatrix = Matrix4X4.CreateTranslation(-Mathf.Lerp(lastCameraPosition.x, cameraPosition.x, timeStacker), -Mathf.Lerp(lastCameraPosition.y, cameraPosition.y, timeStacker), -Mathf.Lerp(lastCameraPosition.z, cameraPosition.z, timeStacker));
		ViewMatrix *= Matrix4X4.CreateRotationY(cameraRotation.y);
		ViewMatrix *= Matrix4X4.CreateRotationX(cameraRotation.x);

		Program.gl.StencilMask(0xFF);
		Program.gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

		Program.gl.UseProgram(Preload.Basic);
		Preload.Basic.SetUniform("view", Main.ViewMatrix, transpose: false);
		Preload.Basic.SetUniform("projection", Main.ProjectionMatrix, transpose: false);

		Program.gl.Enable(EnableCap.DepthTest);
		Program.gl.Enable(EnableCap.Blend);
		Program.gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		Program.gl.Enable(EnableCap.CullFace);
		Program.gl.CullFace(GLEnum.Back);
		Program.gl.ActiveTexture(Silk.NET.OpenGL.TextureUnit.Texture0);
		Program.gl.BindTexture(TextureTarget.Texture2D, Atlas.atlas._id);
		foreach (Chunk chunk in world.chunks.Values) {
			chunk.Render();
		}
		Program.gl.Disable(EnableCap.CullFace);
		Program.gl.BindVertexArray(0);

		if (rayCollides) {
			Program.gl.UseProgram(Preload.Selection);
			Preload.Selection.SetUniform("offset", (float) rayResult.blockPosition.x, rayResult.blockPosition.y, rayResult.blockPosition.z);
			Preload.Selection.SetUniform("view", Main.ViewMatrix, transpose: false);
			Preload.Selection.SetUniform("projection", Main.ProjectionMatrix, transpose: false);
			Program.gl.BindVertexArray(Program._anyVao);
			Program.gl.DrawArrays(PrimitiveType.Lines, 0, 24);
			Program.gl.BindVertexArray(0);
		}

		Program.gl.UseProgram(0);
	}
}
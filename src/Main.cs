using Moincroft.Definitions;
using Moincroft.Definitions.Models;
using Silk.NET.Input;
using Silk.NET.SDL;
using StbImageWriteSharp;

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
	public static BlockId SelectedBlockId;
	private static bool showUI = true;
	private static bool takeScreenshot = false;

	public static void Initialize() {
		Console.WriteLine("Initializing...");
		Preload.Initialize();
		Atlas.Initialize();
		ModelLoader.Initialize();
		BlockStateLoader.Initialize();
		Blocks.Initialize();

		SelectedBlockId = Blocks.AIR;
		// Entities.Initialize();

		input = Program.window.CreateInput();
		for (int i = 0; i < input.Keyboards.Count; i++) {
			IKeyboard keyboard = input.Keyboards[i];
			keyboard.KeyDown += KeyDown;
			keyboard.KeyUp += KeyUp;
		}
		for (int i = 0; i < input.Mice.Count; i++) {
			IMouse mouse = input.Mice[i];
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
		Keys.Press(key);
	}

	private static void KeyUp(IKeyboard keyboard, Key key, int arg3) {
		Keys.Release(key);
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
		if (!rayCollides)
			return;

		int x = rayResult.blockPosition.x;
		int y = rayResult.blockPosition.y;
		int z = rayResult.blockPosition.z;
		if (button == MouseButton.Left || button == MouseButton.Middle) { }
		else if (button == MouseButton.Right) {
			x += rayResult.normal.x;
			y += rayResult.normal.y;
			z += rayResult.normal.z;
		}
		else {
			return;
		}

		Chunk? chunk = world.GetChunkFromBlock(x, y, z);
		if (chunk == null)
			return;
		x &= 15;
		y &= 15;
		z &= 15;
		BlockPos pos = new BlockPos(x, y, z);
		if (button == MouseButton.Middle) {
			BlockType blockType = chunk.GetBlock(pos);
			Block block = BlockRegistry.GetBlock(blockType.Type);
			Property[] properties = block.Properties;

			IKeyboard keyboard = input.Keyboards[0];
			for (int i = 0; i < properties.Length; i++) {
				if (keyboard.IsKeyPressed(Key.Number1 + i)) {
					Property property = properties[i];

					int currentIndex = blockType.State.GetIndex(property);
					int nextIndex = (currentIndex + 1) % property.ValueCount;
					chunk.SetBlock(pos, blockType.With(property, nextIndex));
				}
			}
		}
		else {
			chunk.SetBlock(pos, button == MouseButton.Left ? default : new BlockType(SelectedBlockId, 0));
		}
		if (!chunk.CalculateLight()) {
			chunk.QueueRefresh();
			if (x == 0)
				world.GetChunk(chunk.cx - 1, chunk.cy, chunk.cz)?.QueueRefresh();
			if (x == 15)
				world.GetChunk(chunk.cx + 1, chunk.cy, chunk.cz)?.QueueRefresh();
			if (y == 0)
				world.GetChunk(chunk.cx, chunk.cy - 1, chunk.cz)?.QueueRefresh();
			if (y == 15)
				world.GetChunk(chunk.cx, chunk.cy + 1, chunk.cz)?.QueueRefresh();
			if (z == 0)
				world.GetChunk(chunk.cx, chunk.cy, chunk.cz - 1)?.QueueRefresh();
			if (z == 15)
				world.GetChunk(chunk.cx, chunk.cy, chunk.cz + 1)?.QueueRefresh();

			if (x == 0 && z == 0)
				world.GetChunk(chunk.cx - 1, chunk.cy, chunk.cz - 1)?.QueueRefresh();
			if (x == 0 && z == 15)
				world.GetChunk(chunk.cx - 1, chunk.cy, chunk.cz + 1)?.QueueRefresh();
			if (x == 15 && z == 0)
				world.GetChunk(chunk.cx + 1, chunk.cy, chunk.cz - 1)?.QueueRefresh();
			if (x == 15 && z == 15)
				world.GetChunk(chunk.cx + 1, chunk.cy, chunk.cz + 1)?.QueueRefresh();
			if (x == 0 && y == 0)
				world.GetChunk(chunk.cx - 1, chunk.cy - 1, chunk.cz)?.QueueRefresh();
			if (x == 0 && y == 15)
				world.GetChunk(chunk.cx - 1, chunk.cy + 1, chunk.cz)?.QueueRefresh();
			if (x == 15 && y == 0)
				world.GetChunk(chunk.cx + 1, chunk.cy - 1, chunk.cz)?.QueueRefresh();
			if (x == 15 && y == 15)
				world.GetChunk(chunk.cx + 1, chunk.cy + 1, chunk.cz)?.QueueRefresh();
			if (y == 0 && z == 0)
				world.GetChunk(chunk.cx, chunk.cy - 1, chunk.cz - 1)?.QueueRefresh();
			if (y == 0 && z == 15)
				world.GetChunk(chunk.cx, chunk.cy - 1, chunk.cz + 1)?.QueueRefresh();
			if (y == 15 && z == 0)
				world.GetChunk(chunk.cx, chunk.cy + 1, chunk.cz - 1)?.QueueRefresh();
			if (y == 15 && z == 15)
				world.GetChunk(chunk.cx, chunk.cy + 1, chunk.cz + 1)?.QueueRefresh();
			if (x == 0 && y == 0 && z == 0)
				world.GetChunk(chunk.cx - 1, chunk.cy - 1, chunk.cz - 1)?.QueueRefresh();
			if (x == 0 && y == 0 && z == 15)
				world.GetChunk(chunk.cx - 1, chunk.cy - 1, chunk.cz + 1)?.QueueRefresh();
			if (x == 0 && y == 15 && z == 0)
				world.GetChunk(chunk.cx - 1, chunk.cy + 1, chunk.cz - 1)?.QueueRefresh();
			if (x == 0 && y == 15 && z == 15)
				world.GetChunk(chunk.cx - 1, chunk.cy + 1, chunk.cz + 1)?.QueueRefresh();
			if (x == 15 && y == 0 && z == 0)
				world.GetChunk(chunk.cx + 1, chunk.cy - 1, chunk.cz - 1)?.QueueRefresh();
			if (x == 15 && y == 0 && z == 15)
				world.GetChunk(chunk.cx + 1, chunk.cy - 1, chunk.cz + 1)?.QueueRefresh();
			if (x == 15 && y == 15 && z == 0)
				world.GetChunk(chunk.cx + 1, chunk.cy + 1, chunk.cz - 1)?.QueueRefresh();
			if (x == 15 && y == 15 && z == 15)
				world.GetChunk(chunk.cx + 1, chunk.cy + 1, chunk.cz + 1)?.QueueRefresh();
		}
		else {
			if (y == 15)
				world.GetChunk(chunk.cx, chunk.cy + 1, chunk.cz)?.QueueRefresh();

			Chunk? lightChunk = chunk;
			while (true) {
				lightChunk.QueueRefresh();
				if (x == 0)
					world.GetChunk(lightChunk.cx - 1, lightChunk.cy, lightChunk.cz)?.QueueRefresh();
				if (x == 15)
					world.GetChunk(lightChunk.cx + 1, lightChunk.cy, lightChunk.cz)?.QueueRefresh();
				if (z == 0)
					world.GetChunk(lightChunk.cx, lightChunk.cy, lightChunk.cz - 1)?.QueueRefresh();
				if (z == 15)
					world.GetChunk(lightChunk.cx, lightChunk.cy, lightChunk.cz + 1)?.QueueRefresh();

				lightChunk = world.GetChunk(lightChunk.cx, lightChunk.cy - 1, lightChunk.cz);
				if (lightChunk == null)
					break;
				if (!lightChunk.CalculateLight()) {
					lightChunk.QueueRefresh();
					if (x == 0)
						world.GetChunk(lightChunk.cx - 1, lightChunk.cy, lightChunk.cz)?.QueueRefresh();
					if (x == 15)
						world.GetChunk(lightChunk.cx + 1, lightChunk.cy, lightChunk.cz)?.QueueRefresh();
					if (z == 0)
						world.GetChunk(lightChunk.cx, lightChunk.cy, lightChunk.cz - 1)?.QueueRefresh();
					if (z == 15)
						world.GetChunk(lightChunk.cx, lightChunk.cy, lightChunk.cz + 1)?.QueueRefresh();

					if (x == 0 && z == 0)
						world.GetChunk(lightChunk.cx - 1, lightChunk.cy, lightChunk.cz - 1)?.QueueRefresh();
					if (x == 0 && z == 15)
						world.GetChunk(lightChunk.cx - 1, lightChunk.cy, lightChunk.cz + 1)?.QueueRefresh();
					if (x == 15 && z == 0)
						world.GetChunk(lightChunk.cx + 1, lightChunk.cy, lightChunk.cz - 1)?.QueueRefresh();
					if (x == 15 && z == 15)
						world.GetChunk(lightChunk.cx + 1, lightChunk.cy, lightChunk.cz + 1)?.QueueRefresh();

					break;
				}
			}
		}

		rayCollides = WorldRay.Cast(world, cameraPosition, cameraRotation.AngleToDirection, Config.MaxInteractionDistance, out rayResult);
	}

	public static void Update() {
		lastCameraPosition = cameraPosition;

		Vector2 movement = Vector2.Zero;
		float speed = Keys.Pressed(Key.ControlLeft) ? 0.4f : 0.15f;
		if (Keys.Pressed(Key.W)) {
			movement.y -= speed;
		}
		if (Keys.Pressed(Key.S)) {
			movement.y += speed;
		}
		if (Keys.Pressed(Key.A)) {
			movement.x -= speed;
		}
		if (Keys.Pressed(Key.D)) {
			movement.x += speed;
		}
		if (Keys.Pressed(Key.ShiftLeft)) {
			cameraPosition.y -= speed;
		}
		if (Keys.Pressed(Key.Space)) {
			cameraPosition.y += speed;
		}
		if (Keys.JustPressed(Key.Q)) {
			SelectedBlockId = (BlockId) (SelectedBlockId - 1 + BlockRegistry.Count);
		}
		if (Keys.JustPressed(Key.E)) {
			SelectedBlockId++;
			;
		}
		SelectedBlockId = (BlockId) (SelectedBlockId % BlockRegistry.Count);
		cameraPosition.x += Mathf.Cos(-cameraRotation.y) * movement.x + Mathf.Sin(-cameraRotation.y) * movement.y;
		cameraPosition.z += -Mathf.Sin(-cameraRotation.y) * movement.x + Mathf.Cos(-cameraRotation.y) * movement.y;

		rayCollides = WorldRay.Cast(world, cameraPosition, cameraRotation.AngleToDirection, Config.MaxInteractionDistance, out rayResult);
		BlockType type = world.GetBlock(rayResult.blockPosition.x, rayResult.blockPosition.y, rayResult.blockPosition.z);
		Block block = BlockRegistry.GetBlock(type.Type);
		Program.data = $"{BlockRegistry.GetBlock(SelectedBlockId).data.Id}  -  {block.data.Id} {type.State.PropertyKey}";

		Vector3i offset = world.GetChunkPositionFromBlock((int) cameraPosition.x, (int) cameraPosition.y, (int) cameraPosition.z);
		for (int x = -Config.RenderDistance; x <= Config.RenderDistance; x++) {
			for (int z = -Config.RenderDistance; z <= Config.RenderDistance; z++) {
				for (int y = 2; y >= 0; y--) {
					world.VisibleChunk(offset.x + x, y, offset.z + z);
				}
			}
		}

		world.RemeshChunks();

		if (Keys.JustPressed(Key.F2)) {
			takeScreenshot = true;
		}

		if (Keys.JustPressed(Key.F1)) {
			showUI = !showUI;
		}

		Keys.End();
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

		if (rayCollides && showUI) {
			Program.gl.UseProgram(Preload.Selection);
			Preload.Selection.SetUniform("offset", (float) rayResult.blockPosition.x, rayResult.blockPosition.y, rayResult.blockPosition.z);
			Preload.Selection.SetUniform("view", Main.ViewMatrix, transpose: false);
			Preload.Selection.SetUniform("projection", Main.ProjectionMatrix, transpose: false);
			Program.gl.BindVertexArray(Program._anyVao);
			Program.gl.DrawArrays(PrimitiveType.Lines, 0, 24);
			Program.gl.BindVertexArray(0);
		}

		Program.gl.UseProgram(0);

		if (takeScreenshot) {
			takeScreenshot = false;

			Program.gl.Flush();
			int width = Program.window.Size.X;
			int height = Program.window.Size.Y;
			byte[] pixels = new byte[width * height * 4];

			unsafe {
				fixed (byte* ptr = pixels) {
					Program.gl.ReadPixels(
						0, 0,
						(uint) width,
						(uint) height,
						Silk.NET.OpenGL.PixelFormat.Rgba,
						Silk.NET.OpenGL.PixelType.UnsignedByte,
						ptr
					);
				}
			}

			byte[] flipped = new byte[pixels.Length];
			int rowSize = width * 4;

			for (int y = 0; y < height; y++) {
				System.Buffer.BlockCopy(
					pixels,
					y * rowSize,
					flipped,
					(height - 1 - y) * rowSize,
					rowSize
				);
			}

			string path = "screenshot.png";
			ImageWriter writer = new ImageWriter();

			unsafe {
				fixed (byte* ptr = flipped) {
					using FileStream stream = File.Create(path);

					writer.WritePng(ptr, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
				}
			}
		}
	}
}
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

	public static void Initialize() {
		Console.WriteLine("Initializing...");
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

		Audio.Initialize();

		ViewMatrix = Matrix4X4.CreateTranslation(0f, -4f, -25f);
		float fov = 90f * (Mathf.PI / 180f);
		ProjectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(fov, 800f / 600f, 0.1f, 1000f);
	}

	private static Vector3 CreateAsteroid(float x) {
		float theta = random.NextSingle() * Mathf.PI * 2f;
		float rad = random.NextSingle() * 300f + 20f;
		return new Vector3(x, Mathf.Cos(theta) * rad, Mathf.Sin(theta) * rad);
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
	}

	public static void Render() {
		Gl.StencilMask(0xFF);
		Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

	}
}
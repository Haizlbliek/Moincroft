namespace Moincroft;

public static class Preload {
	public static Shader Basic;

	public static void Initialize() {
		Basic = Shader.Load("assets/shaders/Basic.vert", "assets/shaders/Basic.frag");
	}
}
namespace Moincroft;

public static class Preload {
	public static Shader Basic;
	public static Shader Selection;
	public static Texture atlas;

	public static void Initialize() {
		Basic = Shader.Load("assets/shaders/Basic.vert", "assets/shaders/Basic.frag");
		Selection = Shader.Load("assets/shaders/Selection.vert", "assets/shaders/Selection.frag");
		atlas = Texture.Load("assets/texture_atlas.png");

		Program.gl.UseProgram(Basic);
		int loc = Program.gl.GetUniformLocation(Basic, "uTexture");
		Program.gl.Uniform1(loc, 0);
		Program.gl.UseProgram(0);
	}
}
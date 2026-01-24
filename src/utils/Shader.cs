using System.IO;
using Silk.NET.OpenGL;

namespace Moincroft.Utils;

public class Shader {
	public readonly uint shader;

	private Shader(uint shader) {
		this.shader = shader;
	}

	public void Dispose() {
		Program.gl.DeleteProgram(this.shader);
	}

	public static Shader Load(string vertexPath, string fragmentPath, string? geometryPath = null) {
		uint vertexShader = Program.gl.CreateShader(ShaderType.VertexShader);
		Program.gl.ShaderSource(vertexShader, File.ReadAllText(vertexPath));
		Program.gl.CompileShader(vertexShader);

		Program.gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vertexStatus);
		if (vertexStatus != (int) GLEnum.True) {
			throw new Exception(Program.gl.GetShaderInfoLog(vertexShader));
		}


		uint fragmentShader = Program.gl.CreateShader(ShaderType.FragmentShader);
		Program.gl.ShaderSource(fragmentShader, File.ReadAllText(fragmentPath));
		Program.gl.CompileShader(fragmentShader);

		Program.gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fragmentStatus);
		if (fragmentStatus != (int) GLEnum.True) {
			throw new Exception(Program.gl.GetShaderInfoLog(fragmentShader));
		}


		uint? geometryShader = null;
		if (geometryPath != null) {
			geometryShader = Program.gl.CreateShader(ShaderType.GeometryShader);
			Program.gl.ShaderSource(geometryShader.Value, File.ReadAllText(geometryPath));
			Program.gl.CompileShader(geometryShader.Value);

			Program.gl.GetShader(geometryShader.Value, ShaderParameterName.CompileStatus, out int geometryStatus);
			if (geometryStatus != (int) GLEnum.True) {
				throw new Exception(Program.gl.GetShaderInfoLog(geometryShader.Value));
			}
		}


		uint shaderProgram = Program.gl.CreateProgram();
		Program.gl.AttachShader(shaderProgram, vertexShader);
		Program.gl.AttachShader(shaderProgram, fragmentShader);
		if (geometryShader.HasValue) {
			Program.gl.AttachShader(shaderProgram, geometryShader.Value);
		}
		Program.gl.LinkProgram(shaderProgram);
		Program.gl.GetProgram(shaderProgram, ProgramPropertyARB.LinkStatus, out int linkStatus);
		if (linkStatus != (int) GLEnum.True) {
			throw new Exception(Program.gl.GetProgramInfoLog(shaderProgram));
		}

		Program.gl.DetachShader(shaderProgram, vertexShader);
		Program.gl.DetachShader(shaderProgram, fragmentShader);
		Program.gl.DeleteShader(vertexShader);
		Program.gl.DeleteShader(fragmentShader);

		if (geometryShader.HasValue) {
			Program.gl.DetachShader(shaderProgram, geometryShader.Value);
			Program.gl.DeleteShader(geometryShader.Value);
		}

		return new Shader(shaderProgram);
	}
}
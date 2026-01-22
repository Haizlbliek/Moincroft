using System.IO;
using Silk.NET.Core;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace Moincroft;

public class Texture {
	public readonly uint _id;

	private Texture(uint id) {
		this._id = id;
	}

	public static unsafe Texture Load(string path) {
		Texture texture = new Texture(Program.gl.GenTexture());
		Program.gl.ActiveTexture(TextureUnit.Texture0);
		Program.gl.BindTexture(TextureTarget.Texture2D, texture._id);
		ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);

		fixed (byte* ptr = result.Data) {
			Program.gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint) result.Width, (uint) result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
		}

#pragma warning disable CS9193
		Program.gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int) TextureWrapMode.Repeat);
		Program.gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int) TextureWrapMode.Repeat);
		Program.gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int) TextureMinFilter.Linear);
		Program.gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int) TextureMagFilter.Nearest);
#pragma warning restore CS9193

		// Program.gl.GenerateMipmap(TextureTarget.Texture2D);

		Program.gl.BindTexture(TextureTarget.Texture2D, 0);

		return texture;
	}

	public static RawImage LoadRawImage(string path) {
		ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);

		return new RawImage(result.Width, result.Height, result.Data);
	}
}
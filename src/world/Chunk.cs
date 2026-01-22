using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;

namespace Moincroft.World;

public class Chunk : IRenderable {
	public World world;
	public BlockId[] blocks;
	public int cx;
	public int cy;
	public int cz;

	public Chunk(World world, int cx, int cy, int cz) {
		this.world = world;
		this.blocks = new BlockId[16 * 16 * 16];
		this.cx = cx;
		this.cy = cy;
		this.cz = cz;

		this._vao = Program.gl.GenVertexArray();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetBlock(int x, int y, int z, BlockId block) {
		this.blocks[x + y * 16 + z * 256] = block;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockId GetBlock(int x, int y, int z) {
		return this.blocks[x + y * 16 + z * 256];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CarefulSetBlock(int x, int y, int z, BlockId block) {
		if (x < 0 || x > 15 || y < 0 || y > 15 || z < 0 || z > 15) return;

		this.SetBlock(x, y, z, block);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockId CarefulGetBlock(int x, int y, int z) {
		if (x < 0 || x > 15 || y < 0 || y > 15 || z < 0 || z > 15) return 0;

		return this.GetBlock(x, y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockId GetBlockOutside(int x, int y, int z) {
		if (x < 0 || x > 15 || y < 0 || y > 15 || z < 0 || z > 15) return this.world.GetBlock(x + this.cx * 16, y + this.cy * 16, z + this.cz * 16);

		return this.GetBlock(x, y, z);
	}


#region Rendering
	public uint _vao;
	public uint _vbo;
	public uint _ebo;
	public bool meshed;
	private float[] verticesArr;
	private uint[] indicesArr;

	public unsafe void GenerateMesh() {
		if (this._vbo != 0) {
			Program.gl.DeleteBuffer(this._vbo);
			this._vbo = 0;
		}
		if (this._ebo != 0) {
			Program.gl.DeleteBuffer(this._ebo);
			this._ebo = 0;
		}

		Program.gl.BindVertexArray(this._vao);

		List<float> vertices = [];
		List<uint> indices = [];

		void AddVertex(float x, float y, float z, float uvX, float uvY) {
			vertices.Add(x);
			vertices.Add(y);
			vertices.Add(z);
			vertices.Add(uvX);
			vertices.Add(uvY);
		}

		const float uvMin = 0f;
		const float uvMax = 1f;

		for (int z = 0; z < 16; z++) {
			for (int x = 0; x < 16; x++) {
				for (int y = 0; y < 16; y++) {
					BlockId type = this.GetBlock(x, y, z);
					if (type == 0) continue;

					BlockId bxm = this.GetBlockOutside(x - 1, y, z);
					BlockId bxp = this.GetBlockOutside(x + 1, y, z);
					BlockId bym = this.GetBlockOutside(x, y - 1, z);
					BlockId byp = this.GetBlockOutside(x, y + 1, z);
					BlockId bzm = this.GetBlockOutside(x, y, z - 1);
					BlockId bzp = this.GetBlockOutside(x, y, z + 1);

					uint i;

					if (bzm == 0) {
						i = (uint) vertices.Count / 5;
						indices.AddRange([ i, i + 1, i + 2, i + 1, i + 2, i + 3 ]);
						AddVertex(x + 0, y + 0, z + 0, uvMin, uvMin);
						AddVertex(x + 1, y + 0, z + 0, uvMax, uvMin);
						AddVertex(x + 0, y + 1, z + 0, uvMin, uvMax);
						AddVertex(x + 1, y + 1, z + 0, uvMax, uvMax);
					}

					if (bzp == 0) {
						i = (uint) vertices.Count / 5;
						indices.AddRange([ i, i + 1, i + 2, i + 1, i + 2, i + 3 ]);
						AddVertex(x + 1, y + 0, z + 1, uvMax, uvMin);
						AddVertex(x + 0, y + 0, z + 1, uvMin, uvMin);
						AddVertex(x + 1, y + 1, z + 1, uvMax, uvMax);
						AddVertex(x + 0, y + 1, z + 1, uvMin, uvMax);
					}

					if (bym == 0) {
						i = (uint) vertices.Count / 5;
						indices.AddRange([ i, i + 1, i + 2, i + 1, i + 2, i + 3 ]);
						AddVertex(x + 0, y + 0, z + 0, uvMin, uvMin);
						AddVertex(x + 1, y + 0, z + 0, uvMax, uvMin);
						AddVertex(x + 0, y + 0, z + 1, uvMin, uvMax);
						AddVertex(x + 1, y + 0, z + 1, uvMax, uvMax);
					}

					if (byp == 0) {
						i = (uint) vertices.Count / 5;
						indices.AddRange([ i, i + 1, i + 2, i + 1, i + 2, i + 3 ]);
						AddVertex(x + 1, y + 1, z + 0, uvMax, uvMin);
						AddVertex(x + 0, y + 1, z + 0, uvMin, uvMin);
						AddVertex(x + 1, y + 1, z + 1, uvMax, uvMax);
						AddVertex(x + 0, y + 1, z + 1, uvMin, uvMax);
					}

					if (bxm == 0) {
						i = (uint) vertices.Count / 5;
						indices.AddRange([ i, i + 1, i + 2, i + 1, i + 2, i + 3 ]);
						AddVertex(x + 0, y + 0, z + 0, uvMin, uvMin);
						AddVertex(x + 0, y + 1, z + 0, uvMax, uvMin);
						AddVertex(x + 0, y + 0, z + 1, uvMin, uvMax);
						AddVertex(x + 0, y + 1, z + 1, uvMax, uvMax);
					}

					if (bxp == 0) {
						i = (uint) vertices.Count / 5;
						indices.AddRange([ i, i + 1, i + 2, i + 1, i + 2, i + 3 ]);
						AddVertex(x + 1, y + 1, z + 0, uvMax, uvMin);
						AddVertex(x + 1, y + 0, z + 0, uvMin, uvMin);
						AddVertex(x + 1, y + 1, z + 1, uvMax, uvMax);
						AddVertex(x + 1, y + 0, z + 1, uvMin, uvMax);
					}
				}
			}
		}


		this.verticesArr = [..vertices];
		this.indicesArr = [..indices];

		this._vbo = Program.gl.GenBuffer();
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, this._vbo);
		fixed (float* buf = this.verticesArr) {
			Program.gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (this.verticesArr.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
		}

		this._ebo = Program.gl.GenBuffer();
		Program.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, this._ebo);
		fixed (uint* buf = this.indicesArr) {
			Program.gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (this.indicesArr.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
		}

		const uint stride = 5 * sizeof(float);
		
		uint positionLoc = Program.gl.GetAttribLocation(Preload.Basic, "aPosition");
		Program.gl.EnableVertexAttribArray(positionLoc);
		Program.gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
		
		uint texCoordLoc = Program.gl.GetAttribLocation(Preload.Basic, "aTexCoord");
		Program.gl.EnableVertexAttribArray(texCoordLoc);
		Program.gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
		
		uint offsetLoc = Program.gl.GetAttribLocation(Preload.Basic, "offset");
		Program.gl.DisableVertexAttribArray(offsetLoc);
		Program.gl.VertexAttrib3(offsetLoc, this.cx * 16f, this.cy * 16f, this.cz * 16f);

		Program.gl.BindVertexArray(0);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
		Program.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

		this.meshed = true;
	}

	public unsafe void Render() {
		if (!this.meshed) return;

		Program.gl.BindVertexArray(this._vao);
		Program.gl.UseProgram(Preload.Basic);
		Program.gl.ActiveTexture(Silk.NET.OpenGL.TextureUnit.Texture0);
		Program.gl.BindTexture(TextureTarget.Texture2D, Preload.atlas._id);
		uint offsetLoc = Program.gl.GetAttribLocation(Preload.Basic, "offset");
		Program.gl.VertexAttrib3(offsetLoc, this.cx * 16f, this.cy * 16f, this.cz * 16f);
		Program.gl.DrawElements(PrimitiveType.Triangles, (uint) this.indicesArr.Length, DrawElementsType.UnsignedInt, (void*) 0);
		Program.gl.UseProgram(0);
		Program.gl.BindVertexArray(0);
	}
#endregion
}
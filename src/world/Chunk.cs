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
		if (x < 0 || x > 15 || y < 0 || y > 15 || z < 0 || z > 15) return this.world.GetBlock(x + this.cx, y + this.cy, z + this.cz);

		return this.GetBlock(x, y, z);
	}


#region Rendering
	public uint _vao;
	public uint _vbo;
	public uint _ebo;
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

		for (int z = 0; z < 16; z++) {
			for (int x = 0; x < 16; x++) {
				for (int y = 0; y < 16; y++) {
					BlockId type = this.GetBlock(x, y, z);
					if (type == 0) continue;

					uint i = (uint) vertices.Count / 3;
					vertices.AddRange(
						x, y, z,
						x + 1, y, z,
						x, y + 1, z,
						x + 1, y + 1, z,
						x, y, z + 1,
						x + 1, y, z + 1,
						x, y + 1, z + 1,
						x + 1, y + 1, z + 1
					);
					indices.AddRange(0u + i, 1u + i, 2u + i, 1u + i, 2u + i, 3u + i);
					indices.AddRange(4u + i, 5u + i, 6u + i, 5u + i, 6u + i, 7u + i);
					indices.AddRange(0u + i, 4u + i, 1u + i, 4u + i, 1u + i, 5u + i);
					indices.AddRange(2u + i, 3u + i, 6u + i, 3u + i, 6u + i, 7u + i);
					indices.AddRange(0u + i, 2u + i, 4u + i, 2u + i, 4u + i, 6u + i);
					indices.AddRange(1u + i, 5u + i, 3u + i, 5u + i, 3u + i, 7u + i);

					// BlockId bxm = this.GetBlockOutside(x - 1, y, z);
					// BlockId bxp = this.GetBlockOutside(x + 1, y, z);
					// BlockId bym = this.GetBlockOutside(x, y - 1, z);
					// BlockId byp = this.GetBlockOutside(x, y + 1, z);
					// BlockId bzm = this.GetBlockOutside(x, y, z - 1);
					// BlockId bzp = this.GetBlockOutside(x, y, z + 1);
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

		uint positionLoc = Program.gl.GetAttribLocation(Preload.Basic, "aPosition");
		Program.gl.EnableVertexAttribArray(positionLoc);
		Program.gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 0, (void*)0);
		uint offsetLoc = Program.gl.GetAttribLocation(Preload.Basic, "offset");
		Program.gl.DisableVertexAttribArray(offsetLoc);
		Program.gl.VertexAttrib3(offsetLoc, this.cx * 16f, this.cy * 16f, this.cz * 16f);

		Program.gl.BindVertexArray(0);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
		Program.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
	}

	public unsafe void Render() {
		Program.gl.BindVertexArray(this._vao);
		Program.gl.UseProgram(Preload.Basic);
		uint offsetLoc = Program.gl.GetAttribLocation(Preload.Basic, "offset");
		Program.gl.VertexAttrib3(offsetLoc, this.cx * 16f, this.cy * 16f, this.cz * 16f);
		Program.gl.DrawElements(PrimitiveType.Triangles, (uint) this.indicesArr.Length, DrawElementsType.UnsignedInt, (void*) 0);
		Program.gl.UseProgram(0);
		Program.gl.BindVertexArray(0);
	}
#endregion
}
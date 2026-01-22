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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsVisiblySolid(int x, int y, int z) {
		BlockId type = this.GetBlockOutside(x, y, z);
		return type > 0; // && Blocks.Blocks.blocks[type].opaque;
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

		void AddVertex(float x, float y, float z, float uvX, float uvY, byte ao) {
			vertices.Add(x);
			vertices.Add(y);
			vertices.Add(z);
			vertices.Add(uvX);
			vertices.Add(uvY);
			vertices.Add(ao);
		}

		const float uvMin = 0f;
		const float uvMax = 1f;
		const int VERTEX_INDEX_COUNT = 6;

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
					byte ao0, ao1, ao2, ao3;

					if (bzm == 0) {
						i = (uint) vertices.Count / VERTEX_INDEX_COUNT;
						indices.AddRange([ i, i + 1, i + 2, i + 1, i + 2, i + 3 ]);
						(ao0, ao1, ao2, ao3) = this.GetAO(x, y, z, 5);
						AddVertex(x + 0, y + 0, z + 0, uvMin, uvMin, ao0);
						AddVertex(x + 1, y + 0, z + 0, uvMax, uvMin, ao1);
						AddVertex(x + 0, y + 1, z + 0, uvMin, uvMax, ao2);
						AddVertex(x + 1, y + 1, z + 0, uvMax, uvMax, ao3);
					}

					if (bzp == 0) {
						i = (uint) vertices.Count / VERTEX_INDEX_COUNT;
						indices.AddRange([ i, i + 1, i + 2, i + 1, i + 2, i + 3 ]);
						(ao1, ao0, ao3, ao2) = this.GetAO(x, y, z, 4);
						AddVertex(x + 1, y + 0, z + 1, uvMax, uvMin, ao0);
						AddVertex(x + 0, y + 0, z + 1, uvMin, uvMin, ao1);
						AddVertex(x + 1, y + 1, z + 1, uvMax, uvMax, ao2);
						AddVertex(x + 0, y + 1, z + 1, uvMin, uvMax, ao3);
					}

					if (bym == 0) {
						i = (uint) vertices.Count / VERTEX_INDEX_COUNT;
						indices.AddRange([ i, i + 1, i + 2, i + 1, i + 2, i + 3 ]);
						(ao3, ao1, ao2, ao0) = this.GetAO(x, y, z, 3);
						AddVertex(x + 0, y + 0, z + 0, uvMin, uvMin, ao0);
						AddVertex(x + 1, y + 0, z + 0, uvMax, uvMin, ao1);
						AddVertex(x + 0, y + 0, z + 1, uvMin, uvMax, ao2);
						AddVertex(x + 1, y + 0, z + 1, uvMax, uvMax, ao3);
					}

					if (byp == 0) {
						i = (uint) vertices.Count / VERTEX_INDEX_COUNT;
						indices.AddRange([ i, i + 1, i + 2, i + 1, i + 2, i + 3 ]);
						(ao3, ao1, ao2, ao0) = this.GetAO(x, y, z, 2);
						AddVertex(x + 1, y + 1, z + 0, uvMax, uvMin, ao1);
						AddVertex(x + 0, y + 1, z + 0, uvMin, uvMin, ao0);
						AddVertex(x + 1, y + 1, z + 1, uvMax, uvMax, ao3);
						AddVertex(x + 0, y + 1, z + 1, uvMin, uvMax, ao2);
					}

					if (bxm == 0) {
						i = (uint) vertices.Count / VERTEX_INDEX_COUNT;
						indices.AddRange([ i, i + 1, i + 2, i + 1, i + 2, i + 3 ]);
						(ao0, ao2, ao1, ao3) = this.GetAO(x, y, z, 1);
						AddVertex(x + 0, y + 0, z + 0, uvMin, uvMin, ao0);
						AddVertex(x + 0, y + 1, z + 0, uvMax, uvMin, ao1);
						AddVertex(x + 0, y + 0, z + 1, uvMin, uvMax, ao2);
						AddVertex(x + 0, y + 1, z + 1, uvMax, uvMax, ao3);
					}

					if (bxp == 0) {
						i = (uint) vertices.Count / VERTEX_INDEX_COUNT;
						indices.AddRange([ i, i + 1, i + 2, i + 1, i + 2, i + 3 ]);
						(ao1, ao3, ao0, ao2) = this.GetAO(x, y, z, 0);
						AddVertex(x + 1, y + 1, z + 0, uvMax, uvMin, ao0);
						AddVertex(x + 1, y + 0, z + 0, uvMin, uvMin, ao1);
						AddVertex(x + 1, y + 1, z + 1, uvMax, uvMax, ao2);
						AddVertex(x + 1, y + 0, z + 1, uvMin, uvMax, ao3);
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

		const uint stride = 6 * sizeof(float);
		
		uint positionLoc = Program.gl.GetAttribLocation(Preload.Basic, "aPosition");
		Program.gl.EnableVertexAttribArray(positionLoc);
		Program.gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
		
		uint texCoordLoc = Program.gl.GetAttribLocation(Preload.Basic, "aTexCoord");
		Program.gl.EnableVertexAttribArray(texCoordLoc);
		Program.gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
		
		uint ambientOcclusionLoc = Program.gl.GetAttribLocation(Preload.Basic, "aAmbientOcclusion");
		Program.gl.EnableVertexAttribArray(ambientOcclusionLoc);
		Program.gl.VertexAttribPointer(ambientOcclusionLoc, 1, VertexAttribPointerType.Float, false, stride, (void*)(5 * sizeof(float)));
		
		uint offsetLoc = Program.gl.GetAttribLocation(Preload.Basic, "offset");
		Program.gl.DisableVertexAttribArray(offsetLoc);
		Program.gl.VertexAttrib3(offsetLoc, this.cx * 16f, this.cy * 16f, this.cz * 16f);

		Program.gl.BindVertexArray(0);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
		Program.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

		this.meshed = true;
	}

	public (byte, byte, byte, byte) GetAO(int x, int y, int z, int faceIndex) {
		sbyte[] offsets = Preload.FaceNeighbours_LOT[faceIndex];

		int mask = 0;
		if (this.IsVisiblySolid(x + offsets[0], y + offsets[1], z + offsets[2])) mask |= 2;
		if (this.IsVisiblySolid(x + offsets[3], y + offsets[4], z + offsets[5])) mask |= 8;
		if (this.IsVisiblySolid(x + offsets[6], y + offsets[7], z + offsets[8])) mask |= 32;
		if (this.IsVisiblySolid(x + offsets[9], y + offsets[10], z + offsets[11])) mask |= 128;
		if (this.IsVisiblySolid(x + offsets[12], y + offsets[13], z + offsets[14])) mask |= 1;
		if (this.IsVisiblySolid(x + offsets[15], y + offsets[16], z + offsets[17])) mask |= 4;
		if (this.IsVisiblySolid(x + offsets[18], y + offsets[19], z + offsets[20])) mask |= 16;
		if (this.IsVisiblySolid(x + offsets[21], y + offsets[22], z + offsets[23])) mask |= 64;

		return Preload.AO_LUT[mask];
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

	const byte FACE_X_POS = 1;
	const byte FACE_X_NEG = 2;
	const byte FACE_Y_POS = 4;
	const byte FACE_Y_NEG = 8;
	const byte FACE_Z_POS = 16;
	const byte FACE_Z_NEG = 32;
	const byte FACE_X = 3;
	const byte FACE_Y = 12;
	const byte FACE_Z = 48;
	const byte FACE_POS = 21;
	const byte FACE_NEG = 42;
}
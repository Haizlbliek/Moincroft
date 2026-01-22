using System.Runtime.CompilerServices;
using Silk.NET.Core;
using Silk.NET.OpenGL;

namespace Moincroft.World;

public class Chunk : ChunkData, IRenderable {
	public unsafe Chunk(World world, int cx, int cy, int cz) : base(world, cx, cy, cz) {
		this._vao = Program.gl.GenVertexArray();
		this._vbo = Program.gl.GenBuffer();
		this._ebo = Program.gl.GenBuffer();

		Program.gl.BindVertexArray(this._vao);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, this._vbo);

		const uint stride = 6 * sizeof(float);
		Program.gl.EnableVertexAttribArray( 0);
		Program.gl.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);

		Program.gl.EnableVertexAttribArray( 1);
		Program.gl.VertexAttribPointer( 1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));

		Program.gl.EnableVertexAttribArray( 2);
		Program.gl.VertexAttribPointer( 2, 1, VertexAttribPointerType.Float, false, stride, (void*)(5 * sizeof(float)));

		Program.gl.BindVertexArray(0);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
	}


#region Rendering
	public uint _vao;
	public uint _vbo;
	public uint _ebo;
	public bool meshed;
	public uint indices;

	public (List<float>, List<uint>) MeshData() {
		List<float> vertices = [];
		List<uint> indices = [];
		uint vertexIndex = 0;

		void AddVertex(float x, float y, float z, float uvX, float uvY, byte ao) {
			vertices.Add(x);
			vertices.Add(y);
			vertices.Add(z);
			vertices.Add(uvX);
			vertices.Add(uvY);
			vertices.Add(ao);
			vertexIndex++;
		}

		const float uvMin = 0f;
		const float uvMax = 1f;

		ChunkData neighbourXm = this.world.GetChunk(this.cx - 1, this.cy, this.cz) ?? World.emptyChunk;
		ChunkData neighbourXp = this.world.GetChunk(this.cx + 1, this.cy, this.cz) ?? World.emptyChunk;
		ChunkData neighbourYm = this.world.GetChunk(this.cx, this.cy - 1, this.cz) ?? World.emptyChunk;
		ChunkData neighbourYp = this.world.GetChunk(this.cx, this.cy + 1, this.cz) ?? World.emptyChunk;
		ChunkData neighbourZm = this.world.GetChunk(this.cx, this.cy, this.cz - 1) ?? World.emptyChunk;
		ChunkData neighbourZp = this.world.GetChunk(this.cx, this.cy, this.cz + 1) ?? World.emptyChunk;

		for (int z = 0; z < 16; z++) {
			for (int x = 0; x < 16; x++) {
				for (int y = 0; y < 16; y++) {
					BlockId type = this.GetBlock(x, y, z);
					if (type == 0) continue;

					BlockId bxm = x > 0 ? this.GetBlock(x - 1, y, z) : neighbourXm.GetBlock(15, y, z);
					BlockId bxp = x < 15 ? this.GetBlock(x + 1, y, z) : neighbourXp.GetBlock(0, y, z);
					BlockId bym = y > 0 ? this.GetBlock(x, y - 1, z) : neighbourYm.GetBlock(x, 15, z);
					BlockId byp = y < 15 ? this.GetBlock(x, y + 1, z) : neighbourYp.GetBlock(x, 0, z);
					BlockId bzm = z > 0 ? this.GetBlock(x, y, z - 1) : neighbourZm.GetBlock(x, y, 15);
					BlockId bzp = z < 15 ? this.GetBlock(x, y, z + 1) : neighbourZp.GetBlock(x, y, 0);

					byte ao0, ao1, ao2, ao3;

					if (bzm == 0) {
						indices.AddRange([ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
						(ao0, ao1, ao2, ao3) = this.GetAO(x, y, z, 5);
						AddVertex(x + 0, y + 0, z + 0, uvMin, uvMin, ao0);
						AddVertex(x + 1, y + 0, z + 0, uvMax, uvMin, ao1);
						AddVertex(x + 0, y + 1, z + 0, uvMin, uvMax, ao2);
						AddVertex(x + 1, y + 1, z + 0, uvMax, uvMax, ao3);
					}

					if (bzp == 0) {
						indices.AddRange([ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
						(ao1, ao0, ao3, ao2) = this.GetAO(x, y, z, 4);
						AddVertex(x + 1, y + 0, z + 1, uvMax, uvMin, ao0);
						AddVertex(x + 0, y + 0, z + 1, uvMin, uvMin, ao1);
						AddVertex(x + 1, y + 1, z + 1, uvMax, uvMax, ao2);
						AddVertex(x + 0, y + 1, z + 1, uvMin, uvMax, ao3);
					}

					if (bym == 0) {
						indices.AddRange([ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
						(ao3, ao1, ao2, ao0) = this.GetAO(x, y, z, 3);
						AddVertex(x + 0, y + 0, z + 0, uvMin, uvMin, ao0);
						AddVertex(x + 1, y + 0, z + 0, uvMax, uvMin, ao1);
						AddVertex(x + 0, y + 0, z + 1, uvMin, uvMax, ao2);
						AddVertex(x + 1, y + 0, z + 1, uvMax, uvMax, ao3);
					}

					if (byp == 0) {
						indices.AddRange([ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
						(ao3, ao1, ao2, ao0) = this.GetAO(x, y, z, 2);
						AddVertex(x + 1, y + 1, z + 0, uvMax, uvMin, ao1);
						AddVertex(x + 0, y + 1, z + 0, uvMin, uvMin, ao0);
						AddVertex(x + 1, y + 1, z + 1, uvMax, uvMax, ao3);
						AddVertex(x + 0, y + 1, z + 1, uvMin, uvMax, ao2);
					}

					if (bxm == 0) {
						indices.AddRange([ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
						(ao0, ao2, ao1, ao3) = this.GetAO(x, y, z, 1);
						AddVertex(x + 0, y + 0, z + 0, uvMin, uvMin, ao0);
						AddVertex(x + 0, y + 1, z + 0, uvMax, uvMin, ao1);
						AddVertex(x + 0, y + 0, z + 1, uvMin, uvMax, ao2);
						AddVertex(x + 0, y + 1, z + 1, uvMax, uvMax, ao3);
					}

					if (bxp == 0) {
						indices.AddRange([ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
						(ao1, ao3, ao0, ao2) = this.GetAO(x, y, z, 0);
						AddVertex(x + 1, y + 1, z + 0, uvMax, uvMin, ao0);
						AddVertex(x + 1, y + 0, z + 0, uvMin, uvMin, ao1);
						AddVertex(x + 1, y + 1, z + 1, uvMax, uvMax, ao2);
						AddVertex(x + 1, y + 0, z + 1, uvMin, uvMax, ao3);
					}
				}
			}
		}

		return (vertices, indices);
	}

	public unsafe void GenerateMesh() {
		Program.gl.BindVertexArray(this._vao);

		(List<float> vertices, List<uint> indices) = this.MeshData();

		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, this._vbo);
		Span<float> vertexSpan = CollectionsMarshal.AsSpan(vertices);
		Program.gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertexSpan.Length * sizeof(float)), (void*) 0, BufferUsageARB.StaticDraw);
		fixed (float* buf = vertexSpan) {
			Program.gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertexSpan.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
		}

		Program.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, this._ebo);
		Span<uint> indexSpan = CollectionsMarshal.AsSpan(indices);
		Program.gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indexSpan.Length * sizeof(uint)), (void*) 0, BufferUsageARB.StaticDraw);
		fixed (uint* buf = indexSpan) {
			Program.gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indexSpan.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
		}

		Program.gl.BindVertexArray(0);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
		Program.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

		this.indices = (uint) indexSpan.Length;
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
		uint offsetLoc = Program.gl.GetAttribLocation(Preload.Basic, "uChunkOffset");
		Program.gl.VertexAttrib3(offsetLoc, this.cx * 16f, this.cy * 16f, this.cz * 16f);
		Program.gl.DrawElements(PrimitiveType.Triangles, this.indices, DrawElementsType.UnsignedInt, (void*) 0);
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
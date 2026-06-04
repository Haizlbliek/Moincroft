namespace Moincroft.World;

public class Chunk : ChunkData {
	public unsafe Chunk(World world, int cx, int cy, int cz) : base(world, cx, cy, cz) {
		this._vao = Program.gl.GenVertexArray();
		this._vbo = Program.gl.GenBuffer();
		this._ebo = Program.gl.GenBuffer();

		Program.gl.BindVertexArray(this._vao);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, this._vbo);

		const uint stride = 6 * sizeof(float);
		Program.gl.EnableVertexAttribArray(0);
		Program.gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);

		Program.gl.EnableVertexAttribArray(1);
		Program.gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));

		Program.gl.EnableVertexAttribArray(2);
		Program.gl.VertexAttribIPointer(2, 1, VertexAttribIType.UnsignedInt, stride, (void*)(5 * sizeof(float)));

		Program.gl.BindVertexArray(0);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
	}


#region Rendering
	public uint _vao;
	public uint _vbo;
	public uint _ebo;
	public bool meshed;
	public uint indices;

	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex {
		public float x, y, z;
		public float u, v;
		public uint data;
	}

	public (List<Vertex>, List<uint>) MeshData() {
		List<Vertex> vertices = [];
		List<uint> indices = [];
		uint vertexIndex = 0;

		void AddVertex(float x, float y, float z, float uvX, float uvY, byte ao) {
			vertices.Add(new Vertex() { x=x, y=y, z=z, u=uvX, v=uvY, data=ao });
			vertexIndex++;
		}

		ChunkData neighbourNX = this.world.GetChunk(this.cx - 1, this.cy, this.cz) ?? World.emptyChunk;
		ChunkData neighbourPX = this.world.GetChunk(this.cx + 1, this.cy, this.cz) ?? World.emptyChunk;
		ChunkData neighbourNY = this.world.GetChunk(this.cx, this.cy - 1, this.cz) ?? World.emptyChunk;
		ChunkData neighbourPY = this.world.GetChunk(this.cx, this.cy + 1, this.cz) ?? World.emptyChunk;
		ChunkData neighbourNZ = this.world.GetChunk(this.cx, this.cy, this.cz - 1) ?? World.emptyChunk;
		ChunkData neighbourPZ = this.world.GetChunk(this.cx, this.cy, this.cz + 1) ?? World.emptyChunk;

		for (int z = 0; z < 16; z++) {
			for (int x = 0; x < 16; x++) {
				for (int y = 0; y < 16; y++) {
					BlockId type = this.GetBlock(x, y, z);
					if (type == 0) continue;

					BlockData data = BlockRegistry.Get(type);
					if (!data.Visible) continue;

					BlockId bnx = x > 0 ? this.GetBlock(x - 1, y, z) : neighbourNX.GetBlock(15, y, z);
					BlockId bpx = x < 15 ? this.GetBlock(x + 1, y, z) : neighbourPX.GetBlock(0, y, z);
					BlockId bny = y > 0 ? this.GetBlock(x, y - 1, z) : neighbourNY.GetBlock(x, 15, z);
					BlockId bpy = y < 15 ? this.GetBlock(x, y + 1, z) : neighbourPY.GetBlock(x, 0, z);
					BlockId bnz = z > 0 ? this.GetBlock(x, y, z - 1) : neighbourNZ.GetBlock(x, y, 15);
					BlockId bpz = z < 15 ? this.GetBlock(x, y, z + 1) : neighbourPZ.GetBlock(x, y, 0);

					byte ao0, ao1, ao2, ao3;

					if (bnz == 0) {
						(ao0, ao1, ao2, ao3) = this.GetAO(x, y, z, 5);
						if (ao0 + ao3 < ao1 + ao2) {
							indices.AddRange([ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
						} else {
							indices.AddRange([ vertexIndex, vertexIndex + 2, vertexIndex + 3, vertexIndex + 3, vertexIndex + 1, vertexIndex + 0 ]);
						}
						byte light = (byte) ((z > 0 ? this.GetBlockLight(x, y, z - 1) : neighbourNZ.GetBlockLight(x, y, 15)) << 4);
						AddVertex(x + 0, y + 0, z + 0, data.Nz.X + data.Nz.W, data.Nz.Y + data.Nz.H, (byte) (ao0 | light));
						AddVertex(x + 1, y + 0, z + 0, data.Nz.X, data.Nz.Y + data.Nz.H, (byte) (ao1 | light));
						AddVertex(x + 0, y + 1, z + 0, data.Nz.X + data.Nz.W, data.Nz.Y, (byte) (ao2 | light));
						AddVertex(x + 1, y + 1, z + 0, data.Nz.X, data.Nz.Y, (byte) (ao3 | light));
					}

					if (bpz == 0) {
						(ao1, ao0, ao3, ao2) = this.GetAO(x, y, z, 4);
						if (ao0 + ao3 < ao1 + ao2) {
							indices.AddRange([ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
						} else {
							indices.AddRange([ vertexIndex, vertexIndex + 2, vertexIndex + 3, vertexIndex + 3, vertexIndex + 1, vertexIndex + 0 ]);
						}
						byte light = (byte) ((z < 15 ? this.GetBlockLight(x, y, z + 1) : neighbourPZ.GetBlockLight(x, y, 0)) << 4);
						AddVertex(x + 1, y + 0, z + 1, data.Pz.X + data.Pz.W, data.Pz.Y + data.Pz.H, (byte) (ao0 | light));
						AddVertex(x + 0, y + 0, z + 1, data.Pz.X, data.Pz.Y + data.Pz.H, (byte) (ao1 | light));
						AddVertex(x + 1, y + 1, z + 1, data.Pz.X + data.Pz.W, data.Pz.Y, (byte) (ao2 | light));
						AddVertex(x + 0, y + 1, z + 1, data.Pz.X, data.Pz.Y, (byte) (ao3 | light));
					}

					if (bny == 0) {
						(ao3, ao1, ao2, ao0) = this.GetAO(x, y, z, 3);
						if (ao0 + ao3 < ao1 + ao2) {
							indices.AddRange([ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
						} else {
							indices.AddRange([ vertexIndex, vertexIndex + 2, vertexIndex + 3, vertexIndex + 3, vertexIndex + 1, vertexIndex + 0 ]);
						}
						byte light = (byte) ((y > 0 ? this.GetBlockLight(x, y - 1, z) : neighbourNY.GetBlockLight(x, 15, z)) << 4);
						AddVertex(x + 0, y + 0, z + 0, data.Ny.X, data.Ny.Y, (byte) (ao0 | light));
						AddVertex(x + 1, y + 0, z + 0, data.Ny.X + data.Ny.W, data.Ny.Y, (byte) (ao1 | light));
						AddVertex(x + 0, y + 0, z + 1, data.Ny.X, data.Ny.Y + data.Ny.H, (byte) (ao2 | light));
						AddVertex(x + 1, y + 0, z + 1, data.Ny.X + data.Ny.W, data.Ny.Y + data.Ny.H, (byte) (ao3 | light));
					}

					if (bpy == 0) {
						(ao2, ao0, ao3, ao1) = this.GetAO(x, y, z, 2);
						if (ao0 + ao3 < ao1 + ao2) {
							indices.AddRange([ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
						} else {
							indices.AddRange([ vertexIndex, vertexIndex + 2, vertexIndex + 3, vertexIndex + 3, vertexIndex + 1, vertexIndex + 0 ]);
						}
						byte light = (byte) ((y < 15 ? this.GetBlockLight(x, y + 1, z) : neighbourPY.GetBlockLight(x, 0, z)) << 4);
						AddVertex(x + 1, y + 1, z + 0, data.Py.X, data.Py.Y + data.Py.H, (byte) (ao0 | light));
						AddVertex(x + 0, y + 1, z + 0, data.Py.X + data.Py.W, data.Py.Y + data.Py.H, (byte) (ao1 | light));
						AddVertex(x + 1, y + 1, z + 1, data.Py.X, data.Py.Y, (byte) (ao2 | light));
						AddVertex(x + 0, y + 1, z + 1, data.Py.X + data.Py.W, data.Py.Y, (byte) (ao3 | light));
					}

					if (bnx == 0) {
						(ao0, ao2, ao1, ao3) = this.GetAO(x, y, z, 1);
						if (ao0 + ao3 < ao1 + ao2) {
							indices.AddRange([ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
						} else {
							indices.AddRange([ vertexIndex, vertexIndex + 2, vertexIndex + 3, vertexIndex + 3, vertexIndex + 1, vertexIndex + 0 ]);
						}
						byte light = (byte) ((x > 0 ? this.GetBlockLight(x - 1, y, z) : neighbourNX.GetBlockLight(15, y, z)) << 4);
						AddVertex(x + 0, y + 0, z + 0, data.Nx.X, data.Nx.Y + data.Nx.H, (byte) (ao0 | light));
						AddVertex(x + 0, y + 1, z + 0, data.Nx.X, data.Nx.Y, (byte) (ao1 | light));
						AddVertex(x + 0, y + 0, z + 1, data.Nx.X + data.Nx.W, data.Nx.Y + data.Nx.H, (byte) (ao2 | light));
						AddVertex(x + 0, y + 1, z + 1, data.Nx.X + data.Nx.W, data.Nx.Y, (byte) (ao3 | light));
					}

					if (bpx == 0) {
						(ao1, ao3, ao0, ao2) = this.GetAO(x, y, z, 0);
						if (ao0 + ao3 < ao1 + ao2) {
							indices.AddRange([ vertexIndex, vertexIndex + 1, vertexIndex + 2, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
						} else {
							indices.AddRange([ vertexIndex, vertexIndex + 2, vertexIndex + 3, vertexIndex + 3, vertexIndex + 1, vertexIndex + 0 ]);
						}
						byte light = (byte) ((x < 15 ? this.GetBlockLight(x + 1, y, z) : neighbourPX.GetBlockLight(0, y, z)) << 4);
						AddVertex(x + 1, y + 1, z + 0, data.Px.X, data.Px.Y, (byte) (ao0 | light));
						AddVertex(x + 1, y + 0, z + 0, data.Px.X, data.Px.Y + data.Px.H, (byte) (ao1 | light));
						AddVertex(x + 1, y + 1, z + 1, data.Px.X + data.Px.W, data.Px.Y, (byte) (ao2 | light));
						AddVertex(x + 1, y + 0, z + 1, data.Px.X + data.Px.W, data.Px.Y + data.Px.H, (byte) (ao3 | light));
					}
				}
			}
		}

		return (vertices, indices);
	}

	public unsafe void GenerateMesh() {
		Program.gl.BindVertexArray(this._vao);

		(List<Vertex> vertices, List<uint> indices) = this.MeshData();

		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, this._vbo);
		Span<Vertex> vertexSpan = CollectionsMarshal.AsSpan(vertices);
		fixed (Vertex* buf = vertexSpan) {
			Program.gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertexSpan.Length * sizeof(Vertex)), buf, BufferUsageARB.StaticDraw);
		}

		Program.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, this._ebo);
		Span<uint> indexSpan = CollectionsMarshal.AsSpan(indices);
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
		sbyte[] offsets = Preload.FaceNeighboursLUT[faceIndex];

		int mask = 0;
		if (this.IsVisiblySolid(x + offsets[0], y + offsets[1], z + offsets[2])) mask |= 2;
		if (this.IsVisiblySolid(x + offsets[3], y + offsets[4], z + offsets[5])) mask |= 8;
		if (this.IsVisiblySolid(x + offsets[6], y + offsets[7], z + offsets[8])) mask |= 32;
		if (this.IsVisiblySolid(x + offsets[9], y + offsets[10], z + offsets[11])) mask |= 128;
		if (this.IsVisiblySolid(x + offsets[12], y + offsets[13], z + offsets[14])) mask |= 1;
		if (this.IsVisiblySolid(x + offsets[15], y + offsets[16], z + offsets[17])) mask |= 4;
		if (this.IsVisiblySolid(x + offsets[18], y + offsets[19], z + offsets[20])) mask |= 16;
		if (this.IsVisiblySolid(x + offsets[21], y + offsets[22], z + offsets[23])) mask |= 64;

		return Preload.AmbientOcclusionVertexLUT[mask];
	}

	public unsafe void Render() {
		if (!this.meshed) return;

		Program.gl.BindVertexArray(this._vao);
		Program.gl.UseProgram(Preload.Basic);
		Program.gl.ActiveTexture(Silk.NET.OpenGL.TextureUnit.Texture0);
		Program.gl.BindTexture(TextureTarget.Texture2D, Atlas.atlas._id);
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
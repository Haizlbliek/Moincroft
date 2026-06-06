using Moincroft.Definitions;
using Moincroft.Definitions.Models;

namespace Moincroft.World;

public class Chunk : ChunkData {
	public bool meshNeedsRefresh = false;

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

	public Vector3 RotateAroundOrigin(Vector3 v, int rx, int ry, int rz) {
		for (int i = 0; i < rx; i++) {
			v = new Vector3(v.x, -v.z, v.y);
		}
		for (int i = 0; i < ry; i++) {
			v = new Vector3(v.z, v.y, -v.x);
		}
		for (int i = 0; i < rz; i++) {
			v = new Vector3(-v.y, v.x, v.z);
		}
		return v;
	}

	public (List<Vertex>, List<uint>) MeshData() {
		List<Vertex> vertices = [];
		List<uint> indices = [];
		uint vertexIndex = 0;

		void AddVertex(float x, float y, float z, Vector2 uv, byte ao) {
			vertices.Add(new Vertex() { x=x, y=y, z=z, u=uv.x, v=uv.y, data=ao });
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
					BlockPos pos = new BlockPos(x, y, z);
					BlockType type = this.GetBlock(pos);
					if (type.Type == 0) continue;

					Block block = BlockRegistry.GetBlock(type.Type);
					if (!block.data.Visible) continue;

					BlockStateItem model = block.data.BlockStateData.GetModel(type, pos);

					foreach (Model.Quad quad in model.model.quads) {
						if (quad.cullFace != Direction.None) {
							BlockType neighborBlock = quad.cullFace switch {
								Direction.West  => x > 0 ? this.GetBlock(pos.NX()) : neighbourNX.GetBlock(new BlockPos(15, y, z)),
								Direction.East  => x < 15 ? this.GetBlock(pos.PX()) : neighbourPX.GetBlock(new BlockPos(0, y, z)),
								Direction.Down  => y > 0 ? this.GetBlock(pos.NY()) : neighbourNY.GetBlock(new BlockPos(x, 15, z)),
								Direction.Up    => y < 15 ? this.GetBlock(pos.PY()) : neighbourPY.GetBlock(new BlockPos(x, 0, z)),
								Direction.North => z > 0 ? this.GetBlock(pos.NZ()) : neighbourNZ.GetBlock(new BlockPos(x, y, 15)),
								Direction.South => z < 15 ? this.GetBlock(pos.PZ()) : neighbourPZ.GetBlock(new BlockPos(x, y, 0)),
								_ => default
							};

							if (neighborBlock.Type != 0 && BlockRegistry.GetBlock(neighborBlock.Type).data.Opaque) {
								continue;
							}
						}

						// byte aoDirIndex = rotatedDirection switch {
						// 	Direction.East  => 0, Direction.West  => 1,
						// 	Direction.Up    => 2, Direction.Down  => 3,
						// 	Direction.South => 4, Direction.North => 5,
						// 	_ => 2
						// };

						byte lightValue = quad.direction switch {
							Direction.West  => (byte) (x > 0 ? this.GetBlockLight(x - 1, y, z) : neighbourNX.GetBlockLight(15, y, z)),
							Direction.East  => (byte) (x < 15 ? this.GetBlockLight(x + 1, y, z) : neighbourPX.GetBlockLight(0, y, z)),
							Direction.Down  => (byte) (y > 0 ? this.GetBlockLight(x, y - 1, z) : neighbourNY.GetBlockLight(x, 15, z)),
							Direction.Up    => (byte) (y < 15 ? this.GetBlockLight(x, y + 1, z) : neighbourPY.GetBlockLight(x, 0, z)),
							Direction.North => (byte) (z > 0 ? this.GetBlockLight(x, y, z - 1) : neighbourNZ.GetBlockLight(x, y, 15)),
							Direction.South => (byte) (z < 15 ? this.GetBlockLight(x, y, z + 1) : neighbourPZ.GetBlockLight(x, y, 0)),
							_ => (byte) this.GetBlockLight(x, y, z)
						};
						byte PackedLight = (byte)(lightValue << 4);

						byte ao0, ao1, ao2, ao3;
						float xMin = quad.from.x / 16f + x;
						float xMax = quad.to.x / 16f + x;
						float yMin = quad.from.y / 16f + y;
						float yMax = quad.to.y / 16f + y;
						float zMin = quad.from.z / 16f + z;
						float zMax = quad.to.z / 16f + z;
						Span<int> i = [0, 1, 2, 3];

						switch (quad.direction) {
							case Direction.NZ:
								(ao0, ao1, ao2, ao3) = this.GetAO(x, y, z, 5);
								if (ao0 + ao3 < ao1 + ao2) {
									indices.AddRange([ vertexIndex + 2, vertexIndex + 1, vertexIndex + 0, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
								} else {
									indices.AddRange([ vertexIndex + 0, vertexIndex + 2, vertexIndex + 3, vertexIndex + 3, vertexIndex + 1, vertexIndex + 0 ]);
								}
								Vector2[] uvNZ = [
									new Vector2(quad.u1, quad.v1),
									new Vector2(quad.u0, quad.v1),
									new Vector2(quad.u1, quad.v0),
									new Vector2(quad.u0, quad.v0),
								];
								AddVertex(xMin, yMin, zMin, uvNZ[i[0]], (byte) (ao0 | PackedLight));
								AddVertex(xMax, yMin, zMin, uvNZ[i[1]], (byte) (ao1 | PackedLight));
								AddVertex(xMin, yMax, zMin, uvNZ[i[2]], (byte) (ao2 | PackedLight));
								AddVertex(xMax, yMax, zMin, uvNZ[i[3]], (byte) (ao3 | PackedLight));
								break;

							case Direction.PZ:
								(ao1, ao0, ao3, ao2) = this.GetAO(x, y, z, 4);
								if (ao0 + ao3 < ao1 + ao2) {
									indices.AddRange([ vertexIndex + 2, vertexIndex + 1, vertexIndex + 0, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
								} else {
									indices.AddRange([ vertexIndex + 0, vertexIndex + 2, vertexIndex + 3, vertexIndex + 3, vertexIndex + 1, vertexIndex + 0 ]);
								}
								Vector2[] uvPZ = [
									new Vector2(quad.u1, quad.v1),
									new Vector2(quad.u0, quad.v1),
									new Vector2(quad.u1, quad.v0),
									new Vector2(quad.u0, quad.v0),
								];
								AddVertex(xMax, yMin, zMax, uvPZ[i[0]], (byte) (ao0 | PackedLight));
								AddVertex(xMin, yMin, zMax, uvPZ[i[1]], (byte) (ao1 | PackedLight));
								AddVertex(xMax, yMax, zMax, uvPZ[i[2]], (byte) (ao2 | PackedLight));
								AddVertex(xMin, yMax, zMax, uvPZ[i[3]], (byte) (ao3 | PackedLight));
								break;

							case Direction.NY:
								(ao3, ao1, ao2, ao0) = this.GetAO(x, y, z, 3);
								if (ao0 + ao3 < ao1 + ao2) {
									indices.AddRange([ vertexIndex + 0, vertexIndex + 1, vertexIndex + 2, vertexIndex + 2, vertexIndex + 1, vertexIndex + 3 ]);
								} else {
									indices.AddRange([ vertexIndex + 0, vertexIndex + 3, vertexIndex + 2, vertexIndex + 0, vertexIndex + 1, vertexIndex + 3 ]);
								}
								Vector2[] uvNY = [
									new Vector2(quad.u0, quad.v0),
									new Vector2(quad.u1, quad.v0),
									new Vector2(quad.u0, quad.v1),
									new Vector2(quad.u1, quad.v1),
								];
								AddVertex(xMin, yMin, zMin, uvNY[i[0]], (byte) (ao0 | PackedLight));
								AddVertex(xMax, yMin, zMin, uvNY[i[1]], (byte) (ao1 | PackedLight));
								AddVertex(xMin, yMin, zMax, uvNY[i[2]], (byte) (ao2 | PackedLight));
								AddVertex(xMax, yMin, zMax, uvNY[i[3]], (byte) (ao3 | PackedLight));
								break;

							case Direction.PY:
								(ao2, ao0, ao3, ao1) = this.GetAO(x, y, z, 2);
								if (ao0 + ao3 < ao1 + ao2) {
									indices.AddRange([ vertexIndex + 0, vertexIndex + 1, vertexIndex + 2, vertexIndex + 2, vertexIndex + 1, vertexIndex + 3 ]);
								} else {
									indices.AddRange([ vertexIndex + 0, vertexIndex + 3, vertexIndex + 2, vertexIndex + 0, vertexIndex + 1, vertexIndex + 3 ]);
								}
								Vector2[] uvPY = [
									new Vector2(quad.u1, quad.v0),
									new Vector2(quad.u0, quad.v0),
									new Vector2(quad.u1, quad.v1),
									new Vector2(quad.u0, quad.v1),
								];
								AddVertex(xMax, yMax, zMin, uvPY[i[0]], (byte) (ao0 | PackedLight));
								AddVertex(xMin, yMax, zMin, uvPY[i[1]], (byte) (ao1 | PackedLight));
								AddVertex(xMax, yMax, zMax, uvPY[i[2]], (byte) (ao2 | PackedLight));
								AddVertex(xMin, yMax, zMax, uvPY[i[3]], (byte) (ao3 | PackedLight));
								break;

							case Direction.NX:
								(ao0, ao2, ao1, ao3) = this.GetAO(x, y, z, 1);
								if (ao0 + ao3 < ao1 + ao2) {
									indices.AddRange([ vertexIndex + 2, vertexIndex + 1, vertexIndex + 0, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
								} else {
									indices.AddRange([ vertexIndex + 0, vertexIndex + 2, vertexIndex + 3, vertexIndex + 3, vertexIndex + 1, vertexIndex + 0 ]);
								}
								Vector2[] uvNX = [
									new Vector2(quad.u0, quad.v1),
									new Vector2(quad.u0, quad.v0),
									new Vector2(quad.u1, quad.v1),
									new Vector2(quad.u1, quad.v0),
								];
								AddVertex(xMin, yMin, zMin, uvNX[i[0]], (byte) (ao0 | PackedLight));
								AddVertex(xMin, yMax, zMin, uvNX[i[1]], (byte) (ao1 | PackedLight));
								AddVertex(xMin, yMin, zMax, uvNX[i[2]], (byte) (ao2 | PackedLight));
								AddVertex(xMin, yMax, zMax, uvNX[i[3]], (byte) (ao3 | PackedLight));
								break;

							case Direction.PX:
								(ao1, ao3, ao0, ao2) = this.GetAO(x, y, z, 0);
								if (ao0 + ao3 < ao1 + ao2) {
									indices.AddRange([ vertexIndex + 2, vertexIndex + 1, vertexIndex + 0, vertexIndex + 1, vertexIndex + 2, vertexIndex + 3 ]);
								} else {
									indices.AddRange([ vertexIndex + 0, vertexIndex + 2, vertexIndex + 3, vertexIndex + 3, vertexIndex + 1, vertexIndex + 0 ]);
								}
								Vector2[] uvPX = [
									new Vector2(quad.u1, quad.v0),
									new Vector2(quad.u1, quad.v1),
									new Vector2(quad.u0, quad.v0),
									new Vector2(quad.u0, quad.v1),
								];
								AddVertex(xMax, yMax, zMin, uvPX[i[0]], (byte) (ao0 | PackedLight));
								AddVertex(xMax, yMin, zMin, uvPX[i[1]], (byte) (ao1 | PackedLight));
								AddVertex(xMax, yMax, zMax, uvPX[i[2]], (byte) (ao2 | PackedLight));
								AddVertex(xMax, yMin, zMax, uvPX[i[3]], (byte) (ao3 | PackedLight));
								break;
						}
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
		if (this.IsVisiblySolid(new BlockPos(x + offsets[0], y + offsets[1], z + offsets[2]))) mask |= 2;
		if (this.IsVisiblySolid(new BlockPos(x + offsets[3], y + offsets[4], z + offsets[5]))) mask |= 8;
		if (this.IsVisiblySolid(new BlockPos(x + offsets[6], y + offsets[7], z + offsets[8]))) mask |= 32;
		if (this.IsVisiblySolid(new BlockPos(x + offsets[9], y + offsets[10], z + offsets[11]))) mask |= 128;
		if (this.IsVisiblySolid(new BlockPos(x + offsets[12], y + offsets[13], z + offsets[14]))) mask |= 1;
		if (this.IsVisiblySolid(new BlockPos(x + offsets[15], y + offsets[16], z + offsets[17]))) mask |= 4;
		if (this.IsVisiblySolid(new BlockPos(x + offsets[18], y + offsets[19], z + offsets[20]))) mask |= 16;
		if (this.IsVisiblySolid(new BlockPos(x + offsets[21], y + offsets[22], z + offsets[23]))) mask |= 64;

		return Preload.AmbientOcclusionVertexLUT[mask];
	}

	public unsafe void Render() {
		if (!this.meshed) return;

		Preload.Basic.SetUniform("uChunkOffset", this.cx * 16f, this.cy * 16f, this.cz * 16f);
		Program.gl.BindVertexArray(this._vao);
		Program.gl.DrawElements(PrimitiveType.Triangles, this.indices, DrawElementsType.UnsignedInt, (void*) 0);
	}

	public void QueueRefresh() {
		if (this.meshNeedsRefresh)
			return;

		this.meshNeedsRefresh = true;
		this.world.chunksToRemesh.Add(this);
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
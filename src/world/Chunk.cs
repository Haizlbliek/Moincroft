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

	public static Vector3 RotateAroundOrigin(Vector3 v, int rx, int ry, int rz) {
		for (int i = 0; i < rx; i++) {
			// REVIEW
			v = new Vector3(v.x, -v.z, v.y);
		}
		for (int i = 0; i < ry; i++) {
			v = new Vector3(-v.z, v.y, v.x);
		}
		for (int i = 0; i < rz; i++) {
			// REVIEW
			v = new Vector3(-v.y, v.x, v.z);
		}
		return v;
	}

	Vector3 RotateModelPos(Vector3 p, int rx, int ry, int rz) {
		p -= new Vector3(0.5f, 0.5f, 0.5f);
		p = RotateAroundOrigin(p, rx, ry, rz);
		p += new Vector3(0.5f, 0.5f, 0.5f);

		return p;
	}

	public (List<Vertex>, List<uint>) MeshData() {
		List<Vertex> vertices = [];
		List<uint> indices = [];
		uint vertexIndex = 0;

		void AddVertex(Vector3 pos, Vector2 uv, byte ao) {
			vertices.Add(new Vertex() { x=pos.x, y=pos.y, z=pos.z, u=uv.x, v=uv.y, data=ao });
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
						Direction rotatedDirection = Preload.Rotate(quad.direction, model.rotationX, model.rotationY, model.rotationZ);
						Direction rotatedCullFace = Preload.Rotate(quad.cullFace, model.rotationX, model.rotationY, model.rotationZ);

						if (rotatedCullFace != Direction.None) {
							BlockType neighborBlock = rotatedCullFace switch {
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

						byte lightValue = rotatedDirection switch {
							Direction.West  => (byte) (x > 0 ? this.GetBlockLight(x - 1, y, z) : neighbourNX.GetBlockLight(15, y, z)),
							Direction.East  => (byte) (x < 15 ? this.GetBlockLight(x + 1, y, z) : neighbourPX.GetBlockLight(0, y, z)),
							Direction.Down  => (byte) (y > 0 ? this.GetBlockLight(x, y - 1, z) : neighbourNY.GetBlockLight(x, 15, z)),
							Direction.Up    => (byte) (y < 15 ? this.GetBlockLight(x, y + 1, z) : neighbourPY.GetBlockLight(x, 0, z)),
							Direction.North => (byte) (z > 0 ? this.GetBlockLight(x, y, z - 1) : neighbourNZ.GetBlockLight(x, y, 15)),
							Direction.South => (byte) (z < 15 ? this.GetBlockLight(x, y, z + 1) : neighbourPZ.GetBlockLight(x, y, 0)),
							_ => (byte) this.GetBlockLight(x, y, z)
						};
						byte PackedLight = (byte)(lightValue << 4);

						FaceBasis faceBasis = Preload.FaceBases[(int) quad.direction];
						(byte ao0, byte ao1, byte ao2, byte ao3) = this.GetAO(pos, faceBasis.Rotated(model.rotationX, model.rotationY, model.rotationZ));
						if (ao0 + ao3 < ao1 + ao2) {
							indices.AddRange([ vertexIndex + 0, vertexIndex + 1, vertexIndex + 2, vertexIndex + 2, vertexIndex + 1, vertexIndex + 3 ]);
						} else {
							indices.AddRange([ vertexIndex + 0, vertexIndex + 1, vertexIndex + 3, vertexIndex + 3, vertexIndex + 2, vertexIndex + 0 ]);
						}
						Vector3 center = (quad.to + quad.from) / 32f;
						Vector3 size = (quad.from - quad.to) / 32f;
						size.x = Mathf.Abs(size.x);
						size.y = Mathf.Abs(size.y);
						size.z = Mathf.Abs(size.z);

						Vector3 front = center + size * (Vector3) faceBasis.Front;
						Vector3 v0 = front + size * (Vector3) (-faceBasis.Right + faceBasis.Up);
						Vector3 v1 = front + size * (Vector3) (faceBasis.Right + faceBasis.Up);
						Vector3 v2 = front + size * (Vector3) (-faceBasis.Right - faceBasis.Up);
						Vector3 v3 = front + size * (Vector3) (faceBasis.Right - faceBasis.Up);

						Vector3 modelPos = new Vector3(x, y, z);
						v0 = this.RotateModelPos(v0, model.rotationX, model.rotationY, model.rotationZ) + modelPos;
						v1 = this.RotateModelPos(v1, model.rotationX, model.rotationY, model.rotationZ) + modelPos;
						v2 = this.RotateModelPos(v2, model.rotationX, model.rotationY, model.rotationZ) + modelPos;
						v3 = this.RotateModelPos(v3, model.rotationX, model.rotationY, model.rotationZ) + modelPos;

						AddVertex(v0, new Vector2(quad.u1, quad.v0), (byte) (ao0 | PackedLight));
						AddVertex(v1, new Vector2(quad.u0, quad.v0), (byte) (ao1 | PackedLight));
						AddVertex(v2, new Vector2(quad.u1, quad.v1), (byte) (ao2 | PackedLight));
						AddVertex(v3, new Vector2(quad.u0, quad.v1), (byte) (ao3 | PackedLight));
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

	public (byte, byte, byte, byte) GetAO(BlockPos pos, FaceBasis faceBasis) {
		BlockPos front = pos + faceBasis.Front;

		int mask = 0;

		if (this.IsVisiblySolid(front + faceBasis.Up - faceBasis.Right)) mask |= 1;
		if (this.IsVisiblySolid(front + faceBasis.Up)) mask |= 2;
		if (this.IsVisiblySolid(front + faceBasis.Up + faceBasis.Right)) mask |= 4;
		if (this.IsVisiblySolid(front + faceBasis.Right)) mask |= 8;
		if (this.IsVisiblySolid(front - faceBasis.Up + faceBasis.Right)) mask |= 16;
		if (this.IsVisiblySolid(front - faceBasis.Up)) mask |= 32;
		if (this.IsVisiblySolid(front - faceBasis.Up - faceBasis.Right)) mask |= 64;
		if (this.IsVisiblySolid(front - faceBasis.Right)) mask |= 128;

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
using System.Runtime.CompilerServices;
using Moincroft.Definitions;
using Moincroft.Definitions.BlockColors;
using Moincroft.Definitions.Models;

namespace Moincroft.World;

public class Chunk : ChunkData {
	public bool meshNeedsRefresh = false;

	public unsafe Chunk(World world, int cx, int cy, int cz) : base(world, cx, cy, cz) {
		this._opaqueVao = Program.gl.GenVertexArray();
		this._opaqueVbo = Program.gl.GenBuffer();
		this._opaqueEbo = Program.gl.GenBuffer();
		this._transparentVao = Program.gl.GenVertexArray();
		this._transparentVbo = Program.gl.GenBuffer();
		this._transparentEbo = Program.gl.GenBuffer();

		const uint stride = 7 * sizeof(float);

		Program.gl.BindVertexArray(this._opaqueVao);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, this._opaqueVbo);

		Program.gl.EnableVertexAttribArray(0);
		Program.gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);

		Program.gl.EnableVertexAttribArray(1);
		Program.gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));

		Program.gl.EnableVertexAttribArray(2);
		Program.gl.VertexAttribIPointer(2, 1, VertexAttribIType.UnsignedInt, stride, (void*)(5 * sizeof(float)));

		Program.gl.EnableVertexAttribArray(3);
		Program.gl.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, true, stride, (void*)(6 * sizeof(float)));

		Program.gl.BindVertexArray(0);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);


		Program.gl.BindVertexArray(this._transparentVao);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, this._transparentVbo);

		Program.gl.EnableVertexAttribArray(0);
		Program.gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);

		Program.gl.EnableVertexAttribArray(1);
		Program.gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));

		Program.gl.EnableVertexAttribArray(2);
		Program.gl.VertexAttribIPointer(2, 1, VertexAttribIType.UnsignedInt, stride, (void*)(5 * sizeof(float)));

		Program.gl.EnableVertexAttribArray(3);
		Program.gl.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, true, stride, (void*)(6 * sizeof(float)));

		Program.gl.BindVertexArray(0);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
	}


#region Rendering
	public uint _opaqueVao;
	public uint _opaqueVbo;
	public uint _opaqueEbo;
	public uint opaqueIndexCount;

	public uint _transparentVao;
	public uint _transparentVbo;
	public uint _transparentEbo;
	public uint transparentIndexCount;

	public bool meshed;

	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex {
		public float x, y, z;
		public float u, v;
		public uint data;
		public uint color;
	}

	public static Vector3 RotateAroundOrigin(Vector3 v, int rx, int ry, int rz) {
		float x = v.x;
		float y = v.y;
		float z = v.z;

		for (int i = 0; i < (rx & 3); i++) {
			float ny = -z;
			float nz = y;
			y = ny;
			z = nz;
		}

		for (int i = 0; i < (ry & 3); i++) {
			float nx = z;
			float nz = -x;
			x = nx;
			z = nz;
		}

		for (int i = 0; i < (rz & 3); i++) {
			float nx = -y;
			float ny = x;
			x = nx;
			y = ny;
		}

		return new Vector3(x, y, z);
	}

	public static Vector3 RotateModelPos(Vector3 p, int rx, int ry, int rz) {
		p.x -= 0.5f; p.y -= 0.5f; p.z -= 0.5f;
		p = RotateAroundOrigin(p, rx, ry, rz);
		p.x += 0.5f; p.y += 0.5f; p.z += 0.5f;

		return p;
	}

	private readonly List<Vertex> vertices = [];
	private uint[] indices = new uint[6 * 6 * 512];

	public void MeshData(RenderLayer renderLayer) {
		this.vertices.Clear();
		uint vertexIndex = 0;

		uint indexPtr = 0;

		ChunkData neighbourNX = this.world.GetChunk(this.cx - 1, this.cy, this.cz) ?? World.emptyChunk;
		ChunkData neighbourPX = this.world.GetChunk(this.cx + 1, this.cy, this.cz) ?? World.emptyChunk;
		ChunkData neighbourNY = this.world.GetChunk(this.cx, this.cy - 1, this.cz) ?? World.emptyChunk;
		ChunkData neighbourPY = this.world.GetChunk(this.cx, this.cy + 1, this.cz) ?? World.emptyChunk;
		ChunkData neighbourNZ = this.world.GetChunk(this.cx, this.cy, this.cz - 1) ?? World.emptyChunk;
		ChunkData neighbourPZ = this.world.GetChunk(this.cx, this.cy, this.cz + 1) ?? World.emptyChunk;

		void AddVertex(Vector3 pos, Vector2 uv, byte ao, int color = -1) {
			this.vertices.Add(new Vertex() {
				x = pos.x,
				y = pos.y,
				z = pos.z,
				u = uv.x,
				v = uv.y,
				data = ao,
				color = (uint) color,
			});
			vertexIndex++;
		}

		void EnsureIndexCapacity(uint needed) {
			if (needed <= this.indices.Length) return;

			int newSize = this.indices.Length * 2;
			while (newSize < needed)
				newSize *= 2;

			Array.Resize(ref this.indices, newSize);
		}

		BlockType GetBlockNeighbour(BlockPos localPos) {
			int x = localPos.x, y = localPos.y, z = localPos.z;
			bool inX = x >= 0 && x < 16;
			bool inY = y >= 0 && y < 16;
			bool inZ = z >= 0 && z < 16;
			if (inX && inY && inZ) return this.GetBlock(localPos);
			if (x <  0 && inY && inZ) return neighbourNX.GetBlock(new BlockPos(15, y, z));
			if (x > 15 && inY && inZ) return neighbourPX.GetBlock(new BlockPos(0, y, z));
			if (y <  0 && inX && inZ) return neighbourNY.GetBlock(new BlockPos(x, 15, z));
			if (y > 15 && inX && inZ) return neighbourPY.GetBlock(new BlockPos(x, 0, z));
			if (z <  0 && inX && inY) return neighbourNZ.GetBlock(new BlockPos(x, y, 15));
			if (z > 15 && inX && inY) return neighbourPZ.GetBlock(new BlockPos(x, y, 0));

			return this.world.GetBlock(x + this.cx * 16, y + this.cy * 16, z + this.cz * 16);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool IsVisiblySolidClean(BlockPos localPos) {
			return this.IsVisiblySolid(GetBlockNeighbour(localPos));
		}

		(byte, byte, byte, byte) GetAO(BlockPos pos, FaceBasis faceBasis) {
			BlockPos front = pos + faceBasis.Front;

			int mask = 0;

			if (IsVisiblySolidClean(front + faceBasis.Up - faceBasis.Right)) mask |= 1;
			if (IsVisiblySolidClean(front + faceBasis.Up)) mask |= 2;
			if (IsVisiblySolidClean(front + faceBasis.Up + faceBasis.Right)) mask |= 4;
			if (IsVisiblySolidClean(front + faceBasis.Right)) mask |= 8;
			if (IsVisiblySolidClean(front - faceBasis.Up + faceBasis.Right)) mask |= 16;
			if (IsVisiblySolidClean(front - faceBasis.Up)) mask |= 32;
			if (IsVisiblySolidClean(front - faceBasis.Up - faceBasis.Right)) mask |= 64;
			if (IsVisiblySolidClean(front - faceBasis.Right)) mask |= 128;

			return Preload.AmbientOcclusionVertexLUT[mask];
		}

		for (int z = 0; z < 16; z++) {
			for (int x = 0; x < 16; x++) {
				for (int y = 0; y < 16; y++) {
					BlockPos pos = new BlockPos(x, y, z);
					BlockType type = this.GetBlock(pos);
					if (type.Type == 0) continue;

					Block block = BlockRegistry.GetBlock(type.Type);
					if (!block.data.Visible) continue;
					if (block.data.RenderLayer != renderLayer) continue;

					BlockStateItem[] models = block.data.BlockStateData.GetModels(type, pos);

					foreach (BlockStateItem model in models)
					foreach (Model.Quad quad in model.model.quads) {
						Direction rotatedDirection = Preload.Rotate(quad.direction, model.rotationX, model.rotationY, model.rotationZ);
						Direction rotatedCullFace = quad.cullFace == Direction.None ? Direction.None : Preload.Rotate(quad.cullFace, model.rotationX, model.rotationY, model.rotationZ);

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

							if (this.Occludes(neighborBlock, block.data)) {
								continue;
							}
						}

						byte lightValue = rotatedCullFace switch {
							Direction.West  => (byte) (x > 0 ? this.GetBlockLight(x - 1, y, z) : neighbourNX.GetBlockLight(15, y, z)),
							Direction.East  => (byte) (x < 15 ? this.GetBlockLight(x + 1, y, z) : neighbourPX.GetBlockLight(0, y, z)),
							Direction.Down  => (byte) (y > 0 ? this.GetBlockLight(x, y - 1, z) : neighbourNY.GetBlockLight(x, 15, z)),
							Direction.Up    => (byte) (y < 15 ? this.GetBlockLight(x, y + 1, z) : neighbourPY.GetBlockLight(x, 0, z)),
							Direction.North => (byte) (z > 0 ? this.GetBlockLight(x, y, z - 1) : neighbourNZ.GetBlockLight(x, y, 15)),
							Direction.South => (byte) (z < 15 ? this.GetBlockLight(x, y, z + 1) : neighbourPZ.GetBlockLight(x, y, 0)),
							_ => (byte) this.GetBlockLight(x, y, z)
						};
						if (quad.lightEmisson >= 0)
							lightValue = (byte) quad.lightEmisson;

						byte PackedLight = (byte)(lightValue << 4);

						FaceBasis faceBasis = Preload.FaceBases[(int) quad.direction];
						(byte ao0, byte ao1, byte ao2, byte ao3) = GetAO(pos, faceBasis.Rotated(model.rotationX, model.rotationY, model.rotationZ));
						EnsureIndexCapacity(indexPtr + 6);
						if (ao0 + ao3 < ao1 + ao2) {
							this.indices[indexPtr++] = vertexIndex + 0;
							this.indices[indexPtr++] = vertexIndex + 1;
							this.indices[indexPtr++] = vertexIndex + 2;
							this.indices[indexPtr++] = vertexIndex + 2;
							this.indices[indexPtr++] = vertexIndex + 1;
							this.indices[indexPtr++] = vertexIndex + 3;
						} else {
							this.indices[indexPtr++] = vertexIndex + 0;
							this.indices[indexPtr++] = vertexIndex + 1;
							this.indices[indexPtr++] = vertexIndex + 3;
							this.indices[indexPtr++] = vertexIndex + 3;
							this.indices[indexPtr++] = vertexIndex + 2;
							this.indices[indexPtr++] = vertexIndex + 0;
						}

						Vector3 modelPos = new Vector3(x, y, z);
						Vector3 v0 = RotateModelPos(quad.v0, model.rotationX, model.rotationY, model.rotationZ) + modelPos;
						Vector3 v1 = RotateModelPos(quad.v1, model.rotationX, model.rotationY, model.rotationZ) + modelPos;
						Vector3 v2 = RotateModelPos(quad.v2, model.rotationX, model.rotationY, model.rotationZ) + modelPos;
						Vector3 v3 = RotateModelPos(quad.v3, model.rotationX, model.rotationY, model.rotationZ) + modelPos;

						int color = quad.tintIndex == -1 ? -1 : BlockColors.GetColor(type, quad.tintIndex);
						AddVertex(v0, quad.uv0, (byte) (ao0 | PackedLight), color);
						AddVertex(v1, quad.uv1, (byte) (ao1 | PackedLight), color);
						AddVertex(v2, quad.uv2, (byte) (ao2 | PackedLight), color);
						AddVertex(v3, quad.uv3, (byte) (ao3 | PackedLight), color);
					}
				}
			}
		}

		if (renderLayer == RenderLayer.Opaque)
			this.opaqueIndexCount = indexPtr;
		else if (renderLayer == RenderLayer.Transparent)
			this.transparentIndexCount = indexPtr;
	}

	public unsafe void GenerateMesh() {
		Program.gl.BindVertexArray(this._opaqueVao);

		this.MeshData(RenderLayer.Opaque);

		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, this._opaqueVbo);
		fixed (Vertex* buf = CollectionsMarshal.AsSpan(this.vertices)) {
			Program.gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (this.vertices.Count * Unsafe.SizeOf<Vertex>()), buf, BufferUsageARB.StaticDraw);
		}

		Program.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, this._opaqueEbo);
		fixed (uint* buf = this.indices) {
			Program.gl.BufferData(
				BufferTargetARB.ElementArrayBuffer,
				this.opaqueIndexCount * sizeof(uint),
				buf,
				BufferUsageARB.StaticDraw
			);
		}

		Program.gl.BindVertexArray(0);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
		Program.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);



		Program.gl.BindVertexArray(this._transparentVao);

		this.MeshData(RenderLayer.Transparent);

		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, this._transparentVbo);
		fixed (Vertex* buf = CollectionsMarshal.AsSpan(this.vertices)) {
			Program.gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (this.vertices.Count * Unsafe.SizeOf<Vertex>()), buf, BufferUsageARB.StaticDraw);
		}

		Program.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, this._transparentEbo);
		fixed (uint* buf = this.indices) {
			Program.gl.BufferData(
				BufferTargetARB.ElementArrayBuffer,
				this.transparentIndexCount * sizeof(uint),
				buf,
				BufferUsageARB.StaticDraw
			);
		}

		Program.gl.BindVertexArray(0);
		Program.gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
		Program.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

		this.meshed = true;
	}

	public unsafe void Render(RenderLayer renderLayer) {
		if (!this.meshed) return;

		Preload.Basic.SetUniform("uChunkOffset", this.cx * 16f, this.cy * 16f, this.cz * 16f);

		if (renderLayer == RenderLayer.Opaque) {
			Program.gl.BindVertexArray(this._opaqueVao);
			Program.gl.DrawElements(PrimitiveType.Triangles, this.opaqueIndexCount, DrawElementsType.UnsignedInt, (void*) 0);
		}
		else {
			Program.gl.BindVertexArray(this._transparentVao);
			Program.gl.DrawElements(PrimitiveType.Triangles, this.transparentIndexCount, DrawElementsType.UnsignedInt, (void*) 0);
		}
	}

	public void QueueRefresh() {
		if (this.meshNeedsRefresh)
			return;

		this.meshNeedsRefresh = true;
		this.world.chunksToRemesh.Add(this);
	}
#endregion
}
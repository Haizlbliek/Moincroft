global using BlockId = uint;
using System.Runtime.CompilerServices;

namespace Moincroft.Definitions.Blocks;

public readonly struct BlockData {
	public readonly bool Visible;
	public readonly bool Opaque;
	public readonly FaceUv Px, Nx, Py, Ny, Pz, Nz;

	public BlockData(FaceUv faces, bool visible = true, bool opaque = true) {
		this.Visible = visible;
		this.Opaque = opaque;
		this.Px = this.Nx = this.Py = this.Ny = this.Pz = this.Nz = faces;
	}

	public BlockData(FaceUv px, FaceUv nx, FaceUv py, FaceUv ny, FaceUv pz, FaceUv nz, bool visible = true, bool opaque = true) {
		this.Visible = visible;
		this.Opaque = opaque;
		this.Px = px;
		this.Nx = nx;
		this.Py = py;
		this.Ny = ny;
		this.Pz = pz;
		this.Nz = nz;
	}
}

public static class BlockRegistry {
	private static BlockData[] _blocks = new BlockData[256];
	public static int Count { get; private set; }

	public static BlockId Register(string id, BlockData data, uint forcedId) {
		if (forcedId >= _blocks.Length) {
			Array.Resize(ref _blocks, (int) (forcedId + 256));
		}

		_blocks[forcedId] = data;

		if (forcedId >= Count) Count = (int)forcedId + 1;

		return forcedId;
	}

	public static BlockId Register(string id, BlockData data) {
		uint index = (uint)Count++;
		if (index >= _blocks.Length) Array.Resize(ref _blocks, _blocks.Length * 2);
		_blocks[index] = data;
		return index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref readonly BlockData Get(BlockId id) {
		return ref _blocks[id];
	}
}

public static class Blocks {
	public static readonly BlockId AIR = BlockRegistry.Register("air", new BlockData(default, false, false), forcedId: 0);
	public static readonly BlockId COBBLESTONE = BlockRegistry.Register("cobblestone", new BlockData(faces: Atlas.GetFace("cobblestone")));
	public static readonly BlockId STONE = BlockRegistry.Register("stone", new BlockData(faces: Atlas.GetFace("stone")));
	public static readonly BlockId DIRT = BlockRegistry.Register("dirt", new BlockData(faces: Atlas.GetFace("dirt")));

	public static void Initialize() {}
}
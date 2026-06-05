using System.Diagnostics.CodeAnalysis;

namespace Moincroft.World;

public readonly struct BlockType : IEquatable<BlockType> {
	public readonly BlockId Type;
	public readonly ushort State;

	public BlockType(BlockId type, ushort state) {
		this.Type = type;
		this.State = state;
	}

	public readonly bool Equals(BlockType other) => this.Type == other.Type && this.State == other.State;

	public override readonly int GetHashCode() => (this.Type << 16) | this.State;

	public override bool Equals([NotNullWhen(true)] object? obj) => obj is BlockType other && this.Equals(other);
}
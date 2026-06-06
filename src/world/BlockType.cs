using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Moincroft.World;

public readonly struct BlockType : IEquatable<BlockType> {
	public readonly BlockId Type;
	private readonly BlockStateId StateId;

	public readonly PropertyState State => BlockRegistry.GetBlock(this.Type).GetState(this.StateId);

	public BlockType(BlockId type, BlockStateId state) {
		this.Type = type;
		this.StateId = state;
	}

	public BlockType With<T>(Property<T> property, T value) where T : notnull {
		Block block = BlockRegistry.GetBlock(this.Type);
		int newValueIndex = property.GetIndexOf(value);
		BlockStateId newStateId = block.ModifyState(this.StateId, property, newValueIndex);
		return new BlockType(this.Type, newStateId);
	}

	public BlockType With(Property property, int valueIndex) {
		Block block = BlockRegistry.GetBlock(this.Type);
		BlockStateId newStateId = block.ModifyState(this.StateId, property, valueIndex);
		return new BlockType(this.Type, newStateId);
	}

	public T Get<T>(Property<T> property) where T : notnull {
		return this.State.Get(property);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool Equals(BlockType other) => this.Type == other.Type && this.StateId == other.StateId;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override readonly int GetHashCode() => (this.Type << 16) | this.StateId;

	public override bool Equals([NotNullWhen(true)] object? obj) => obj is BlockType other && this.Equals(other);
	public static bool operator ==(BlockType left, BlockType right) => left.Equals(right);
	public static bool operator !=(BlockType left, BlockType right) => !left.Equals(right);
}
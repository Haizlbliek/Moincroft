using System.Numerics;
using System.Runtime.CompilerServices;
using Moincroft.Definitions;

namespace Moincroft.World;

public class PalettedContainer<T> where T : IEquatable<T> {
	private T[] palette = [];
	private ulong[] data = [];

	private int bitsPerItem = 0;
	private int valuesPerDatum = 0;
	private ulong itemMask = 0ul;

	public PalettedContainer(T fill) {
		this.palette = [ fill ];
		this.SetupBits();
	}

	private void SetupBits() {
		this.bitsPerItem = Math.Max(BitOperations.Log2((uint) this.palette.Length - 1) + 1, 4);
		this.valuesPerDatum = 64 / this.bitsPerItem;
		this.itemMask = (1ul << this.bitsPerItem) - 1;
		this.data = new ulong[(4095 + this.valuesPerDatum) / this.valuesPerDatum];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int BlockIndex(BlockPos pos) => (pos.x << 8) | (pos.y << 4) | pos.z;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private (int idx, int datum) Index(BlockPos pos) {
		int bIndex = BlockIndex(pos);
		return (bIndex / this.valuesPerDatum, bIndex % this.valuesPerDatum);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetDatum(BlockPos pos) {
		(int idx, int datum) = this.Index(pos);
		return (int) ((this.data[idx] >> (this.bitsPerItem * datum)) & this.itemMask);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetDatum(BlockPos pos, int value) {
		(int idx, int datum) = this.Index(pos);
		int shift = this.bitsPerItem * datum;
		this.data[idx] = this.data[idx] & ~(this.itemMask << shift) | ((ulong) value << shift);
	}

	private void ResizeAndRepack() {
		ulong[] oldData = this.data;
		int oldBitsPerItem = this.bitsPerItem;
		int oldValuesPerDatum = this.valuesPerDatum;
		ulong oldItemMask = this.itemMask;
		this.SetupBits();

		for (int i = 0; i < 4096; i++) {
			int oldIdx = i / oldValuesPerDatum;
			int oldDatum = i % oldValuesPerDatum;
			int newIdx = i / this.valuesPerDatum;
			int newDatum = i % this.valuesPerDatum;
			ulong value = (oldData[oldIdx] >> (oldBitsPerItem * oldDatum)) & oldItemMask;
			int newShift = this.bitsPerItem * newDatum;
			this.data[newIdx] = (this.data[newIdx] & ~(this.itemMask << newShift)) | (value << newShift);
		}
	}

	public T Get(BlockPos pos) {
		return this.palette[this.GetDatum(pos)];
	}

	public void Set(BlockPos pos, T value) {
		int paletteIndex = Array.IndexOf(this.palette, value);

		if (paletteIndex == -1) {
			int oldLength = this.palette.Length;
			Array.Resize(ref this.palette, oldLength + 1);
			this.palette[oldLength] = value;
			paletteIndex = oldLength;

			if ((ulong) paletteIndex > this.itemMask) {
				this.ResizeAndRepack();
			}
		}

		this.SetDatum(pos, paletteIndex);
	}
}
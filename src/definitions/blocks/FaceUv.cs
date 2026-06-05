namespace Moincroft.Definitions.Blocks;

public readonly struct FaceUv : IEquatable<FaceUv> {
	public readonly float X;
	public readonly float Y;
	public readonly float Width;
	public readonly float Height;

	public FaceUv(float x, float y, float w, float h) {
		this.X = x;
		this.Y = y;
		this.Width = w;
		this.Height = h;
	}

	public bool Equals(FaceUv other) => this.X == other.X && this.Y == other.Y && this.Width == other.Width && this.Height == other.Height;

	public override bool Equals(object? obj) => obj is FaceUv other && this.Equals(other);

	public override int GetHashCode() {
		return HashCode.Combine(this.X, this.Y, this.Width, this.Height);
	}

	public static bool operator ==(FaceUv left, FaceUv right) => left.Equals(right);
	public static bool operator !=(FaceUv left, FaceUv right) => !left.Equals(right);

	public override string ToString() => $"({this.X} {this.Y} {this.Width}x{this.Height})";
}
namespace Moincroft.Definitions.Blocks;

public readonly struct FaceUv : IEquatable<FaceUv> {
	public readonly float X;
	public readonly float Y;
	public readonly float W;
	public readonly float H;

	public float U => this.X;
	public float V => this.Y;
	public float Width => this.W;
	public float Height => this.H;

	public FaceUv(float x, float y, float w, float h) {
		this.X = x;
		this.Y = y;
		this.W = w;
		this.H = h;
	}

	public bool Equals(FaceUv other) => this.X == other.X && this.Y == other.Y && this.W == other.W && this.H == other.H;

	public override bool Equals(object? obj) => obj is FaceUv other && this.Equals(other);

	public override int GetHashCode() {
		return HashCode.Combine(this.X, this.Y, this.W, this.H);
	}

	public static bool operator ==(FaceUv left, FaceUv right) => left.Equals(right);
	public static bool operator !=(FaceUv left, FaceUv right) => !left.Equals(right);

	public override string ToString() => $"({this.X} {this.Y} {this.W}x{this.H})";
}
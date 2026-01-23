namespace Moincroft.Blocks;

public readonly struct FaceUv {
	public readonly float x;
	public readonly float y;
	public readonly float w;
	public readonly float h;

	public FaceUv(float x, float y, float w, float h) {
		this.x = x;
		this.y = y;
		this.w = w;
		this.h = h;
	}

	public override string ToString() {
		return "(" + this.x + "  " + this.y + "  " + this.w + "x" + this.h + ")";
	}
}
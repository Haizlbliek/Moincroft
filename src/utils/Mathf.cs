namespace Moincroft.Utils;

public static class Mathf {
	public const float PI = 3.14159265359f;

	public static float Clamp(float value, float min, float max) {
		if (value < min) return min;
		if (value > max) return max;
		return value;
	}

	public static float Lerp(float a, float b, float t) {
		return a + (b - a) * t;
	}

	public static double Lerp(double a, double b, double t) {
		return a + (b - a) * t;
	}

	public static float Sqrt(float value) {
		return MathF.Sqrt(value);
	}

	public static float Sin(float value) {
		return MathF.Sin(value);
	}

	public static float Cos(float value) {
		return MathF.Cos(value);
	}

	public static double Sin(double value) {
		return Math.Sin(value);
	}

	public static double Cos(double value) {
		return Math.Cos(value);
	}

	public static int FloorDivide(int a, int b) {
		return ((a < 0) ^ (b < 0)) && (a % b != 0) ? (a / b - 1) : (a / b);
	}

	public static int Mod(int x, int v) {
		return ((x % v) + v) % v;
	}
}
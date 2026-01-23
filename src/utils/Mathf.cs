using System.Runtime.CompilerServices;

namespace Moincroft.Utils;

public static class Mathf {
	public const float PI = 3.14159265359f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Clamp(float value, float min, float max) {
		if (value < min) return min;
		if (value > max) return max;
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Lerp(float a, float b, float t) {
		return a + (b - a) * t;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Lerp(double a, double b, double t) {
		return a + (b - a) * t;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Sqrt(float value) {
		return MathF.Sqrt(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Sin(float value) {
		return MathF.Sin(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Cos(float value) {
		return MathF.Cos(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Sin(double value) {
		return Math.Sin(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Cos(double value) {
		return Math.Cos(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FloorDivide(int a, int b) {
		return ((a < 0) ^ (b < 0)) && (a % b != 0) ? (a / b - 1) : (a / b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FloorToInt(float a) {
		return (int) MathF.Floor(a);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Mod(int x, int v) {
		return ((x % v) + v) % v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Mod(float x, float v) {
		return ((x % v) + v) % v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Mod1(float x) {
		return ((x % 1f) + 1f) % 1f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Abs(float v) {
		return Math.Abs(v);
	}
}
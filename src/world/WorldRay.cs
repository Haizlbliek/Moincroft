namespace Moincroft.Utils;

public class WorldRay {
	public static bool Cast(World.World world, Vector3 origin, Vector3 direction, float maxDistance, out WorldRayResult result) {
		result = default;

		int x = Mathf.FloorToInt(origin.x);
		int y = Mathf.FloorToInt(origin.y);
		int z = Mathf.FloorToInt(origin.z);

		int stepX = direction.x > 0 ? 1 : -1;
		int stepY = direction.y > 0 ? 1 : -1;
		int stepZ = direction.z > 0 ? 1 : -1;

		float deltaX = direction.x == 0f ? float.PositiveInfinity : Math.Abs(1f / direction.x);
		float deltaY = direction.y == 0f ? float.PositiveInfinity : Math.Abs(1f / direction.y);
		float deltaZ = direction.z == 0f ? float.PositiveInfinity : Math.Abs(1f / direction.z);

		float maxT_X = ((direction.x > 0f) ? (x + 1 - origin.x) : (origin.x - x)) * deltaX;
		float maxT_Y = ((direction.y > 0f) ? (y + 1 - origin.y) : (origin.y - y)) * deltaY;
		float maxT_Z = ((direction.z > 0f) ? (z + 1 - origin.z) : (origin.z - z)) * deltaZ;

		while (true) {
			float currentT;
			if (maxT_X < maxT_Y) {
				currentT = maxT_X < maxT_Z ? maxT_X : maxT_Z;
			} else {
				currentT = maxT_Y < maxT_Z ? maxT_Y : maxT_Z;
			}

			if (currentT > maxDistance) break;

			if (maxT_X < maxT_Y && maxT_X < maxT_Z) {
				x += stepX;
				maxT_X += deltaX;
				result.normal = new Vector3i(-stepX, 0, 0);
			}
			else if (maxT_Y < maxT_Z) {
				y += stepY;
				maxT_Y += deltaY;
				result.normal = new Vector3i(0, -stepY, 0);
			}
			else {
				z += stepZ;
				maxT_Z += deltaZ;
				result.normal = new Vector3i(0, 0, -stepZ);
			}

			if (world.GetBlock(x, y, z).Type != 0) {
				result.blockPosition = new Vector3i(x, y, z);
				return true;
			}
		}

		return false;
	}
}

public struct WorldRayResult {
	public Vector3i blockPosition;
	public Vector3i normal;
}
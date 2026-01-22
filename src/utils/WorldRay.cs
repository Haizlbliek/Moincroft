namespace Moincroft.Utils;

public class WorldRay : Ray {
	public World.World world;

	public WorldRay(World.World world, Vector3 origin, Vector3 endPoint) : base(origin, endPoint) {
		this.world = world;
	}

	public WorldRay(World.World world, Vector3 origin, Vector3 direction, float maxDistance) : base(origin, direction, maxDistance) {
		this.world = world;
	}

	public WorldRayResult? Cast() {
		float epsilon = 0f;
		float maxX = ((this.direction.x < 0f ? 0f : 1f) - Mathf.Mod1(this.origin.x) + epsilon) / this.direction.x;
		float maxY = ((this.direction.y < 0f ? 0f : 1f) - Mathf.Mod1(this.origin.y) + epsilon) / this.direction.y;
		float maxZ = ((this.direction.z < 0f ? 0f : 1f) - Mathf.Mod1(this.origin.z) + epsilon) / this.direction.z;

		float deltaX = Math.Sign(this.direction.x) / this.direction.x;
		float deltaY = Math.Sign(this.direction.y) / this.direction.y;
		float deltaZ = Math.Sign(this.direction.z) / this.direction.z;

		int blockX = Mathf.FloorToInt(this.origin.x);
		int blockY = Mathf.FloorToInt(this.origin.y);
		int blockZ = Mathf.FloorToInt(this.origin.z);

		Vector3i normal = Vector3i.Zero;

		for (int steps = 0; steps < 150; steps++) {
			if (this.world.GetBlock(blockX, blockY, blockZ) != 0) {
				return new WorldRayResult() { blockPosition = new Vector3i(blockX, blockY, blockZ), normal = normal };
			}

			if (maxX < maxY && maxX < maxZ) {
				blockX += Math.Sign(this.direction.x);
				maxX += deltaX;
				normal = new Vector3i(-Math.Sign(this.direction.x), 0, 0);
			} else if (maxY < maxZ) {
				blockY += Math.Sign(this.direction.y);
				maxY += deltaY;
				normal = new Vector3i(0, -Math.Sign(this.direction.y), 0);
			} else {
				blockZ += Math.Sign(this.direction.z);
				maxZ += deltaZ;
				normal = new Vector3i(0, 0, -Math.Sign(this.direction.z));
			}
		}

		return null;
	}
}

public struct WorldRayResult {
	public Vector3i blockPosition;
	public Vector3i normal;
}
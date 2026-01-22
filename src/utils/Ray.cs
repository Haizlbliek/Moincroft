namespace Moincroft.Utils;

public abstract class Ray {
	public Vector3 origin;
	public Vector3 direction;
	public float maxDistance = 1000f;

	public Ray(Vector3 origin, Vector3 endPoint) {
		this.origin = origin;
		this.To = endPoint;
	}

	public Ray(Vector3 origin, Vector3 direction, float maxDistance) {
		this.origin = origin;
		this.direction = direction;
		this.maxDistance = maxDistance;
	}

	public Vector3 To {
		get {
			return this.direction * this.maxDistance;
		}

		set {
			Vector3 to = value - this.origin;
			this.direction = to.Normalized;
			this.maxDistance = to.Length;
		}
	}
}
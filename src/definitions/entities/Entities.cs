global using EntityId = uint;
using System.Runtime.CompilerServices;

namespace Moincroft.Definitions.Entities;

public readonly struct EntityType {
	public readonly string Name;
	public readonly Func<Entity> Factory;

	public EntityType(string name, Func<Entity> factory) {
		this.Name = name;
		this.Factory = factory;
	}
}

public static class EntityRegistry {
	private static EntityType[] _types = new EntityType[64];
	public static int Count { get; private set; }

	public static EntityId Register(string name, Func<Entity> factory) {
		uint index = (uint) Count++;
		if (index >= _types.Length) Array.Resize(ref _types, _types.Length * 2);
		_types[index] = new EntityType(name, factory);
		return index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Entity Spawn(EntityId id) {
		return _types[id].Factory();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref readonly EntityType Get(EntityId id) {
		return ref _types[id];
	}
}

public static class Entities {
	public static readonly EntityId PLAYER = EntityRegistry.Register("player", () => new Player());

	public static void Initialize() {}
}
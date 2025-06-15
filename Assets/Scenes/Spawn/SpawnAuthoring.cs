using Unity.Entities;
using UnityEngine;

public class SpawnAuthoring : MonoBehaviour
{
    public GameObject Prefab;

    public uint Num;
}

public struct Spawn : IComponentData
{
    public struct Initializer : IComponentData { }

    public Entity Prefab;

    public uint Num;
}

class SpawnBaker : Baker<SpawnAuthoring>
{
    public override void Bake(SpawnAuthoring authoring)
    {
        if (authoring.Prefab == null)
        {
            return;
        }

        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new Spawn
        {
            Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
            Num = authoring.Num
        });
        AddComponent(entity, new Spawn.Initializer());
    }
}

public partial struct SpawnSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<Spawn>())
        {
            return;
        }

        var entity = SystemAPI.GetSingletonEntity<Spawn>();
        if (state.EntityManager.HasComponent<Spawn.Initializer>(entity))
        {
            var spawn = state.EntityManager.GetComponentData<Spawn>(entity);
            for (int i = 0; i < spawn.Num; i++)
            {
                state.EntityManager.Instantiate(spawn.Prefab);
            }
            state.EntityManager.RemoveComponent<Spawn.Initializer>(entity);
        }
    }
}

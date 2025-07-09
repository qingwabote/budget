using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
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
    private Unity.Mathematics.Random m_Random;

    public void OnCreate(ref SystemState state)
    {
        m_Random = new Unity.Mathematics.Random(1);
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<Spawn>())
        {
            return;
        }

        var spawn_entity = SystemAPI.GetSingletonEntity<Spawn>();
        if (state.EntityManager.HasComponent<Spawn.Initializer>(spawn_entity))
        {
            var spawn = state.EntityManager.GetComponentData<Spawn>(spawn_entity);
            for (int i = 0; i < spawn.Num; i++)
            {
                var entity = state.EntityManager.Instantiate(spawn.Prefab);
                SystemAPI.GetComponentRW<LocalTransform>(entity).ValueRW.Position = new float3(m_Random.NextFloat(-3, 4), 0, m_Random.NextFloat(-6, 7));
            }
            state.EntityManager.RemoveComponent<Spawn.Initializer>(spawn_entity);
        }
        else
        {
            var sysHandle = World.DefaultGameObjectInjectionWorld.GetExistingSystem<LocalToWorldSystem>();
            ref var sysState = ref World.DefaultGameObjectInjectionWorld.Unmanaged.ResolveSystemStateRef(sysHandle);
            sysState.Enabled = false;
        }
    }
}

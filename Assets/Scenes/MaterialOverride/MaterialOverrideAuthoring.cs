using System.Collections;
using System.Collections.Generic;
using Graphix;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct MaterialOverride : IComponentData
{
    public struct Initializer : IComponentData { }

    public Entity Prefab;

    public uint Num;
}

public class MaterialOverrideAuthoring : MonoBehaviour
{
    public GameObject Prefab;

    public uint Num;

    class Baker : Baker<MaterialOverrideAuthoring>
    {
        public override void Bake(MaterialOverrideAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new MaterialOverride
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                Num = authoring.Num
            });
            AddComponent(entity, new MaterialOverride.Initializer());
        }
    }
}

[MaterialProperty("_BaseColor")]
struct MaterialColor : IComponentData
{
    public float4 Value;
}

public partial struct MaterialOverrideSystem : ISystem
{
    private Unity.Mathematics.Random m_Random;

    public void OnCreate(ref SystemState state)
    {
        m_Random = new Unity.Mathematics.Random(2);
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<MaterialOverride>())
        {
            return;
        }

        var MaterialOverrideEntity = SystemAPI.GetSingletonEntity<MaterialOverride>();
        if (state.EntityManager.HasComponent<MaterialOverride.Initializer>(MaterialOverrideEntity))
        {
            var spawn = state.EntityManager.GetComponentData<MaterialOverride>(MaterialOverrideEntity);
            for (int i = 0; i < spawn.Num; i++)
            {
                var entity = state.EntityManager.Instantiate(spawn.Prefab);
                state.EntityManager.AddComponentData(entity, new MaterialColor { Value = new float4(m_Random.NextFloat(0, 1), m_Random.NextFloat(0, 1), m_Random.NextFloat(0, 1), 1.0f) });
                SystemAPI.GetComponentRW<LocalTransform>(entity).ValueRW.Position = new float3(m_Random.NextFloat(-3, 3), m_Random.NextFloat(-3, 3), m_Random.NextFloat(-3, 3));
            }
            state.EntityManager.RemoveComponent<MaterialOverride.Initializer>(MaterialOverrideEntity);
        }
        else
        {
            var sysHandle = World.DefaultGameObjectInjectionWorld.GetExistingSystem<LocalToWorldSystem>();
            ref var sysState = ref World.DefaultGameObjectInjectionWorld.Unmanaged.ResolveSystemStateRef(sysHandle);
            sysState.Enabled = false;
        }
    }
}

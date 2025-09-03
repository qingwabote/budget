using System.Collections;
using System.Collections.Generic;
using Graphix;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct SkinningSpawn : IComponentData
{
    public struct Initializer : IComponentData { }

    public Entity Prefab;

    public uint Num;
}

public class SkinningSpawnAuthoring : MonoBehaviour
{
    public GameObject Prefab;

    public uint Num;

    public class Baker : Baker<SkinningSpawnAuthoring>
    {
        public override void Bake(SkinningSpawnAuthoring authoring)
        {
            if (authoring.Prefab == null)
            {
                return;
            }

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SkinningSpawn
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                Num = authoring.Num
            });
            AddComponent(entity, new SkinningSpawn.Initializer());
        }
    }
}

public partial struct SkinningSpawnSystem : ISystem
{
    private Unity.Mathematics.Random m_Random;

    public void OnCreate(ref SystemState state)
    {
        m_Random = new Unity.Mathematics.Random(2);
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<SkinningSpawn>())
        {
            return;
        }

        var skinningSpawnEntity = SystemAPI.GetSingletonEntity<SkinningSpawn>();
        if (state.EntityManager.HasComponent<SkinningSpawn.Initializer>(skinningSpawnEntity))
        {
            var spawn = state.EntityManager.GetComponentData<SkinningSpawn>(skinningSpawnEntity);
            for (int i = 0; i < spawn.Num; i++)
            {
                var entity = state.EntityManager.Instantiate(spawn.Prefab);
                SystemAPI.GetComponentRW<LocalTransform>(entity).ValueRW.Position = new float3(m_Random.NextInt(-3, 4), 0, m_Random.NextInt(-6, 7));
                SystemAPI.GetComponentRW<Graphix.AnimationState>(entity).ValueRW.ClipIndex = m_Random.NextInt(0, 3);
            }
            state.EntityManager.RemoveComponent<SkinningSpawn.Initializer>(skinningSpawnEntity);
        }
    }
}

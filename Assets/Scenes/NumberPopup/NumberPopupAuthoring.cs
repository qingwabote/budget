using Graphix;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public struct NumberPopup : IComponentData
{
    public struct Initializer : IComponentData { }

    public Entity Prefab;

    public int Num;
}

public class NumberPopupAuthoring : MonoBehaviour
{
    public GameObject Prefab;

    public int Num;

    class NumberPopupBaker : Baker<NumberPopupAuthoring>
    {
        public override void Bake(NumberPopupAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new NumberPopup
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                Num = authoring.Num
            });
            AddComponent(entity, new NumberPopup.Initializer());
        }
    }
}

public partial struct NumberPopupSystem : ISystem
{
    private Unity.Mathematics.Random m_Random;

    public void OnCreate(ref SystemState state)
    {
        m_Random = new Unity.Mathematics.Random(1);
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<NumberPopup>())
        {
            return;
        }

        var NumberPopupEntity = SystemAPI.GetSingletonEntity<NumberPopup>();
        if (state.EntityManager.HasComponent<NumberPopup.Initializer>(NumberPopupEntity))
        {
            var spawn = state.EntityManager.GetComponentData<NumberPopup>(NumberPopupEntity);
            for (int i = 0; i < spawn.Num; i++)
            {
                var entity = state.EntityManager.Instantiate(spawn.Prefab);
                SystemAPI.GetComponentRW<LocalTransform>(entity).ValueRW.Position = new float3(m_Random.NextFloat(-170, 170), m_Random.NextFloat(-426, 380), 0);
            }
            state.EntityManager.RemoveComponent<NumberPopup.Initializer>(NumberPopupEntity);
        }
        else
        {
            foreach (var (mms, indexes, colors, offsets) in SystemAPI.Query<DynamicBuffer<MaterialMeshInfoBuffered>, DynamicBuffer<NumberIndex>, DynamicBuffer<NumberColor>, DynamicBuffer<NumberOffset>>())
            {
                var mm = mms[0];
                mms.Clear();
                var color = colors[0];
                colors.Clear();

                indexes.Clear();
                offsets.Clear();
                var number = m_Random.NextInt(0, 99);
                do
                {
                    indexes.Add(new() { Value = number % 10 });
                    number /= 10;
                } while (number != 0);

                var w = 0.5f;
                for (int i = 0; i < indexes.Length; i++)
                {
                    var offset = -(w * i) - w / 2 + w * indexes.Length / 2;
                    offsets.Add(new() { Value = new float4(offset, 0.5f, 0, 0) });

                    mms.Add(mm);
                    colors.Add(color);
                }
            }
        }
    }
}

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class GlideAuthoring : MonoBehaviour { }

struct Glide : IComponentData
{
    public float3 destination;
    public float speed;
}

class GlideBaker : Baker<GlideAuthoring>
{
    public override void Bake(GlideAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new Glide());
    }
}

public partial struct GlideSystem : ISystem
{
    private int _ProfileEntry;

    private Unity.Mathematics.Random _Random;

    public void OnCreate(ref SystemState state)
    {
        // _ProfileEntry = Profile.DefineEntry("Glide");

        _Random = new Unity.Mathematics.Random(1);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Profile.Begin(_ProfileEntry);

        foreach (var (transform, dirft) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<Glide>>())
        {
            var d = dirft.ValueRW.destination - transform.ValueRW.Position;
            if (math.length(d) < 1)
            {
                dirft.ValueRW.destination = new float3(_Random.NextInt(-3, 4), 0, _Random.NextInt(-6, 7));
                continue;
            }

            // Quaternion.LookRotation(d);
            float3 x = math.normalize(math.cross(math.up(), d));
            var rot = math.quaternion(math.float3x3(x, math.cross(d, x), d));

            transform.ValueRW.Rotation = math.slerp(transform.ValueRW.Rotation, rot, 0.05f);

            var move = math.mul(transform.ValueRW.Rotation, new float3(0, 0, 0.05f));
            transform.ValueRW.Position += move;
        }

        // if (IsBursted())
        // {
        //     Debug.Log("IsBursted true");
        // }
        // else
        // {
        //     Debug.Log("IsBursted false");
        // }

        // Profile.End(_ProfileEntry);
    }

    // https://discussions.unity.com/t/when-where-and-why-to-put-burstcompile-with-mild-under-the-hood-explanation/896228
    [BurstDiscard]
    void SetFalseIfUnBursted(ref bool val)
    {
        val = false;
    }
    bool IsBursted()
    {
        bool ret = true;
        SetFalseIfUnBursted(ref ret);
        return ret;
    }
}

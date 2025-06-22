using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Budget
{
    [UpdateBefore(typeof(Solo))]
    partial struct SkinnedAnimationFilter : ISystem
    {
        // [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (info, joint) in SystemAPI.Query<SkinInfoComponent, RefRW<SkinJoint>>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                var offset = info.Value.Store.Add();
                // memory may be reallocated after Add();
                unsafe
                {
                    joint.ValueRW.Matrices = (long)(info.Value.Store.Source + offset);
                }
                info.Value.Offset = offset;
            }
        }
    }

    [UpdateAfter(typeof(Solo))]
    partial struct SkinnedAnimationUpdater : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (nodes, joint) in SystemAPI.Query<DynamicBuffer<SkinNode>, RefRW<SkinJoint>>())
            {
                var matrixes = new NativeArray<float4x4>(nodes.Length, Allocator.Temp);
                for (int i = 0; i < nodes.Length; i++)
                {
                    ref var node = ref nodes.ElementAt(i);
                    var local = SystemAPI.GetComponentRO<LocalTransform>(node.Target);
                    if (node.Parent == -1)
                    {
                        matrixes[i] = float4x4.TRS(local.ValueRO.Position, local.ValueRO.Rotation, local.ValueRO.Scale);
                    }
                    else
                    {
                        matrixes[i] = math.mul(matrixes[node.Parent], float4x4.TRS(local.ValueRO.Position, local.ValueRO.Rotation, local.ValueRO.Scale));
                    }
                }

                ref var inverseBindMatrices = ref joint.ValueRO.InverseBindMatrices.Value.Data;
                var jointOffset = joint.ValueRO.Index;
                unsafe
                {
                    var JointSource = (float4x4*)joint.ValueRO.Matrices;
                    for (int i = 0; i < inverseBindMatrices.Length; i++)
                    {
                        var res = math.mul(matrixes[i + jointOffset], inverseBindMatrices[i]);
                        UnsafeUtility.MemCpy(JointSource + i, &res, UnsafeUtility.SizeOf<float4x4>());
                    }
                }
            }
        }
    }

    [UpdateAfter(typeof(SkinnedAnimationUpdater))]
    partial struct SkinnedAnimationUploader : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var skin in SystemAPI.Query<SkinInfoComponent>())
            {
                skin.Value.Store.Update();
            }
        }
    }
}
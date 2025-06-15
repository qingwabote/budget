using Unity.Entities;
using UnityEngine;

namespace Budget
{
    [UpdateBefore(typeof(Solo))]
    partial struct SkinnedAnimationFilter : ISystem
    {
        // [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (info, joint, _, entity) in SystemAPI.Query<SkinInfo, RefRW<SkinJoint>, DynamicBuffer<ClipBinging>>().WithEntityAccess().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                Debug.Log("SkinnedAnimationFilter");
                SystemAPI.SetBufferEnabled<ClipBinging>(entity, false);
                unsafe
                {
                    joint.ValueRW.StoreSource = info.Store.Source;
                }
            }
        }
    }

    [UpdateAfter(typeof(Solo))]
    partial struct SkinnedAnimationUpdater : ISystem
    {
        // [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (_, entity) in SystemAPI.Query<RefRW<SkinJoint>>().WithEntityAccess())
            {

            }
        }
    }
}
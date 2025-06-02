using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Budget.GLTF
{
    partial struct AnimationSystem : ISystem
    {
        private int _ProfileEntry;

        public void OnCreate(ref SystemState state)
        {
            _ProfileEntry = Profile.DefineEntry("Animation");
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Profile.Begin(_ProfileEntry);

            foreach (var (animation, clipBingings, channelTargets, entity) in SystemAPI.Query<RefRW<Animation>, DynamicBuffer<ClipBinging>, DynamicBuffer<ChannelTarget>>().WithEntityAccess())
            {
                var time = animation.ValueRW.Time;

                ref var clipBinging = ref clipBingings.ElementAt(animation.ValueRO.Index);
                var result = new NativeArray<float>(clipBinging.Outputs, Allocator.Temp);
                unsafe
                {
                    clipBinging.Blob.Value.Sample((float*)result.GetUnsafePtr(), time);
                }

                ref var channels = ref clipBinging.Blob.Value.channels;
                float duration = 0;
                var offset = 0;
                for (int i = 0; i < channels.Length; i++)
                {
                    ref var channel = ref channels[i];
                    var target = channelTargets.ElementAt(clipBinging.TargetIndex + i).Value;
                    switch (channel.path)
                    {
                        case ChannelPath.TRANSLATION:
                            if (target != Entity.Null)
                            {
                                SystemAPI.GetComponentRW<LocalTransform>(target).ValueRW.Position = new float3(result[offset], result[offset + 1], result[offset + 2]);
                            }
                            offset += 3;
                            break;
                        case ChannelPath.ROTATION:
                            if (target != Entity.Null)
                            {
                                SystemAPI.GetComponentRW<LocalTransform>(target).ValueRW.Rotation = new float4(result[offset], result[offset + 1], result[offset + 2], result[offset + 3]);
                            }
                            offset += 4;
                            break;
                        case ChannelPath.SCALE:
                            if (target != Entity.Null)
                            {
                                SystemAPI.GetComponentRW<LocalTransform>(target).ValueRW.Scale = result[offset];
                            }
                            offset += 3;
                            break;
                        default:
                            throw new Exception($"unsupported path: ${channel.path}");
                    }

                    duration = math.max(duration, channel.input[^1]);
                }

                result.Dispose();

                if (time == duration)
                {
                    time = 0;
                }
                else
                {
                    time += SystemAPI.Time.DeltaTime;
                    time = math.min(time, duration);
                }
                animation.ValueRW.Time = time;
            }

            Profile.End(_ProfileEntry);
        }
    }
}
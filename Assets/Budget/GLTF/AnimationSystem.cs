using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Budget.GLTF
{
    public partial struct AnimationMakingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (animation, entity) in SystemAPI.Query<AnimationMaking>().WithEntityAccess())
            {
                var channelTargets = ecb.AddBuffer<ChannelTarget>(entity);
                var clipBindings = ecb.AddBuffer<ClipBinging>(entity);
                int index = 0;
                foreach (var animationClip in animation.Clips)
                {
                    foreach (var node in animationClip.Nodes)
                    {
                        var target = entity;
                        foreach (var name in node.Split("/"))
                        {
                            if (target == Entity.Null)
                            {
                                break;
                            }

                            var children = SystemAPI.GetBuffer<Child>(target);
                            target = Entity.Null;
                            foreach (var child in children)
                            {
                                if (SystemAPI.ManagedAPI.GetComponent<Name>(child.Value).Value == name)
                                {
                                    target = child.Value;
                                    break;
                                }
                            }
                        }
                        channelTargets.Add(new ChannelTarget
                        {
                            Value = target
                        });
                    }

                    int outputs = 0;
                    ref BlobArray<Channel> channels = ref animationClip.Blob.Value.channels;
                    for (int i = 0; i < channels.Length; i++)
                    {
                        switch (channels[i].path)
                        {
                            case ChannelPath.TRANSLATION:
                                outputs += 3;
                                break;
                            case ChannelPath.ROTATION:
                                outputs += 4;
                                break;
                            case ChannelPath.SCALE:
                                outputs += 3;
                                break;
                            default:
                                throw new Exception($"unsupported path: {channels[i].path}");
                        }
                    }

                    clipBindings.Add(new ClipBinging
                    {
                        Blob = animationClip.Blob,
                        TargetIndex = index,
                        Outputs = outputs
                    });

                    index += animation.Clips.Length;
                }
                ecb.AddComponent(entity, new AnimationState
                {
                    Index = animation.Index
                });
                ecb.RemoveComponent<AnimationMaking>(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [UpdateAfter(typeof(AnimationMakingSystem))]
    partial struct AnimationStateSystem : ISystem
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

            foreach (var (animation, clipBingings, channelTargets, entity) in SystemAPI.Query<RefRW<AnimationState>, DynamicBuffer<ClipBinging>, DynamicBuffer<ChannelTarget>>().WithEntityAccess())
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
using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Budget
{
    public struct ChannelTarget : IBufferElementData
    {
        public Entity Value;
    }

    public struct ClipBinging : IBufferElementData, IEnableableComponent
    {
        public BlobAssetReference<Clip> Blob;
        public int TargetIndex;
        public int Outputs;

        public float Duration
        {
            get
            {
                float duration = 0;
                ref var channels = ref Blob.Value.channels;
                for (int i = 0; i < channels.Length; i++)
                {
                    ref var channel = ref channels[i];
                    duration = math.max(duration, channel.input[^1]);
                }
                return duration;
            }
        }
    }

    class AnimationAuthoring : MonoBehaviour
    {
        public AnimationClip[] Clips;
        public int ClipIndex;
    }

    public struct AnimationState : IComponentData
    {
        public int ClipIndex;
        public float Time;
    }

    class AnimationBaker : Baker<AnimationAuthoring>
    {
        public override void Bake(AnimationAuthoring authoring)
        {
            if (authoring.Clips == null)
            {
                return;
            }

            foreach (var clip in authoring.Clips)
            {
                if (clip == null)
                {
                    return;
                }
            }

            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var channelTargets = AddBuffer<ChannelTarget>(entity);
            var clipBindings = AddBuffer<ClipBinging>(entity);
            foreach (var clip in authoring.Clips)
            {
                foreach (var node in clip.Nodes)
                {
                    int outputs = 0;
                    ref BlobArray<Channel> channels = ref clip.Blob.Value.channels;
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
                        Blob = clip.Blob,
                        TargetIndex = channelTargets.Length,
                        Outputs = outputs
                    });

                    var target = authoring.transform;
                    foreach (var name in node.Split("/"))
                    {
                        for (int i = 0; i < target.childCount; i++)
                        {
                            var child = target.GetChild(i);
                            if (child.name == name)
                            {
                                target = child;
                                break;
                            }
                        }
                    }
                    channelTargets.Add(new ChannelTarget
                    {
                        Value = GetEntity(target, TransformUsageFlags.Dynamic)
                    });
                }
            }

            AddComponent(entity, new AnimationState
            {
                ClipIndex = authoring.ClipIndex
            });
        }
    }
}
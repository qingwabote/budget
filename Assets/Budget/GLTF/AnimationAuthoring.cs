using Unity.Entities;
using UnityEngine;

namespace Budget.GLTF
{
    public struct ChannelTarget : IBufferElementData
    {
        public Entity Value;
    }

    public struct ClipBinging : IBufferElementData
    {
        public BlobAssetReference<Clip> Blob;
        public int TargetIndex;
        public int Outputs;
    }


    public class AnimationMaking : IComponentData
    {
        public AnimationClip[] Clips;
        public int Index;
    }

    struct AnimationState : IComponentData
    {
        public int Index;
        public float Time;
    }

    class AnimationAuthoring : MonoBehaviour
    {
        public AnimationClip[] Clips;
        public int Index;
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
            AddComponentObject(entity, new AnimationMaking
            {
                Clips = authoring.Clips,
                Index = authoring.Index
            });
        }
    }
}
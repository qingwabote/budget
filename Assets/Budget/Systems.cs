using Unity.Entities;
using Unity.Transforms;

namespace Budget
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial struct SkinnedAnimationFilter : ISystem { }

    [UpdateInGroup(typeof(TransformSystemGroup)), UpdateBefore(typeof(LocalToWorldSystem)), UpdateAfter(typeof(SkinnedAnimationFilter))]
    public partial class AnimationSamplerGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(TransformSystemGroup)), UpdateAfter(typeof(AnimationSamplerGroup))]
    public partial struct AnimationTimeStepper : ISystem { }

    [UpdateInGroup(typeof(TransformSystemGroup)), UpdateAfter(typeof(AnimationSamplerGroup))]
    public partial struct SkinnedAnimationUpdater : ISystem { }

    [UpdateInGroup(typeof(TransformSystemGroup)), UpdateAfter(typeof(SkinnedAnimationUpdater))]
    public partial struct SkinnedAnimationUploader : ISystem { }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct Batcher : ISystem { }
}
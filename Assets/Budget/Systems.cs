using Unity.Entities;

namespace Budget
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct SkinnedAnimationFilter : ISystem { }

    [UpdateInGroup(typeof(LateSimulationSystemGroup)), UpdateAfter(typeof(SkinnedAnimationFilter))]
    public partial class AnimationSamplerGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(LateSimulationSystemGroup)), UpdateAfter(typeof(AnimationSamplerGroup))]
    public partial struct AnimationTimeStepper : ISystem { }

    [UpdateInGroup(typeof(LateSimulationSystemGroup)), UpdateAfter(typeof(AnimationSamplerGroup))]
    public partial struct SkinnedAnimationUpdater : ISystem { }

    [UpdateInGroup(typeof(LateSimulationSystemGroup)), UpdateAfter(typeof(SkinnedAnimationUpdater))]
    public partial struct SkinnedAnimationUploader : ISystem { }

    [UpdateInGroup(typeof(LateSimulationSystemGroup)), UpdateAfter(typeof(SkinnedAnimationUploader))]
    public partial struct Batcher : ISystem { }
}
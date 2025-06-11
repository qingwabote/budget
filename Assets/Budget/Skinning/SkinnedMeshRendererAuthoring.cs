using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Budget
{
    [BakingType]
    public class SkinnedMeshRendererBaking : IComponentData
    {
        public Mesh Mesh;
        public Material Material;
    }

    public class SkinnedMeshRendererAuthoring : MonoBehaviour { }

    class SkinnedMeshRendererBaker : Baker<SkinnedMeshRendererAuthoring>
    {
        public override void Bake(SkinnedMeshRendererAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var meshRenderer = authoring.GetComponent<UnityEngine.SkinnedMeshRenderer>();
            // if (authoring.gameObject == meshRenderer.rootBone.gameObject)
            // {
            //     var root = CreateAdditionalEntity(TransformUsageFlags.None);
            //     var joints = AddBuffer<SkinJoint>(root);
            //     foreach (var bone in meshRenderer.bones)
            //     {

            //         joints.Add(new SkinJoint
            //         {
            //             Value =
            //         })
            //     }
            // }
            AddComponentObject(entity, new SkinnedMeshRendererBaking
            {
                Mesh = meshRenderer.sharedMesh,
                Material = meshRenderer.sharedMaterial
            });
        }
    }

    public class SkinnedMeshRenderer : IComponentData
    {
        public Mesh Mesh;
        public Material Material;
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    partial struct SkinnedMeshRendererSystem : ISystem
    {
        private int _ProfileEntry;

        public void OnCreate(ref SystemState state)
        {
            _ProfileEntry = Profile.DefineEntry("SkinnedMeshRenderer");
        }

        public void OnUpdate(ref SystemState state)
        {
            Profile.Begin(_ProfileEntry);

            NativeArray<Matrix4x4> instData = new(1, Allocator.Temp);
            var jointsOffset = new float[1];
            jointsOffset[0] = 6;
            var matProps = new MaterialPropertyBlock();
            matProps.SetFloatArray("_JointsOffset", jointsOffset);
            foreach (var (renderer, localToWorld) in SystemAPI.Query<SkinnedMeshRenderer, RefRO<LocalToWorld>>())
            {
                var rp = new RenderParams(renderer.Material)
                {
                    matProps = matProps
                };
                instData[0] = localToWorld.ValueRO.Value;
                Graphics.RenderMeshInstanced(rp, renderer.Mesh, 0, instData, 1);
            }
            instData.Dispose();

            Profile.End(_ProfileEntry);
        }
    }
}


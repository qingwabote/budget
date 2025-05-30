using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Budget
{
    [TemporaryBakingType]
    class SkinnedMeshRendererBaking : IComponentData
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
            AddComponentObject(entity, new SkinnedMeshRendererBaking
            {
                Mesh = meshRenderer.sharedMesh,
                Material = meshRenderer.sharedMaterial
            });
        }
    }

    class SkinnedMeshRenderer : IComponentData
    {
        public Mesh Mesh;
        public Material Material;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    partial struct SkinnedMeshRendererBakingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var materials = new Dictionary<Material, Material>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (renderer, entity) in SystemAPI.Query<SkinnedMeshRendererBaking>().WithEntityAccess())
            {
                Material material;
                if (!materials.TryGetValue(renderer.Material, out material))
                {
                    material = new Material(renderer.Material);
                    material.shader = Shader.Find("Budget/PBRGraph-Universal");
                    materials.Add(renderer.Material, material);
                }
                ecb.AddComponent(entity, new SkinnedMeshRenderer
                {
                    Mesh = renderer.Mesh,
                    Material = material
                });
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
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
            foreach (var (renderer, localToWorld) in SystemAPI.Query<SkinnedMeshRenderer, RefRO<LocalToWorld>>())
            {
                var rp = new RenderParams(renderer.Material);
                // Graphics.RenderMesh(rp, renderer.Mesh, 0, localToWorld.ValueRO.Value);
                instData[0] = localToWorld.ValueRO.Value;
                Graphics.RenderMeshInstanced(rp, renderer.Mesh, 0, instData, 1);
            }
            instData.Dispose();

            Profile.End(_ProfileEntry);
        }
    }
}


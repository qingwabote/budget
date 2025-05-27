using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Budget
{
    class MeshRenderer : IComponentData
    {
        public Mesh Mesh;
        public Material Material;
    }

    public class MeshRendererAuthoring : MonoBehaviour { }

    class MeshRendererBaker : Baker<MeshRendererAuthoring>
    {
        public override void Bake(MeshRendererAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            var meshFilter = authoring.GetComponent<MeshFilter>();

            var meshRenderer = authoring.GetComponent<UnityEngine.MeshRenderer>();

            AddComponentObject(entity, new MeshRenderer { Mesh = meshFilter.sharedMesh, Material = meshRenderer.sharedMaterial });
        }
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    partial struct MeshRendererSystem : ISystem
    {
        private int _ProfileEntry;

        public void OnCreate(ref SystemState state)
        {
            _ProfileEntry = Profile.DefineEntry("MeshRenderer");

            state.RequireForUpdate<MeshRenderer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            Profile.Begin(_ProfileEntry);

            NativeArray<Matrix4x4> instData = new(1, Allocator.Temp);
            foreach (var (renderer, localToWorld) in SystemAPI.Query<MeshRenderer, RefRO<LocalToWorld>>())
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


using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Budget
{
    class Batch
    {
        public static Dictionary<KeyValuePair<int, int>, Batch> Cache = new();

        public readonly Mesh Mesh;
        public readonly Material Material;

        public readonly NativeList<Matrix4x4> Worlds;
        public readonly IReadOnlyDictionary<int, List<float>> Properties;

        public Batch(Mesh mesh, Material material, IReadOnlyDictionary<int, List<float>> properties)
        {
            Mesh = mesh;
            Material = material;

            Worlds = new(1, Allocator.Persistent);
            Properties = properties;
        }
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    partial struct Batcher : ISystem
    {
        private int _mProfileEntry;

        public void OnCreate(ref SystemState state)
        {
            _mProfileEntry = Profile.DefineEntry("Batcher");
        }

        public void OnUpdate(ref SystemState state)
        {
            Profile.Begin(_mProfileEntry);

            // NativeArray<Matrix4x4> instData = new(1, Allocator.Temp);
            foreach (var (modelCompont, localToWorld) in SystemAPI.Query<ModelComponet, RefRO<LocalToWorld>>())
            {
                var model = modelCompont.Value;
                var rp = new RenderParams(model.Material);
                Graphics.RenderMesh(rp, model.Mesh, 0, localToWorld.ValueRO.Value);
                // instData[0] = localToWorld.ValueRO.Value;
                // Graphics.RenderMeshInstanced(rp, model.Mesh, 0, instData, 1);
            }
            // instData.Dispose();

            Profile.End(_mProfileEntry);
        }
    }
}
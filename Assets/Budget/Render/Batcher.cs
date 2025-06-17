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

        public int Count;

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
        private int _mTimeEntry;
        private int _mNumEntry;

        public void OnCreate(ref SystemState state)
        {
            _mTimeEntry = Profile.DefineEntry("Batcher");
            _mNumEntry = Profile.DefineEntry("Batch Count");
        }

        public void OnUpdate(ref SystemState state)
        {
            Profile.Begin(_mTimeEntry);
            foreach (var (modelCompont, localToWorld) in SystemAPI.Query<ModelComponet, RefRO<LocalToWorld>>())
            {
                var model = modelCompont.Value;
                KeyValuePair<int, int> key = new(model.Mesh.GetInstanceID(), model.Material.GetInstanceID());
                if (!Batch.Cache.TryGetValue(key, out Batch batch))
                {
                    Batch.Cache.Add(key, batch = new Batch(model.Mesh, model.Material, model.Properties()));
                }
                model.Properties(batch.Properties);
                batch.Worlds.Add(localToWorld.ValueRO.Value);
                batch.Count++;
            }

            int count = 0;
            foreach (var batch in Batch.Cache.Values)
            {
                if (batch.Count < 1)
                {
                    continue;
                }

                var rp = new RenderParams(batch.Material);
                Graphics.RenderMeshInstanced(rp, batch.Mesh, 0, batch.Worlds.AsArray(), batch.Count);

                batch.Worlds.Clear();
                batch.Count = 0;

                count++;
            }
            Profile.End(_mTimeEntry);

            Profile.Set(_mNumEntry, count);
        }
    }
}
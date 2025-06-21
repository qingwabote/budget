using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Budget
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    partial struct Batcher : ISystem
    {
        private static readonly TransientPool<Batch> s_Queue = new();
        private static readonly MaterialPropertyBlock s_MPB = new();

        private int m_BatchEntry;
        private int m_CountEntry;
        private int m_RenderEntry;

        public void OnCreate(ref SystemState state)
        {
            m_BatchEntry = Profile.DefineEntry("Batch");
            m_CountEntry = Profile.DefineEntry("Count");
            m_RenderEntry = Profile.DefineEntry("Render");
        }

        public void OnUpdate(ref SystemState state)
        {
            Profile.Begin(m_BatchEntry);
            NativeHashMap<int, int> cache = new(8, Allocator.Temp);
            foreach (var modelCompont in SystemAPI.Query<ModelComponet>())
            {
                var model = modelCompont.Value;

                int key = model.Hash();
                Batch batch;
                if (cache.TryGetValue(key, out int index))
                {
                    batch = s_Queue.Data[index];
                }
                else
                {
                    cache.Add(key, s_Queue.Count);
                    batch = s_Queue.Get();
                    batch.Mesh = model.Mesh;
                    batch.Material = model.Material;
                    batch.MaterialProperty.Clear();
                    model.MaterialProperty(batch.MaterialProperty);
                    batch.InstanceWorlds.Clear();
                    batch.InstanceCount = 0;
                }
                batch.InstanceWorlds.Add(SystemAPI.GetComponentRO<LocalToWorld>(model.Transform).ValueRO.Value);
                model.InstanceProperty(batch.MaterialProperty);
                batch.InstanceCount++;
            }
            Profile.End(m_BatchEntry);
            Profile.Set(m_CountEntry, s_Queue.Count);

            Profile.Begin(m_RenderEntry);
            for (int i = 0; i < s_Queue.Count; i++)
            {
                var batch = s_Queue.Data[i];

                s_MPB.Clear();
                foreach (var (id, texture) in batch.MaterialProperty.Textures)
                {
                    s_MPB.SetTexture(id, texture);
                }
                foreach (var (id, list) in batch.MaterialProperty.Floats)
                {
                    s_MPB.SetFloatArray(id, list);
                }

                var rp = new RenderParams(batch.Material)
                {
                    matProps = s_MPB
                };
                Graphics.RenderMeshInstanced(rp, batch.Mesh, 0, batch.InstanceWorlds.AsArray().Reinterpret<Matrix4x4>(), batch.InstanceCount);
            }
            Profile.End(m_RenderEntry);
        }
    }
}
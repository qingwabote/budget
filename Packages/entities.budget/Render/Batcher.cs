using Bastard;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Budget
{
    public partial struct Batcher : ISystem
    {
        private static readonly RecycleQueue<Batch> s_Queue = new();
        private static readonly MaterialPropertyBlock s_MPB = new();

        private int m_BatchEntry;
        private int m_CountEntry;
        private int m_DrawEntry;

        public void OnCreate(ref SystemState state)
        {
            m_BatchEntry = Profile.DefineEntry("Batch");
            m_CountEntry = Profile.DefineEntry("Count");
            m_DrawEntry = Profile.DefineEntry("Draw");
        }

        public void OnUpdate(ref SystemState state)
        {
            using (new Profile.Scope(m_BatchEntry))
            {
                NativeHashMap<int, int> cache = new(8, Allocator.Temp);
                foreach (var (models, world) in SystemAPI.Query<ModelArray, RefRO<LocalToWorld>>())
                {
                    foreach (var model in models.Value)
                    {
                        if (!model.Initialized)
                        {
                            model.Initialize(ref state);
                            model.Initialized = true;
                        }

                        int key = model.Hash();
                        Batch batch;
                        if (cache.TryGetValue(key, out int index))
                        {
                            batch = s_Queue.Data[index];
                        }
                        else
                        {
                            cache.Add(key, s_Queue.Count);
                            batch = s_Queue.Push();
                            batch.Mesh = model.Mesh;
                            batch.Material = model.Material;
                            model.MaterialProperty(batch.MaterialProperty);
                        }
                        batch.InstanceWorlds.Add(world.ValueRO.Value);
                        model.InstanceProperty(batch.MaterialProperty);
                        batch.InstanceCount++;
                    }
                }
            }

            Profile.Set(m_CountEntry, s_Queue.Count);

            using (new Profile.Scope(m_DrawEntry))
            {
                foreach (var batch in s_Queue.Drain())
                {
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

                    batch.MaterialProperty.Clear();
                    batch.InstanceWorlds.Clear();
                    batch.InstanceCount = 0;
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Budget
{
    class Pool<T> where T : new()
    {
        private readonly List<T> m_Data = new();
        public IReadOnlyList<T> Data => m_Data;

        private int m_Count = 0;
        public int Count => m_Count;

        private readonly Func<T> m_Creator;

        public Pool(Func<T> creator = null)
        {
            m_Creator = creator;
        }

        public T Get()
        {
            if (m_Data.Count == m_Count)
            {
                m_Data.Add(m_Creator == null ? new T() : m_Creator());
            }
            return m_Data[m_Count++];
        }

        public void Reset()
        {
            m_Count = 0;
        }
    }

    public class Batch
    {
        public Mesh Mesh;
        public Material Material;

        public NativeList<float4x4> Worlds;

        public IReadOnlyDictionary<int, List<float>> Properties;

        public int Count;
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    partial struct Batcher : ISystem
    {
        private static readonly Pool<NativeList<float4x4>> s_WorldsPool = new(() => new NativeList<float4x4>(Allocator.Persistent));
        private static readonly Pool<Dictionary<int, List<float>>> s_PropertiesPool = new();
        private static readonly Pool<List<float>> s_PropertyPool = new();

        private static readonly Pool<Batch> s_Queue = new();

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
            s_WorldsPool.Reset();
            s_Queue.Reset();
            NativeHashMap<int, int> cache = new(8, Allocator.Temp);
            foreach (var (modelCompont, localToWorld) in SystemAPI.Query<ModelComponet, RefRO<LocalToWorld>>())
            {
                var model = modelCompont.Value;

                int key = model.Mesh.GetHashCode() ^ model.Material.GetHashCode();
                if (!cache.TryGetValue(key, out int batch))
                {
                    batch = s_Queue.Count;
                    s_Queue.Get();
                    s_Queue.Data[batch].Mesh = model.Mesh;
                    s_Queue.Data[batch].Material = model.Material;
                    s_Queue.Data[batch].Worlds = s_WorldsPool.Get();
                    s_Queue.Data[batch].Worlds.Clear();
                    s_Queue.Data[batch].Count = 0;
                    cache.Add(key, batch);
                }
                s_Queue.Data[batch].Worlds.Add(localToWorld.ValueRO.Value);
                s_Queue.Data[batch].Count++;
            }
            Profile.End(m_BatchEntry);
            Profile.Set(m_CountEntry, s_Queue.Count);

            Profile.Begin(m_RenderEntry);
            for (int i = 0; i < s_Queue.Count; i++)
            {
                var batch = s_Queue.Data[i];
                var rp = new RenderParams(batch.Material);
                Graphics.RenderMeshInstanced(rp, batch.Mesh, 0, batch.Worlds.AsArray().Reinterpret<Matrix4x4>(), batch.Count);
            }
            Profile.End(m_RenderEntry);
        }
    }
}
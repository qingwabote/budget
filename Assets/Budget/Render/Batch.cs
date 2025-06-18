using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Budget
{
    public class Batch
    {
        public Mesh Mesh;
        public Material Material;
        public MaterialProperty MaterialProperty = new();

        public NativeList<float4x4> InstanceWorlds = new(Allocator.Persistent);
        public int InstanceCount;
    }
}
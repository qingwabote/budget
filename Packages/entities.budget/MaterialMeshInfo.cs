using Unity.Entities;
using UnityEngine;

namespace Budget
{
    public class MaterialMeshInfo : IComponentData
    {
        public Mesh Mesh;
        public Material Material;
    }
}
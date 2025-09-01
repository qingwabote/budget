using Unity.Entities;
using UnityEngine;

namespace Budget
{
    [TemporaryBakingType]
    public class MaterialMeshBaking : IComponentData
    {
        public Mesh Mesh;
        public Material Material;
    }
}
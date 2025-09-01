using Unity.Entities;

namespace Budget
{
    public struct MaterialMeshInfo : IComponentData
    {
        public int Material;
        public int Mesh;
    }
}
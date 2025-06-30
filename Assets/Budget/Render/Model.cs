using Unity.Entities;
using UnityEngine;

namespace Budget
{
    public class Model
    {
        public Mesh Mesh;
        public Material Material;

        public bool Initialized;

        virtual public void Initialize(ref SystemState state) { }

        virtual public int Hash() { return HashCode.Combine(Mesh.GetHashCode(), Material.GetHashCode()); }

        virtual public void MaterialProperty(MaterialProperty output) { }

        virtual public void InstanceProperty(MaterialProperty input) { }
    }

    public class ModelArray : IComponentData
    {
        public Model[] Value;
    }
}
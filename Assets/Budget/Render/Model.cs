using Unity.Entities;
using UnityEngine;

namespace Budget
{
    public class Model
    {
        public Entity Transform;

        public Mesh Mesh;
        public Material Material;

        virtual public void Initialize(ref SystemState state) { }

        virtual public int Hash() { return HashCode.Combine(Mesh.GetHashCode(), Material.GetHashCode()); }

        virtual public void MaterialProperty(MaterialProperty output) { }

        virtual public void InstanceProperty(MaterialProperty input) { }
    }

    public class ModelComponet : IComponentData
    {
        public Model Value;
        public bool Initialized;
    }
}
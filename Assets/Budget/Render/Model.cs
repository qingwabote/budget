using Unity.Entities;
using UnityEngine;

namespace Budget
{
    public class Model
    {
        public Mesh Mesh;
        public Material Material;
        virtual public void MaterialProperty(MaterialProperty output) { }
        virtual public int Hash() { return HashCode.Combine(Mesh.GetHashCode(), Material.GetHashCode()); }

        virtual public void InstanceProperty(MaterialProperty output) { }
    }

    public class ModelComponet : IComponentData { public Model Value; }
}
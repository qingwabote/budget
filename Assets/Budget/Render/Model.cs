using Unity.Entities;
using UnityEngine;

namespace Budget
{
    public class Model
    {
        public Mesh Mesh;
        public Material Material;
    }

    public class ModelComponet : IComponentData
    {
        public Model Value;
    }
}
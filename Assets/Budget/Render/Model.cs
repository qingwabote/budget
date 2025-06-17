using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Budget
{
    public class Model
    {
        public Mesh Mesh;
        public Material Material;

        virtual public IReadOnlyDictionary<int, List<float>> Properties() { return null; }

        virtual public void Properties(IReadOnlyDictionary<int, List<float>> output) { }
    }

    public class ModelComponet : IComponentData { public Model Value; }
}
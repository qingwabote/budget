using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Budget
{
    public class Model
    {
        private static readonly int[] s_Properties = { };

        public Mesh Mesh;
        public Material Material;

        virtual public int[] Properties() { return s_Properties; }

        virtual public void Properties(IReadOnlyDictionary<int, List<float>> output) { }
    }

    public class ModelComponet : IComponentData { public Model Value; }
}
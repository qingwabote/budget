using System.Collections.Generic;
using UnityEngine;

namespace Budget
{
    public class MaterialProperty
    {
        public Dictionary<int, Texture> Textures = new();
        public Dictionary<int, List<float>> Floats = new();

        public void Clear()
        {
            Textures.Clear();
            Floats.Clear();
        }
    }
}
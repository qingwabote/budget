using System.Collections.Generic;
using UnityEngine;

namespace Budget
{
    public class SkinnedModel : Model
    {
        public static readonly int OFFSET = Shader.PropertyToID("_JointsOffset");

        private static readonly int[] s_Properties = { OFFSET };

        override public int[] Properties() { return s_Properties; }

        override public void Properties(IReadOnlyDictionary<int, List<float>> output)
        {

        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Budget
{
    public class SkinnedModel : Model
    {
        public static readonly int OFFSET = Shader.PropertyToID("_JointsOffset");

        override public IReadOnlyDictionary<int, List<float>> Properties()
        {
            Dictionary<int, List<float>> dict = new()
            {
                { OFFSET, new List<float>() }
            };
            return dict;
        }

        override public void Properties(IReadOnlyDictionary<int, List<float>> output)
        {

        }
    }
}
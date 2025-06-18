using UnityEngine;

namespace Budget
{
    public class SkinnedModel : Model
    {
        public static readonly int OFFSET = Shader.PropertyToID("_JointsOffset");

        override public void InstanceProperty(MaterialProperty output)
        {

        }
    }
}
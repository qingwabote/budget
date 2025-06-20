using UnityEngine;

namespace Budget
{
    public class SkinnedModel : Model
    {
        public static readonly int OFFSET = Shader.PropertyToID("_JointsOffset");

        public SkinInfo Skin;

        override public void InstanceProperty(MaterialProperty output)
        {

        }
    }
}
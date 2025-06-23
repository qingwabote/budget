using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Budget
{
    public class SkinnedModel : Model
    {
        private static TransientPool<List<float>> s_Floats;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            s_Floats = new();
        }


        public static readonly int JOINTS = Shader.PropertyToID("_JointMap");
        public static readonly int OFFSET = Shader.PropertyToID("_JointMapOffset");

        public SkinInfo Skin;

        override public void Initialize(ref SystemState state)
        {
            Skin = state.EntityManager.GetComponentObject<SkinInfoComponent>(Transform).Value;
        }

        override public int Hash() { return HashCode.Combine(Mesh.GetHashCode(), Material.GetHashCode(), Skin.Store.Texture.GetHashCode()); }

        override public void MaterialProperty(MaterialProperty output)
        {
            output.Textures.Add(JOINTS, Skin.Store.Texture);
            var offsets = s_Floats.Get();
            offsets.Clear();
            output.Floats.Add(OFFSET, offsets);
        }

        override public void InstanceProperty(MaterialProperty input)
        {
            input.Floats.TryGetValue(OFFSET, out var offsets);
            offsets.Add(Skin.Offset);
        }
    }
}
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Budget
{
    public class TextureView : MemoryView<float>
    {
        private static int Length2extent(int length)
        {
            var texels = math.ceil(length / 4.0f);
            var extent = math.ceil(math.sqrt(texels));
            var n = math.ceil(math.log2(extent));
            return (int)math.pow(2, n);
        }

        public readonly Texture2D Texture;

        public TextureView(int length = 0, int capacity = 16) : base(length)
        {
            var extent = Length2extent(math.max(length, capacity));
            Texture = new Texture2D(extent, extent, TextureFormat.RGBAFloat, false, true);
            m_Source = Texture.GetPixelData<float>(0);
        }

        protected override void Reserve(int capacity)
        {
            var extent = Length2extent(capacity);
            if (Texture.width >= extent)
            {
                return;
            }
            // FIXME: UB - accessing old memory after Reinitialize
            unsafe
            {
                var Source = m_Source.GetUnsafePtr();
                var Length = m_Source.Length;
                if (!Texture.Reinitialize(extent, extent))
                {
                    Debug.Log("Texture.Reinitialize failed!");
                    return;
                }
                m_Source = Texture.GetPixelData<float>(0);
                UnsafeUtility.MemCpy(m_Source.GetUnsafePtr(), Source, Length);
            }
        }

        protected override void Upload()
        {
            Texture.Apply(false);
        }
    }
}
using Unity.Mathematics;
using UnityEngine;

namespace Budget
{
    public class TextureView<T> : MemoryView<T> where T : unmanaged
    {
        private static int Length2extent(int length)
        {
            var texels = math.ceil(length / 4);
            var extent = math.ceil(math.sqrt(texels));
            var n = math.ceil(math.log2(extent));
            return (int)math.pow(2, n);
        }

        public readonly Texture2D Texture;

        public TextureView(int length = 0) : this(Length2extent(length), TextureFormat.RGBAFloat) { }

        private TextureView(int extent, TextureFormat format) : base(extent * extent * 4)
        {
            Texture = new Texture2D(extent, extent, format, false, true);
        }
    }
}
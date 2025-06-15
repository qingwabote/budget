using Unity.Collections;
using Unity.Mathematics;

namespace Budget
{
    public abstract class MemoryView<T> where T : unmanaged
    {
        protected NativeArray<T> _mSource;

        public ref NativeArray<T> Source => ref _mSource;

        private int _Length;
        public int Length
        {
            get => _Length;
        }

        public MemoryView(int length)
        {
            // capacity = math.max(length, capacity);
            _Length = length;
        }

        public int AddBlock(int length)
        {
            var offset = _Length;
            Resize(offset + length);
            return offset;
        }


        public void Resize(int length)
        {
            Reserve(length);
            _Length = length;
        }

        protected abstract void Reserve(int capacity);
    }
}
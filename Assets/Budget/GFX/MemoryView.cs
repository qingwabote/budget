using Unity.Collections;
using Unity.Mathematics;

namespace Budget
{
    public abstract class MemoryView<T> where T : unmanaged
    {
        public readonly NativeList<T> Source;

        private int _Length;
        public int Length
        {
            get => _Length;
        }

        public MemoryView(int length, int capacity = 0)
        {
            capacity = math.max(length, capacity);
            Source = new NativeList<T>(capacity, Allocator.Persistent);
            _Length = length;
        }
    }
}
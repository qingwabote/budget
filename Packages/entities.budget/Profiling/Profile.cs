using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace Budget
{
    public struct Entry
    {
        public FixedString32Bytes Name;
        public float Delta;
        public float Value;
    }

    public struct Profile
    {
        private class EntriesTag { }
        public static readonly SharedStatic<NativeList<Entry>> Entries = SharedStatic<NativeList<Entry>>.GetOrCreate<Profile, EntriesTag>();

        public static int DefineEntry(FixedString32Bytes name)
        {
            if (!Entries.Data.IsCreated)
            {
                Entries.Data = new NativeList<Entry>(Allocator.Persistent);
            }

            Entries.Data.Add(new Entry()
            {
                Name = name
            });
            return Entries.Data.Length - 1;
        }

        public static void Set(int entry, int value)
        {
            ref Entry ent = ref Entries.Data.ElementAt(entry);
            ent.Value = value;
            ent.Delta = -1;
        }

        public static void Begin(int entry)
        {
            ref Entry ent = ref Entries.Data.ElementAt(entry);
            ent.Value = Time.realtimeSinceStartup;
        }

        public static void End(int entry)
        {
            ref Entry ent = ref Entries.Data.ElementAt(entry);
            ent.Delta += Time.realtimeSinceStartup - ent.Value;
        }
    }
}
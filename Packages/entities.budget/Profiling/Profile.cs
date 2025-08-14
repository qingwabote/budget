using System;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace Budget
{
    public struct Entry
    {
        public FixedString32Bytes Name;
        public float Delta;
    }

    public struct Profile
    {
        public struct Scope : IDisposable
        {
            private int m_Entry;
            private float m_Time;

            public Scope(int entry)
            {
                m_Entry = entry;
                m_Time = Time.realtimeSinceStartup;
            }

            public void Dispose()
            {
                Set(m_Entry, (Time.realtimeSinceStartup - m_Time) * 1000);
            }
        }

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

        public static void Set(int entry, float value)
        {
            ref Entry ent = ref Entries.Data.ElementAt(entry);
            ent.Delta += value;
        }
    }
}
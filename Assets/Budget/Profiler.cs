using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct Entry
{
    public FixedString32Bytes Name;
    public float Delta;
    public float Value;
}

public struct Profile
{
    private class _EntriesTag { }
    public static readonly SharedStatic<NativeList<Entry>> _Entries = SharedStatic<NativeList<Entry>>.GetOrCreate<Profile, _EntriesTag>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        _Entries.Data = new NativeList<Entry>(Allocator.Persistent);
    }

    public static int DefineEntry(FixedString32Bytes name)
    {
        _Entries.Data.Add(new Entry()
        {
            Name = name
        });
        return _Entries.Data.Length - 1;
    }

    public static void Set(int entry, int value)
    {
        ref Entry ent = ref _Entries.Data.ElementAt(entry);
        ent.Value = value;
    }

    public static void Begin(int entry)
    {
        ref Entry ent = ref _Entries.Data.ElementAt(entry);
        ent.Value = Time.realtimeSinceStartup;
    }

    public static void End(int entry)
    {
        ref Entry ent = ref _Entries.Data.ElementAt(entry);
        ent.Delta += Time.realtimeSinceStartup - ent.Value;
    }
}

public class Profiler : MonoBehaviour
{
    private float _time;
    private uint _frames = 1;

    private TextMeshProUGUI _label;

    void Start()
    {
        _label = GetComponent<TextMeshProUGUI>();
    }

    // after ecs system onupdate
    void LateUpdate()
    {
        if (_time < 1.0f)
        {
            _frames++;
            _time += Time.unscaledDeltaTime;
            return;
        }

        int PadRight = 12;
        int PadLeft = 8;

        string name = "FPS".PadRight(PadRight);
        string text = $"{name} {math.round(_frames / _time).ToString().PadLeft(PadLeft)}";

        ref var entries = ref Profile._Entries.Data;
        for (int i = 0; i < entries.Length; i++)
        {
            ref var entry = ref entries.ElementAt(i);
            name = entry.Name.ToString().PadRight(PadRight);
            if (entry.Delta > 0)
            {
                string ms = (entry.Delta / _frames * 1000).ToString("F3").PadLeft(PadLeft);
                text += $"\n{name} {ms}ms";
                entry.Delta = 0;
            }
            else
            {
                string value = entry.Value.ToString().PadLeft(PadLeft);
                text += $"\n{name} {value}";
            }

        }

        _label.text = text;

        _frames = 1;
        _time = Time.unscaledDeltaTime;
    }
}

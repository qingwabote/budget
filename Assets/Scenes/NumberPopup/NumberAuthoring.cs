using Graphix;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

[MaterialProperty("_Index")]
[WriteGroup(typeof(MaterialMeshBaking))]
public struct NumberIndex : IBufferElementData
{
    public float Value;
}

[MaterialProperty("_BaseColor")]
public struct NumberColor : IBufferElementData
{
    public float4 Value;
}

[MaterialProperty("_Offset")]
public struct NumberOffset : IBufferElementData
{
    public float4 Value;
}

public class NumberAuthoring : MonoBehaviour
{
    public Color Color;

    class NumberBaker : Baker<NumberAuthoring>
    {
        public override void Bake(NumberAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable);
            var meshFilter = authoring.GetComponent<MeshFilter>();
            var meshRenderer = authoring.GetComponent<MeshRenderer>();
            AddComponentObject(entity, new MaterialMeshBufferedBaking
            {
                Meshes = new[] { meshFilter.sharedMesh },
                Materials = new[] { meshRenderer.sharedMaterial }
            });
            var numberIndexes = AddBuffer<NumberIndex>(entity);
            numberIndexes.Add(new());
            var numberColor = AddBuffer<NumberColor>(entity);
            numberColor.Add(new() { Value = UnsafeUtility.As<Color, float4>(ref authoring.Color) });
            var numberOffset = AddBuffer<NumberOffset>(entity);
            numberOffset.Add(new());
        }
    }
}
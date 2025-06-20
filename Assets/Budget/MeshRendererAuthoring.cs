using Unity.Entities;
using UnityEngine;

namespace Budget
{
    public class MeshRendererAuthoring : MonoBehaviour { }

    class MeshRendererBaker : Baker<MeshRendererAuthoring>
    {
        public override void Bake(MeshRendererAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            var meshFilter = authoring.GetComponent<MeshFilter>();

            var meshRenderer = authoring.GetComponent<MeshRenderer>();

            var model = new Model
            {
                Transform = entity,
                Mesh = meshFilter.sharedMesh,
                Material = meshRenderer.sharedMaterial
            };
            AddComponentObject(entity, new ModelComponet { Value = model });
        }
    }
}


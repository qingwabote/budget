using Unity.Entities;
using UnityEngine;

namespace Budget
{
    public class SkinnedMeshRendererAuthoring : MonoBehaviour
    {
        public SkinAuthoring Skin;
        public Material Material;
    }

    class SkinnedMeshRendererBaker : Baker<SkinnedMeshRendererAuthoring>
    {
        public override void Bake(SkinnedMeshRendererAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var meshRenderer = authoring.GetComponent<SkinnedMeshRenderer>();
            AddComponentObject(entity, new SkinnedModel
            {
                Entity = GetEntity(authoring.Skin, TransformUsageFlags.None),
                Mesh = meshRenderer.sharedMesh,
                Material = authoring.Material
            });
        }
    }
}


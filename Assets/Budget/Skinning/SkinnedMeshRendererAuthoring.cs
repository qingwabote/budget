using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Budget
{
    [BakingType]
    public class SkinnedMeshRendererBaking : IComponentData
    {
        public Mesh Mesh;
        public Material Material;
        public Entity Skin;
    }

    public class SkinnedMeshRendererAuthoring : MonoBehaviour
    {
        public SkinAuthoring Skin;
    }

    class SkinnedMeshRendererBaker : Baker<SkinnedMeshRendererAuthoring>
    {
        public override void Bake(SkinnedMeshRendererAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var meshRenderer = authoring.GetComponent<SkinnedMeshRenderer>();
            // if (authoring.gameObject == meshRenderer.rootBone.gameObject)
            // {
            //     var root = CreateAdditionalEntity(TransformUsageFlags.None);
            //     var joints = AddBuffer<SkinJoint>(root);
            //     foreach (var bone in meshRenderer.bones)
            //     {

            //         joints.Add(new SkinJoint
            //         {
            //             Value =
            //         })
            //     }
            // }
            AddComponentObject(entity, new SkinnedMeshRendererBaking
            {
                Mesh = meshRenderer.sharedMesh,
                Material = meshRenderer.sharedMaterial,
                Skin = GetEntity(authoring.Skin, TransformUsageFlags.None)
            });
        }
    }
}


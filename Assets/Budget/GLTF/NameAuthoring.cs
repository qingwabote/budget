using Unity.Entities;
using UnityEngine;

namespace Budget.GLTF
{
    public class Name : IComponentData
    {
        public string Value;
    }

    public class NameAuthoring : MonoBehaviour { }

    class NameBaker : Baker<NameAuthoring>
    {
        public override void Bake(NameAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new Name
            {
                Value = authoring.name
            });
        }
    }
}
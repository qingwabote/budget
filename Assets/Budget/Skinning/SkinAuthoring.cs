using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Budget
{
    public class SkinAuthoring : MonoBehaviour
    {
        [HideInInspector]
        public Skin Skin;
        public bool Baking;
    }

    public class SkinInfo : IComponentData
    {
        public Skin Proto;
        public bool Baking;
        public Skin.Store Store => Baking ? Proto.Persistent : Proto.Persistent;
    }

    public struct SkinNode : IBufferElementData
    {
        public Entity Target;
        public int Parent;
    }

    [ChunkSerializable]
    public unsafe struct SkinJoint : IComponentData
    {
        public int Index;

        public float* StoreSource;
        public int StoreOffset;
    }

    class SkinBaker : Baker<SkinAuthoring>
    {
        public override void Bake(SkinAuthoring authoring)
        {
            var entity = CreateAdditionalEntity(TransformUsageFlags.None);

            var transforms = new List<Transform>();
            {
                var parent1 = authoring.transform;
                var path = authoring.Skin.Joints[0].Split("/");
                for (int i = 0; i < path.Length - 1; i++)
                {
                    var name = path[i];
                    var err = true;
                    for (int j = 0; j < parent1.childCount; j++)
                    {
                        var child = parent1.GetChild(j);
                        if (child.name == name)
                        {
                            transforms.Add(child);
                            parent1 = child;
                            err = false;
                            break;
                        }
                    }
                    if (err)
                    {
                        throw new Exception($"{name} not exists");
                    }
                }
            }
            var JointStart = transforms.Count;
            foreach (var path in authoring.Skin.Joints)
            {
                var target = authoring.transform;
                foreach (var name in path.Split("/"))
                {
                    var err = true;
                    for (int i = 0; i < target.childCount; i++)
                    {
                        var child = target.GetChild(i);
                        if (child.name == name)
                        {
                            target = child;
                            err = false;
                            break;
                        }
                    }
                    if (err)
                    {
                        throw new Exception($"{name} not exists");
                    }
                }
                transforms.Add(target);
            }

            var nodes = AddBuffer<SkinNode>(entity);
            for (int i = 0; i < transforms.Count; i++)
            {
                var child = transforms[i];
                var parent = i - 1;
                for (; parent > -1; parent--)
                {
                    if (transforms[parent] == child.parent)
                    {
                        break;
                    }
                }
                nodes.Add(new SkinNode
                {
                    Target = GetEntity(child.gameObject, TransformUsageFlags.Dynamic),
                    Parent = parent
                });
            }
            AddComponent(entity, new SkinJoint
            {
                Index = JointStart,
                StoreSource = null,
                StoreOffset = -1
            });
            AddComponentObject(entity, new SkinInfo
            {
                Proto = authoring.Skin,
                Baking = authoring.Baking
            });
        }
    }
}
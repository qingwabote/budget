using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

namespace Budget
{
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    partial struct SkinnedMeshRendererBaker : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var materials = new Dictionary<Material, Material>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (renderer, entity) in SystemAPI.Query<SkinnedMeshRendererBaking>().WithEntityAccess())
            {
                if (!materials.TryGetValue(renderer.Material, out Material material))
                {
                    // FIXME: when to release the material
                    material = new Material(renderer.Material);
                    material.shader = Shader.Find("Budget/PBRGraph-Universal");
                    materials.Add(renderer.Material, material);
                }

                var model = new SkinnedModel
                {
                    Transform = renderer.Skin,
                    Mesh = renderer.Mesh,
                    Material = material,
                    Skin = state.EntityManager.GetComponentObject<SkinInfoComponent>(renderer.Skin).Value
                };
                ecb.AddComponent(entity, new ModelComponet { Value = model });
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}


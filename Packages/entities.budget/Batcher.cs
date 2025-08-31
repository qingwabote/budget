using Bastard;
using Unity.Entities;
using Unity.Transforms;

namespace Budget
{
    public partial struct Batcher : ISystem
    {
        private int m_BatchEntry;

        public void OnCreate(ref SystemState state)
        {
            m_BatchEntry = Profile.DefineEntry("Batch");
        }

        public void OnUpdate(ref SystemState state)
        {
            using (new Profile.Scope(m_BatchEntry))
            {
                foreach (var (model, world) in SystemAPI.Query<MaterialMeshInfo, RefRO<LocalToWorld>>().WithOptions(EntityQueryOptions.FilterWriteGroup))
                {
                    if (Batch.Register(HashCode.Combine(model.Mesh.GetHashCode(), model.Material.GetHashCode()), out Batch batch))
                    {
                        batch.Material = model.Material;
                        batch.Mesh = model.Mesh;
                    }
                    batch.InstanceWorlds.Add(world.ValueRO.Value);
                    batch.InstanceCount++;
                }
            }
        }
    }
}
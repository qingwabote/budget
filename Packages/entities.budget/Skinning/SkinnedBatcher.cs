using Bastard;
using Unity.Entities;
using Unity.Transforms;

namespace Budget
{
    public partial struct SkinnedBatcher : ISystem
    {
        private int m_BatchEntry;

        public void OnCreate(ref SystemState state)
        {
            m_BatchEntry = Profile.DefineEntry("SkinnedBatch");
        }

        public void OnUpdate(ref SystemState state)
        {
            using (new Profile.Scope(m_BatchEntry))
            {
                foreach (var model in SystemAPI.Query<SkinnedModel>())
                {
                    if (!model.Initialized)
                    {
                        model.Initialize(ref state);
                        model.Initialized = true;
                    }

                    if (Batch.Register(out Batch batch, model))
                    {
                        model.MaterialProperty(batch.MaterialProperty);
                    }
                    batch.InstanceWorlds.Add(state.EntityManager.GetComponentData<LocalToWorld>(model.Entity).Value);
                    model.InstanceProperty(batch.MaterialProperty);
                    batch.InstanceCount++;
                }
            }
        }
    }
}
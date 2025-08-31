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
                foreach (var (models, world) in SystemAPI.Query<ModelArray, RefRO<LocalToWorld>>())
                {
                    foreach (var model in models.Value)
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
                        batch.InstanceWorlds.Add(world.ValueRO.Value);
                        model.InstanceProperty(batch.MaterialProperty);
                        batch.InstanceCount++;
                    }
                }
            }
        }
    }
}
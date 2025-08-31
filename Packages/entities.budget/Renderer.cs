using Unity.Entities;

namespace Budget
{
    public partial struct Renderer : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            Batch.Render();
        }
    }
}
using FedoraEngine.ECS.Entities;

namespace FedoraEngine.ECS.Components.Collision
{
    public interface ITriggerListener
    {
        void OnOverlap(Entity entity);
    }
}

using FedoraEngine.ECS.Entities;
using System.Collections.Generic;

namespace FedoraEngine.ECS.Systems
{
    public abstract class EntityProcessingSystem : ProcessingSystem
    {
        public virtual void Update(IList<Entity> entities) { }

        public virtual void OnEntityAddedToScene(Entity entity) { }

        public virtual void OnEntityRemovedFromScene(Entity entity) { }
    }
}

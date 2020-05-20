using FedoraEngine.ECS.Systems;
using System.Collections.Generic;

namespace FedoraEngine.ECS.Managers
{
    public abstract class Manager
    {
        public virtual void Update(IList<ProcessingSystem> systems) { }
    }
}

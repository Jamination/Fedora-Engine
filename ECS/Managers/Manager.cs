using FedoraEngine.ECS.Systems;
using System.Collections.Generic;

namespace FedoraEngine.ECS.Managers
{
    public abstract class Manager
    {
        public abstract void Update(IList<ProcessingSystem> systems);
    }
}

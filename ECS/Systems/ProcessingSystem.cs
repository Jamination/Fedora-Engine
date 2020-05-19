using FedoraEngine;
using FedoraEngine.ECS.Scenes;

namespace FedoraEngine.ECS.Systems
{
    public abstract class ProcessingSystem
    {
        public Scene? Scene => Core.Scene;

        public virtual void Update() { }
    }
}

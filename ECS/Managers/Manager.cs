using FedoraEngine.ECS.Scenes;
using FedoraEngine.ECS.Systems;
using System.Collections.Generic;

namespace FedoraEngine.ECS.Managers
{
    public abstract class Manager
    {
        public virtual void Update(HashSet<ProcessingSystem> systems) { }

        public virtual void OnSceneChanged(Scene scene) { }

        public virtual void OnLoad() { }

        public virtual void OnInitialize() { }

        public virtual void OnQuit() { }
    }
}

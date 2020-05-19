namespace FedoraEngine.ECS.Components.TileMap
{
    public abstract class OgmoMapLayer
    {
        public abstract void Load(OgmoMap map);

        public virtual void Update() { }

        public virtual void Draw() { }
    }
}

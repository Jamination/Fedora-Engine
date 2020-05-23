using FedoraEngine.Graphics;

namespace FedoraEngine.ECS.Components.Graphics
{
    public sealed class DepthSort : Component, IUpdateable
    {
        public SortModes SortMode;

        public void Update()
        {
            switch (SortMode)
            {
                case SortModes.YSort:
                    foreach (var component in Entity.DrawableComponents)
                        ((IDrawable)component).RenderLayer = component.Transform.Position.Y;
                    break;
                case SortModes.DescendingYSort:
                    foreach (var component in Entity.DrawableComponents)
                        ((IDrawable)component).RenderLayer = -component.Transform.Position.Y;
                    break;
                case SortModes.XSort:
                    foreach (var component in Entity.DrawableComponents)
                        ((IDrawable)component).RenderLayer = component.Transform.Position.X;
                    break;
                case SortModes.DescendingXSort:
                    foreach (var component in Entity.DrawableComponents)
                        ((IDrawable)component).RenderLayer = -component.Transform.Position.X;
                    break;
            }
        }
    }
}

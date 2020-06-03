using Microsoft.Xna.Framework;

namespace FedoraEngine.ECS.Components.Graphics
{
    public sealed class MovingBackground : Component, IUpdateable
    {
        public Vector2 Direction { get; set; }

        public Vector2 Tiling = new Vector2(256f, 256f);

        public MovingBackground(Vector2 direction, Vector2 tiling)
        {
            Direction = direction;
            Tiling = tiling;
        }

        public void Update()
        {
            Position += Direction;

            if (Position.X >= Tiling.X + Scene.ScreenCentre.X)
                Position = new Vector2(Scene.ScreenCentre.X, Position.Y);
            if (Position.Y >= Tiling.Y + Scene.ScreenCentre.Y)
                Position = new Vector2(Position.X, Scene.ScreenCentre.Y);

            if (Position.X <= -Tiling.X + Scene.ScreenCentre.X)
                Position = new Vector2(Scene.ScreenCentre.X, Position.Y);
            if (Position.Y <= -Tiling.Y + Scene.ScreenCentre.Y)
                Position = new Vector2(Position.X, Scene.ScreenCentre.Y);
        }
    }
}

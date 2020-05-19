using Microsoft.Xna.Framework;

namespace FedoraEngine.ECS.Components.TileMap
{
    public class OgmoTile
    {
        public Vector2 Type;

        public readonly Transform Transform;

        public Vector2 Position { get; private set; }

        public Color Colour;

        public readonly uint TileWidth;
        public readonly uint TileHeight;

        public bool Collidable { get; private set; } = true;

        public bool OneWay { get; private set; } = false;

        public Rectangle AABB
        {
            get
            {
                return new Rectangle(
                    (int)Transform.Position.X + (int)Position.X * (int)Transform.Scale.X,
                    (int)Transform.Position.Y + (int)Position.Y * (int)Transform.Scale.Y,
                    (int)TileWidth * (int)Transform.Scale.X,
                    (int)TileHeight * (int)Transform.Scale.Y
                );
            }
        }

        public OgmoTile(Vector2 type, Color colour, Vector2 position, Transform transform, uint tileWidth, uint tileHeight, bool collidable, bool oneWay)
        {
            Type = type;
            Colour = colour;
            Position = position;
            Transform = transform;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            Collidable = collidable;
            OneWay = oneWay;
        }
    }
}

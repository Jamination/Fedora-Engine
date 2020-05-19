using Microsoft.Xna.Framework;

namespace FedoraEngine.ECS.Components.TileMap
{
    public sealed class OgmoSlopedTile : OgmoTile
    {
        public short SlopeAngle { get; private set; } = 0;

        public OgmoSlopedTile(Vector2 type, Color colour, Vector2 position, Transform transform, uint tileWidth, uint tileHeight, bool collidable, short slopeAngle, bool oneWay) : base(type, colour, position, transform, tileWidth, tileHeight, collidable, oneWay)
        {
            SlopeAngle = slopeAngle;
        }
    }
}

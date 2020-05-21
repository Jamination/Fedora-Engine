using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FedoraEngine.Graphics
{
    public sealed class Sprite
    {
        public Texture2D Texture { get; private set; }

        public Color Colour { get; set; } = Color.White;

        public Vector2 Scale { get; set; } = Vector2.One;

        public Vector2 Origin { get; set; } = Vector2.Zero;

        public SpriteEffects Flip { get; set; } = SpriteEffects.None;

        public float Rotation { get; set; } = 0f;

        public float LayerDepth { get; set; } = 0f;

        public bool Centered { get; set; } = true;

        public Rectangle SourceRect;

        public RectangleF Bounds
        {
            get
            {
                return new RectangleF(Origin.X, Origin.Y, Texture.Width * Scale.X, Texture.Height * Scale.Y);
            }
        }

        public Sprite(Texture2D texture)
        {
            Texture = texture;
            SourceRect = new Rectangle(0, 0, Texture.Width, Texture.Height);
        }

        public Sprite(Texture2D texture, Color colour)
        {
            Texture = texture;
            SourceRect = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Colour = colour;
        }
    }
}

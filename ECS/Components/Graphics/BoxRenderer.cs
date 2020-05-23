using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FedoraEngine.ECS.Components.Graphics
{
    public sealed class BoxRenderer : Component, IDrawable
    {
        public Rectangle Bounds;

        public Color Colour { get; set; }

        public bool Centered = true;

        public Texture2D RectTexture { get; set; }

        public new Rectangle AABB
        {
            get => new Rectangle((int)Position.X, (int)Position.Y, Bounds.Width * (int)Scale.X, Bounds.Height * (int)Scale.Y);
        }

        public float RenderLayer { get; set; }

        public BoxRenderer(Rectangle rect, Color colour)
        {
            Bounds = rect;
            Colour = colour;

            RectTexture = new Texture2D(Graphics, Bounds.Width, Bounds.Height);

            Color[] data = new Color[Bounds.Width * Bounds.Height];

            for (int i = 0; i < data.Length; i++)
                data[i] = Color.White;

            RectTexture.SetData(data);
        }

        public BoxRenderer(int x, int y, uint width, uint height, Color colour)
        {
            Bounds = new Rectangle(x, y, (int)width, (int)height);
            Colour = colour;

            RectTexture = new Texture2D(Graphics, Bounds.Width, Bounds.Height);

            Color[] data = new Color[Bounds.Width * Bounds.Height];

            for (int i = 0; i < data.Length; i++)
                data[i] = Color.White;

            RectTexture.SetData(data);
        }

        public BoxRenderer(uint width, uint height, Color colour)
        {
            Bounds = new Rectangle(0, 0, (int)width, (int)height);
            Colour = colour;

            RectTexture = new Texture2D(Graphics, Bounds.Width, Bounds.Height);

            Color[] data = new Color[Bounds.Width * Bounds.Height];

            for (int i = 0; i < data.Length; i++)
                data[i] = Color.White;

            RectTexture.SetData(data);
        }

        public void Draw()
        {
            Vector2 CenterOrigin = Vector2.Zero;

            if (Centered)
                CenterOrigin = new Vector2(Bounds.Width * .5f, Bounds.Height * .5f);

            SpriteBatch.Draw(
                    RectTexture,
                    Position,
                    null,
                    Colour,
                    Rotation,
                    CenterOrigin,
                    Transform.LocalScale,
                    SpriteEffects.None,
                    0f
            );
        }
    }
}

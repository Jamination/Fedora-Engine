using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FedoraEngine.ECS.Components.Graphics
{
    public sealed class BoxRenderer : Component, IDrawable
    {
        public Rectangle Bounds;
        public Color Colour;

        public bool Centered = true;

        private Texture2D _rectTexture;

        public BoxRenderer(Rectangle rect, Color colour)
        {
            Bounds = rect;
            Colour = colour;

            _rectTexture = new Texture2D(Graphics, Bounds.Width, Bounds.Height);

            Color[] data = new Color[Bounds.Width * Bounds.Height];

            for (int i = 0; i < data.Length; i++)
                data[i] = Colour;

            _rectTexture.SetData(data);
        }

        public void Draw()
        {
            Vector2 CenterOrigin = Vector2.Zero;

            if (Centered)
                CenterOrigin = new Vector2(Bounds.Width * .5f, Bounds.Height * .5f);

            SpriteBatch.Draw(
                    _rectTexture,
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

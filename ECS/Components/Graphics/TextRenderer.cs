using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FedoraEngine.ECS.Components.Graphics
{
    public sealed class TextRenderer : Component, IDrawable
    {
        public string Text { get; set; }

        public SpriteFont Font { get; set; }

        public float RenderLayer { get; set; }

        public bool Sorting { get; set; }

        public float Rotation { get; set; } = 0f;

        public float Scale { get; set; } = 1f;

        public Vector2 Origin { get; set; } = Vector2.Zero;

        public Color Colour { get; set; } = Color.White;

        public SpriteEffects TextEffects;

        public TextRenderer(string text, SpriteFont font, Color colour, float scale = 1f, float rotation = 0f)
        {
            Text = text;
            Font = font;
            Colour = colour;
            Scale = scale;
            Rotation = rotation;
        }

        public TextRenderer(string text, SpriteFont font, Color colour, Vector2 offset, float scale = 1f, float rotation = 0f)
        {
            Text = text;
            Font = font;
            Colour = colour;
            Origin = offset;
            Scale = scale;
            Rotation = rotation;
        }

        public void Draw()
        {
            Core.SpriteBatch.DrawString(Font, Text, Position, Colour, Rotation, Origin, Scale, TextEffects, 0f);
        }
    }
}

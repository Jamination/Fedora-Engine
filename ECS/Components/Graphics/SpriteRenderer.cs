using Microsoft.Xna.Framework;
using FedoraEngine.Graphics;
using System;
using System.Drawing;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Point = Microsoft.Xna.Framework.Point;

namespace FedoraEngine.ECS.Components.Graphics
{
    public sealed class SpriteRenderer : Component, IDrawable
    {
        public Sprite Sprite { get; set; }

        public int RepeatAmountX { get; set; } = 0;

        public int RepeatAmountY { get; set; } = 0;

        public SpriteRenderer() { }

        public new Rectangle AABB => new Rectangle(Position.ToPoint(), new Point((int)Sprite.Bounds.Width, (int)Sprite.Bounds.Height));

        public float RenderLayer { get; set; }

        public bool Sorting { get; set; } = true;

        public SpriteRenderer(Sprite sprite)
        {
            Sprite = sprite;
        }

        public void Draw()
        {
            if (Sprite == null)
                return;

            Vector2 centerOrigin = Vector2.Zero;

            if (Sprite.Centered)
                centerOrigin = new Vector2(Sprite.SourceRect.Width * Sprite.Scale.X * .5f, Sprite.SourceRect.Height * Sprite.Scale.Y * .5f);

            var spriteRect = new RectangleF(Position.X - centerOrigin.X + Sprite.Bounds.X, Position.Y - centerOrigin.Y + Sprite.Bounds.Y, Sprite.Bounds.Width * Scale.X, Sprite.Bounds.Height * Scale.Y);
            if (Sprite == null || !spriteRect.IntersectsWith(new RectangleF(Scene.MainCamera.ScreenBounds.X, Scene.MainCamera.ScreenBounds.Y, Scene.MainCamera.ScreenBounds.Width, Scene.MainCamera.ScreenBounds.Height)))
            {
                return;
            }

            SpriteBatch.Draw(
                Sprite.Texture,
                new Vector2(
                    (int)Math.Round(Transform.Position.X + (int)Math.Round(Sprite.Origin.X)),
                    (int)Math.Round(Transform.Position.Y) + (int)Math.Round(Sprite.Origin.Y)),
                Sprite.SourceRect,
                Sprite.Colour,
                Transform.Rotation + Sprite.Rotation,
                Sprite.Origin + centerOrigin,
                Transform.LocalScale * Sprite.Scale,
                Sprite.Flip,
                Sprite.LayerDepth
            );
        }
    }
}

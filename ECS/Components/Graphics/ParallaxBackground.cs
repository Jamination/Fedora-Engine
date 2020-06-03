using FedoraEngine.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace FedoraEngine.ECS.Components.Graphics
{
    public sealed class ParallaxBackground : Component, IDrawable
    {
        public Vector2 ParallaxAmount { get; set; } = new Vector2();

        public float RenderLayer { get; set; } = -100f;

        public bool Sorting { get; set; } = false;

        public int TileX { get; set; } = 1;

        public int TileY { get; set; } = 1;

        public Sprite Sprite { get; set; }

        public ParallaxBackground(float xAmount, float yAmount, Sprite sprite)
        {
            ParallaxAmount = new Vector2(xAmount, yAmount);
            Sprite = sprite;
        }

        public ParallaxBackground(Vector2 amount, Sprite sprite)
        {
            ParallaxAmount = amount;
            Sprite = sprite;
        }

        public ParallaxBackground(float xAmount, float yAmount, Sprite sprite, int tileX, int tileY)
        {
            ParallaxAmount = new Vector2(xAmount, yAmount);
            Sprite = sprite;
            TileX = tileX;
            TileY = tileY;
        }

        public ParallaxBackground(Vector2 amount, Sprite sprite, int tileX, int tileY)
        {
            ParallaxAmount = amount;
            Sprite = sprite;
            TileX = tileX;
            TileY = tileY;
        }

        public void Draw()
        {
            if (Sprite == null)
                return;

            Vector2 centerOrigin = Vector2.Zero;

            if (Sprite.Centered)
                centerOrigin = new Vector2(Sprite.SourceRect.Width * Sprite.Scale.X * TileX * .5f, Sprite.SourceRect.Height * TileY * Sprite.Scale.Y * .5f);

            SpriteBatch.Draw(
                Sprite.Texture,
                new Vector2(
                    (int)Math.Round(Transform.Position.X + (int)Math.Round(Sprite.Origin.X) - SceneCamera.Transform.Translation.X * ParallaxAmount.X),
                    (int)Math.Round(Transform.Position.Y) + (int)Math.Round(Sprite.Origin.Y) - SceneCamera.Transform.Translation.Y * ParallaxAmount.Y),
                new Rectangle(Sprite.SourceRect.X, Sprite.SourceRect.Y, Sprite.SourceRect.Width * TileX, Sprite.SourceRect.Height * TileY),
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

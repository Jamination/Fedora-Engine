using FedoraEngine.Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FedoraEngine.UI
{
    public sealed class Stage
    {
        public SpriteFont Font;

        public SpriteBatch SpriteBatch => Core.SpriteBatch;

        private float _tick = 0f;

        public void SetupFonts(SpriteFont font)
        {
            Font = font;
        }

        public bool DoButton(string text, Vector2 position, int width, int height, Vector2 origin, Color colour, Color hoveringColour, Color pressedColour, bool centered = true, float scale = 1f, float rotation = 0f, SpriteEffects effects = SpriteEffects.None)
        {
            bool pressed = false;
            bool heldDown = false;
            bool hovering = false;

            float hoverScaleBob = 0f;

            Vector2 centerOrigin = Vector2.Zero;

            var buttonRect = new Rectangle((int)position.X, (int)position.Y, width, height);

            if (centered)
                centerOrigin = new Vector2(buttonRect.Width * scale * .5f, buttonRect.Height * scale * .5f);

            buttonRect.Location -= centerOrigin.ToPoint();

            if (buttonRect.Contains(Input.WorldMousePosition))
            {
                hovering = true;
                hoverScaleBob = ((float)Math.Sin(_tick) + .5f) * .1f;
                if (Input.IsLeftMouseReleased())
                    pressed = true;
                else if (Input.IsLeftMouseDown())
                    heldDown = true;
            }

            if (pressed || heldDown)
                SpriteBatch.DrawString(Font, text, position, pressedColour, rotation, origin + centerOrigin, scale + hoverScaleBob, effects, 0f);
            else if (hovering)
                SpriteBatch.DrawString(Font, text, position, hoveringColour, rotation, origin + centerOrigin, scale + hoverScaleBob, effects, 0f);
            else
                SpriteBatch.DrawString(Font, text, position, colour, rotation, origin + centerOrigin, scale, effects, 0f);

            _tick += .05f;

            return pressed;
        }
    }
}

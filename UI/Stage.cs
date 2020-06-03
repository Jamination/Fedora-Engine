using FedoraEngine.Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable enable

namespace FedoraEngine.UI
{
    public sealed class Stage
    {
        public SpriteFont? Font;

        public SpriteBatch SpriteBatch => Core.SpriteBatch;

        private float _tick = 0f;

        public int HotItem { get; private set; } = -1;

        public void SetupFonts(SpriteFont font)
        {
            Font = font;
        }

        public void DoText(string text, Vector2 position, Vector2 origin, Color colour, bool centered = true, float scale = 1f, float rotation = 0f, SpriteEffects spriteEffects = SpriteEffects.None, float depth = 0f)
        {
            Vector2 centerOrigin = Vector2.Zero;

            if (centered)
                centerOrigin = Font!.MeasureString(text) * .5f;

            SpriteBatch.DrawString(Font, text, position, colour, rotation, origin + centerOrigin, scale, spriteEffects, depth);
        }

        public bool DoButton(int id, string text, Vector2 position, Vector2 origin, Color colour, Color hoveringColour, Color pressedColour, bool centered = true, float scale = 1f, float rotation = 0f, SoundEffect? hoverSound = null, SoundEffect? pressSound = null, float hoverVolume = 1f, float pressVolume = 1f, SpriteEffects spriteEffects = SpriteEffects.None, float depth = 0f)
        {
            bool pressed = false;
            bool heldDown = false;
            bool hovering = false;

            float hoverScaleBob = 0f;

            Vector2 centerOrigin = Vector2.Zero;

            var buttonRect = new Rectangle((int)position.X, (int)position.Y, (int)Font!.MeasureString(text).X * (int)scale, (int)Font.MeasureString(text).Y * (int)scale);

            if (centered)
                centerOrigin = new Vector2((buttonRect.Width / scale) * .5f, (buttonRect.Height / scale) * .5f);

            buttonRect.Location -= centerOrigin.ToPoint() * new Point((int)scale, (int)scale);

            if (buttonRect.Contains(Input.WorldMousePosition))
            {
                hovering = true;
                hoverScaleBob = ((float)Math.Sin(_tick) + .5f) * .1f;
                if (Input.IsLeftMouseReleased())
                    pressed = true;
                else if (Input.IsLeftMouseDown())
                    heldDown = true;
            }

            if (pressed)
            {
                var pressSoundEffectInstance = pressSound?.CreateInstance();
                pressSoundEffectInstance!.Volume = pressVolume;
                pressSoundEffectInstance?.Play();
            }
            else if (hovering && HotItem != id)
            {
                var hoverSoundEffectInstance = hoverSound?.CreateInstance();
                hoverSoundEffectInstance!.Volume = hoverVolume;
                hoverSoundEffectInstance?.Play();
            }

            if (hovering)
                HotItem = id;

            if (pressed || heldDown)
                SpriteBatch.DrawString(Font, text, position, pressedColour, rotation, origin + centerOrigin, scale * .8f, spriteEffects, depth);
            else if (hovering)
                SpriteBatch.DrawString(Font, text, position, hoveringColour, rotation, origin + centerOrigin, scale + hoverScaleBob, spriteEffects, depth);
            else
                SpriteBatch.DrawString(Font, text, position, colour, rotation, origin + centerOrigin, scale, spriteEffects, depth);

            _tick += .05f;

            return pressed;
        }
    }
}

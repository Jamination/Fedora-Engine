using FedoraEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FedoraEngine.ECS.Components.Graphics
{
    public sealed class SpriteAnimator : Component, IUpdateable
    {
        public Dictionary<string, Animation> Animations { get; private set; }

        public SpriteRenderer SpriteRenderer => Entity.GetComponent<SpriteRenderer>();

        public int CurrentFrame { get; set; }

        public bool Playing { get; private set; } = false;

        public string CurrentAnimation;

        public float TimeScale = 1f;

        private float _timer = 0f;

        public bool FlipX = false;

        public bool IsPlaying(string animation)
        {
            return CurrentAnimation == animation;
        }

        public SpriteAnimator(Dictionary<string, Animation> animations, string currentAnimation)
        {
            Animations = animations;
            CurrentAnimation = currentAnimation;
        }

        public Animation Play(string animation)
        {
            Playing = true;

            if (Equals(animation, Animations[CurrentAnimation]))
                return Animations[CurrentAnimation];

            CurrentAnimation = animation;
            _timer = 0;
            CurrentFrame = 0;

            return Animations[CurrentAnimation];
        }

        public void Stop()
        {
            CurrentAnimation = "";
            Playing = false;
            _timer = 0;
            CurrentFrame = 0;
        }

        public void StopOnCurrentFrame()
        {
            CurrentAnimation = "";
            Playing = false;
            _timer = 0;
        }

        public void Update()
        {
            if (Playing && CurrentAnimation != "")
            {
                _timer += Time.DeltaTime * 32f * TimeScale;

                if (_timer > Animations[CurrentAnimation].FrameSpeed)
                {
                    _timer = 0f;

                    CurrentFrame++;

                    if (CurrentFrame >= Animations[CurrentAnimation].FrameCount)
                    {
                        if (Animations[CurrentAnimation].Looping)
                            CurrentFrame = 0;
                        else
                            StopOnCurrentFrame();
                    }
                }

                if (FlipX)
                    Animations[CurrentAnimation].Sprite.Flip = SpriteEffects.FlipHorizontally;
                else
                    Animations[CurrentAnimation].Sprite.Flip = SpriteEffects.None;

                Animations[CurrentAnimation].Sprite.SourceRect = new Rectangle(
                    CurrentFrame * Animations[CurrentAnimation].FrameWidth,
                    0,
                    Animations[CurrentAnimation].FrameWidth,
                    Animations[CurrentAnimation].FrameHeight
                );
                SpriteRenderer.Sprite = Animations[CurrentAnimation].Sprite;
            }
        }
    }
}

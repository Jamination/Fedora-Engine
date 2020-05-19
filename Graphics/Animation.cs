namespace FedoraEngine.Graphics
{
    public readonly struct Animation
    {
        public readonly Sprite Sprite;

        public int FrameCount { get; }

        public float FrameSpeed { get; }

        public bool Looping { get; }

        public int FrameWidth => Sprite.Texture.Width / FrameCount;

        public int FrameHeight => Sprite.Texture.Height;

        public Animation(Sprite sprite, int frameCount, float frameSpeed, bool looping)
        {
            Sprite = sprite;
            FrameCount = frameCount;
            FrameSpeed = frameSpeed;
            Looping = looping;
        }
    }
}

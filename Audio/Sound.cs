using Microsoft.Xna.Framework.Audio;

namespace FedoraEngine.Audio
{
    public struct Sound
    {
        public SoundEffect SoundEffect { get; set; }

        public bool Looping { get; set; }

        public float Volume { get; set; }

        public float Pitch { get; set; }

        public float Pan { get; set; }

        public Sound(string filePath, bool looping = false, float volume = 1f, float pitch = 0f, float pan = 0f)
        {
            SoundEffect = Core.Scene.Content.Load<SoundEffect>(filePath);
            Looping = looping;
            Volume = volume;
            Pitch = pitch;
            Pan = pan;
        }
    }
}

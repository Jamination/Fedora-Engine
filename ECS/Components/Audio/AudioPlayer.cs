using FedoraEngine.Audio;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace FedoraEngine.ECS.Components.Audio
{
    public sealed class AudioPlayer : Component, IUpdateable
    {
        public Dictionary<string, Sound> Sounds { get; private set; }

        public HashSet<SoundEffectInstance> CurrentlyPlayingSoundEffects { get; private set; }

        private Queue<SoundEffectInstance> _soundsToRemove;

        public AudioPlayer(Dictionary<string, Sound> sounds)
        {
            Sounds = sounds;
            CurrentlyPlayingSoundEffects = new HashSet<SoundEffectInstance>();
            _soundsToRemove = new Queue<SoundEffectInstance>();
        }

        public AudioPlayer(string key, Sound sound)
        {
            Sounds = new Dictionary<string, Sound>();
            CurrentlyPlayingSoundEffects = new HashSet<SoundEffectInstance>();
            _soundsToRemove = new Queue<SoundEffectInstance>();
            Sounds.Add(key, sound);
        }

        public void AddSound(string key, Sound sound)
        {
            Sounds.Add(key, sound);
        }

        public void RemoveSound(string key)
        {
            Sounds.Remove(key);
        }

        public void Play(string key)
        {
            var sound = Sounds[key];
            var soundEffectInstance = sound.SoundEffect.CreateInstance();
            soundEffectInstance.IsLooped = sound.Looping;
            soundEffectInstance.Volume = sound.Volume;
            soundEffectInstance.Pitch = sound.Pitch;
            soundEffectInstance.Pan = sound.Pan;
            soundEffectInstance.Play();
            CurrentlyPlayingSoundEffects.Add(soundEffectInstance);
        }

        public override void OnRemovedFromEntity()
        {
            foreach (var sound in CurrentlyPlayingSoundEffects)
            {
                sound.Stop();
            }

            CurrentlyPlayingSoundEffects.Clear();
            Sounds.Clear();
        }

        public void Update()
        {
            foreach (var sound in CurrentlyPlayingSoundEffects)
            {
                if (sound.State == SoundState.Stopped)
                    _soundsToRemove.Enqueue(sound);

                if (Core.Scene.Paused)
                    sound.Pause();
            }

            foreach (var sound in _soundsToRemove)
                CurrentlyPlayingSoundEffects.Remove(sound);
        }
    }
}

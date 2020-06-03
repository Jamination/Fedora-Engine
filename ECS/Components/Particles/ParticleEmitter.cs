using FedoraEngine.Particles;
using FedoraEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace FedoraEngine.ECS.Components.Particles
{
    public sealed class ParticleEmitter : Component, IUpdateable, IDrawable
    {
        public HashSet<Particle> Particles { get; private set; }

        public Queue<Particle> ParticlesToRemove { get; private set; }

        public int LifeTime { get; set; } = 60;

        public int Amount { get; set; } = 1;

        public float RenderLayer { get; set; }

        public bool Sorting { get; set; }

        public Texture2D[] ParticleTextures { get; set; }

        private int _timer = 0;

        public Vector2 ParticleVelocity { get; set; }

        public Vector2 ParticleVelocityRandomness { get; set; }

        public float ParticleScale { get; set; } = 1f;

        public float ParticleScaleRandomness { get; set; }

        public float ParticleRotation { get; set; } = 0f;

        public float ParticleRotationRandomness { get; set; }

        public float ParticleAngularVelocity { get; set; } = 0f;

        public float ParticleAngularVelocityRandomness { get; set; }

        public Color ParticleColour { get; set; } = Color.White;

        //public Color ParticleColourRandomness { get; set; }

        public int ParticleCount => Particles.Count;

        public int ParticleFrequency { get; set; } = 1;

        public int ParticleFrequencyRandomness { get; set; }

        public float ScaleDecrease { get; set; } = 0f;

        public Vector2 LinearAccel { get; set; }

        public bool Emitting { get; set; } = false;

        public ParticleEmitter(Texture2D[] particleTextures, int lifeTime, Vector2 velocity, float scale, float angularVelocity)
        {
            ParticleTextures = particleTextures;
            LifeTime = lifeTime;
            ParticleVelocity = velocity;
            ParticleScale = scale;
            ParticleAngularVelocity = angularVelocity;

            Particles = new HashSet<Particle>();
            ParticlesToRemove = new Queue<Particle>();
        }

        public void Update()
        {
            if (Emitting)
            {
                if (_timer >= ParticleFrequency)
                {
                    for (int i = 0; i < Amount; i++)
                        GenerateParticle();
                    _timer = 0;
                }
            }

            foreach (var particle in Particles)
            {
                if (particle.LifeTime <= 0)
                {
                    ParticlesToRemove.Enqueue(particle);
                    continue;
                }
                particle.LifeTime--;
                particle.Velocity += LinearAccel;
                particle.Position += particle.Velocity * Time.DeltaTime;
                particle.Rotation += particle.AngularVelocity;
                particle.Scale -= ScaleDecrease * Time.DeltaTime;
                if (particle.Scale <= 0f)
                    ParticlesToRemove.Enqueue(particle);
            }

            foreach (var particleToRemove in ParticlesToRemove)
            {
                Particles.Remove(particleToRemove);
            }

            ParticlesToRemove.Clear();

            if (Emitting)
                _timer++;
            else
                _timer = 0;
        }

        private void GenerateParticle()
        {
            var random = new Random();
            Particles.Add(new Particle(
                Transform.Position,
                ParticleTextures[random.Next(ParticleTextures.Length)],
                ParticleScale + ((float)random.NextDouble() - .5f) * ParticleScaleRandomness,
                ParticleRotation + ((float)random.NextDouble() - .5f) * ParticleRotationRandomness,
                LifeTime,
                ParticleVelocity + new Vector2(((float)random.NextDouble() - .5f) * ParticleVelocityRandomness.X, ((float)random.NextDouble() - .5f) * ParticleVelocityRandomness.Y),
                ParticleAngularVelocity + ((float)random.NextDouble() - .5f) * ParticleAngularVelocityRandomness,
                ParticleColour
            ));
        }

        public void Draw()
        {
            foreach (var particle in Particles)
            {
                SpriteBatch.Draw(
                    particle.Texture,
                    particle.Position,
                    null,
                    particle.Colour,
                    particle.Rotation,
                    new Vector2(particle.Texture.Width * .5f, particle.Texture.Height * .5f),
                    particle.Scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }
}

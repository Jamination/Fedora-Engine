using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FedoraEngine.Particles
{
    public sealed class Particle
    {
        public int LifeTime { get; set; }

        public Vector2 Velocity { get; set; }

        public Vector2 Position { get; set; }

        public float Scale { get; set; }

        public float AngularVelocity { get; set; }

        public float Rotation { get; set; }

        public Color Colour { get; set; }

        public Texture2D Texture { get; set; }

        public Particle(Vector2 position, Texture2D texture, float scale, float rotation, int lifeTime, Vector2 velocity, float angularVelocity, Color colour)
        {
            Position = position;
            Texture = texture;
            Scale = scale;
            Rotation = rotation;
            LifeTime = lifeTime;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            Colour = colour;
        }
    }
}

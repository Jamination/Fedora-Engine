using Microsoft.Xna.Framework;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Utilities;

namespace FedoraEngine.ECS.Components.Physics
{
    public sealed class PhysicsBody : Component, IUpdateable
    {
        public Body Body { get; set; }

        public PhysicsBody(Body body)
        {
            Body = body;
        }

        public void Update()
        {
            Transform.LocalPosition = ConvertUnits.ToDisplayUnits(Body.Position);
            Transform.LocalRotation = Body.Rotation;
        }
    }
}

using FedoraEngine.ECS.Components.Collision;
using FedoraEngine.ECS.Entities;
using FedoraEngine.ECS.Systems;
using FedoraEngine.Utils;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FedoraEngine.ECS.Components.Physics
{
    public class Mover : Component
    {
        public bool IsOnFloor { get; private set; } = false;

        public bool HasCollidedThisFrame = false;

        public List<Entity> EntitiesCollidedWithThisFrame;

        public override void OnAddedToEntity()
        {
            EntitiesCollidedWithThisFrame = new List<Entity>();
        }

        public void Move(ref Vector2 velocity)
        {
            CalculateVelocity(ref velocity);
            Position += velocity * Time.DeltaTime;
        }

        public void CalculateVelocity(ref Vector2 velocity)
        {
            HasCollidedThisFrame = false;
            IsOnFloor = false;

            EntitiesCollidedWithThisFrame.Clear();

            var collider = GetComponent<BoxCollider>();

            if (!collider.Collidable)
                return;

            foreach (var entCollider in CollisionSystem.BoxColliders)
            {
                if (entCollider == null || entCollider.CollisionLayer != collider.CollisionLayer || entCollider.Entity == Entity || !entCollider.Collidable || !entCollider.Entity.Enabled)
                    continue;

                var entMover = entCollider.GetComponent<Mover>();
                if (entMover != null && entMover.EntitiesCollidedWithThisFrame != null)
                {
                    if (entMover.EntitiesCollidedWithThisFrame.Contains(Entity))
                        continue;
                }

                var newPosition = Position;
                var newVelocity = velocity;

                if (CollisionSystem.IsTouchingLeft(collider, entCollider, velocity.X))
                {
                    newPosition = new Vector2(entCollider.GlobalAABB.Left - collider.AABB.Location.X - collider.GlobalAABB.Width * .5f, newPosition.Y);
                    newVelocity.X = 0f;
                    HasCollidedThisFrame = true;
                }
                else if (CollisionSystem.IsTouchingRight(collider, entCollider, velocity.X))
                {
                    newPosition = new Vector2(entCollider.GlobalAABB.Right + collider.AABB.Location.X + collider.GlobalAABB.Width * .5f, newPosition.Y);
                    newVelocity.X = 0f;
                    HasCollidedThisFrame = true;
                }

                if (CollisionSystem.IsTouchingTop(collider, entCollider, velocity.Y))
                {
                    IsOnFloor = true;
                    newPosition = new Vector2(newPosition.X, entCollider.GlobalAABB.Top - collider.AABB.Location.Y - collider.GlobalAABB.Height * .5f);
                    newVelocity.Y = 0f;
                    HasCollidedThisFrame = true;
                }
                else if (CollisionSystem.IsTouchingBottom(collider, entCollider, velocity.Y))
                {
                    newPosition = new Vector2(newPosition.X, entCollider.GlobalAABB.Bottom - collider.AABB.Location.Y + collider.GlobalAABB.Height * .5f);
                    newVelocity.Y = 0f;
                    HasCollidedThisFrame = true;
                }

                if (HasCollidedThisFrame)
                    EntitiesCollidedWithThisFrame.Add(entCollider.Entity);

                Position = newPosition;
                velocity = newVelocity;
            }
        }
    }
}

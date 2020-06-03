using FedoraEngine.ECS.Entities;
using FedoraEngine.ECS.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

#nullable enable

namespace FedoraEngine.ECS.Components
{
    public abstract class Component
    {
        public Entity Entity;

        public Scene Scene => Entity.Scene;

        public Camera SceneCamera => Scene.MainCamera;

        private bool _enabled = true;

        protected Rectangle aabb;

        public Entity? Parent => Entity.Parent?.Entity;

        public Rectangle AABB
        {
            get => new Rectangle(Position.ToPoint(), aabb.Size);
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (_enabled)
                    OnEnabled();
                else
                    OnDisabled();
            }
        }

        public Transform Transform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Entity.Transform;
            set => Entity.Transform = value;
        }

        public Vector2 Position
        {
            get => Transform.Position;
            set => Transform.Position = value;
        }

        public Vector2 Scale
        {
            get => Transform.Scale;
            set => Transform.Scale = value;
        }

        public float Rotation
        {
            get => Transform.Rotation;
            set => Transform.Rotation = value;
        }

        public Component()
        {
            if (Entity == null)
                Entity = new Entity("N/A");
        }

        protected GraphicsDevice Graphics => Core.Graphics.GraphicsDevice;

        protected SpriteBatch SpriteBatch => Core.SpriteBatch;

        public T GetComponent<T>() where T : Component
        {
            return Entity.GetComponent<T>();
        }

        public virtual void OnLoad() { }

        public virtual void OnAddedToEntity() { }

        public virtual void OnRemovedFromEntity() { }

        public virtual void OnTransformChanged() { }

        public virtual void OnPositionChanged() { }

        public virtual void OnRotationChanged() { }

        public virtual void OnScaleChanged() { }

        public virtual void OnEntityEnabled() { }

        public virtual void OnEntityDisabled() { }

        public virtual void OnEnabled() { }

        public virtual void OnDisabled() { }
    }
}

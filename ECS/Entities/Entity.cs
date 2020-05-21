using FedoraEngine.ECS.Components;
using FedoraEngine.ECS.Scenes;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FedoraEngine.ECS.Entities
{
    public sealed class Entity
    {
        public List<Component> Components { get; private set; }

        public readonly string Name;

        public uint Id;

        public Transform Transform;

        public bool Enabled = true;

        public Scene Scene => Core.Scene;

        public Transform Parent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.Parent;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Transform.SetParent(value);
        }

        public List<Transform> Children
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Transform.Children;
        }

        public Entity(string name)
        {
            Name = name;
            Components = new List<Component>();
            Transform = new Transform(this);
        }

        public Entity SetParent(Entity parent)
        {
            Transform.SetParent(parent.Transform);
            return this;
        }

        public T GetComponent<T>() where T : Component
        {
            return (T)Components.FirstOrDefault(c => typeof(T).IsAssignableFrom(c.GetType()));
        }

        public bool HasComponent<T>() where T : Component
        {
            return Components.FirstOrDefault(c => typeof(T).IsAssignableFrom(c.GetType())) != null;
        }

        public IList<T> GetComponentsOfType<T>() where T : Component
        {
            var components = new List<T>();
            foreach (var component in Components)
            {
                if (component is T)
                    components.Add((T)component);
            }
            return components;
        }

        public IList<Component> GetComponents()
        {
            return Components;
        }

        public T AddComponent<T>(T component) where T : Component
        {
            component.Entity = this;
            Components.Add(component);
            component.OnAddedToEntity();
            return component;
        }

        public void OnTransformChanged(Transform.Component componentType)
        {
            foreach (var component in Components)
                component.OnTransformChanged();

            switch (componentType)
            {
                case Transform.Component.Position:
                    foreach (var component in Components)
                        component.OnPositionChanged();
                    break;
                case Transform.Component.Rotation:
                    foreach (var component in Components)
                        component.OnRotationChanged();
                    break;
                case Transform.Component.Scale:
                    foreach (var component in Components)
                        component.OnScaleChanged();
                    break;
            }
        }

        public Entity AddComponents(IEnumerable<Component> components)
        {
            var enumerable = components as Component[] ?? components.ToArray();
            foreach (var component in enumerable)
            {
                component.Entity = this;
                Components.Add(component);
            }

            foreach (var component in enumerable)
            {
                component.OnAddedToEntity();
            }
            return this;
        }

        public void UpdateComponents()
        {
            if (!Enabled)
                return;

            foreach (var component in Components)
            {
                if (!component.Enabled) continue;
                if (component is IUpdateable updateableComponent)
                {
                    updateableComponent.Update();
                }
            }
        }

        public void DrawComponents()
        {
            if (!Enabled)
                return;

            foreach (var component in Components)
            {
                if (!component.Enabled) continue;
                if (component is IDrawable drawableComponent)
                {
                    drawableComponent.Draw();
                }
            }
        }

        public void Destroy()
        {
            Enabled = false;
            foreach (var component in Components)
            {
                component.OnRemovedFromEntity();
            }
            Components.Clear();
        }
    }
}

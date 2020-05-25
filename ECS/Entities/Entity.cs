using FedoraEngine.ECS.Components;
using FedoraEngine.ECS.Scenes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FedoraEngine.ECS.Entities
{
    public sealed class Entity : IDisposable
    {
        public List<Component> Components { get; private set; }

        public readonly string Name;

        public uint Id;

        public Transform Transform;

        private bool _enabled = true;

        public float RenderLayer = 0f;

        public bool Sorting = true;

        public Rectangle AABB
        {
            get
            {
                var rect = new Rectangle(Transform.Position.ToPoint(), new Point());
                foreach (var component in Components)
                    rect = Rectangle.Union(component.AABB, rect);
                return rect;
            }
        }

        public List<Component> DrawableComponents { get; private set; }

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

        private bool _destroying = false;

        public bool Destroyed { get; private set; } = false;

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

        public bool Global { get; set; } = false;

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

        public void OnEnabled()
        {
            foreach (var component in Components)
                component.OnEntityEnabled();
        }

        public void OnDisabled()
        {
            foreach (var component in Components)
                component.OnEntityDisabled();
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
            if (_destroying)
                Dispose();

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

            DrawableComponents = Components.AsEnumerable().Where(c => typeof(IDrawable).IsAssignableFrom(c.GetType())).ToList();
            DrawableComponents.Sort((layer1, layer2) => ((IDrawable)layer1).RenderLayer.CompareTo(((IDrawable)layer2).RenderLayer));

            foreach (var component in DrawableComponents)
            {
                if (!component.Enabled) continue;
                ((IDrawable)component).Draw();
            }
        }

        public void Destroy()
        {
            _destroying = true;
        }

        public void Dispose()
        {
            Enabled = false;
            foreach (var component in Components)
            {
                component.OnRemovedFromEntity();
            }
            Components.Clear();

            _destroying = false;
            Destroyed = true;
        }
    }
}

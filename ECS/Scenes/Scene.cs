using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FedoraEngine.ECS.Entities;
using FedoraEngine.ECS.Systems;
using FedoraEngine.ECS.Components;
using FedoraEngine.Utils;
using VelcroPhysics.Dynamics;

namespace FedoraEngine.ECS.Scenes
{
    public class Scene : IDisposable
    {
        public Color ClearColour = new Color(.2f, .2f, .25f, 1f);

        public readonly List<Entity> Entities;

        public readonly List<ProcessingSystem> Systems;

        public CollisionSystem CollisionSystem;

        public readonly Camera MainCamera;

        public readonly Core CurrentCore;

        public bool Paused = false;

        public World World { get; set; }

        public GraphicsDevice Graphics
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Core.Graphics.GraphicsDevice;
        }

        public SpriteBatch SpriteBatch
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Core.SpriteBatch;
        }

        public BetterContentManager Content { get; protected set; }

        public Scene(Core core)
        {
            CurrentCore = core;
            Content = new BetterContentManager(Core.Services, "Content");
            Systems = new List<ProcessingSystem>();
            Entities = new List<Entity>();

            MainCamera = new Camera();

            CollisionSystem = (CollisionSystem)RegisterSystem(new CollisionSystem());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Load() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reload()
        {
            foreach (var entity in Entities)
                entity.Destroy();

            Entities.Clear();
            World.Clear();

            Core.GlobalDebugCollisionsEnabled = false;
            Load();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity AddEntity(Entity entity)
        {
            Entities.Add(entity);
            entity.Id = (uint)Entities.Count;

            foreach (var child in entity.Transform.Children)
                AddEntity(child.Entity);

            foreach (var processingSystem in Systems)
            {
                var system = (EntityProcessingSystem)processingSystem;
                system.OnEntityAddedToScene(entity);
            }

            return entity;
        }

        public Entity RemoveEntity(Entity entity)
        {
            Entities.Remove(entity);
            foreach (var processingSystem in Systems)
            {
                var system = (EntityProcessingSystem)processingSystem;
                system.OnEntityRemovedFromScene(entity);
            }

            return entity;
        }

        public Entity DestroyEntity(Entity entity)
        {
            entity.Destroy();
            RemoveEntity(entity);
            return entity;
        }

        public Entity RemoveEntity(string entityName)
        {
            foreach (var entity in Entities)
            {
                if (entity.Name == entityName)
                    RemoveEntity(entity);
                return entity;
            }
            Console.WriteLine($"Could not remove entity: {entityName}");
            return null;
        }

        public Entity DestroyEntity(string entityName)
        {
            foreach (var entity in Entities)
            {
                if (entity.Name == entityName)
                {
                    entity.Destroy();
                    RemoveEntity(entity);
                }
                return entity;
            }
            Console.WriteLine($"Could not destroy entity: {entityName}");
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ProcessingSystem RegisterSystem(ProcessingSystem system)
        {
            Systems.Add(system);
            return system;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity FindEntity(string name)
        {
            foreach (var entity in Entities)
            {
                if (entity.Name == name)
                    return entity;
            }
            Console.WriteLine($"Could not find entity {name}.");
            return null;
        }

        public virtual void Update()
        {
            if (Paused)
                return;

            if (World != null)
                World.Step((float)Core.GameTime.ElapsedGameTime.TotalMilliseconds * .001f);

            foreach (var processingSystem in Systems)
                processingSystem.Update();

            foreach (EntityProcessingSystem entitySystem in Systems)
                entitySystem.Update(Entities);

            var entitiesToDestroy = new HashSet<Entity>();

            foreach (var entity in Entities)
            {
                entity.UpdateComponents();

                if (entity.Destroyed)
                    entitiesToDestroy.Add(entity);
            }

            foreach (var entity in entitiesToDestroy)
                Entities.Remove(entity);

            MainCamera.Update();
        }

        public virtual void Draw()
        {
            Graphics.Clear(ClearColour);

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateTranslation((int)Math.Round(MainCamera.Transform.Translation.X), (int)Math.Round(MainCamera.Transform.Translation.Y), 0));

            foreach (var entity in Entities)
                entity.DrawComponents();
        }

        public void Dispose()
        {
            foreach (var entity in Entities)
                entity.Destroy();
            Entities.Clear();

            Content.Unload();
            Content.Dispose();
        }
    }
}

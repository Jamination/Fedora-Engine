﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FedoraEngine.ECS.Entities;
using FedoraEngine.ECS.Systems;
using FedoraEngine.ECS.Components;
using FedoraEngine.Utils;
using VelcroPhysics.Dynamics;
using FedoraEngine.Graphics;
using System.Linq;
using FedoraEngine.UI;

namespace FedoraEngine.ECS.Scenes
{
    public class Scene : IDisposable
    {
        public Color ClearColour = new Color(64, 64, 64);

        public readonly List<Entity> Entities;

        public readonly HashSet<ProcessingSystem> Systems;

        public CollisionSystem CollisionSystem;

        public Camera MainCamera { get; private set; }

        public readonly Core CurrentCore;

        public bool Paused = false;

        public bool Loaded { get; protected set; } = false;

        public bool MouseEnabled
        {
            get => Core.Instance.IsMouseVisible;
            set => Core.Instance.IsMouseVisible = value;
        }

        public SortModes SortMode { get; set; } = SortModes.None;

        public Vector2 ScreenCentre => Core.ScreenCentre;

        public Stage Stage { get; set; }

        public World World { get; set; }

        private bool _reloadingDeferred = false;

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
            Systems = new HashSet<ProcessingSystem>();
            Entities = new List<Entity>();

            Stage = new Stage();

            MainCamera = new Camera();

            CollisionSystem = (CollisionSystem)RegisterSystem(new CollisionSystem());
        }

        public void ChangeScene(Scene scene)
        {
            Core.ChangeScene(scene);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Load()
        {
            _reloadingDeferred = false;
            foreach (var entity in Entities)
            {
                foreach (var component in entity.Components)
                    component.OnLoad();
            }
            Loaded = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reload()
        {
            foreach (var entity in Entities)
            {
                entity.Destroy();
                entity.Dispose();
            }

            Entities.Clear();

            if (World != null)
                World.Clear();

            Content.Unload();
            Content.Dispose();
            Content = new BetterContentManager(Core.Services, "Content");

            Core.GlobalDebugCollisionsEnabled = false;
            MainCamera = new Camera();
            Load();
        }

        public void ReloadDeferred()
        {
            _reloadingDeferred = true;
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

        public Entity FindGlobalEntity(string name)
        {
            return Core.Instance.FindGlobalEntity(name);
        }

        public virtual void Update()
        {
            if (_reloadingDeferred)
                Reload();
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

            SpriteBatch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointWrap, transformMatrix: Matrix.CreateTranslation((int)Math.Round(MainCamera.Transform.Translation.X), (int)Math.Round(MainCamera.Transform.Translation.Y), 0));

            switch (SortMode)
            {
                case SortModes.YSort:
                    foreach (var entity in Entities.Where(e => e.Sorting))
                        entity.RenderLayer = entity.Transform.Position.Y;
                    break;
                case SortModes.DescendingYSort:
                    foreach (var entity in Entities.Where(e => e.Sorting))
                        entity.RenderLayer = -entity.Transform.Position.Y;
                    break;
                case SortModes.XSort:
                    foreach (var entity in Entities.Where(e => e.Sorting))
                        entity.RenderLayer = entity.Transform.Position.X;
                    break;
                case SortModes.DescendingXSort:
                    foreach (var entity in Entities.Where(e => e.Sorting))
                        entity.RenderLayer = -entity.Transform.Position.X;
                    break;
            }

            Entities.Sort((ent1, ent2) => ent1.RenderLayer.CompareTo(ent2.RenderLayer));

            foreach (var entity in Entities)
                entity.DrawComponents();

            HandleUI();
        }

        public virtual void HandleUI() { }

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

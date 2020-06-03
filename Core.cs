using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FedoraEngine.ECS.Scenes;
using FedoraEngine.Engine.Input;
using System.Collections.Generic;
using FedoraEngine.ECS.Managers;
using System;
using FedoraEngine.Utils;
using MonoGame.ImGui;
using ImGuiNET;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using MonoGame.ImGui.Extensions;
using FedoraEngine.ECS.Entities;

namespace FedoraEngine
{
    public class Core : Game
    {
        public static GraphicsDeviceManager Graphics { get; protected set; }

        public static SpriteBatch SpriteBatch { get; protected set; }

        public static new BetterContentManager Content { get; protected set; }

        public new static GameServiceContainer Services => ((Game)_instance).Services;

        public static GameTime GameTime { get; protected set; }

        public static ImGUIRenderer ImGUIRenderer { get; protected set; }

        public static bool ExitOnEscapePress = true;

        public static Scene Scene { get; protected set; }

        public static Scene NextScene { get; protected set; }

        public static Core Instance => _instance;

        public static RenderTarget2D MainRenderTarget { get; protected set; }

        public static HashSet<Manager> Managers { get; protected set; }

        public static Vector2 ScreenCentre => new Vector2(Instance.GraphicsDevice.Viewport.Width * .5f, Instance.GraphicsDevice.Viewport.Height * .5f);

        public static Vector2 WorldScreenCentre => Scene.MainCamera.ScreenToWorld(ScreenCentre);

        public static HashSet<Entity> GlobalEntities { get; protected set; }

        private string _windowTitle;
        private readonly string _baseWindowTitle;

        private static bool _globalDebugCollisionsEnabled = false;

        public static uint StartWindowWidth { get; protected set; }

        public static uint StartWindowHeight { get; protected set; }

        public uint CurrentWindowWidth => (uint)Window.ClientBounds.Width;

        public uint CurrentWindowHeight => (uint)Window.ClientBounds.Height;

        private int _prevFps;
        private int _fps;

        private bool _fullscreen = false;

        private bool _steppedFrame = false;

        public static bool GlobalDebugCollisionsEnabled
        {
            get => _globalDebugCollisionsEnabled;
            set
            {
                _globalDebugCollisionsEnabled = value;
                Scene.CollisionSystem.UpdateDebugCollisions();
            }
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set { _windowTitle = value; Window.Title = value; }
        }

        private static bool _debugEnabled = false;

        public static bool DebugEnabled
        {
            get => _debugEnabled;
            set
            {
                bool prevDebugEnabled = _debugEnabled;
                _debugEnabled = value;
                if (!prevDebugEnabled && _debugEnabled)
                    Instance.IsMouseVisible = true;
                else
                    Instance.IsMouseVisible = !Instance.IsMouseVisible;
            }
        }

        public static bool PhysicsEnabled { get; set; } = false;

        private static Core _instance;

        public Core(uint windowWidth, uint windowHeight, string windowTitle, bool fullscreen = false)
        {
            _instance = this;

            StartWindowWidth = windowWidth;
            StartWindowHeight = windowHeight;
            _windowTitle = windowTitle;
            _fullscreen = fullscreen;

            IsMouseVisible = true;
            IsFixedTimeStep = true;

            Graphics = new GraphicsDeviceManager(this);

            Graphics.IsFullScreen = _fullscreen;
            Graphics.PreferredBackBufferWidth = (int)StartWindowWidth;
            Graphics.PreferredBackBufferHeight = (int)StartWindowHeight;
            Graphics.SynchronizeWithVerticalRetrace = true;
            Graphics.PreferMultiSampling = true;
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;

            Graphics.ApplyChanges();

            Content = new BetterContentManager(Services, "Content");

            Window.AllowUserResizing = true;
            Window.AllowAltF4 = true;

            _baseWindowTitle = windowTitle;

            MainRenderTarget = new RenderTarget2D(GraphicsDevice, (int)windowWidth, (int)windowHeight, false, SurfaceFormat.Color, DepthFormat.None, 1, RenderTargetUsage.DiscardContents);
            
            Managers = new HashSet<Manager>();
            GlobalEntities = new HashSet<Entity>();

            ImGUIRenderer = new ImGUIRenderer(Instance);
            ImGUIRenderer.Initialize();
            ImGUIRenderer.RebuildFontAtlas();
        }

        public Entity FindGlobalEntity(string name)
        {
            foreach (var entity in GlobalEntities)
            {
                if (entity.Name == name)
                    return entity;
            }
            Console.WriteLine($"Could not find entity {name}.");
            return null;
        }

        public static void Quit()
        {
            foreach (var manager in Managers)
                manager.OnQuit();
            Instance.Exit();
        }

        protected override void Initialize()
        {
            foreach (var manager in Managers)
                manager.OnInitialize();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            foreach (var manager in Managers)
                manager.OnLoad();
            Scene.Load();
        }

        protected override void UnloadContent()
        {
            Scene.Dispose();
            NextScene = null;

            Content.Unload();
            Content.Dispose();
        }

        public static void ChangeScene(Scene scene)
        {
            NextScene = scene;
        }

        public static void RegisterGlobalManager(Manager manager)
        {
            Managers.Add(manager);
        }

        public static void AddGlobalEntity(Entity entity)
        {
            GlobalEntities.Add(entity);
            entity.Global = true;
        }

        public static void RemoveGlobalEntity(Entity entity)
        {
            entity.Destroy();
            entity.Dispose();
            GlobalEntities.Remove(entity);
        }

        protected override void Update(GameTime gameTime)
        {
            GameTime = gameTime;

            Input.UpdateState();

            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Input.IsKeyDown(Input.KeyMap["quit"])) && ExitOnEscapePress)
                Exit();

#if DEBUG
            if (Input.IsKeyPressed(Input.KeyMap["reloadScene"]) && !DebugEnabled)
                Scene.Reload();

            if (Input.IsKeyPressed(Input.KeyMap["toggleDebugCollisions"]))
                GlobalDebugCollisionsEnabled = !GlobalDebugCollisionsEnabled;

            if (Input.IsKeyPressed(Input.KeyMap["pauseScene"]))
                Scene.Paused = !Scene.Paused;

            if (Input.IsKeyPressed(Input.KeyMap["toggleImGui"]))
                DebugEnabled = !DebugEnabled;
#endif

            if (Input.IsKeyPressed(Input.KeyMap["toggleFullScreen"]))
                Graphics.ToggleFullScreen();

            if (NextScene != null)
            {
                Scene.Dispose();
                Scene = NextScene;
                NextScene = null;
                foreach (var manager in Managers)
                    manager.OnSceneChanged(Scene);
                Scene.Load();
            }

            foreach (var manager in Managers)
                manager.Update(Scene.Systems);

            foreach (var entity in GlobalEntities)
                entity.UpdateComponents();

            if (Input.IsKeyPressed(Input.KeyMap["stepForwardOneFrame"]) && Scene.Paused)
            {
                Scene.Paused = false;
                Scene.Update();
                Scene.Paused = true;
            }
            else if (!Scene.Paused)
                Scene.Update();

#if DEBUG
            _prevFps = _fps;
            _fps = (int)Math.Ceiling(1000 / gameTime.ElapsedGameTime.TotalMilliseconds);

            if (_fps != _prevFps)
                WindowTitle = $"{_baseWindowTitle} : {_fps} fps";
#endif
            base.Update(gameTime);
        }

        protected virtual void BuildImGui(GameTime gameTime)
        {
            ImGui.BeginMainMenuBar();
            ImGui.Checkbox("Debug Draw", ref _globalDebugCollisionsEnabled);
            ImGui.EndMainMenuBar();

            ImGui.Begin("Scene Settings");
            ImGui.Checkbox("Paused", ref Scene.Paused);
            var clearColour = new System.Numerics.Vector4(Scene.ClearColour.ToVector3().ToNumericVector3(), Scene.ClearColour.A / 255);
            ImGui.ColorEdit4("Clear Colour", ref clearColour);
            Scene.ClearColour = new Color(clearColour.X, clearColour.Y, clearColour.Z, clearColour.W);
            ImGui.End();

            ImGui.Begin("Entity Tree");

            var entitiesToDestroy = new HashSet<Entity>();

            if (GlobalEntities.Count > 0 && ImGui.CollapsingHeader("Global Entities"))
            {
                foreach (var entity in GlobalEntities)
                {
                    ImGui.Bullet();
                    if (ImGui.CollapsingHeader($"{entity.Name}{entity.Id}"))
                    {
                        ImGui.Indent();

                        bool enabled = entity.Enabled;
                        ImGui.Checkbox("Enabled", ref enabled);
                        if (entity.Enabled != enabled)
                            entity.Enabled = enabled;

                        if (ImGui.Button("Destroy"))
                            entitiesToDestroy.Add(entity);

                        ImGui.Spacing();

                        ImGui.InputFloat("Render Layer", ref entity.RenderLayer);
                        ImGui.Checkbox("Sorting", ref entity.Sorting);

                        ImGui.Spacing();

                        ImGui.Text($"AABB: {entity.AABB}");

                        ImGui.TextColored(new System.Numerics.Vector4(Color.Yellow.ToVector3().ToNumericVector3(), 1f), "Transform:");
                        ImGui.Indent();

                        Vector2 tempPos = entity.Transform.LocalPosition;

                        ImGui.InputFloat("Position X", ref tempPos.X);
                        ImGui.InputFloat("Position Y", ref tempPos.Y);

                        if (entity.Transform.LocalPosition != tempPos)
                            entity.Transform.LocalPosition = tempPos;

                        ImGui.NewLine();

                        Vector2 tempScale = entity.Transform.LocalScale;

                        ImGui.InputFloat("Scale X", ref tempScale.X);
                        ImGui.InputFloat("Scale Y", ref tempScale.Y);

                        if (entity.Transform.LocalScale != tempScale)
                            entity.Transform.LocalScale = tempScale;

                        ImGui.NewLine();

                        float tempRotation = entity.Transform.LocalRotation;

                        ImGui.InputFloat("Rotation", ref tempRotation);

                        if (entity.Transform.LocalRotation != tempRotation)
                            entity.Transform.LocalRotation = tempRotation;

                        ImGui.Unindent();

                        ImGui.NewLine();

                        ImGui.TextColored(new System.Numerics.Vector4(Color.Yellow.ToVector3().ToNumericVector3(), 1f), "Components:");
                        ImGui.Indent();

                        foreach (var component in entity.Components)
                        {
                            if (ImGui.CollapsingHeader(component.GetType().Name))
                            {
                                ImGui.Indent();
                                foreach (var property in component.GetType().GetProperties())
                                {
                                    ImGui.Bullet();
                                    if (property.GetValue(component) is int)
                                    {
                                        int value = (int)property.GetValue(component);
                                        ImGui.InputInt(property.Name, ref value);
                                        if (property.SetMethod != null && property.SetMethod.IsPublic)
                                            property.SetValue(component, value);
                                    }
                                    else if (property.GetValue(component) is float)
                                    {
                                        float value = (float)property.GetValue(component);
                                        ImGui.InputFloat(property.Name, ref value);
                                        if (property.SetMethod != null && property.SetMethod.IsPublic)
                                            property.SetValue(component, value);
                                    }
                                    else if (property.GetValue(component) is bool)
                                    {
                                        bool value = (bool)property.GetValue(component);
                                        ImGui.Checkbox(property.Name, ref value);
                                        if (property.SetMethod != null && property.SetMethod.IsPublic)
                                            property.SetValue(component, value);
                                    }
                                    else if (property.GetValue(component) is string)
                                    {
                                        string value = (string)property.GetValue(component);
                                        ImGui.InputTextMultiline(property.Name, ref value, 1000, new System.Numerics.Vector2(value.Length * 10f, 200f));
                                        if (property.SetMethod != null && property.SetMethod.IsPublic)
                                            property.SetValue(component, value);
                                    }
                                    else if (property.GetValue(component) is Vector2)
                                    {
                                        Vector2 value = (Vector2)property.GetValue(component);
                                        ImGui.InputFloat($"{property.Name} X", ref value.X);
                                        ImGui.Bullet();
                                        ImGui.InputFloat($"{property.Name} Y", ref value.Y);
                                        if (property.SetMethod != null && property.SetMethod.IsPublic)
                                            property.SetValue(component, value);
                                    }
                                    else if (property.GetValue(component) is Color)
                                    {
                                        Color value = (Color)property.GetValue(component);
                                        System.Numerics.Vector4 colour = new System.Numerics.Vector4(value.ToVector3().ToNumericVector3(), value.A / 255);
                                        ImGui.ColorPicker4(property.Name, ref colour);
                                        if (property.SetMethod != null && property.SetMethod.IsPublic)
                                            property.SetValue(component, new Color(colour.X, colour.Y, colour.Z, colour.W));
                                    }
                                    else
                                        ImGui.TextWrapped($"{property.Name}: {property.GetValue(component)}");
                                    ImGui.NewLine();
                                }
                                ImGui.Unindent();
                            }
                        }

                        ImGui.Unindent();

                        ImGui.Unindent();
                    }
                }
            }

            if (Scene.Entities.Count > 0 && ImGui.CollapsingHeader("Scene Entities"))
            {
                foreach (var entity in Scene.Entities)
                {
                    ImGui.Bullet();
                    if (ImGui.CollapsingHeader($"{entity.Name}{entity.Id}"))
                    {
                        ImGui.Indent();

                        bool enabled = entity.Enabled;
                        ImGui.Checkbox("Enabled", ref enabled);
                        if (entity.Enabled != enabled)
                            entity.Enabled = enabled;

                        if (ImGui.Button("Destroy"))
                            entitiesToDestroy.Add(entity);

                        ImGui.Spacing();

                        ImGui.InputFloat("Render Layer", ref entity.RenderLayer);
                        ImGui.Checkbox("Sorting", ref entity.Sorting);

                        ImGui.Spacing();

                        ImGui.Text($"AABB: {entity.AABB}");

                        ImGui.TextColored(new System.Numerics.Vector4(Color.Yellow.ToVector3().ToNumericVector3(), 1f), "Transform:");
                        ImGui.Indent();

                        Vector2 tempPos = entity.Transform.LocalPosition;

                        ImGui.InputFloat("Position X", ref tempPos.X);
                        ImGui.InputFloat("Position Y", ref tempPos.Y);

                        if (entity.Transform.LocalPosition != tempPos)
                            entity.Transform.LocalPosition = tempPos;

                        ImGui.NewLine();

                        Vector2 tempScale = entity.Transform.LocalScale;

                        ImGui.InputFloat("Scale X", ref tempScale.X);
                        ImGui.InputFloat("Scale Y", ref tempScale.Y);

                        if (entity.Transform.LocalScale != tempScale)
                            entity.Transform.LocalScale = tempScale;

                        ImGui.NewLine();

                        float tempRotation = entity.Transform.LocalRotation;

                        ImGui.InputFloat("Rotation", ref tempRotation);

                        if (entity.Transform.LocalRotation != tempRotation)
                            entity.Transform.LocalRotation = tempRotation;

                        ImGui.Unindent();

                        ImGui.NewLine();

                        ImGui.TextColored(new System.Numerics.Vector4(Color.Yellow.ToVector3().ToNumericVector3(), 1f), "Components:");
                        ImGui.Indent();

                        foreach (var component in entity.Components)
                        {
                            if (ImGui.CollapsingHeader(component.GetType().Name))
                            {
                                ImGui.Indent();
                                foreach (var property in component.GetType().GetProperties())
                                {
                                    ImGui.Bullet();
                                    if (property.GetValue(component) is int)
                                    {
                                        int value = (int)property.GetValue(component);
                                        ImGui.InputInt(property.Name, ref value);
                                        if (property.SetMethod != null && property.SetMethod.IsPublic)
                                            property.SetValue(component, value);
                                    }
                                    else if (property.GetValue(component) is float)
                                    {
                                        float value = (float)property.GetValue(component);
                                        ImGui.InputFloat(property.Name, ref value);
                                        if (property.SetMethod != null && property.SetMethod.IsPublic)
                                            property.SetValue(component, value);
                                    }
                                    else if (property.GetValue(component) is bool)
                                    {
                                        bool value = (bool)property.GetValue(component);
                                        ImGui.Checkbox(property.Name, ref value);
                                        if (property.SetMethod != null && property.SetMethod.IsPublic)
                                            property.SetValue(component, value);
                                    }
                                    else if (property.GetValue(component) is string)
                                    {
                                        string value = (string)property.GetValue(component);
                                        ImGui.InputTextMultiline(property.Name, ref value, 1000, new System.Numerics.Vector2(value.Length * 10f, 200f));
                                        if (property.SetMethod != null && property.SetMethod.IsPublic)
                                            property.SetValue(component, value);
                                    }
                                    else if (property.GetValue(component) is Vector2)
                                    {
                                        Vector2 value = (Vector2)property.GetValue(component);
                                        ImGui.InputFloat($"{property.Name} X", ref value.X);
                                        ImGui.Bullet();
                                        ImGui.InputFloat($"{property.Name} Y", ref value.Y);
                                        if (property.SetMethod != null && property.SetMethod.IsPublic)
                                            property.SetValue(component, value);
                                    }
                                    else if (property.GetValue(component) is Color)
                                    {
                                        Color value = (Color)property.GetValue(component);
                                        System.Numerics.Vector4 colour = new System.Numerics.Vector4(value.ToVector3().ToNumericVector3(), value.A / 255);
                                        ImGui.ColorPicker4(property.Name, ref colour);
                                        if (property.SetMethod != null && property.SetMethod.IsPublic)
                                            property.SetValue(component, new Color(colour.X, colour.Y, colour.Z, colour.W));
                                    }
                                    else
                                        ImGui.TextWrapped($"{property.Name}: {property.GetValue(component)}");
                                    ImGui.NewLine();
                                }
                                ImGui.Unindent();
                            }
                        }

                        ImGui.Unindent();

                        ImGui.Unindent();
                    }
                }
            }

            ImGui.End();

            foreach (var entity in entitiesToDestroy)
                entity.Destroy();
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.SetRenderTarget(MainRenderTarget);
            foreach (var entity in GlobalEntities)
                entity.DrawComponents();
            Scene.Draw();
            Scene.SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);

            float outputAspectRatio = Window.ClientBounds.Width / (float)Window.ClientBounds.Height;
            float preferredAspectRatio = StartWindowWidth / (float)StartWindowHeight;

            Rectangle dst;
            if (outputAspectRatio <= preferredAspectRatio)
            {
                // output is taller than it is wider, bars on top/bottom
                int presentHeight = (int)((Window.ClientBounds.Width / preferredAspectRatio) + 0.5f);
                int barHeight = (Window.ClientBounds.Height - presentHeight) / 2;
                dst = new Rectangle(0, barHeight, Window.ClientBounds.Width, presentHeight);
            }
            else
            {
                // output is wider than it is tall, bars left/right
                int presentWidth = (int)((Window.ClientBounds.Height * preferredAspectRatio) + 0.5f);
                int barWidth = (Window.ClientBounds.Width - presentWidth) / 2;
                dst = new Rectangle(barWidth, 0, presentWidth, Window.ClientBounds.Height);
            }

            GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1f, 0);
            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default);
            SpriteBatch.Draw(MainRenderTarget, dst, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            SpriteBatch.End();

            GraphicsDevice.Textures[0] = null;

#if DEBUG
            if (DebugEnabled)
            {
                ImGUIRenderer.BeginLayout(gameTime);
                BuildImGui(gameTime);
                ImGUIRenderer.EndLayout();
            }
#endif
        }
    }
}

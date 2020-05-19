using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FedoraEngine.ECS.Scenes;
using FedoraEngine.Engine.Input;
using System.Collections.Generic;
using FedoraEngine.ECS.Managers;
using System;
using FedoraEngine.Utils;

namespace FedoraEngine
{
    public class Core : Game
    {
        public static GraphicsDeviceManager Graphics { get; protected set; }

        public static SpriteBatch SpriteBatch { get; protected set; }

        public static new BetterContentManager Content { get; protected set; }

        public new static GameServiceContainer Services => ((Game)_instance).Services;

        public static GameTime GameTime { get; protected set; }

        public static bool ExitOnEscapePress = true;

        public static Scene Scene { get; protected set; }

        public static Scene NextScene { get; protected set; }

        public static Core Instance => _instance;

        public static RenderTarget2D MainRenderTarget { get; protected set; }

        public static List<Manager> Managers { get; protected set; }

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
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            Managers = new List<Manager>();

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

        protected override void Update(GameTime gameTime)
        {
            GameTime = gameTime;

            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Input.IsKeyDown(Input.KeyMap["quit"])) && ExitOnEscapePress)
                Exit();

            if (Input.IsKeyPressed(Input.KeyMap["reloadScene"]))
                Scene.Reload();

            if (Input.IsKeyPressed(Input.KeyMap["toggleDebugCollisions"]))
                GlobalDebugCollisionsEnabled = !GlobalDebugCollisionsEnabled;

            if (NextScene != null)
            {
                Scene.Dispose();
                Scene = NextScene;
                NextScene = null;
            }

            foreach (var manager in Managers)
                manager.Update(Scene.Systems);

            Scene.Update();

#if DEBUG
            _prevFps = _fps;
            _fps = (int)Math.Ceiling(1000 / gameTime.ElapsedGameTime.TotalMilliseconds);

            if (_fps != _prevFps)
                WindowTitle = $"{_baseWindowTitle} : {_fps} fps";
#endif
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.SetRenderTarget(MainRenderTarget);
            Scene.Draw();
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
            SpriteBatch.Draw(MainRenderTarget, dst, Color.White);
            SpriteBatch.End();

            GraphicsDevice.Textures[0] = null;
        }
    }
}

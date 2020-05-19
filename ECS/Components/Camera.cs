using FedoraEngine.ECS.Entities;
using FedoraEngine.ECS.Scenes;
using FedoraEngine.Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FedoraEngine.ECS.Components
{
    public class Camera
    {
        public Matrix Transform;

        private Matrix _position;
        
        private readonly Matrix _zoom;

        private readonly Matrix _rotation;

        private Matrix _offset;

        private readonly float _zoomLevel = 1f;

        private const float MaxSpeed = 12f;
        private const float Speed = 600f;

        private float _drag = .05f;

        private float _currentSpeed = Speed;

        private bool _debugMode = false;

        private readonly bool _centered = true;
        
        public Rectangle Bounds { get; set; }

        public Scene Scene => Core.Scene;

        public Rectangle ScreenBounds
        {
            get
            {
                if (Scene != null)
                    return new Rectangle(
                        (int) -_position.Translation.X - (int) _offset.Translation.X - 32,
                        (int) -_position.Translation.Y - (int) _offset.Translation.Y - 32,
                        (int) Scene.CurrentCore.CurrentWindowWidth / (int) _zoomLevel + 128,
                        (int) Scene.CurrentCore.CurrentWindowHeight / (int) _zoomLevel + 128
                    );
                return default;
            }
        }

        public Camera()
        {
            new Rectangle(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue);

            _zoom = Matrix.CreateScale(_zoomLevel);
            _rotation = Matrix.CreateRotationZ(0f);
            if (_centered)
                _offset = Matrix.CreateTranslation((Core.StartWindowWidth / 2) / _zoomLevel, (Core.StartWindowHeight / 2) / _zoomLevel, 0f);
        }

        public void Follow(Entity entity)
        {
            if (!_debugMode && entity != null)
            {
                _position = Matrix.Lerp(_position, Matrix.CreateTranslation(
                    -entity.Transform.Position.X,
                    -entity.Transform.Position.Y,
                    0
                ), _drag);
            }
        }

        public void Update()
        {
#if DEBUG
            if (Input.IsKeyPressed(Input.KeyMap["toggleDebugCamera"]))
            {
                _debugMode = !_debugMode;
            }

            if (_debugMode)
            {
                Move();
            }
#endif
            //Position.Translation = Vector3.Clamp(Position.Translation, new Vector3(Bounds.X, Bounds.Y, 0f), new Vector3(Bounds.Width, Bounds.Height, 0f));
            //Position.Translation = new Vector3((int)Position.Translation.X, -360f, 0f);
            Transform = _zoom * _rotation * _position * _offset;
        }

        private void Move()
        {
            if (Input.IsKeyPressed(Input.KeyMap["debugCameraSprint"]))
            {
                _currentSpeed = MaxSpeed;
            }
            else
            {
                _currentSpeed = Speed;
            }

            if (Input.IsKeyPressed(Keys.A))
            {
                _position.Translation += new Vector3(_currentSpeed, 0f, 0f);
            }
            else if (Input.IsKeyPressed(Keys.D))
            {
                _position.Translation += new Vector3(-_currentSpeed, 0f, 0f);
            }

            if (Input.IsKeyPressed(Keys.W))
            {
                _position.Translation += new Vector3(0f, _currentSpeed, 0f);
            }
            else if (Input.IsKeyPressed(Keys.S))
            {
                _position.Translation += new Vector3(0f, -_currentSpeed, 0f);
            }
        }

        public static Vector2 ScreenToWorld(Vector2 onScreen, Matrix transform)
        {
            var matrix = Matrix.Invert(transform);
            return Vector2.Transform(onScreen, matrix);
        }
    }
}

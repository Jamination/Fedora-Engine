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

        public float DragX { get; set; } = .05f;

        public float DragY { get; set; } = .05f;

        private float _currentSpeed = Speed;

        private bool _debugMode = false;

        private readonly bool _centered = true;
        
        public Rectangle Bounds { get; set; }

        public Entity FollowTarget { get; private set; }

        public Scene Scene => Core.Scene;

        public bool LockXMovement { get; set; } = false;

        public bool LockYMovement { get; set; } = false;

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

            _position = Matrix.CreateTranslation(new Vector3(-Core.StartWindowWidth / 2, -Core.StartWindowHeight / 2, 0f));
            _zoom = Matrix.CreateScale(_zoomLevel);
            _rotation = Matrix.CreateRotationZ(0f);
            if (_centered)
                _offset = Matrix.CreateTranslation((Core.StartWindowWidth / 2) / _zoomLevel, (Core.StartWindowHeight / 2) / _zoomLevel, 0f);
        }

        public void Follow(Entity target)
        {
            if (!_debugMode && target != null)
            {
                FollowTarget = target;
            }
        }

        public void StopFollowing()
        {
            FollowTarget = null;
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

            if (FollowTarget != null)
            {
                if (!LockXMovement && !LockYMovement)
                {
                    _position = Matrix.Lerp(_position, Matrix.CreateTranslation(
                        -FollowTarget.Transform.Position.X,
                        _position.Translation.Y,
                        0
                    ), DragX);
                    _position = Matrix.Lerp(_position, Matrix.CreateTranslation(
                        _position.Translation.X,
                        -FollowTarget.Transform.Position.Y,
                        0
                    ), DragY);
                }
                else if (LockXMovement)
                {
                    _position = Matrix.Lerp(_position, Matrix.CreateTranslation(
                        _position.Translation.X,
                        -FollowTarget.Transform.Position.Y,
                        0
                    ), DragY);
                }
                else if (LockYMovement)
                {
                    _position = Matrix.Lerp(_position, Matrix.CreateTranslation(
                        -FollowTarget.Transform.Position.X,
                        _position.Translation.Y,
                        0
                    ), DragX);
                }
            }

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

        public Vector2 ScreenToWorld(Vector2 onScreen)
        {
            var matrix = Matrix.Invert(Transform);
            return Vector2.Transform(onScreen, matrix);
        }
    }
}

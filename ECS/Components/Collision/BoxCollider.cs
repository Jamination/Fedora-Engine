using FedoraEngine.ECS.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FedoraEngine.ECS.Components.Collision
{
    public sealed class BoxCollider : Component, IDrawable
    {
        private Rectangle _aabb;

        public Rectangle AABB
        {
            get { return _aabb; }
            set
            {
                _aabb = value;
                UpdateDebugTexture();
            }
        }

        public bool Collidable = true;
        public bool Centered = true;

        public CollisionSystem CollisionSystem => Scene.CollisionSystem;

        public HashSet<BoxCollider> Colliders => CollisionSystem.BoxColliders;

        public Rectangle GlobalAABB
        {
            get
            {
                if (Centered)
                    return new Rectangle(Transform.Position.ToPoint() + AABB.Location - new Point(AABB.Width * (int)Scale.X / 2, AABB.Height * (int)Scale.Y / 2), AABB.Size * Transform.Scale.ToPoint());
                else
                    return new Rectangle(Transform.Position.ToPoint() + AABB.Location, AABB.Size * Transform.Scale.ToPoint());
            }
        }

        public bool DebugRender = false;

        private Texture2D _debugRectTexture;

        public BoxCollider(int x, int y, ushort width, ushort height)
        {
            AABB = new Rectangle(x, y, width, height);
        }

        public BoxCollider(Rectangle boundingBox)
        {
            AABB = boundingBox;
        }

        private void UpdateDebugTexture()
        {
            _debugRectTexture = new Texture2D(Graphics, AABB.Width, AABB.Height);

            Color[] data = new Color[AABB.Width * AABB.Height];

            for (int i = 0; i < data.Length; i++)
                data[i] = new Color(1f, 0f, 0f, .1f);

            _debugRectTexture.SetData(data);
        }

        public void Draw()
        {
            if (DebugRender)
            {
                SpriteBatch.Draw(
                    _debugRectTexture,
                    GlobalAABB.Location.ToVector2(),
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    Transform.Scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }
}

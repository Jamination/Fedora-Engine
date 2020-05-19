using FedoraEngine.ECS.Components.Collision;
using FedoraEngine.ECS.Components.TileMap;
using FedoraEngine.ECS.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FedoraEngine.ECS.Components.Physics
{
    public class TileMover : Component
    {
        public bool IsOnFloor { get; private set; } = false;

        public bool IsOnCeiling { get; private set; } = false;

        public bool IsOnSlope { get; private set; } = false;

        public bool IsOnWall { get; private set; } = false;

        public bool IsOnLeftSlope { get; private set; } = false;

        public bool IsOnRightSlope { get; private set; } = false;

        public bool IsOnOneWayTile { get; private set; } = false;

        public BoxCollider Collider;

        public override void OnAddedToEntity()
        {
            Collider = GetComponent<BoxCollider>();
        }

        public void CalculateVelocity(ref Vector2 velocity)
        {
            IsOnFloor = false;
            IsOnWall = false;
            IsOnSlope = false;
            IsOnOneWayTile = false;

            IsOnRightSlope = false;
            IsOnLeftSlope = false;

            var neighbouringSlopeTiles = new List<OgmoSlopedTile>();

            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Left,
                Collider.GlobalAABB.Bottom - 1));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Left,
                Collider.GlobalAABB.Bottom - 8));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Left,
                Collider.GlobalAABB.Bottom));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Left,
                Collider.GlobalAABB.Bottom + 8));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Left + (int)(velocity.X * Time.DeltaTime),
                Collider.GlobalAABB.Bottom + 1));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Left + (int)(velocity.X * Time.DeltaTime),
                Collider.GlobalAABB.Bottom + 8));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Left,
                Collider.GlobalAABB.Bottom + 16));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Right,
                Collider.GlobalAABB.Bottom - 1));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Right,
                Collider.GlobalAABB.Bottom - 8));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Right,
                Collider.GlobalAABB.Bottom));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Right,
                Collider.GlobalAABB.Bottom + 8));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Right + (int)(velocity.X * Time.DeltaTime),
                Collider.GlobalAABB.Bottom + 1));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Right + (int)(velocity.X * Time.DeltaTime),
                Collider.GlobalAABB.Bottom + 8));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Right,
                Collider.GlobalAABB.Bottom + 16));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Left,
                Collider.GlobalAABB.Top - 1));
            neighbouringSlopeTiles.AddRange(CollisionSystem.GetSlopedTilesAt(Collider.GlobalAABB.Right,
                Collider.GlobalAABB.Top - 1));

            foreach (var tile in neighbouringSlopeTiles)
            {
                if (tile == null || tile.Type == new Vector2(-1, -1) || !tile.Collidable)
                    continue;

                if (CollisionSystem.IsRectTouchingBottom(Collider.GlobalAABB, tile.AABB, velocity.Y))
                {
                    IsOnCeiling = true;
                    velocity.Y = 0f;
                    Transform.Position = new Vector2(Transform.Position.X, tile.AABB.Bottom + Collider.GlobalAABB.Height * .5f);
                    continue;
                }

                if (tile.SlopeAngle < 0)
                {
                    if (tile is OgmoSlopedTile slopedTile)
                    {
                        float posX = slopedTile.AABB.Location.X - Collider.GlobalAABB.Right;

                        if (posX <= slopedTile.TileWidth && posX + 1 >= -slopedTile.AABB.Height && velocity.Y >= 0f && Collider.GlobalAABB.Bottom >= slopedTile.AABB.Top + posX + Collider.AABB.Height)
                        {
                            Position = new Vector2(Position.X, slopedTile.AABB.Location.Y - (posX * slopedTile.SlopeAngle));
                            velocity.Y = 0f;
                            IsOnSlope = true;
                            IsOnFloor = false;
                            IsOnRightSlope = true;
                        }
                    }
                }
                else if (tile.SlopeAngle > 0)
                {
                    if (tile is OgmoSlopedTile slopedTile)
                    {
                        float posX = (slopedTile.TileHeight * slopedTile.Transform.Scale.Y) + (slopedTile.AABB.Location.X - Collider.GlobalAABB.Left);

                        if (posX >= 0 && velocity.Y >= 0f && posX - 1 <= slopedTile.AABB.Height && Collider.GlobalAABB.Bottom >= slopedTile.AABB.Top - posX + Collider.AABB.Height)
                        {
                            Position = new Vector2(Position.X, slopedTile.AABB.Location.Y - (posX * slopedTile.SlopeAngle));
                            velocity.Y = 0f;
                            IsOnSlope = true;
                            IsOnFloor = false;
                            IsOnLeftSlope = true;
                        }
                    }
                }
            }

            IsOnCeiling = false;
            var neighbouringTiles = new List<OgmoTile>();

            neighbouringTiles.AddRange(CollisionSystem.GetTilesAt(Collider.GlobalAABB.Left + (int)(velocity.X * Time.DeltaTime),
                Collider.GlobalAABB.Top));
            neighbouringTiles.AddRange(CollisionSystem.GetTilesAt(Collider.GlobalAABB.Left + (int)(velocity.X * Time.DeltaTime),
                Collider.GlobalAABB.Bottom));
            neighbouringTiles.AddRange(CollisionSystem.GetTilesAt(Collider.GlobalAABB.Left + (int)(velocity.X * Time.DeltaTime),
                (int)Position.Y));

            neighbouringTiles.AddRange(CollisionSystem.GetTilesAt(Collider.GlobalAABB.Right + (int)(velocity.X * Time.DeltaTime),
                Collider.GlobalAABB.Top));
            neighbouringTiles.AddRange(CollisionSystem.GetTilesAt(Collider.GlobalAABB.Right + (int)(velocity.X * Time.DeltaTime),
                Collider.GlobalAABB.Bottom));
            neighbouringTiles.AddRange(CollisionSystem.GetTilesAt(Collider.GlobalAABB.Right + (int)(velocity.X * Time.DeltaTime),
                (int)Position.Y));

            neighbouringTiles.AddRange(CollisionSystem.GetTilesAt(Collider.GlobalAABB.Left,
                Collider.GlobalAABB.Bottom + (int)(velocity.Y * Time.DeltaTime)));
            neighbouringTiles.AddRange(CollisionSystem.GetTilesAt(Collider.GlobalAABB.Right,
                Collider.GlobalAABB.Bottom + (int)(velocity.Y * Time.DeltaTime)));
            neighbouringTiles.AddRange(CollisionSystem.GetTilesAt((int)Position.X,
                Collider.GlobalAABB.Bottom + (int)(velocity.Y * Time.DeltaTime)));

            neighbouringTiles.AddRange(CollisionSystem.GetTilesAt(Collider.GlobalAABB.Left,
                Collider.GlobalAABB.Top + (int)(velocity.Y * Time.DeltaTime)));
            neighbouringTiles.AddRange(CollisionSystem.GetTilesAt(Collider.GlobalAABB.Right,
                Collider.GlobalAABB.Top + (int)(velocity.Y * Time.DeltaTime)));
            neighbouringTiles.AddRange(CollisionSystem.GetTilesAt((int)Position.X,
                Collider.GlobalAABB.Top + (int)(velocity.Y * Time.DeltaTime)));

            foreach (var tile in neighbouringTiles)
            {
                if (tile == null || tile.Type == new Vector2(-1, -1) || !tile.Collidable || tile is OgmoSlopedTile)
                    continue;
                
                if (!tile.OneWay && CollisionSystem.IsRectTouchingLeft(Collider.GlobalAABB, tile.AABB, velocity.X))
                {
                    IsOnWall = true;
                    velocity.X = 0f;
                    Transform.Position = new Vector2(tile.AABB.Left - Collider.GlobalAABB.Width * .5f, Transform.Position.Y);
                }
                else if (!tile.OneWay && CollisionSystem.IsRectTouchingRight(Collider.GlobalAABB, tile.AABB, velocity.X))
                {
                    IsOnWall = true;
                    velocity.X = 0f;
                    Transform.Position = new Vector2(tile.AABB.Right + Collider.GlobalAABB.Width * .5f, Transform.Position.Y);
                }
                else if (!tile.OneWay && CollisionSystem.IsRectTouchingTop(Collider.GlobalAABB, tile.AABB, velocity.Y))
                {
                    IsOnFloor = true;
                    velocity.Y = 0f;
                    Transform.Position = new Vector2(Transform.Position.X, tile.AABB.Top - Collider.GlobalAABB.Height * .5f);
                }
                else if (!tile.OneWay && CollisionSystem.IsRectTouchingBottom(Collider.GlobalAABB, tile.AABB, velocity.Y))
                {
                    IsOnCeiling = true;
                    velocity.Y = 0f;
                    Transform.Position = new Vector2(Transform.Position.X, tile.AABB.Bottom + Collider.GlobalAABB.Height * .5f);
                }

                if (tile.OneWay && CollisionSystem.IsRectTouchingTop(Collider.GlobalAABB, tile.AABB, velocity.Y))
                {
                    if (Collider.GlobalAABB.Bottom <= tile.AABB.Top)
                    {
                        IsOnOneWayTile = true;
                        IsOnFloor = true;
                        velocity.Y = 0f;
                        Transform.Position = new Vector2(Transform.Position.X, tile.AABB.Top - Collider.GlobalAABB.Height * .5f);
                    }
                }
            }
        }
    }
}

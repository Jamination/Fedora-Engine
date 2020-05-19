using FedoraEngine.ECS.Components;
using FedoraEngine.ECS.Components.Collision;
using FedoraEngine.ECS.Components.TileMap;
using FedoraEngine.ECS.Entities;
using FedoraEngine.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FedoraEngine.ECS.Systems
{
    public sealed class CollisionSystem : EntityProcessingSystem
    {
        public static HashSet<BoxCollider> BoxColliders { get; private set; }

        public static HashSet<OgmoMap> Maps { get; private set; }

        public static HashSet<Component> TriggerableComponents { get; private set; }

        public CollisionSystem()
        {
            BoxColliders = new HashSet<BoxCollider>();
            TriggerableComponents = new HashSet<Component>();
            Maps = new HashSet<OgmoMap>();
        }

        public override void Update(IList<Entity> entities)
        {
            foreach (var component in TriggerableComponents)
            {
                var collider = component.GetComponent<BoxCollider>();

                if (collider == null)
                    continue;

                foreach (var entity in entities)
                {
                    if (component.Entity == entity)
                        continue;

                    var entityCollider = entity.GetComponent<BoxCollider>();

                    if (entityCollider == null)
                        continue;

                    if (collider.GlobalAABB.Intersects(entityCollider.GlobalAABB))
                        ((ITriggerListener)component).OnOverlap(entity);
                }
            }
        }

        public void UpdateDebugCollisions()
        {
            foreach (var collider in BoxColliders)
                collider.DebugRender = Core.GlobalDebugCollisionsEnabled;
        }

        public override void OnEntityAddedToScene(Entity entity)
        {
            var collider = entity.GetComponent<BoxCollider>();
            var map = entity.GetComponent<OgmoMap>();

            if (collider != null)
                BoxColliders.Add(collider);
            if (map != null)
                Maps.Add(map);

            foreach (var component in entity.Components)
            {
                if (typeof(ITriggerListener).IsAssignableFrom(component.GetType()) && component.GetComponent<BoxCollider>() != null)
                {
                    Console.WriteLine("AAAAAAAA");
                    TriggerableComponents.Add(component);
                }
            }
        }

        public override void OnEntityRemovedFromScene(Entity entity)
        {
            var collider = entity.GetComponent<BoxCollider>();
            var map = entity.GetComponent<OgmoMap>();

            if (collider != null)
                BoxColliders.Remove(collider);
            if (map != null)
                Maps.Remove(map);

            foreach (var component in TriggerableComponents)
            {
                if (component.Entity == entity)
                    TriggerableComponents.Remove(component);
            }
        }

        #region TileMapCollisions

        public static bool IsTileAt(int x, int y)
        {
            foreach (var map in Maps)
            {
                foreach (var layer in map.MapData.Layers)
                {
                    if (layer.IsTileAt(x, y))
                        return true;
                }
            }
            return false;
        }

        public static IList<OgmoTile> GetTilesAt(int x, int y)
        {
            var tiles = new List<OgmoTile>();

            foreach (var map in Maps)
            {
                foreach (var layer in map.MapData.Layers)
                {
                    if (layer != null)
                        tiles.Add(layer.GetTileAt(x, y));
                }
            }
            return tiles;
        }

        public static IList<OgmoSlopedTile> GetSlopedTilesAt(int x, int y)
        {
            var tiles = new List<OgmoSlopedTile>();

            foreach (var map in Maps)
            {
                foreach (var layer in map.MapData.Layers)
                {
                    if (layer.GetTileAt(x, y) is OgmoSlopedTile)
                    {
                        if (layer != null)
                            tiles.Add((OgmoSlopedTile)layer.GetTileAt(x, y));
                    }
                }
            }
            return tiles;
        }

        #endregion

        #region EntityCollisions

        public static bool IsTouchingLeft(BoxCollider collider1, BoxCollider collider2, float margin)
        {
            return collider1.GlobalAABB.Right + margin * Time.DeltaTime > collider2.GlobalAABB.Left &&
              collider1.GlobalAABB.Left < collider2.GlobalAABB.Left &&
              collider1.GlobalAABB.Bottom > collider2.GlobalAABB.Top &&
              collider1.GlobalAABB.Top < collider2.GlobalAABB.Bottom;
        }

        public static bool IsTouchingRight(BoxCollider collider1, BoxCollider collider2, float margin)
        {
            return collider1.GlobalAABB.Left + margin * Time.DeltaTime < collider2.GlobalAABB.Right &&
              collider1.GlobalAABB.Right > collider2.GlobalAABB.Right &&
              collider1.GlobalAABB.Bottom > collider2.GlobalAABB.Top &&
              collider1.GlobalAABB.Top < collider2.GlobalAABB.Bottom;
        }

        public static bool IsTouchingTop(BoxCollider collider1, BoxCollider collider2, float margin)
        {
            return collider1.GlobalAABB.Bottom + margin * Time.DeltaTime > collider2.GlobalAABB.Top &&
              collider1.GlobalAABB.Top < collider2.GlobalAABB.Top &&
              collider1.GlobalAABB.Right > collider2.GlobalAABB.Left &&
              collider1.GlobalAABB.Left < collider2.GlobalAABB.Right;
        }

        public static bool IsTouchingBottom(BoxCollider collider1, BoxCollider collider2, float margin)
        {
            return collider1.GlobalAABB.Top + margin * Time.DeltaTime < collider2.GlobalAABB.Bottom &&
              collider1.GlobalAABB.Bottom > collider2.GlobalAABB.Bottom &&
              collider1.GlobalAABB.Right > collider2.GlobalAABB.Left &&
              collider1.GlobalAABB.Left < collider2.GlobalAABB.Right;
        }

        public static bool IsRectTouchingLeft(Rectangle rect1, Rectangle rect2, float margin)
        {
            return rect1.Right + margin * Time.DeltaTime > rect2.Left &&
              rect1.Left < rect2.Left &&
              rect1.Bottom > rect2.Top &&
              rect1.Top < rect2.Bottom;
        }

        public static bool IsRectTouchingRight(Rectangle rect1, Rectangle rect2, float margin)
        {
            return rect1.Left + margin * Time.DeltaTime < rect2.Right &&
              rect1.Right > rect2.Right &&
              rect1.Bottom > rect2.Top &&
              rect1.Top < rect2.Bottom;
        }

        public static bool IsRectTouchingTop(Rectangle rect1, Rectangle rect2, float margin)
        {
            return rect1.Bottom + margin * Time.DeltaTime > rect2.Top &&
              rect1.Top < rect2.Top &&
              rect1.Right > rect2.Left &&
              rect1.Left < rect2.Right;
        }

        public static bool IsRectTouchingBottom(Rectangle rect1, Rectangle rect2, float margin)
        {
            return rect1.Top + margin * Time.DeltaTime < rect2.Bottom &&
              rect1.Bottom > rect2.Bottom &&
              rect1.Right > rect2.Left &&
              rect1.Left < rect2.Right;
        }

        #endregion
    }
}

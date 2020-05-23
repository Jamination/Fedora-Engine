using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;

namespace FedoraEngine.ECS.Components.TileMap
{
    public sealed class OgmoTileMapLayer : OgmoMapLayer, IDisposable
    {
        public OgmoTile[,] Tiles { get; private set; }

        public JArray DataCoords2D { get; private set; }

        public OgmoMap TileMap { get; set; }

        public readonly uint TileWidth;

        public readonly uint TileHeight;

        public string Name { get; private set; }

        public string Tileset { get; private set; }

        public Texture2D TileAtlas { get; private set; }

        public int RenderLayer { get; set; }

        public OgmoTileMapLayer(OgmoMap map, string name, JArray dataCoords2D, uint gridCellWidth, uint gridCellHeight, string tileset, Texture2D tileAtlas)
        {
            TileMap = map;
            Name = name;
            Tileset = tileset;
            TileAtlas = tileAtlas;
            TileWidth = gridCellWidth;
            TileHeight = gridCellHeight;
            DataCoords2D = dataCoords2D;
        }

        public bool IsTileAt(int x, int y)
        {
            if (TileMap != null)
            {
                var (posX, posY) = TileMap.Transform.Position;

                if (x < posX || y < posY || x >= posX + TileMap.MapData.Width * TileMap.Scale.X || y >= posY + TileMap.MapData.Height * TileMap.Scale.Y)
                    return false;
            }

            if (TileMap != null)
            {
                Vector2 tilePosition = new Vector2(x: x / TileWidth / TileMap.Scale.X, y: y / TileHeight / TileMap.Scale.Y);

                if (Tiles != null && Tiles[(int)Math.Floor(tilePosition.X), (int)Math.Floor(tilePosition.Y)].Type != new Vector2(-1f, -1f))
                    return true;
            }

            return false;
        }

        public OgmoTile GetTileAt(int x, int y)
        {
            if (TileMap != null)
            {
                Vector2 position = TileMap.Transform.Position;

                if (x <= position.X || y <= position.Y || x >= position.X + TileMap.MapData.Width * TileMap.Scale.X || y >= position.Y + TileMap.MapData.Height * TileMap.Scale.Y)
                    return null;
            }

            if (TileMap != null)
            {
                Vector2 tilePosition = new Vector2(x: x / (int)TileWidth / TileMap.Scale.X, y / (int)TileHeight / TileMap.Scale.Y);

                if (Tiles[(int)Math.Floor(tilePosition.X), (int)Math.Floor(tilePosition.Y)] != null)
                    return Tiles[(int)Math.Floor(tilePosition.X), (int)Math.Floor(tilePosition.Y)];
            }

            return null;
        }

        public override void Load(OgmoMap map)
        {
            TileMap = map;

            TileAtlas = TileMap.Entity.Scene.Content.LoadTexture($"Sprites/Tilesets/{Tileset}.png");

            Tiles = new OgmoTile[TileMap.MapData.Width / TileWidth, TileMap.MapData.Height / TileHeight];

            if (DataCoords2D != null)
            {
                int[][][] data = DataCoords2D.ToObject<int[][][]>();

                if (data != null)
                    for (int i = 0; i < data.Length; i++)
                    {
                        for (int j = 0; j < data[i].GetLength(0); j++)
                        {
                            if (data[i][j].Length > 1)
                            {
                                Vector2 type = new Vector2(data[i][j][0], data[i][j][1]);

                                bool isCollidableInDictionary =
                                    TileMap.NonCollidableTiles.TryGetValue(type, out var nonCollidable);
                                if (!isCollidableInDictionary)
                                    nonCollidable = false;

                                bool isSlopedInDictionary =
                                    TileMap.SlopedTiles.TryGetValue(type, out short slopeDirection);
                                if (!isSlopedInDictionary)
                                    slopeDirection = 0;

                                bool isOneWayInDictionary =
                                     TileMap.OneWayTiles.TryGetValue(type, out bool oneWay);
                                if (!isOneWayInDictionary)
                                    oneWay = false;

                                if (slopeDirection == 0)
                                    Tiles[j, i] = new OgmoTile(type, Color.White,
                                        new Vector2(j * TileWidth, i * TileHeight), TileMap.Transform, TileWidth,
                                        TileHeight, !nonCollidable, oneWay);
                                else
                                    Tiles[j, i] = new OgmoSlopedTile(type, Color.White,
                                        new Vector2(j * TileWidth, i * TileHeight), TileMap.Transform, TileWidth,
                                        TileHeight, !nonCollidable, slopeDirection, oneWay);
                            }
                            else
                                Tiles[j, i] = new OgmoTile(new Vector2(-1, -1), Color.White,
                                    new Vector2(j * TileWidth, i * TileHeight), TileMap.Transform, TileWidth,
                                    TileHeight, false, false);
                        }
                    }
            }
        }

        public override void Draw()
        {
            Vector2 centerOrigin = Vector2.Zero;

            if (TileMap != null && TileMap.Centered)
                centerOrigin = new Vector2(TileMap.MapData.Width * .5f, TileMap.MapData.Height * .5f);

            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    if (Tiles[x, y].Type == new Vector2(-1, -1) || !Core.Scene.MainCamera.ScreenBounds.Intersects(Tiles[x, y].AABB))
                        continue;

                    if (Core.GlobalDebugCollisionsEnabled && Tiles[x, y].Collidable)
                        Tiles[x, y].Colour = Color.HotPink;
                    else
                        Tiles[x, y].Colour = Color.White;

                    if (TileMap != null)
                        Core.SpriteBatch.Draw(
                            TileAtlas,
                            new Vector2(
                                x * TileWidth * (int)Math.Round(TileMap.Transform.Scale.X) + (int)Math.Round(TileMap.Transform.Position.X),
                                y * TileHeight * (int)Math.Round(TileMap.Transform.Scale.Y) + (int)Math.Round(TileMap.Transform.Position.Y)
                            ),
                            new Rectangle((int) Tiles[x, y].Type.X * (int) TileWidth,
                                (int) Tiles[x, y].Type.Y * (int) TileHeight, (int) TileWidth, (int) TileHeight),
                            Tiles[x, y].Colour,
                            0f,
                            new Vector2((int) centerOrigin.X, (int) centerOrigin.Y),
                            new Vector2((int) TileMap.Transform.Scale.X, (int) TileMap.Transform.Scale.Y),
                            SpriteEffects.None,
                            0f
                        );
                }
            }
        }

        public void Dispose()
        {
            TileAtlas.Dispose();
        }
    }
}

using Newtonsoft.Json;
using System.IO;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FedoraEngine.ECS.Components.TileMap
{
    public sealed class OgmoMap : Component, IUpdateable, IDrawable
    {
        public OgmoMapData MapData { get; private set; }

        public bool Centered { get; set; } = false;

        public float RenderLayer { get; set; }

        private string _filePath;

        public Dictionary<Vector2, bool> NonCollidableTiles;
        public Dictionary<Vector2, bool> OneWayTiles;
        public Dictionary<Vector2, short> SlopedTiles;

        public OgmoMap()
        {
            NonCollidableTiles = new Dictionary<Vector2, bool>();
            OneWayTiles = new Dictionary<Vector2, bool>();
            SlopedTiles = new Dictionary<Vector2, short>();
            MapData = new OgmoMapData(32, 32, new List<OgmoTileMapLayer>());
        }

        public void Reload()
        {
            LoadMap(_filePath);
        }

        public void LoadMap(string filePath)
        {
            _filePath = filePath;
            try
            {
                string jsonFromFile;
                using (var reader = new StreamReader(filePath))
                {
                    jsonFromFile = reader.ReadToEnd();
                    reader.Close();
                }

                MapData = JsonConvert.DeserializeObject<OgmoMapData>(jsonFromFile);

                MapData.Layers.Reverse();

                foreach (var layer in MapData.Layers)
                    layer.Load(this);
            } catch (JsonException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Update()
        {
            foreach (var layer in MapData.Layers)
                layer.Update();
        }

        public void Draw()
        {
            MapData.Layers.Sort((layer1, layer2) => layer1.RenderLayer.CompareTo(layer2.RenderLayer));

            foreach (var layer in MapData.Layers)
                layer.Draw();
        }
    }
}

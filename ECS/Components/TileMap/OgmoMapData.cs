using System;
using System.Collections.Generic;

namespace FedoraEngine.ECS.Components.TileMap
{
    public class OgmoMapData
    {
        public uint Width { get; private set; }

        public uint Height { get; private set; }

        public List<OgmoTileMapLayer> TileLayers { get; private set; }

        public OgmoMap TileMap { get; set; }

        public OgmoMapData(uint width, uint height, List<OgmoTileMapLayer> layers)
        {
            Width = width;
            Height = height;
            TileLayers = layers;
            Console.Write(width);
        }
    }
}

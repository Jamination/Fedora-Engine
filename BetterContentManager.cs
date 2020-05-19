using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace FedoraEngine
{
    public sealed class BetterContentManager : ContentManager
    {
        public new Dictionary<string, object> LoadedAssets;

        public BetterContentManager(IServiceProvider? serviceProvider, string rootDirectory) : base(serviceProvider, rootDirectory)
        {
            LoadedAssets = new Dictionary<string, object>();
        }

        public Texture2D LoadTexture(string assetPath)
        {
            if (LoadedAssets.ContainsKey(assetPath))
                return (Texture2D)LoadedAssets[assetPath];

            Texture2D texture;

            using (var reader = new StreamReader($"Content/{assetPath}"))
            {
                texture = Texture2D.FromStream(Core.Graphics?.GraphicsDevice, reader.BaseStream);
                reader.Close();
            }

            LoadedAssets.Add(assetPath, texture);

            return texture;
        }

        public override void Unload()
        {
            foreach (var asset in LoadedAssets.Values)
            {
                if (asset is GraphicsResource)
                {
                    ((GraphicsResource)asset).Dispose();
                }
            }
            base.Unload();
        }
    }
}

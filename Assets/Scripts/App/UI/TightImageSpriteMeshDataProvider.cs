using System;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public static class TightImageSpriteMeshDataProvider
    {
        private static readonly Dictionary<Sprite, SpriteMeshData> spriteMeshDataMap = new Dictionary<Sprite, SpriteMeshData>();

        public static SpriteMeshData GetSpriteMeshData(Sprite sprite)
        {
            if (sprite == null)
                throw new ArgumentNullException(nameof(sprite));

            if (!spriteMeshDataMap.TryGetValue(sprite, out SpriteMeshData spriteMeshData))
            {
                spriteMeshData = new SpriteMeshData();
                spriteMeshData.SpriteInstanceId = sprite.GetInstanceID();
                spriteMeshData.Triangles = sprite.triangles;
                spriteMeshData.UV = sprite.uv;
                spriteMeshData.Vertices = sprite.vertices;
                spriteMeshDataMap.Add(sprite, spriteMeshData);
            }

            return spriteMeshData;
        }

        public class SpriteMeshData
        {
            public int SpriteInstanceId;

            public ushort[] Triangles;

            public Vector2[] UV;

            public Vector2[] Vertices;
        }
    }
}

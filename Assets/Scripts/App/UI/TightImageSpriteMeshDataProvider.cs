using System;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public static class TightImageSpriteMeshDataProvider
    {
        private static readonly Dictionary<Sprite, SpriteMeshData> SpriteMeshDataMap = new Dictionary<Sprite, SpriteMeshData>();

        public static SpriteMeshData GetSpriteMeshData(Sprite sprite)
        {
            if (sprite == null)
                throw new ArgumentNullException(nameof(sprite));

            if (SpriteMeshDataMap.TryGetValue(sprite, out SpriteMeshData spriteMeshData))
                return spriteMeshData;

            spriteMeshData = new SpriteMeshData
            {
                SpriteInstanceId = sprite.GetInstanceID(),
                Triangles = sprite.triangles,
                UV = sprite.uv,
                Vertices = sprite.vertices
            };
            SpriteMeshDataMap.Add(sprite, spriteMeshData);

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

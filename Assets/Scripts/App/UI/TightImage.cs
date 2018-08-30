using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace LoomNetwork.CZB {
    public class TightImage : Image {
        protected override void OnPopulateMesh(VertexHelper toFill) {
            if (overrideSprite == null)
            {
                base.OnPopulateMesh(toFill);
            }
            else
            {
                switch (type)
                {
                    case Type.Simple:
                        if (overrideSprite.packed && overrideSprite.packingMode == SpritePackingMode.Rectangle) {
                            base.OnPopulateMesh(toFill);
                        } else {
                            GenerateTightMeshSprite(toFill, overrideSprite, preserveAspect);
                        }

                        break;
                    default:
                        base.OnPopulateMesh(toFill);
                        break;
                }
            }
        }

        private void GenerateTightMeshSprite(VertexHelper toFill, Sprite sprite, bool preserveAspect)
        {
            TightImageSpriteMeshDataProvider.SpriteMeshData spriteMeshData = 
                TightImageSpriteMeshDataProvider.GetSpriteMeshData(sprite);
            ushort[] spriteTriangles = spriteMeshData.Triangles;
            Vector2[] spriteUV = spriteMeshData.UV;
            Vector2[] spriteVertices = spriteMeshData.Vertices;
            
            toFill.Clear();

            Vector4 drawingDimensions = GetDrawingDimensions(preserveAspect);

            Vector2 spriteNormalizedPivot = new Vector2(
                sprite.pivot.x / sprite.rect.width,
                sprite.pivot.y / sprite.rect.height
            );
            
            Vector2 normalizeScaleFactor = new Vector2(
                1f / sprite.bounds.size.x,
                1f / sprite.bounds.size.y
            );

            Vector2 normalizeShift = new Vector2(
                spriteNormalizedPivot.x,
                spriteNormalizedPivot.y
            );

            Vector2 shift = new Vector2(
                drawingDimensions.x,
                drawingDimensions.y
            );

            Vector2 scaleFactor = new Vector2(
                drawingDimensions.z - drawingDimensions.x,
                drawingDimensions.w - drawingDimensions.y
            );

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;
            int spriteVerticesLength = spriteVertices.Length;
            for (int i = 0; i < spriteVerticesLength; i++) {
                Vector2 spriteVertex = spriteVertices[i];

                spriteVertex.x *= normalizeScaleFactor.x;
                spriteVertex.y *= normalizeScaleFactor.y;

                spriteVertex.x += normalizeShift.x;
                spriteVertex.y += normalizeShift.y;

                spriteVertex.x *= scaleFactor.x;
                spriteVertex.y *= scaleFactor.y;

                spriteVertex.x += shift.x;
                spriteVertex.y += shift.y;

                vertex.position = spriteVertex;
                vertex.uv0 = spriteUV[i];
                
                toFill.AddVert(vertex);
            }

            int spriteTrianglesLength = spriteTriangles.Length;
            for (int i = 0; i < spriteTrianglesLength; i += 3) {
                toFill.AddTriangle(spriteTriangles[i], spriteTriangles[i + 1], spriteTriangles[i + 2]);
            }
        }

        /// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
        private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
        {
            Sprite activeSprite = overrideSprite;
            Vector2 size = activeSprite == null ? Vector2.zero : new Vector2(activeSprite.rect.width, activeSprite.rect.height);

            Rect r = GetPixelAdjustedRect();
            // Debug.Log(string.Format("r:{2}, size:{0}, padding:{1}", size, padding, r));

            if (shouldPreserveAspect && size.sqrMagnitude > 0.0f)
            {
                float spriteRatio = size.x / size.y;
                float rectRatio = r.width / r.height;

                if (spriteRatio > rectRatio)
                {
                    float oldHeight = r.height;
                    r.height = r.width * (1.0f / spriteRatio);
                    r.y += (oldHeight - r.height) * rectTransform.pivot.y;
                }
                else
                {
                    float oldWidth = r.width;
                    r.width = r.height * spriteRatio;
                    r.x += (oldWidth - r.width) * rectTransform.pivot.x;
                }
            }

            return new Vector4(
                r.x,
                r.y,
                r.x + r.width,
                r.y + r.height
            );
        }
    }
}
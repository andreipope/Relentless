using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VramConsumer : MonoBehaviour {
    private Color32[] _buff = new Color32[512 * 512];
    private List<Texture2D> _textures = new List<Texture2D>();

    public void AllocMb(int mb)
    {
        for (int i = 0; i < mb; i++) {
            AllocTexture();
        }
    }

    private void AllocTexture() {
        Texture2D texture2D = new Texture2D(512, 512, TextureFormat.ARGB32, false);
        texture2D.SetPixels32(_buff);
        texture2D.Apply(false, true);

        // force load
        RenderTexture rt = RenderTexture.GetTemporary(512, 512, 0, RenderTextureFormat.ARGB32);
        RenderTexture.active = rt;
        GL.LoadOrtho();
        Graphics.DrawTexture(new Rect(0,0,1,1), texture2D);
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        _textures.Add(texture2D);
    }
}

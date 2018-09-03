using System.Collections;
using TMPro;
using UnityEngine;

public class FadeTool : MonoBehaviour
{
    public TextMeshPro[] Texts;

    public SpriteRenderer[] Sprites;

    public MeshRenderer[] MeshRenderers;

    public void FadeIn()
    {
        StartCoroutine(FadeInAsync());
    }

    private IEnumerator FadeInAsync()
    {
        float size = 0.01f;
        int turns = 100;

        for (int i = 0; i < turns; i++)
        {
            foreach (TextMeshPro item in Texts)
            {
                item.color = FadeItem(size, item.color);
            }

            foreach (SpriteRenderer item in Sprites)
            {
                item.color = FadeItem(size, item.color);
            }

            foreach (MeshRenderer item in MeshRenderers)
            {
                item.material.color = FadeItem(size, item.material.color);
            }

            yield return null;
        }
    }

    private Color FadeItem(float value, Color color)
    {
        Color clr = color;
        clr.a = Mathf.Clamp(clr.a - value, 0, 1);
        return clr;
    }
}

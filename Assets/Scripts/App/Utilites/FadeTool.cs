using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FadeTool : MonoBehaviour
{
    public TextMeshPro[] texts;
    public SpriteRenderer[] sprites;
    public MeshRenderer[] meshRenderers;


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
            foreach (var item in texts)
                item.color = FadeItem(size, item.color);

            foreach (var item in sprites)
                item.color = FadeItem(size, item.color);

            foreach (var item in meshRenderers)
                item.material.color = FadeItem(size, item.material.color);

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
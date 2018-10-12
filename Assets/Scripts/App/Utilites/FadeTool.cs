using System.Collections;
using TMPro;
using UnityEngine;

public class FadeTool : MonoBehaviour
{
    public TextMeshPro[] Texts;

    public SpriteRenderer[] Sprites;

    public MeshRenderer[] MeshRenderers;

    [SerializeField]
    private bool _startFade = false;

    [SerializeField]
    private float _basicValue = 1;

    private bool _isFadeIn = false;

    [SerializeField]
    private float _step = 0.01f;

    public void Start()
    {
        if(_startFade)
        {
            FadeIn();
        }
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInAsync());
    }

    private IEnumerator FadeInAsync()
    {
        int iterations = Mathf.FloorToInt(_basicValue / _step);

        for (int i = 0; i < iterations; i++)
        {
            foreach (TextMeshPro item in Texts)
            {
                item.color = FadeItem(_step, item.color);
            }

            foreach (SpriteRenderer item in Sprites)
            {
                item.color = FadeItem(_step, item.color);
            }

            foreach (MeshRenderer item in MeshRenderers)
            {
                item.material.color = FadeItem(_step, item.material.color);
            }

            yield return null;
        }
    }

    private Color FadeItem(float step, Color color)
    {
        Color clr = color;
        int direction = _isFadeIn ? 1 : -1;
        clr.a = Mathf.Clamp(clr.a + step * direction, 0, 1);
        return clr;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class LoopSpriteAnimation : MonoBehaviour
    {
        [Range(0.1f, 150f)]
        public float Speed = 1f;

        public List<Sprite> Frames;

        public SpriteRenderer SpriteRenderer;

        private void Start()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            WaitForSeconds delay = new WaitForSeconds(Time.fixedDeltaTime / Speed);

            while (true)
            {
                for (int i = 0; i < Frames.Count; i++)
                {
                    SpriteRenderer.sprite = Frames[i];
                    yield return delay;
                }
            }
        }
    }
}

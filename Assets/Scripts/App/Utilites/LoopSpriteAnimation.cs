// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class LoopSpriteAnimation : MonoBehaviour
    {
        [Range(0.1f, 150f)]
        public float speed = 1f;

        public List<Sprite> frames;

        public SpriteRenderer spriteRenderer;


        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            var delay = new WaitForSeconds(Time.fixedDeltaTime / speed);

            while (true)
            {
                for (int i = 0; i < frames.Count; i++)
                {
                    spriteRenderer.sprite = frames[i];
                    yield return delay;
                }
            }
        }
    }
}
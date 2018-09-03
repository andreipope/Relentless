using UnityEngine;

namespace Loom.ZombieBattleground
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemScaleGravityModifier : MonoBehaviour
    {
        public bool Is2D = true;

        private ParticleSystem _particleSystem;

        private float _initialGravityModifierMultiplier;

        private void Start()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _initialGravityModifierMultiplier = _particleSystem.main.gravityModifierMultiplier;
        }

        private void Update()
        {
            Vector3 lossyScale = transform.lossyScale;
            float scale = Is2D ? ((Vector2) lossyScale).magnitude : lossyScale.magnitude;
            ParticleSystem.MainModule mainModule = _particleSystem.main;
            mainModule.gravityModifierMultiplier = _initialGravityModifierMultiplier * scale;
        }
    }
}

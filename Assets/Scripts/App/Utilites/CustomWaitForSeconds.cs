using UnityEngine;

namespace Loom.ZombieBattleground
{
    /// <summary>
    ///   <para>Suspends the coroutine execution for the given amount of seconds using unscaled time.</para>
    /// </summary>
    public class CustomWaitForSeconds : CustomYieldInstruction
    {
        private float _waitTime;
        private bool _waitedAtLeastOnce = false;

        /// <summary>
        ///   <para>Creates a yield instruction to wait for a given number of seconds using unscaled time.</para>
        /// </summary>
        /// <param name="time"></param>
        public CustomWaitForSeconds(float time)
        {
            _waitTime = Time.time + time;
        }

        public override bool keepWaiting
        {
            get
            {
                if (_waitedAtLeastOnce)
                    return Time.time < _waitTime;

                _waitedAtLeastOnce = true;
                return true;
            }
        }
    }
}

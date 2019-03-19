using System;

namespace Loom.ZombieBattleground
{
    public static class UnitTestDetector
    {
        private static bool _isRunningUnitTest;

        public static event Action CheckRequested;

        public static bool IsRunningUnitTests
        {
            get
            {
                CheckRequested?.Invoke();
                return _isRunningUnitTest;
            }
        }

        public static void SetRunningUnitTest(bool isRunning)
        {
            _isRunningUnitTest = isRunning;
        }
    }
}

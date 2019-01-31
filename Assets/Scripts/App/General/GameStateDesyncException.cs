using System;

namespace Loom.ZombieBattleground
{
    public class GameStateDesyncException : Exception
    {
        private string Differences { get; }

        public GameStateDesyncException(string differences)
        {
            Differences = differences;
        }

        public override string Message => $"Differences:\n{Differences}";
    }
}

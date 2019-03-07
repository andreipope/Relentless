using System.IO;
using log4net.Core;
using log4net.Layout.Pattern;

namespace Loom.ZombieBattleground
{
    public class CounterPatternLayoutConverter : PatternLayoutConverter
    {
        private int _counter = 1;

        public void ResetCounter()
        {
            _counter = 1;
        }

        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            writer.Write(_counter.ToString());
            _counter++;
        }
    }
}

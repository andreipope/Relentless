using System;
using System.IO;
using log4net.Core;
using log4net.Util;

namespace Loom.ZombieBattleground {
    public class CustomHtmlLayout : HtmlLayout
    {
        public CustomHtmlLayout(string pattern) : base(pattern)
        {
            CustomCss = @"
        .item-logger {
            font-weight: 600;
        }
";
        }

        protected override bool IsFilteredPatternConverter(PatternConverter patternConverter)
        {
            switch (GetPatternConverterName(patternConverter))
            {
                case "Logger":
                case "Message":
                    return true;
                default:
                    return false;
            }
        }

        protected override string GetLogItemCellClass(PatternConverter patternConverter, LoggingEvent loggingEvent)
        {
            switch (GetPatternConverterName(patternConverter))
            {
                case "Time":
                    return "text-monospace small";
                case "Logger":
                    return "item-logger";
                default:
                    return base.GetLogItemCellClass(patternConverter, loggingEvent);
            }
        }

        protected override string GetLogItemCellStyle(PatternConverter patternConverter, LoggingEvent loggingEvent)
        {
            switch (GetPatternConverterName(patternConverter))
            {
                case "Logger":
                    // Use name hashcode as a hue
                    double hue =
                        unchecked((uint) loggingEvent.LoggerName.GetHashCode()) /
                        (double) uint.MaxValue * 360f;
                    (int r, int g, int b) = ColorFromHsv(hue, 1, 0.7);
                    return $"color: rgb({r}, {g}, {b});";
                default:
                    return base.GetLogItemCellStyle(patternConverter, loggingEvent);
            }
        }

        protected override string CreatePatternConverterName(PatternConverter patternConverter)
        {
            string typeName = patternConverter.GetType().Name;
            switch (typeName)
            {
                case "UtcDatePatternConverter":
                    return "Time";
                default:
                    return base.CreatePatternConverterName(patternConverter);
            }
        }

        protected override void WriteCell(LoggingEvent loggingEvent, PatternConverter patternConverter, TextWriter htmlWriter)
        {
            switch (GetPatternConverterName(patternConverter))
            {
                case "Message":
                    base.WriteCell(loggingEvent, patternConverter, htmlWriter);

                    // Write exception in the same cell, at the end of the cell
                    string exceptionString = loggingEvent.GetExceptionString();
                    if (!String.IsNullOrWhiteSpace(exceptionString))
                    {
                        htmlWriter.WriteLine("");
                        htmlWriter.WriteLine(exceptionString);
                    }
                    break;
                default:
                    base.WriteCell(loggingEvent, patternConverter, htmlWriter);
                    break;
            }
        }

        protected override void WriteException(TextWriter writer, TextWriter htmlWriter, LoggingEvent loggingEvent)
        {
        }

        private static (int r, int g, int b) ColorFromHsv(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            switch (hi) {
                case 0:
                    return (v, t, p);
                case 1:
                    return (q, v, p);
                case 2:
                    return (p, v, t);
                case 3:
                    return (p, q, v);
                case 4:
                    return (t, p, v);
                default:
                    return (v, p, q);
            }
        }
    }
}

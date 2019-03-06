using System;
using System.IO;
using System.Text;
using log4net.Core;
using log4net.Util;

namespace Loom.ZombieBattleground {
    public class CustomHtmlLayout : HtmlLayout
    {
        public CustomHtmlLayout(string pattern) : base(pattern)
        {
            //language=css
            CustomCss = @"
        .item-logger {
            font-weight: 600;
        }

        .log-message-exception {
            display: none;
            margin-top: 0.5em;
        }
";

            //language=javascript
            CustomJavascriptBeforeLoad = @"
checkIfMustUseExpandCollapseFunction = function(row, index) {
    var result = row.cellsText[index].length > maxTextLengthBeforeCollapse
    if (!result)
        return false;

    var logException = row.cells[index].querySelector('.log-message-exception')
    if (logException != null) {
        result = (row.cellsText[index].length - logException.innerText.length) > maxTextLengthBeforeCollapse
    }
    return result;
}
";

            //language=javascript
            CustomJavascriptAfterLoad = @"
         var showExceptions = false
        var logExceptions = $("".log-message-exception"")

        var exceptionSwitchCheckbox = document.createElement('input')
        exceptionSwitchCheckbox.type = ""checkbox"";
        exceptionSwitchCheckbox.name = ""name"";
        exceptionSwitchCheckbox.id = ""exception-switch"";

        var exceptionSwitchCheckboxLabel = document.createElement('label')
        exceptionSwitchCheckboxLabel.htmlFor = exceptionSwitchCheckbox.id
        exceptionSwitchCheckboxLabel.innerHTML = ""&nbsp;Show stacktraces"";

        filterInput[0].parentNode.insertBefore(exceptionSwitchCheckbox, filterInput[0].nextSibling)
        filterInput[0].parentNode.insertBefore(exceptionSwitchCheckboxLabel, exceptionSwitchCheckbox.nextSibling)

        function setShowLogExceptions(show) {
            exceptionSwitchCheckbox.checked = show
            if (show) {
                logExceptions.show()
            } else {
                logExceptions.hide()
            }
        }

        exceptionSwitchCheckbox.onchange = function() {
            showExceptions = !showExceptions
            setShowLogExceptions(showExceptions)
        }

        setShowLogExceptions(showExceptions)
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

        protected override void WriteCell(LoggingEvent loggingEvent, PatternConverter patternConverter, TextWriter writer, TextWriter htmlWriter)
        {
            switch (GetPatternConverterName(patternConverter))
            {
                case "Message":
                    base.WriteCell(loggingEvent, patternConverter, writer, htmlWriter);

                    // Write exception in the same cell, at the end of the cell
                    string exceptionString = loggingEvent.GetExceptionString();
                    if (!String.IsNullOrWhiteSpace(exceptionString))
                    {
                        writer.WriteLine("");
                        htmlWriter.WriteLine(exceptionString);
                    }
                    else
                    {
                        writer.WriteLine("");
                        writer.Write(@"<div class=""log-message-exception text-monospace small"">");
                        htmlWriter.Write(GetStackTrace(loggingEvent.LocationInformation.StackFrames));
                        writer.WriteLine(@"</div>");
                    }
                    break;
                default:
                    base.WriteCell(loggingEvent, patternConverter, writer, htmlWriter);
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

        private static string GetStackTrace(StackFrameItem[] stackFrames)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < stackFrames.Length; i++)
            {
                StackFrameItem frame = stackFrames[i];
                stringBuilder.Append(frame.ClassName);
                stringBuilder.Append('.');
                AppendMethodInformation(stringBuilder, frame.Method);

                if (!String.IsNullOrEmpty(frame.FileName))
                {
                    stringBuilder.Append(" (");
                    stringBuilder.Append(frame.FileName);
                    stringBuilder.Append(':');
                    stringBuilder.Append(frame.LineNumber);
                    stringBuilder.Append(')');
                }

                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        private static void AppendMethodInformation(StringBuilder stringBuilder, MethodItem method)
        {
            try
            {
                stringBuilder.Append(method.Name);
                stringBuilder.Append('(');

                string[] parameters = method.Parameters;
                if (parameters != null && parameters.GetUpperBound(0) > 0)
                {
                    int upperBound = parameters.GetUpperBound(0);
                    for (int i = 0; i <= upperBound; ++i)
                    {
                        stringBuilder.Append(parameters[i]);
                        if (i != upperBound)
                        {
                            stringBuilder.Append(", ");
                        }
                    }
                }

                stringBuilder.Append(')');
            }
            catch (Exception ex)
            {
                stringBuilder.AppendLine("An exception occurred while retrieving method information. " + ex);
            }
        }
    }
}

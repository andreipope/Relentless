using System;
using System.IO;
using System.Text;
using log4net.Core;
using log4net.Layout;
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

        .log-message-stacktrace {
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

    var logException = row.cells[index].querySelector('.log-message-stacktrace')
    if (logException != null) {
        result = (row.cellsText[index].length - logException.innerText.length) > maxTextLengthBeforeCollapse
    }
    return result;
}
";

            //language=javascript
            CustomJavascriptAfterLoad = @"
         var showStackTraces = false
        var logStackTraces = $("".log-message-stacktrace"")

        var exceptionSwitchCheckbox = document.createElement('input')
        exceptionSwitchCheckbox.type = ""checkbox"";
        exceptionSwitchCheckbox.name = ""name"";
        exceptionSwitchCheckbox.id = ""exception-switch"";

        var exceptionSwitchCheckboxLabel = document.createElement('label')
        exceptionSwitchCheckboxLabel.htmlFor = exceptionSwitchCheckbox.id
        exceptionSwitchCheckboxLabel.innerHTML = ""&nbsp;Show stack traces"";

        filterInput[0].parentNode.insertBefore(exceptionSwitchCheckbox, filterInput[0].nextSibling)
        filterInput[0].parentNode.insertBefore(exceptionSwitchCheckboxLabel, exceptionSwitchCheckbox.nextSibling)

        function setShowLogStackTraces(show) {
            exceptionSwitchCheckbox.checked = show
            if (show) {
                logStackTraces.show()
            } else {
                logStackTraces.hide()
            }
        }

        exceptionSwitchCheckbox.onchange = function() {
            showStackTraces = !showStackTraces
            logTable.hide()
            setShowLogStackTraces(showStackTraces)
            logTable.show()
        }

        setShowLogStackTraces(showStackTraces)
";
        }

        protected override void ProcessPatternLayout(PatternLayout patternLayout)
        {
            patternLayout.AddConverter("counter", typeof(CounterPatternLayoutConverter));
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
                case "#":
                    return "text-monospace small text-center";
                case "Logger":
                    return "item-logger";
                default:
                    return base.GetLogItemCellClass(patternConverter, loggingEvent);
            }
        }

        protected override string GetLogItemHeaderCellClass(PatternConverter patternConverter)
        {
            switch (GetPatternConverterName(patternConverter)) {
                case "#":
                    return "fit text-center";
                default:
                    return base.GetLogItemHeaderCellClass(patternConverter);
            }
        }

        protected override string GetLogItemCellStyle(PatternConverter patternConverter, LoggingEvent loggingEvent)
        {
            switch (GetPatternConverterName(patternConverter))
            {
                case "Logger":
                    // Use name hashcode as a hue
                    double hue =
                        FastHash(loggingEvent.LoggerName) /
                        (double) ulong.MaxValue;
                    UnityEngine.Color32 color = UnityEngine.Color.HSVToRGB((float) hue, 1f, 0.7f);
                    return $"color: rgb({color.r}, {color.g}, {color.b});";
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
                case "CounterPatternLayoutConverter":
                    return "#";
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
                        writer.Write(@"<div class=""text-monospace small"">");
                        htmlWriter.Write(exceptionString);
                        writer.Write(@"</div>");
                    }
                    else
                    {
                        string stackTrace = GetStackTrace(loggingEvent.LocationInformation.StackFrames);
                        if (!String.IsNullOrWhiteSpace(stackTrace)) { 
                            writer.Write(@"<div class=""log-message-stacktrace text-monospace small"">");
                            htmlWriter.Write(stackTrace);
                            writer.Write(@"</div>");
                        }
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

        private static string GetStackTrace(StackFrameItem[] stackFrames)
        {
            if (stackFrames == null || stackFrames.Length == 0)
                return "";

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

        private static ulong FastHash(string stringToHash)
        {
            ulong hashedValue = 3074457345618258791ul;
            for (int i = 0; i < stringToHash.Length; i++)
            {
                hashedValue += stringToHash[i];
                hashedValue *= 3074457345618258799ul;
            }

            return hashedValue;
        }
    }
}

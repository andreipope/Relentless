using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using log4net.Core;
using log4net.Layout;
using log4net.Util;

namespace Loom.ZombieBattleground
{
    public partial class HtmlLayout : LayoutSkeleton
    {
        public string Pattern { get; set; }

        public string LogName { get; set; } = "";

        public string StartDateFormat { get; set; } = "ddd, d MMM yyyy HH:mm:ss UTC";

        public int MaxTextLengthBeforeCollapse { get; set; } = 1000;

        public int CollapsedTextHeight { get; set; } = 250;

        public string CustomCss { get; set; } = "";

        public string CustomJavascriptAfterLoad { get; set; } = "";

        public string CustomJavascriptBeforeLoad { get; set; } = "";

        public int ConverterCount => _converterCount;

        private readonly List<int> _filteredCellIndexes = new List<int>();
        private readonly Dictionary<Type, string> _converterNames = new Dictionary<Type, string>();
        private PatternParser _patternParser;
        private PatternConverter _patternConverterHead;
        private bool _isHeaderWritten;
        private int _converterCount;

        public HtmlLayout(string pattern)
        {
            Pattern = pattern;
        }

        public override void ActivateOptions()
        {
            ExposedPatternLayout exposedPatternLayout = new ExposedPatternLayout(Pattern);
            ProcessPatternLayout(exposedPatternLayout);
            _patternParser = exposedPatternLayout.CreatePatternParser(Pattern);
            _patternConverterHead = _patternParser.Parse();
            _converterCount = 0;
            _converterNames.Clear();
            _filteredCellIndexes.Clear();
            for (PatternConverter patternConverter = PatternConverterHead; patternConverter != null; patternConverter = patternConverter.Next)
            {
                if (IsFilteredPatternConverter(patternConverter))
                {
                    _filteredCellIndexes.Add(_converterCount);
                }

                _converterCount++;
            }
        }

        protected virtual void ProcessPatternLayout(PatternLayout patternLayout)
        {
        }

        public override void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            HtmlEscapingTextWriterAdapter htmlWriter = new HtmlEscapingTextWriterAdapter(writer);
            if (!_isHeaderWritten)
            {
                _isHeaderWritten = true;
                WriteHeader(writer, htmlWriter);
            }

            writer.Write("<tr");

            string rowClass = GetLogItemRowClass(loggingEvent);
            if (!String.IsNullOrWhiteSpace(rowClass))
            {
                writer.Write(" class=\"");
                htmlWriter.Write(rowClass);
                writer.Write("\"");
            }
            writer.WriteLine(">");

            for (PatternConverter patternConverter = PatternConverterHead; patternConverter != null; patternConverter = patternConverter.Next)
            {
                writer.Write("<td");

                string cellClass = GetLogItemCellClass(patternConverter, loggingEvent);
                if (!String.IsNullOrWhiteSpace(cellClass))
                {
                    writer.Write(" class=\"");
                    htmlWriter.Write(cellClass);
                    writer.Write("\"");
                }

                string cellStyle = GetLogItemCellStyle(patternConverter, loggingEvent);
                if (!String.IsNullOrWhiteSpace(cellStyle))
                {
                    writer.Write(" style=\"");
                    htmlWriter.Write(cellStyle);
                    writer.Write("\"");
                }

                writer.Write(">");
                writer.Write("<div>");
                WriteCell(loggingEvent, patternConverter, writer, htmlWriter);
                writer.Write("</div>");
                writer.WriteLine("</td>");
            }
            writer.WriteLine("</tr>");

            WriteException(writer, htmlWriter, loggingEvent);
        }

        public override string ContentType => "text/html";

        public override bool IgnoresException
        {
            get => false;
            set => throw new NotSupportedException();
        }

        protected PatternConverter PatternConverterHead => _patternConverterHead;

        protected virtual void WriteHeader(TextWriter writer, TextWriter htmlWriter)
        {
            string headerPart1 = HeaderPart1;
            headerPart1 =
                headerPart1
                    .Replace("{{LOG_NAME}}", LogName)
                    .Replace("{{START_DATE}}", DateTime.UtcNow.ToString(StartDateFormat, CultureInfo.InvariantCulture))
                    .Replace("{{FILTERED_CELL_INDEXES}}", String.Join(", ", _filteredCellIndexes))
                    .Replace("{{MAX_TEXT_LENGTH_BEFORE_COLLAPSE}}", MaxTextLengthBeforeCollapse.ToString())
                    .Replace("{{COLLAPSED_TEXT_HEIGHT}}", CollapsedTextHeight.ToString())
                    .Replace("{{CUSTOM_CSS}}", CustomCss)
                    .Replace("{{JS_BEFORE_LOAD}}", CustomJavascriptBeforeLoad)
                    .Replace("{{JS_AFTER_LOAD}}", CustomJavascriptAfterLoad)
                ;

            writer.Write(headerPart1);
            for (PatternConverter patternConverter = PatternConverterHead; patternConverter != null; patternConverter = patternConverter.Next)
            {
                string name = GetPatternConverterName(patternConverter);

                writer.Write("<th");

                string cellClass = GetLogItemHeaderCellClass(patternConverter);
                if (!String.IsNullOrWhiteSpace(cellClass))
                {
                    writer.Write(" class=\"");
                    htmlWriter.Write(cellClass);
                    writer.Write("\"");
                }

                writer.Write(">");
                htmlWriter.Write(name);
                writer.WriteLine("</th>");
            }
            writer.Write(HeaderPart2);
        }

        protected virtual bool IsFilteredPatternConverter(PatternConverter patternConverter)
        {
            return true;
        }

        protected virtual void WriteCell(LoggingEvent loggingEvent, PatternConverter patternConverter, TextWriter writer, TextWriter htmlWriter)
        {
            patternConverter.Format(htmlWriter, loggingEvent);
        }

        protected virtual void WriteException(TextWriter writer, TextWriter htmlWriter, LoggingEvent loggingEvent)
        {
            string exceptionString = loggingEvent.GetExceptionString();
            if (!String.IsNullOrWhiteSpace(exceptionString))
            {
                writer.WriteLine("<tr class=\"table-danger special-row\">");
                writer.WriteLine($"<td colspan=\"{ConverterCount}\">");
                htmlWriter.WriteLine(exceptionString);
                writer.WriteLine("</td>");
                writer.WriteLine("</tr>");
            }
        }

        protected virtual string GetLogItemRowClass(LoggingEvent loggingEvent)
        {
            if (loggingEvent.Level >= Level.Error)
                return "table-danger";

            if (loggingEvent.Level >= Level.Warn)
                return "table-warning";

            return "";
        }

        protected virtual string GetLogItemHeaderCellClass(PatternConverter patternConverter)
        {
            if (patternConverter.GetType().Name == "MessagePatternConverter")
                return "";

            return "fit";
        }

        protected virtual string GetLogItemCellClass(PatternConverter patternConverter, LoggingEvent loggingEvent)
        {
            if (patternConverter.GetType().Name == "MessagePatternConverter")
                return "preformatted";

            return "";
        }

        protected virtual string GetLogItemCellStyle(PatternConverter patternConverter, LoggingEvent loggingEvent)
        {
            return "";
        }

        protected string GetPatternConverterName(PatternConverter patternConverter)
        {
            Type type = patternConverter.GetType();
            if (!_converterNames.TryGetValue(type, out string name))
            {
                name = CreatePatternConverterName(patternConverter);
                _converterNames.Add(type, name);
            }

            return name;
        }

        protected virtual string CreatePatternConverterName(PatternConverter patternConverter)
        {
            Type type = patternConverter.GetType();
            string name = type.Name;
            int suffixIndex = name.LastIndexOf("PatternConverter", StringComparison.Ordinal);
            if (suffixIndex != -1)
            {
                name = name.Substring(0, suffixIndex);
            }

            return name;
        }

        private class ExposedPatternLayout : PatternLayout
        {
            public ExposedPatternLayout(string pattern)
                : base(pattern) { }

            /// <summary>Create the pattern parser instance</summary>
            /// <param name="pattern">the pattern to parse</param>
            /// <returns>The <see cref="T:log4net.Util.PatternParser" /> that will format the event</returns>
            /// <remarks>
            /// <para>
            /// Creates the <see cref="T:log4net.Util.PatternParser" /> used to parse the conversion string. Sets the
            /// global and instance rules on the <see cref="T:log4net.Util.PatternParser" />.
            /// </para>
            /// </remarks>
            public new PatternParser CreatePatternParser(string pattern)
            {
                return base.CreatePatternParser(pattern);
            }
        }

        private class HtmlEscapingTextWriterAdapter : TextWriterAdapter
        {
            public HtmlEscapingTextWriterAdapter(TextWriter writer) : base(writer) { }

            /// <summary>Writes a character to the wrapped TextWriter</summary>
            /// <param name="value">the value to write to the TextWriter</param>
            /// <remarks>
            /// <para>
            /// Writes a character to the wrapped TextWriter
            /// </para>
            /// </remarks>
            public override void Write(char value)
            {
                WebUtility.HtmlEncode(value.ToString(), Writer);
            }

            /// <summary>Writes a character buffer to the wrapped TextWriter</summary>
            /// <param name="buffer">the data buffer</param>
            /// <param name="index">the start index</param>
            /// <param name="count">the number of characters to write</param>
            /// <remarks>
            /// <para>
            /// Writes a character buffer to the wrapped TextWriter
            /// </para>
            /// </remarks>
            public override void Write(char[] buffer, int index, int count)
            {
                WebUtility.HtmlEncode(new String(buffer, index, count), Writer);
            }

            /// <summary>Writes a string to the wrapped TextWriter</summary>
            /// <param name="value">the value to write to the TextWriter</param>
            /// <remarks>
            /// <para>
            /// Writes a string to the wrapped TextWriter
            /// </para>
            /// </remarks>
            public override void Write(string value)
            {
                WebUtility.HtmlEncode(value, Writer);
            }
        }
    }
}

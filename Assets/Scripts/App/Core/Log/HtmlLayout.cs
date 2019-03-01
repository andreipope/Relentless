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
    public class HtmlLayout : LayoutSkeleton
    {
        public string Pattern { get; set; }

        public string LogName { get; set; } = "";

        public string StartDateFormat { get; set; } = "ddd, d MMM yyyy HH:mm:ss UTC";

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
            _patternParser = new ExposedPatternLayout(Pattern).CreatePatternParser(Pattern);
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

        public override void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            HtmlEscapingTextWriterAdapter htmlWriter = new HtmlEscapingTextWriterAdapter(writer);
            if (!_isHeaderWritten)
            {
                _isHeaderWritten = true;
                WriteHeader(writer, htmlWriter);
            }

            writer.Write("<tr class=\"");
            htmlWriter.Write(GetLogItemRowClass(loggingEvent));
            writer.WriteLine("\">");


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

                writer.Write(">");
                WriteCell(loggingEvent, patternConverter, htmlWriter);
                writer.WriteLine("</td>");
            }
            writer.WriteLine("</tr>");

            WriteException(writer, htmlWriter, loggingEvent, _converterCount);
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
                    .Replace("{{FILTERED_CELL_INDEXES}}", String.Join(", ", _filteredCellIndexes));

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

        protected virtual void WriteCell(LoggingEvent loggingEvent, PatternConverter patternConverter, TextWriter htmlWriter)
        {
            patternConverter.Format(htmlWriter, loggingEvent);
        }

        protected virtual void WriteException(TextWriter writer, TextWriter htmlWriter, LoggingEvent loggingEvent, int converterCount)
        {
            string exceptionString = loggingEvent.GetExceptionString();
            if (!String.IsNullOrWhiteSpace(exceptionString))
            {
                writer.WriteLine("<tr class=\"table-danger special-row\">");
                writer.WriteLine($"<td colspan=\"{converterCount}\">");
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
            public PatternParser CreatePatternParser(string pattern)
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

        //language=html
        private const string HeaderPart1 = @"<!DOCTYPE html>
<html>
  <head>
    <title>{{LOG_NAME}} Log Messages </title>
    <meta charset=""utf-8"">

    <!-- Latest compiled and minified CSS -->
    <link rel=""stylesheet"" href=""https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css"">

    <!-- jQuery library -->
    <script src=""https://ajax.googleapis.com/ajax/libs/jquery/3.3.1/jquery.min.js""></script>

    <!-- mark.js -->
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/mark.js/8.11.1/jquery.mark.min.js""></script>

    <style type=""text/css"">
        .table td.fit,
        .table th.fit {
            white-space: nowrap;
            width: 1%;
        }

        .preformatted {
            white-space: pre-wrap;
            word-break: break-all;
        }
    </style>
    <script>
    var filteredCellIndexes = [ {{FILTERED_CELL_INDEXES}} ]
    
    $(document).ready(function() {
        var logsRows = $(""#log-table tr:not(.special-row)"").toArray().splice(1)
        var currentMark = null
        logsRows = logsRows.map(function(row) {
            cells = Array.from(row.querySelectorAll('td'))
            cells = filteredCellIndexes.map(function(x) { return cells[x] })
            cellsText = cells.map(function(x) { return x.innerText.toLowerCase() })
            return {
                'row': row,
                'cells': cells,
                'cellsText': cellsText
            }
        })
        $(""#filter"").on(""keyup"", function() {
            var value = $(this).val().toLowerCase().trim();
            if (currentMark != null) {
                currentMark.unmark()
            }

            filteredCells = []
            logsRows.forEach(function(row) {
                visible = false
                for (index = 0; index < row.cells.length; ++index) {
                    cellText = row.cellsText[index]
                    if (cellText.includes(value)) {
                        visible = true
                        filteredCells.push(row.cells[index])
                    }
                }

                if (visible) {
                    $(row.row).show()
                } else {
                    $(row.row).hide()
                }
            })

            currentMark = new Mark(filteredCells)
            currentMark.mark(value)
        });
    });
    </script>
  </head>
<body>

        <div class=""container-fluid"">
                <br>
                <h4>{{LOG_NAME}} Log</h4>
                <h6>Started at {{START_DATE}}</h6>

<input id=""filter"" type=""text"" placeholder=""Filter"" class=""form-control"">
<br>

<table class=""table table-sm table-striped table-bordered"" id=""log-table"">

<thead class=""thead-light"">
<tr>
";
        //language=html
        private const string HeaderPart2 = @"</tr>
</thead>
";
    }
}

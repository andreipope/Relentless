namespace Loom.ZombieBattleground {
    public partial class HtmlLayout
    {
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

        #loading-modal
        {
            display: block;
            background: rgba(255, 255, 255, 0.8);
        }

        #loading-modal h4
        {
            width: 100%;
        }

        #log-table {
            display: none;
        }

        .collapsed-message {
            overflow: hidden;
        }

        {{CUSTOM_CSS}}
    </style>
    <script>
    $(document).ready(function() {
        var filteredCellIndexes = [ {{FILTERED_CELL_INDEXES}} ]
        var maxTextLengthBeforeCollapse = {{MAX_TEXT_LENGTH_BEFORE_COLLAPSE}}

        var checkIfMustUseExpandCollapseFunction = function(row, index) {
            return row.cellsText[index].length > maxTextLengthBeforeCollapse
        }

        var logTable = null
        var filterInput = null
        var logsRows = null
        var currentMark = null

        function wrapElement (toWrap, wrapper) {
            wrapper = wrapper || document.createElement('div');
            toWrap.parentNode.appendChild(wrapper);
            return wrapper.appendChild(toWrap);
        };

        function debounce(func, wait, immediate) {
            var timeout;
            return function() {
                var context = this, args = arguments;
                var later = function() {
                    timeout = null;
                    if (!immediate) func.apply(context, args);
                };
                var callNow = immediate && !timeout;
                clearTimeout(timeout);
                timeout = setTimeout(later, wait);
                if (callNow) func.apply(context, args);
            };
        };

        function handleFilter() {
            var appliedChange = false
            var value = filterInput.val().toLowerCase().trim();
            if (currentMark != null) {
                appliedChange = true
                logTable.hide()
                currentMark.unmark()
            }

            if (value.length < 2) {
                if (appliedChange) {
                    logTable.show()
                }
                return
            }

            logTable.hide()

            var filteredCells = []
            logsRows.forEach(function(row) {
                var visible = false
                for (index = 0; index < row.cells.length; ++index) {
                    if (row.cellsText[index].includes(value)) {
                        visible = true
                        filteredCells.push(row.cells[index])
                    }
                }

                if (row.visible == visible)
                    return

                row.visible = visible
                if (visible) {
                    $(row.row).show()
                } else {
                    $(row.row).hide()
                }
            })

            currentMark = new Mark(filteredCells)
            currentMark.mark(value)

            logTable.show()
        }

        function addExpandCollapse(element) {
            const collapsedAttributeName = 'aria-collapsed'
            function switchState(link, container) {
                var isCollapsed = link.getAttribute(collapsedAttributeName) == 'true'
                isCollapsed = !isCollapsed
                setState(link, container, isCollapsed)
            }

            function setState(link, container, isCollapsed) {
                const expandString = ""Expand""
                const collapseString = ""Collapse""

                link.innerText = isCollapsed ? expandString : collapseString
                height = isCollapsed ? ""{{COLLAPSED_TEXT_HEIGHT}}"" + ""px"" : ""none""
                $(container).css({ maxHeight: height })
                link.setAttribute(collapsedAttributeName, isCollapsed)
            }

            var wrapper = document.createElement('div')
            wrapper.classList.add('collapsed-message')
            wrapElement(element, wrapper)

            var link = document.createElement('a')
            link.href = ""#""
            link.setAttribute(collapsedAttributeName, false)
            wrapper.parentNode.appendChild(link)

            link.onclick = function(e) {
                e.preventDefault()
                switchState(link, wrapper)
            }
            setState(link, wrapper, true)
        }

        {{JS_BEFORE_LOAD}}

        // Preload data
        logTable = $('#log-table')
        logsRows = $(""#log-table tr:not(.special-row)"").toArray().splice(1)
        filterInput = $(""#filter"")

        logsRows = logsRows.map(function(row) {
            cells = Array.from(row.querySelectorAll('td'))
            cells = filteredCellIndexes.map(function(x) { return cells[x] })
            cellsText = cells.map(function(x) { return x.innerText.toLowerCase() })
            return {
                'row': row,
                'cells': cells,
                'cellsText': cellsText,
                'visible': true
            }
        })

        // Apply expand/collapse
        logsRows.forEach(function(row) {
            for (index = 0; index < row.cells.length; ++index) {
                if (checkIfMustUseExpandCollapseFunction(row, index)) {
                    addExpandCollapse(row.cells[index].childNodes[0])
                }
            }
        })

        // Handle filter field
        filterInput.on(""keyup"", debounce(function() {
            handleFilter()
        }, 250, false));

        handleFilter()

        {{JS_AFTER_LOAD}}

        // Finish loading
        logTable.show()
        $('#loading-modal').hide()
    });
    </script>
  </head>
<body>

        <div class=""container-fluid"">
                <div class=""modal"" id=""loading-modal"" style=""display: block;"">
                    <div class=""modal-dialog modal-dialog-centered"">
                        <div class=""modal-content"">
                            <div class=""modal-header"">
                                <h4 class=""modal-title text-center"">Loading...</h4>
                            </div>
                        </div>
                    </div>
                </div>

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

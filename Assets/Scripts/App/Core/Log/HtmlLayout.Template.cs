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

    <!-- readmore.js -->
    <script src=""https://fastcdn.org/Readmore.js/2.1.0/readmore.min.js""></script>

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
            background: rgba(255, 255, 255, 0.8);
        }

        {{CUSTOM_CSS}}
    </style>
    <script>
    $(document).ready(function() {
        var filteredCellIndexes = [ {{FILTERED_CELL_INDEXES}} ]
        var maxTextLengthBeforeCollapse = {{MAX_TEXT_LENGTH_BEFORE_COLLAPSE}}

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
            var value = $(""#filter"").val().toLowerCase().trim();
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
    
            if (value.length > 1) {
                currentMark = new Mark(filteredCells)
                currentMark.mark(value)
            }
        }

        var logsRows = $(""#log-table tr:not(.special-row)"").toArray().splice(1)
        var currentMark = null

        // Preload data
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

        // Apply expand/collapse
        logsRows.forEach(function(row) {
            for (index = 0; index < row.cells.length; ++index) {
                if (row.cellsText[index].length > maxTextLengthBeforeCollapse) {
                    wrapper = document.createElement('div')
                    wrapper.style = 'overflow: hidden;'
                    wrapElement(row.cells[index].childNodes[0], wrapper)
                    $(wrapper).readmore({
                        collapsedHeight: {{COLLAPSED_TEXT_HEIGHT}},
                        moreLink: '<a href=""#"">Expand</a>',
                        lessLink: '<a href=""#"">Collapse</a>',
                    });
                }
            }
        })

        // Handle filter field
        $(""#filter"").on(""keyup"", debounce(function() {
            handleFilter()
        }, 250, false));

        handleFilter()
        
        // Finish loading
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
                                <h4 class=""modal-title text-center"" style=""width: 100%;"">Loading...</h4>
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

IoTApp.createModule('IoTApp.LocationsIndex', function () {
    "use strict";

    var self = this;

    var init = function (locationProperties) {
        self.locationProperties = locationProperties;
        self.dataTableContainer = $('#locationsTable');
        self.locationGrid = $(".details_grid");
        self.locationGridClosed = $(".details_grid_closed");
        self.locationsGridContainer = $(".grid_container");
        self.buttonDetailsGrid = $(".button_details_grid");
        self.reloadGrid = this.reloadGrid;
        self.callFromMap = resources.fromMap;

        if (self.callFromMap === "True")
        {
            self.latitude = parseFloat(resources.mapLat);
            self.longitude = parseFloat(resources.mapLong);
        }

        _initializeDatatable();
        _initializeButtonArea();

        self.buttonDetailsGrid.on("click", function () {
            toggleProperties();
            fixHeights();
        });

        $(window).on("load", function () {
            fixHeights();
            setGridWidth();
        });

        $(window).on("resize", function () {
            fixHeights();
            setGridWidth();
        });
    }

    var _selectRowFromDataTable = function (row) {
        var rowData = row.data();
        if (rowData != null) {
            self.dataTable.$(".selected").removeClass("selected");
            row.nodes().to$().addClass("selected");
            self.selectedRow = row.index();
            self.latitude = rowData["latitude"];
            self.longitude = rowData["longitude"];
            self.locationProperties.init(rowData["latitude"], rowData["longitude"], self.reloadGrid);
        }
    }

    var _setDefaultRowAndPage = function () {
        if (self.isDefaultLocationAvailable === true) {
            if (self.defaultSelectedRow) {
                var node = self.dataTable.row(self.defaultSelectedRow);
                _selectRowFromDataTable(node);

                if (self.callFromMap === "True") {
                    self.callFromMap = "False";
                    self.defaultSelectedRow = null;

                    var pageLength = 20;

                    var found = self.dataTable.$(".selected");

                    var listItems = self.dataTable.$('tr');
                    var index = listItems.index(found);

                    var page = Math.floor(index / pageLength);

                    self.dataTable.page(page).draw('page');
                }
            }
            else
            {
                self.isDefaultLocationAvailable = false;
            }
            
        } else {
            // if selected location is no longer displayed in grid, then close the details pane
            closeAndClearProperties();
        }


    }

    var changeLocationStatus = function () {
        var tableStatus = self.dataTable;

        var cells_status_critical = tableStatus.cells(".table_status:contains('critical')").nodes();
        $(cells_status_critical).addClass('status_failed');
        $(cells_status_critical).html(resources.critical);

        var cells_status_caution = tableStatus.cells(".table_status:contains('caution')").nodes();
        $(cells_status_caution).addClass('status_completed_with_errors');
        $(cells_status_caution).html(resources.caution);

        var cells_status_green = tableStatus.cells(".table_status:contains('green')").nodes();
        $(cells_status_green).addClass('status_running');
        $(cells_status_green).html(resources.green);
    }

    var _initializeDatatable = function () {

        var onTableDrawn = function () {
            changeLocationStatus();
            _setDefaultRowAndPage();

            var pagingDiv = $('#locationsTable_paginate');
            if (pagingDiv) {
                if (self.dataTable.page.info().pages > 1) {
                    $(pagingDiv).show();
                } else {
                    $(pagingDiv).hide();
                }
            }            
        };

        var onTableRowClicked = function () {
            _selectRowFromDataTable(self.dataTable.row(this));
        }

        var htmlEncode = function (data) {
            // "trick" to HTML encode data from JS--essentially dip it in a <div> and pull it out again
            return data ? $('<div/>').text(data).html() : null;
        }

        self.dataTable = self.dataTableContainer.DataTable({
            "autoWidth": false,
            "pageLength": 20,
            "displayStart": 0,
            "pagingType": "simple_numbers",
            "paging": true,
            "lengthChange": false,
            "processing": false,
            "serverSide": false,
            "dom": "<'dataTables_header'i<'location_list_button_area'>>lrtp?",
            "ajax": onDataTableAjaxCalled,
            "language": {
                "info": resources.locationsList + " (_TOTAL_)",
                "paginate": {
                    "previous": resources.previousPaging,
                    "next": resources.nextPaging
                }
            },
            "columns": [
                {
                    "defaultContent": '<input type="checkbox" />',
                    "width": '1%',
                    "className": 'dt-body-center'
                    //"mRender": function (data, type, full, meta) {
                    //    return '<input type="checkbox">';
                    //}
                },
                {
                    "data": "status",
                    "mRender": function (data) {
                        if (data === 2) {
                            return htmlEncode("critical");
                        } else if (data === 1) {
                            return htmlEncode("caution");
                        } else {
                            return htmlEncode(data);
                        }                        
                    },
                    "name": "locationStatus"
                },
                {
                    "data": "latitude",
                    "mRender": function (data) {
                        return IoTApp.Helpers.Numbers.formatCoordinates(data);
                    },
                    "name": "latitude"
                },
                {
                    "data": "longitude",
                    "mRender": function (data) {
                        return IoTApp.Helpers.Numbers.formatCoordinates(data);
                    },
                    "name": "longitude"
                },
                {
                    "data": "noOfDevices",
                    "mRender": function (data) {
                        return IoTApp.Helpers.Numbers.localizeNumber(data);
                    },
                    "name": "noOfDevices"
                }
            ],
            "columnDefs": [
                { className: "table_status", "width": "100px", "targets": [1] },
                { className: "table_checkbox", "searchable": false, "orderable": false, "targets": [0] },
                { "searchable": true, "targets": [2] }              
            ],
            "order": [[2, "asc"]]
        });

        self.dataTableContainer.on("draw.dt", onTableDrawn);
        self.dataTableContainer.on("error.dt", function (e, settings, techNote, message) {
            IoTApp.Helpers.Dialog.displayError(resources.unableToRetrieveLocationsFromService);
        });

        self.dataTableContainer.find("tbody").delegate("td:not(:first-child)", "click", onTableRowClicked);
        self.dataTableContainer.find("tbody").on("change", 'input[type="checkbox"]', toggleDeleteButton);

        /* DataTables workaround - reset progress animation display for use with DataTables api */
        $('.loader_container').css('display', 'block');
        $('.loader_container').css('background-color', '#ffffff');
        self.dataTableContainer.on('processing.dt', function (e, settings, processing) {
            $('.loader_container').css('display', processing ? 'block' : 'none');
            _setGridContainerScrollPositionIfRowIsSelected();
        });

        var _setGridContainerScrollPositionIfRowIsSelected = function () {
            if ($("tbody .selected").length > 0) {
                $('.grid_container')[0].scrollTop = $("tbody .selected").offset().top - $('.grid_container').offset().top - 50;
            }
        }
    }    

    var onDataTableAjaxCalled = function (data, fnCallback) {

        // create a success callback to track the selected row, and then call the DataTables callback
        var successCallback = function (json, a, b) {

            if (self.latitude && self.longitude) {
                // iterate through the data before passing it on to grid, and try to
                // find and save the selected deviceID value

                // reset this value each time
                self.isDefaultLocationAvailable = false;

                for (var i = 0, len = json.data.length; i < len; ++i) {
                    var data = json.data[i];
                    if (data &&
                        (data.latitude === self.latitude) && (data.longitude === self.longitude)) {
                        self.defaultSelectedRow = i;
                        self.isDefaultLocationAvailable = true;
                        break;
                    }
                }
            }

            // pass data on to grid (otherwise grid will spin forever)
            fnCallback(json, a, b);
        }

        self.getLocationList = $.ajax({
            "dataType": 'json',
            'type': 'POST',
            'url': '/api/v1/locations/list',
            'cache': false,
            'data': data,
            'success': successCallback
        });

        self.getLocationList.fail(function () {
            $('.loader_container').hide();
            IoTApp.Helpers.Dialog.displayError(resources.failedToRetrieveLocations);
        });
    }

    /* Set the heights of scrollable elements for correct overflow behavior */
    function fixHeights() {
        // set height of device details pane
        var fixedHeightVal = $(window).height() - $(".navbar").height();
        $(".height_fixed").height(fixedHeightVal);
    }

    /* Hide/show the Location Details pane */
    var toggleProperties = function () {
        self.locationGrid.toggle();
        self.locationGridClosed.toggle();
        setGridWidth();
    }

    // close the location details pane (called when location is no longer shown)
    var closeAndClearProperties = function () {
        // only toggle if we are already open!
        if (self.locationGrid.is(":visible")) {
            toggleProperties();
        }

        // clear the details pane (so it's clean!)
        // Even though we're working with locations, we still use the no_device_selected class
        // So we don't have to duplicate a bunch of styling for now
        var noLocationSelected = resources.noLocationSelected;
        $('#details_grid_container').html('<div class="details_grid__no_selection">' + noLocationSelected + '</div>');
    }

    var setGridWidth = function () {
        var gridContainer = $(".grid_container");

        // Set the grid VERY NARROW initially--otherwise if panels are expanding, 
        // the existing grid will be too wide to fit, and it will be pushed *below* the 
        // side panes--roughly doubling the height of the content. In this case, 
        // the browser will add a vertical scrollbar on the window.
        //
        // If this happens, the code in this function will collect data
        // with the grid pushed below and a scrollbar on the right--so 
        // $(window).width() will be too narrow (by the width of the scrollbar).
        // When the grid is correctly sized, it will move back up, and the 
        // browser will remove the scrollbar. But at that point there will be a gap
        // the width of the scrollbar, as the final measurement will be off by 
        // the width of the scrollbar.

        // set grid container to 1 px width--see comment block above
        gridContainer.width(1);

        var locationGridVisible = $(".details_grid").is(':visible');

        var locationGridWidth = locationGridVisible ? self.locationGrid.width() : self.locationGridClosed.width();

        var windowWidth = $(window).width();

        // check for min width (otherwise we over-shrink the grid)
        if (windowWidth < 800) {
            windowWidth = 800;
        }

        var gridWidth = windowWidth - locationGridWidth - 98;
        gridContainer.width(gridWidth);
    }

    var reloadGrid = function () {
        self.dataTable.ajax.reload();
    }

    var _initializeButtonArea = function () {
        var $buttonArea = $('.location_list_button_area');
        
        $('<button/>', {
            id: 'btnDeleteSelected',
            "class": 'button_base devicelist_toolbar_button devicelist_toolbar_button_gray device_list_button_edit_column',
            text: 'Delete Selected',
            click: function () {
                deleteSelectedLocations();
            }
        }).attr("disabled", "disabled").appendTo($buttonArea);

        $('<button/>', {
            id: 'btnDeleteAllLocations',
            "class": 'button_base devicelist_toolbar_button devicelist_toolbar_button_gray device_list_button_edit_column',
            text: 'Reset All',
            click: function () {
                deleteAllLocations();
            }
        }).appendTo($buttonArea);        
    }

    var toggleDeleteButton = function () {
        var btnDeleteSelected = $("#btnDeleteSelected");
        var row = $(this).closest('tr');

        if (btnDeleteSelected !== null) {
            if (this.checked) {
                
                row.addClass("canDelete");

                if (btnDeleteSelected.is(":disabled")) {
                    btnDeleteSelected.removeAttr('disabled');
                }
            }
            else {

                row.removeClass("canDelete");

                var checkedRows = self.dataTableContainer.dataTable().$('input[type="checkbox"]:checked', { "page": "all" });
                if (checkedRows.length === 0) {
                    btnDeleteSelected.attr('disabled', 'disabled');
                }
            }
        }
    }   

    var deleteSelectedLocations = function () {

        var selectedRows = self.dataTable.rows('.canDelete');
        var locations = [];
        var row;
        var checkedRow;
        var rowData;;
        var latitude;
        var longitude;
        var obj;

        selectedRows.every(function (){
            row = this;
            checkedRow = $(row.node()).find('input').prop('checked');
            if (checkedRow)
            {
                var rowData = row.data();
                var latitude = rowData["latitude"];
                var longitude = rowData["longitude"];
                var obj = { latitude: latitude, longitude: longitude };
                locations.push(obj);
            }
            checkedRow = null;
        });

        if (locations.length > 0)
        {
            //pass to action method            
            $.post('/Location/DeleteSelected', { locations: locations }, function () {
                location.reload();
            }).fail(function (response) {
                IoTApp.Helpers.Dialog.displayError(resources.failedToDelete);
            });
        }
    }

    var deleteAllLocations = function () {
        var onSuccessfulDeletion = function () {
            reloadGrid();
        }

        $.post('/api/v1/locations/delete/all', function (response) {
            if (response === true) {
                IoTApp.Helpers.Dialog.displayError(resources.successfullyDeletedLocationJerks);
            } else {
                IoTApp.Helpers.Dialog.displayError(resources.failedToDeleteLocationJerks);
            }
        }).done(function () {
            reloadGrid();
            }).fail(function () {
                IoTApp.Helpers.Dialog.displayError(resources.failedToDeleteLocationJerks);
            });
    }

    return {
        init: init,
        toggleProperties: toggleProperties,
        reloadGrid: reloadGrid
    }

}, [jQuery, resources]);


$(function () {
    "use strict";

    IoTApp.LocationsIndex.init(IoTApp.LocationProperties);
});
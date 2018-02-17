IoTApp.createModule('IoTApp.LocationReport', function () {
    "use strict";

    var self = this;

    self.deviceSelectionTable = $('#dropDownContainerTable tbody');
    self.addDeviceButton = $('#btnAddDevice');
    self.viewRuleButton = $('#btnViewRule');
    self.deviceArray = resources.deviceArray;
    self.deviceOptionAdded = false;
    self.backButton = $("#btnBack");
    self.viewGraphButton = $("#btnViewGraph");
    self.topPane = {
        container: $("#topPane"),
        detailsPane: $("#topPaneDetails"),
        graphPane: $("#topPane").find('.device_report_graph_pane'),
        isInitialized: false
    };

    self.bottomPane = {
        container: $("#bottomPane"),
        detailsPane: $("#bottomPaneDetails"),
        graphPane: $("#bottomPane").find('.device_report_graph_pane'),
        isInitialized: false
    };

    var init = function () {        

        self.backButton.on("click", function () {
            backButtonClicked();
        });

        self.viewRuleButton.on("click", function () {
            viewRuleButtonClicked();
        });

        self.addDeviceButton.on("click", function () {
            addRowToDeviceSelectionTable();
            self.deviceOptionAdded = true;

            $(this).addClass("button_not_active");
        });

        self.viewGraphButton.on("click", function () {
            resizeGraphPane();
            viewGraphs();
        });

        addRowToDeviceSelectionTable();

        var targetGraphControls = [];
        targetGraphControls.push(self.topPane.graphPane);
        targetGraphControls.push(self.bottomPane.graphPane);

        createVisuals(targetGraphControls);
        resizeGraphPane();
        viewGraphs();
    }

    var createSelectList = function (array, index) {
        var listId = "ddlSelectDevice_" +(isSecondList() === true ? 1 : index);

        var deviceSelectList = $('<select/>').attr({ id: listId, class: 'location_report_select' });

        for (var i = 0; i < array.length; i++)
        {
            $('<option/>').attr('value', array[i]).text(array[i]).appendTo(deviceSelectList);
        }

        return deviceSelectList;
    }

    var addRowToDeviceSelectionTable = function () {

        if (self.deviceOptionAdded === false)
        {
            var rowIndex = self.deviceSelectionTable.find('tr:last').length + 1;

            var newRow = $('<tr/>').append($('<td/>').append(createSelectList(self.deviceArray, rowIndex)));

            var lnkClear = $('<a/>').addClass("edit_form__link btnClear")
                .append($('<small/>').addClass("text_small")
                    .text(resources.btnClearText));

            newRow.append($('<td/>').attr('style', 'width:10%').append(lnkClear));
            self.deviceSelectionTable.append(newRow);

            self.btnClear = $('.btnClear').unbind('click');

            self.btnClear = $('.btnClear').on('click', function () {
                removeDeviceOption($(this));
            });

            if (rowIndex < 2) {
                self.btnClear.attr("style", "visibility:hidden");
            }
            else
            {
                self.btnClear.removeAttr("style");
            }
        }        
    }

    var isSecondList = function () {
        var secondList = $("#ddlSelectDevice_2");
        if (secondList.length > 0) {
            return true;
        }
        else
        {
            return false;
        }
    }

    var removeDeviceOption = function (row) {
        row.closest('tr').remove();
        self.deviceOptionAdded = false;
        self.addDeviceButton.removeClass("button_not_active");
        self.btnClear.attr("style", "visibility:hidden");
    }

    var backButtonClicked = function () {
        var url = resources.redirectUrl.replace("&amp;", "&");
        location.href = url;
    }

    var viewRuleButtonClicked = function () {
        var url = resources.locationRuleUrl.replace("&amp;", "&");
        location.href = url;
    }

    var createDataView = function createDataView(data, telemetryFields) {
        var categoryIdentities;
        var categoryMetadata;
        var categoryValues;
        var columns;
        var dataValues;
        var dataView;
        var dataViewTransform;
        var fieldExpr;
        var graphData;
        var graphMetadata;

        dataViewTransform = powerbi.data.DataViewTransform;

        fieldExpr =
            powerbi.data.SQExprBuilder.fieldDef({
                entity: 'table1',
                column: 'time'
            });

        graphData = produceGraphData(data);

        categoryValues = graphData.timestamps;

        categoryIdentities = categoryValues.map(
            function (value) {
                var expr =
                    powerbi.data.SQExprBuilder.equal(
                        fieldExpr,
                        powerbi.data.SQExprBuilder.text(value));
                return powerbi.data.createDataViewScopeIdentity(expr);
            });

        var graphMetadataColumns = [
            {
                displayName: 'Time',
                isMeasure: true,
                queryName: 'timestamp',
                type: powerbi.ValueType.fromDescriptor({ dateTime: true })
            }
        ];

        columns = [];

        // Create a new column for values
        if (Array.isArray(telemetryFields) && telemetryFields.length > 0) {
            for (var i = 0; i < telemetryFields.length; i++) {
                graphMetadataColumns.push({
                    displayName: telemetryFields[i].displayName || convertToDisplayName(telemetryFields[i].name),
                    isMeasure: true,
                    format: "0.0",
                    queryName: telemetryFields[i].name.toLowerCase(),
                    type: powerbi.ValueType.fromDescriptor({ numeric: true })
                });

                columns.push({
                    source: graphMetadataColumns[graphMetadataColumns.length - 1],
                    values: graphData[telemetryFields[i].name.toLowerCase()] || []
                })
            }
        } else if (data[0]) {
            for (var field in data[0].values) {
                graphMetadataColumns.push({
                    displayName: convertToDisplayName(field),
                    isMeasure: true,
                    format: "0.0",
                    queryName: field.toLowerCase(),
                    type: powerbi.ValueType.fromDescriptor({ numeric: true })
                });
                columns.push({
                    source: graphMetadataColumns[graphMetadataColumns.length - 1],
                    values: graphData[field.toLowerCase()] || []
                });
            }
        }

        graphMetadata = {
            columns: graphMetadataColumns
        };

        dataValues = dataViewTransform.createValueColumns(columns);

        categoryMetadata = {
            categories: [{
                source: graphMetadata.columns[0],
                values: categoryValues,
                identity: categoryIdentities
            }],
            values: dataValues
        };

        dataView = {
            metadata: graphMetadata,
            categorical: categoryMetadata
        };

        return dataView;
    };

    var createDefaultStyles = function createDefaultStyles() {

        var dataColors = new powerbi.visuals.DataColorPalette();

        return {
            titleText: {
                color: { value: 'rgba(51,51,51,1)' },
                fontFamily: 'Verdana'
            },
            subTitleText: {
                color: { value: 'rgba(145,145,145,1)' }
            },
            colorPalette: {
                dataColors: dataColors,
            },
            labelText: {
                color: {
                    value: 'rgba(51,51,51,1)'
                },
                fontSize: '11px'
            },
            isHighContrast: false,
        };
    };

    var createVisuals = function createVisuals(targetControls) {       

        if (targetControls != null && targetControls.length > 0)
        {
            try {
                targetControls.forEach(function (targetControl, index) {
                    var height;
                    var pluginService;
                    var singleVisualHostServices;
                    var width;
                    var visual;

                    pluginService = powerbi.visuals.visualPluginFactory.create();
                    singleVisualHostServices = powerbi.visuals.defaultVisualHostServices;

                    height = $(targetControl).height();
                    width = $(targetControl).width();

                    // Get a plugin                    
                    visual = pluginService.getPlugin('lineChart').create();

                    visual.init({
                        // empty DOM element the visual should attach to.
                        element: targetControl,
                        // host services
                        host: singleVisualHostServices,
                        style: createDefaultStyles(),
                        viewport: {
                            height: height,
                            width: width
                        },
                        settings: { slicingEnabled: true },
                        interactivity: { isInteractiveLegend: true, selection: true },
                        animation: { transitionImmediate: true }
                    });

                    if (targetControl.selector.includes('#topPane')) {
                        self.topPane.visual = visual;
                        self.topPane.isInitialized = true;
                    }
                    else {
                        if (targetControl.selector.includes('#bottomPane')) {
                            self.bottomPane.visual = visual;
                            self.bottomPane.isInitialized = true;
                        }
                    }
                });
            }
            catch (err)
            {
                
            }
        }
    };

    var produceGraphData = function produceGraphData(data) {
        var dateTime;
        var i;
        var item;
        var results;

        results = {
            timestamps: []
        };

        if (data[0]) {
            for (var field in data[0].values) {
                results[field.toLowerCase()] = [];
            }
            for (i = 0; i < data.length; ++i) {
                item = data[i];
                for (var field in item.values) {
                    results[field.toLowerCase()].push(item.values[field]);
                }

                dateTime = new Date(item.timestamp);
                if (!dateTime.replace) {
                    dateTime.replace = ('' + this).replace;
                }

                results.timestamps.push(dateTime);
            }
        }

        return results;
    };

    var redraw = function (isTopPane) {
        var height;
        var width;
        var targetControl;
        var lastData;
        var fields;
        var hasVisualsBeenInitialized;
        var visual;
        var currentPane = isTopPane ? self.topPane : self.bottomPane;

        targetControl = currentPane.graphPane;
        hasVisualsBeenInitialized = currentPane.isInitialized;
        visual = currentPane.visual;

        if (currentPane.ajaxData)
        {
            lastData = currentPane.ajaxData.graphModels;
            fields = currentPane.ajaxData.graphFields;
        }

        if (!targetControl) {
            return;
        }

        height = $(targetControl).height();
        width = $(targetControl).width();

        if (lastData && hasVisualsBeenInitialized) {
            visual.update({
                dataViews: [createDataView(lastData, fields)],
                viewport: {
                    height: height,
                    width: width
                },
                duration: 0
            });
        }
    };

    var convertToDisplayName = function (fieldName) {
        return fieldName
            .replace(/([A-Z])/g, ' $1') // Spaces in front of capitals
            .replace(/^([a-z])/g, function (match, firstLetter) { // Make first letter upper case
                return firstLetter.toUpperCase();
            });
    }

    var viewGraphs = function () {        

        var rows = self.deviceSelectionTable.find('tr');
        var expandTopPane = rows.length > 1 ? false : true;

        self.topPane.container.addClass("graphPane_hidden");
        self.bottomPane.container.addClass("graphPane_hidden");
        self.topPane.ajaxData = null;
        self.bottomPane.ajaxData = null;

        rows.each(function (i,row) {
            var row = $(row);
            var selectedOption = row.find("td > select > :selected");
            var deviceId = selectedOption.text();            
            var url = resources.getJerkGraphUrl
                .replace("{deviceId}", deviceId)
                .replace("{latitude}", resources.latitude)
                .replace("{longitude}", resources.longitude);
            var isTopPane = i === 0 ? true : false;
            var graphPane = isTopPane ? self.topPane.container : self.bottomPane.container;

            graphPane.find('.txt_deviceid').html(deviceId);

            if (isTopPane === true)
            {
                graphPane.removeClass("no_bottom_pane");
                if (expandTopPane === true) {
                    graphPane.addClass("no_bottom_pane");
                }
            }

            var onSuccess = function (data) {
                var heading = resources.noData;
                var speed = resources.noData;

                if (data != null)
                {
                    heading = data.heading;
                    speed = data.speed;

                    var objAjaxData = {
                        deviceId: data.deviceId,
                        heading: data.heading,
                        speed: data.speed,
                        graphFields: data.locationJerkGraphFields,
                        graphModels: data.locationJerkGraphModels
                    };

                    if (isTopPane === true) {
                        self.topPane.ajaxData = objAjaxData;
                    }
                    else
                    {
                        self.bottomPane.ajaxData = objAjaxData;
                    }
                }                

                graphPane.find('.txt_heading').html(heading);
                graphPane.find('.txt_speed').html(speed);

                redraw(isTopPane);
            }

            $.ajax({
                dataType: 'json',
                type: 'GET',
                url: url,
                cache: false,
                success: onSuccess,
                error: function () {
                    IoTApp.Helpers.Dialog.displayError(resources.failedToGetGraphData);
                }
            }).done(function () {
                graphPane.removeClass("graphPane_hidden");
                });

        });
    }

    var resizeGraphPane = function () {
        var rows = self.deviceSelectionTable.find('tr');

        rows.each(function (i) {
            var height;
            var width;
            var padding;
            var targetControl;
            var targetControlContainer;
            var targetControlDetailsPane;
            var isTopPane = i === 0 ? true : false;
            var currentPane = isTopPane ? self.topPane : self.bottomPane;
            var graphPane = currentPane.container;           

            targetControl = currentPane.graphPane;
            targetControlContainer = currentPane.container;
            targetControlDetailsPane = currentPane.detailsPane;

            if (currentPane.detailsPane.width() < self.topPane.detailsPane.width())
            {
                targetControlDetailsPane = self.topPane.detailsPane;
            }
            
            padding = 82;

            if (targetControlContainer &&
                targetControlDetailsPane &&
                targetControl) {

                height = targetControl.height();

                width = targetControlContainer.width() - targetControlDetailsPane.width() - padding;

                targetControl.height(height);
                targetControl.width(width);
            }

            if (currentPane.ajaxData) {
                graphPane.find('.txt_deviceid').html(currentPane.ajaxData.deviceId);
                graphPane.find('.txt_heading').html(currentPane.ajaxData.heading);
                graphPane.find('.txt_speed').html(currentPane.ajaxData.speed);

                redraw(isTopPane);
            }
        });
    }

    return {
        init: init,
        resizeGraphPane: resizeGraphPane
    }
}, [jQuery, resources, powerbi]);
IoTApp.createModule(
    'IoTApp.Dashboard.TelemetryHistorySummary',
    function initTelemetryHistorySummary() {
        'use strict';

        var averageDeviceSpeedContainer;
        var averageDeviceSpeedControl;
        var averageDeviceSpeedLabel;
        var averageSpeedVisual;
        var lastAvgSpeed;
        var lastMaxSpeed;
        var lastMinSpeed;
        var maxDeviceSpeedContainer;
        var maxDeviceSpeedControl;
        var maxDeviceSpeedLabel;
        var maxSpeedVisual;
        var maxValue;
        var minDeviceSpeedContainer;
        var minDeviceSpeedControl;
        var minDeviceSpeedLabel;
        var minSpeedVisual;
        var minValue;

        var createDataView = function createDataView(indicatedValue) {

            var categoryMetadata;
            var dataView;
            var dataViewTransform;
            var graphMetadata;

            dataViewTransform = powerbi.data.DataViewTransform;

            graphMetadata = {
                columns: [
                    {
                        isMeasure: true,
                        roles: { 'Y': true },
                        objects: { general: { formatString: resources.telemetryGaugeNumericFormat } },
                    },
                    {
                        isMeasure: true,
                        roles: { 'MinValue': true },
                    },
                    {
                        isMeasure: true,
                        roles: { 'MaxValue': true },
                    }
                ],
                groups: [],
                measures: [0]
            };

            categoryMetadata = {
                values: dataViewTransform.createValueColumns([
                    {
                        source: graphMetadata.columns[0],
                        values: [indicatedValue],
                    }, {
                        source: graphMetadata.columns[1],
                        values: [minValue],
                    }, {
                        source: graphMetadata.columns[2],
                        values: [maxValue],
                    }])
            };

            dataView = {
                metadata: graphMetadata,
                single: { value: indicatedValue },
                categorical: categoryMetadata
            };

            return dataView;
        };

        var createDefaultStyles = function createDefaultStyles() {

            var dataColors = new powerbi.visuals.DataColorPalette();

            return {
                titleText: {
                    color: { value: 'rgba(51,51,51,1)' }
                },
                subTitleText: {
                    color: { value: 'rgba(145,145,145,1)' }
                },
                colorPalette: {
                    dataColors: dataColors,
                },
                labelText: {
                    color: {
                        value: 'rgba(51,51,51,1)',
                    },
                    fontSize: '11px'
                },
                isHighContrast: false,
            };
        };

        var createVisual = function createVisual(targetControl) {

            var height;
            var pluginService;
            var singleVisualHostServices;
            var visual;
            var width;

            height = $(targetControl).height();
            width = $(targetControl).width();

            pluginService = powerbi.visuals.visualPluginFactory.create();
            singleVisualHostServices = powerbi.visuals.singleVisualHostServices;

            // Get a plugin
            visual = pluginService.getPlugin('gauge').create();

            visual.init({
                element: targetControl,
                host: singleVisualHostServices,
                style: createDefaultStyles(),
                viewport: {
                    height: height,
                    width: width
                },
                settings: { slicingEnabled: true },
                interactivity: { isInteractiveLegend: false, selection: false },
                animation: { transitionImmediate: true }
            });

            return visual;
        };

        var init = function init(telemetryHistorySummarySettings) {

            maxValue = telemetryHistorySummarySettings.gaugeMaxValue;
            minValue = telemetryHistorySummarySettings.gaugeMinValue;

            averageDeviceSpeedContainer = telemetryHistorySummarySettings.averageDeviceSpeedContainer;
            averageDeviceSpeedControl = telemetryHistorySummarySettings.averageDeviceSpeedControl;
            averageDeviceSpeedLabel = telemetryHistorySummarySettings.averageDeviceSpeedLabel;
            maxDeviceSpeedContainer = telemetryHistorySummarySettings.maxDeviceSpeedContainer;
            maxDeviceSpeedControl = telemetryHistorySummarySettings.maxDeviceSpeedControl;
            maxDeviceSpeedLabel = telemetryHistorySummarySettings.maxDeviceSpeedLabel;
            minDeviceSpeedContainer = telemetryHistorySummarySettings.minDeviceSpeedContainer;
            minDeviceSpeedControl = telemetryHistorySummarySettings.minDeviceSpeedControl;
            minDeviceSpeedLabel = telemetryHistorySummarySettings.minDeviceSpeedLabel;

            averageSpeedVisual = createVisual(averageDeviceSpeedControl);
            maxSpeedVisual = createVisual(maxDeviceSpeedControl);
            minSpeedVisual = createVisual(minDeviceSpeedControl);
        };

        var redraw = function redraw() {
            var height;
            var width;

            if (minDeviceSpeedControl &&
                minSpeedVisual &&
                (lastMinSpeed || (lastMinSpeed === 0))) {
                height = minDeviceSpeedControl.height();
                width = minDeviceSpeedControl.width();

                minSpeedVisual.update({
                    dataViews: [createDataView(lastMinSpeed)],
                    viewport: {
                        height: height,
                        width: width
                    },
                    duration: 0
                });
            }

            if (maxDeviceSpeedControl &&
                maxSpeedVisual &&
                (lastMaxSpeed || (lastMaxSpeed === 0))) {
                height = maxDeviceSpeedControl.height();
                width = maxDeviceSpeedControl.width();

                maxSpeedVisual.update({
                    dataViews: [createDataView(lastMaxSpeed)],
                    viewport: {
                        height: height,
                        width: width
                    },
                    duration: 0
                });
            }

            if (averageDeviceSpeedControl &&
                averageSpeedVisual &&
                (lastAvgSpeed || (lastAvgSpeed === 0))) {
                height = averageDeviceSpeedControl.height();
                width = averageDeviceSpeedControl.width();

                averageSpeedVisual.update({
                    dataViews: [createDataView(lastAvgSpeed)],
                    viewport: {
                        height: height,
                        width: width
                    },
                    duration: 0
                });
            }
        };

        var resizeTelemetryHistorySummaryGuages =
            function resizeTelemetryHistorySummaryGuages() {

                var height;
                var padding;
                var width;

                padding = 0;

                if (averageDeviceSpeedContainer &&
                    averageDeviceSpeedLabel &&
                    averageDeviceSpeedControl) {

                    height =
                        averageDeviceSpeedContainer.height() -
                        averageDeviceSpeedLabel.height() -
                        padding;

                    width = averageDeviceSpeedContainer.width() - padding;

                    averageDeviceSpeedControl.height(height);
                    averageDeviceSpeedControl.width(width);
                }

                if (maxDeviceSpeedContainer &&
                    maxDeviceSpeedLabel &&
                    maxDeviceSpeedControl) {

                    height =
                        maxDeviceSpeedContainer.height() -
                        maxDeviceSpeedLabel.height() -
                        padding;

                    width = maxDeviceSpeedContainer.width() - padding;

                    maxDeviceSpeedControl.height(height);
                    maxDeviceSpeedControl.width(width);
                }

                if (minDeviceSpeedContainer &&
                    minDeviceSpeedLabel &&
                    minDeviceSpeedControl) {

                    height =
                        minDeviceSpeedContainer.height() -
                        minDeviceSpeedLabel.height() -
                        padding;

                    width = minDeviceSpeedContainer.width() - padding;

                    minDeviceSpeedControl.height(height);
                    minDeviceSpeedControl.width(width);
                }

                redraw();
            };

        var updateTelemetryHistorySummaryData =
            function updateTelemetryHistorySummaryData(
                minSpeed,
                maxSpeed,
                avgSpeed) {

                lastAvgSpeed = avgSpeed;
                lastMaxSpeed = maxSpeed;
                lastMinSpeed = minSpeed;

                redraw();
        };

        return {
            init: init,
            resizeTelemetryHistorySummaryGuages: resizeTelemetryHistorySummaryGuages,
            updateTelemetryHistorySummaryData: updateTelemetryHistorySummaryData
        };
    },
    [jQuery, resources, powerbi]);
IoTApp.createModule('IoTApp.LocationProperties', function () {
    "use strict";

    var self = this;

    var init = function (latitude, longitude, updateCallback) {
        self.latitude = latitude;
        self.longitude = longitude;
        self.updateCallback = updateCallback;
        getLocationPropertiesView();
    }

    var getLocationPropertiesView = function () {
        $('#loadingElement').show();

        $.ajaxSetup({ cache: false });

        $.get('/Location/GetLocationProperties', { latitude: self.latitude, longitude: self.longitude }, function (response) {
            if (!$(".details_grid").is(':visible')) {
                IoTApp.LocationsIndex.toggleProperties();
            }
            onLocationPropertiesDone(response);
        }).fail(function (response) {
            $('#loadingElement').hide();
            IoTApp.Helpers.RenderRetryError(resources.unableToRetrieveLocationsFromService, $('#details_grid_container'), function () { getLocationPropertiesView(); });
        });
    }

    var onLocationPropertiesDone = function (html) {
        $('#loadingElement').hide();
        $('#details_grid_container').empty();
        $('#details_grid_container').html(html);

        var viewReportButton = $("#btnViewReport")
        if (viewReportButton != null)
        {
            viewReportButton.on("click", function () {
                $("#frmReport").submit();
                return false;
            });
        }

        setDetailsPaneLoaderHeight();
    }

    var setDetailsPaneLoaderHeight = function () {
        /* Set the height of the Location Details progress animation background to accommodate scrolling */
        var progressAnimationHeight = $("#details_grid_container").height() + $(".details_grid__grid_subhead.button_details_grid").outerHeight();

        $(".loader_container_details").height(progressAnimationHeight);
    };

    return {
        init: init
    }

}, [jQuery, resources]);
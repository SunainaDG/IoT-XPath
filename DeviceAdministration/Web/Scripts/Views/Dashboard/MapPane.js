IoTApp.createModule('IoTApp.MapPane', (function () {
    "use strict";

    var self = this;
    var mapApiKey = null;
    var map;
    var pinInfobox;
    var boundsSet = false;
    var selectedLocations = [];
    var selectionBox;
    var deleteButton;
    var cancelButton;

    var init = function () {
        selectionBox = document.getElementById('selectedLocations');
        cancelButton = document.getElementById('cnlSelection');
        deleteButton = document.getElementById('dltSelection');        
        $.ajaxSetup({ cache: false });
        getMapKey();

        $(cancelButton).on('click', function () {
            var tblSelectedLocations = $("#tblSeletedLocations");
            selectedLocations = [];
            $(tblSelectedLocations).find("tbody").empty();
            selectionBox.style.display = "none";
        });

        $(deleteButton).on('click', function () {
            deleteSelectedLocations();
        });
    }

    var getMapKey = function () {
        $.get('/api/v1/telemetry/mapApiKey', {}, function (response) {
            self.mapApiKey = response;
            finishMap();
        });
    }

    var viewLocation = function (latitude, longitude) {
        var url = resources.redirectToLocationUrl.replace("ReplaceLat", latitude).replace("ReplaceLong", longitude).replace("&amp;","&");
        location.href = url;
    }

    var finishMap = function () {
        //var options = {
        //    credentials: self.mapApiKey,
        //    mapTypeId: Microsoft.Maps.MapTypeId.road, //aerial //canvasDark //grayscale
        //    animate: false,
        //    enableSearchLogo: false,
        //    enableClickableLogo: false,
        //    navigationBarMode: Microsoft.Maps.NavigationBarMode.minified,
        //    bounds: Microsoft.Maps.LocationRect.fromEdges(71, -28, -55, 28)
        //};

        var options = {
            zoom: 8,
            center: new google.maps.LatLng(-34, 151),
            mapTypeId: google.maps.MapTypeId.ROADMAP, //aerial //canvasDark //grayscale            
            zoomControl: true
        };

        // Initialize the map
        //self.map = new Microsoft.Maps.Map('#deviceMap', options);
        self.map = new google.maps.Map(document.getElementById('deviceMap'), options);

        // Hide the infobox when the map is moved.
        //Microsoft.Maps.Events.addHandler(self.map, 'viewchange', hideInfobox);
        google.maps.event.addListener(self.map, 'click', hideInfobox);
    }

    var deleteSelectedLocations = function () {
        var locations = [];

        $.each(selectedLocations, function (index, element) {
            var obj = { latitude: element.lat, longitude: element.lng };
            locations.push(obj);
        });

        if (locations.length > 0) {
            //pass to action method            
            $.post('/Location/DeleteSelected', { locations: locations }, function () {
                location.reload();
            }).fail(function (response) {
                IoTApp.Helpers.Dialog.displayError(resources.failedToDelete);
            });
        }
    };

    var isLocationSelected = function (lat,lng) {
        var selectedIndex = -1;

        $.each(selectedLocations, function (index, element) {
            if (element.lat === lat && element.lng === lng) {
                selectedIndex = index;
                return false;
            }
        });
        return selectedIndex;
    };

    var updateSelectedLocations = function (lat,lng) {
        
        //var latlng = selectedPin.customInfo.split(",");
        //var lat = latlng[0];
        //var lng = latlng[1];

        var selected;
        var tblSelectedLocations = $("#tblSeletedLocations");
        var index = isLocationSelected(lat, lng);

        if (index != -1) {
            selected = selectedLocations[index];
            selectedLocations.splice(index, 1);

            //change back icon color to original
            //selectedPin.setIcon(selected.statusicon);            
        }
        else {

            selected = {
                "lat": lat,
                "lng": lng
            };

            selectedLocations.push(selected);

            //var pinIcon = selectedPin.icon;

            //selected = {
            //    "lat": lat,
            //    "lng": lng,
            //    "statusicon": pinIcon
            //};

            //selectedLocations.push(selected);
            //selectedPin.setIcon(resources.selectedIcon);
        }  

        $(tblSelectedLocations).find("tbody").empty();

        $.each(selectedLocations, function (index, element) {
            var rowContent = "<tr><td>" + element.lat + "</td><td>" + element.lng + "</td></tr>";
            $(rowContent).appendTo(tblSelectedLocations);
        });

        if (selectedLocations.length > 0) {
            selectionBox.style.display = "block";
        }
        else {
            selectionBox.style.display = "none";
        }
    }

    var onMapPinClicked = function () {        
        hideInfobox();
        if (event.ctrlKey) {
            updateSelectedLocations(this.lat, this.lng);
        }
        else
        {
            //IoTApp.Dashboard.DashboardDevicePane.setSelectedDevice(this.deviceId);
            displayInfobox(this.totalDeviceCount, this.maxJerk, this.highestjerks, this.location, this.markerObject, this.lat, this.lng);
        }        
    }

    var displayInfobox = function (totalDeviceCount, maxJerk, highestjerks, location, markerObject,lat,lng) {
        //hideInfobox();

        var i;
        var j;
        var maxDeviceColWidth = 5;
        var title = "Highest Jerk: " + maxJerk.toString();
        var infoWindowContent = '';
        var actionLabel = "No. of Jerked Devices: " + totalDeviceCount.toString();                

        var descriptionTable = "";

        for (i = 0; i < highestjerks.length; ++i) {
            j = highestjerks[i];
            if (j.deviceId.length > maxDeviceColWidth)
            {
                maxDeviceColWidth = j.deviceId.length;
            }
            descriptionTable += '<tr><td>' + j.deviceId + ':</td><td>' + j.jerkValue + '(' + j.jerkType + ')</td><td>' + IoTApp.Helpers.Dates.localizeDate(j.jerkTime, 'L LTS')+'</td></tr>';
        }

        descriptionTable += '<tr><td colspan="2"><a onclick="IoTApp.MapPane.viewLocation(' + lat + ',' + lng + ')" href="javascript:void(0);">' + actionLabel + '</a></td></tr>';

        //infoWindowContent = '<div><h3>' + title + '</h3><table>' + descriptionTable + '</table><a onclick="viewLocation()" href="javascript:void(0);">' + actionLabel +'</a></div>';
        infoWindowContent = '<div><h3>' + title + '</h3><table>' + descriptionTable + '</table></div>';

        var width = ((maxDeviceColWidth + 36) * 7) + 55;
        var horizOffset = -(width / 2);

        //var width = (title.length * 7) + 55;
        //var horizOffset = -(width / 2);

        var infobox = new google.maps.InfoWindow({
            title: title,
            content: infoWindowContent,
            maxWidth: 1700,
            position: location,
            zIndex: 1
        });

        //var infobox = new Microsoft.Maps.Infobox(location, {
        //    title: title,
        //    description: description,
        //    maxWidth: 1700,
        //    maxHeight: 400,
        //    actions: [{
        //        label: actionLabel,
        //        eventHandler: function () { viewLocation() }
        //    }],
        //    offset: new Microsoft.Maps.Point(horizOffset, 35),
        //    showPointer: false
        //});
        //infobox.setMap(self.map);
        //$('.infobox-close').css('z-index', 1);

        self.pinInfobox = infobox;

        infobox.open(self.map, markerObject);
    }

    var hideInfobox = function () {
        if (self.pinInfobox != null) {
            self.pinInfobox.close();
            self.pinInfobox = null;
        }
    }

    var setDeviceLocationData = function setDeviceLocationData(minLatitude, minLongitude, maxLatitude, maxLongitude, deviceLocations) {
        var i;
        var loc;
        var mapOptions;
        var pin;
        var pinOptions;

        if (selectedLocations.length > 0) {
            selectionBox.style.display = "block";
        }
        else {
            selectionBox.style.display = "none";
        }

        if (!self.map) {
            return;
        }

        if (!boundsSet) {
            boundsSet = true;
            self.map.fitBounds(
                new google.maps.LatLngBounds(
                    new google.maps.LatLng(maxLatitude, minLongitude),
                    new google.maps.LatLng(minLatitude, maxLongitude)
                )
            );

            //mapOptions = self.map.getOptions();
            //mapOptions.bounds =
            //    Microsoft.Maps.LocationRect.fromCorners(
            //        new Microsoft.Maps.Location(maxLatitude, minLongitude),
            //        new Microsoft.Maps.Location(minLatitude, maxLongitude));
            //self.map.setView(mapOptions);
        }

        //self.map.entities.clear();
        if (deviceLocations) {
            for (i = 0; i < deviceLocations.length; ++i) {
                loc = new google.maps.LatLng(deviceLocations[i].latitude, deviceLocations[i].longitude);                

                pinOptions = {
                    position: loc,
                    map: self.map,
                    zIndex: deviceLocations[i].status,
                    title: deviceLocations[i].latitude + "," + deviceLocations[i].longitude
                };

                switch (deviceLocations[i].status) {
                    case 1:
                        pinOptions.icon = resources.cautionStatusIcon;
                        break;

                    case 2:
                        pinOptions.icon = resources.criticalStatusIcon;
                        break;
                         
                    default:
                        pinOptions.icon = resources.allClearStatusIcon;
                        break;
                }                

                //pin = new Microsoft.Maps.Pushpin(loc, pinOptions);
                //Microsoft.Maps.Events.addHandler(pin, 'click', onMapPinClicked.bind({ totalDeviceCount: deviceLocations[i].totalDeviceCount, maxJerk: deviceLocations[i].maxJerk, highestjerks: deviceLocations[i].highestJerks, location: loc }));
                //self.map.entities.push(pin);

                pin = new google.maps.Marker(pinOptions);
                google.maps.event.addListener(pin, 'click', onMapPinClicked.bind({totalDeviceCount: deviceLocations[i].totalDeviceCount, maxJerk: deviceLocations[i].maxJerk, highestjerks: deviceLocations[i].highestJerks, location: loc, markerObject: pin, lat: deviceLocations[i].latitude, lng: deviceLocations[i].longitude}));
                //google.maps.event.addListener(pin, 'click', function () {
                //    if (event.ctrlKey)
                //    {
                //        hideInfobox();
                //        updateSelectedLocations(this);
                //    }                    
                //})
                                                                
                //self.map.entities.push(pin);
            }
        }
    }

    return {
        init: init,
        setDeviceLocationData: setDeviceLocationData,
        viewLocation: viewLocation
    }
}), [jQuery, resources]);

$(function () {
    "use strict";

    IoTApp.MapPane.init();
});
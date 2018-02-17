IoTApp.createModule('IoTApp.EditLocationRuleProperties', function () {
    "use strict";

    var self = this;

    var init = function () {
        self.regionId = $("#txtRegionId").val();
        self.countrySelectList = $("#ddlCountries");
        self.stateSelectList = $("#ddlStates");
        self.hiddenRegionField = $("#hdnRegion");
        self.regionField = $("#txtRegion");        
        self.backButton = $(".header_main__button_back");

        self.backButton.show();
        self.backButton.off("click").click(backButtonClicked);
        
        populateCountrySelectList();        

        self.countrySelectList.on("change", function () {
            onSelectedCountryChanged(self.countrySelectList.find(':selected').val());
        });
        self.stateSelectList.on("change", function () {
            onSelectedStateChanged(self.stateSelectList.find(':selected').val());
        });        
    }

    var backButtonClicked = function () {
        history.back();
    }

    var populateCountrySelectList = function () {

        self.countrySelectList.append($('<option>', {
            value: "Select",
            text: "Select Country"
        }));

        self.stateSelectList.append($('<option>', {
            value: "Select",
            text: "Select State"
        }));

        var successCallBack = function (data) {
            if (data.countries != null && data.stateDictionary != null) {
                $.each(data.countries, function (i, item) {
                    self.countrySelectList.append($('<option>', {
                        value: item,
                        text: item
                    }));
                });

                self.stateDictionary = data.stateDictionary;
            }

            setLocationRegion(self.hiddenRegionField.val());
        }

        if (self.regionId == resources.defaultRuleName) {
            self.countrySelectList.attr("disabled", "disabled");
            self.countrySelectList.addClass("input_text--readonly");

            self.stateSelectList.attr("disabled", "disabled");
            self.stateSelectList.addClass("input_text--readonly");
        }
        else {
            $.ajax({
                dataType: 'json',
                type: 'GET',
                url: '/api/v1/locationrules/countrystatelist',
                cache: false,
                success: successCallBack,
                error: function () {
                    IoTApp.Helpers.Dialog.displayError(resources.failedToGetCountryStateList);
                    $('#ddlCountries').hide();
                    $('#ddlStates').hide();
                }
            });
        }
    }

    var onSelectedCountryChanged = function (countryChoice) {
        var statesArray = null;
        self.stateSelectList.html('');
        self.stateSelectList.append($('<option>', {
            value: "Select",
            text: "Select State"
        }));

        if (countryChoice == "Select") {
            self.hiddenRegionField.val('');
            self.regionField.val('');
        }
        else {
            self.hiddenRegionField.val(countryChoice);
            self.regionField.val(countryChoice);

            $.each(self.stateDictionary, function (key, value) {
                if (key.toLowerCase() == countryChoice.toLowerCase()) {
                    statesArray = value.split('|');
                    return false;
                }
            });

            if (statesArray != null && statesArray.length > 0) {
                $.each(statesArray, function (i, item) {
                    self.stateSelectList.append($('<option>', {
                        value: item,
                        text: item
                    }));
                });
            }
        }
    }

    var onSelectedStateChanged = function (stateChoice) {

        var selectedCountry = self.countrySelectList.find(':selected').val();

        if (stateChoice == "Select") {
            self.hiddenRegionField.val(selectedCountry);
            self.regionField.val(selectedCountry);
        }
        else {
            self.hiddenRegionField.val(selectedCountry + "#" + stateChoice);
            self.regionField.val(selectedCountry + "," + stateChoice);
        }
    }

    var setLocationRegion = function (region) {
        if (region !== "")
        {
            var countryStateArray = region.split("#");
            var country = "Select";
            var state = "Select";

            if (countryStateArray.length > 1) {
                country = countryStateArray[0];
                state = countryStateArray[1];
            }
            else
            {
                country = countryStateArray[0];
            }

            $('#ddlCountries > option').each(function () {
                if (this.value.toLowerCase() == country.toLowerCase()) {
                    $(this).prop("selected", true);
                    return false;
                } else {
                    $(this).removeProp("selected");
                }
            });

            onSelectedCountryChanged(country);

            $('#ddlStates > option').each(function () {
                if (this.value.toLowerCase() == state.toLowerCase()) {
                    $(this).prop("selected", true);
                } else {
                    $(this).removeProp("selected");
                }
            });

            onSelectedStateChanged(state);
        }
    }

    var onBegin = function () {
        $('#update_rule_properties').attr("disabled", "disabled");
    }

    var onSuccess = function (result) {
        $('#update_rule_properties').removeAttr("disabled");
        if (result.success) {
            location.href = resources.redirectUrl;
        } else {
            if (result.error) {
                IoTApp.Helpers.Dialog.displayError(result.error);
            } else {
                IoTApp.Helpers.Dialog.displayError(resources.ruleUpdateError);
            }
            if (result.entity != null) {
                // since the data may have changed on the server, update the data
                updateLayout(result.entity);
            }
        }
    }

    var onFailure = function (result) {
        $('#update_rule_properties').removeAttr("disabled");
        IoTApp.Helpers.Dialog.displayError(resources.ruleUpdateError);
        if (result.entity != null) {
            updateLayout(result.entity);
        }
    }

    var onComplete = function () {
        $('#loadingElement').hide();
    }

    var updateLayout = function (rule) {
        rule = JSON.parse(rule);
        $('#Etag').val(rule.Etag);
        $('#EnabledState').attr({ "data-val": rule.EnabledState.toString(), "value": rule.EnabledState.toString() });
        if (rule.EnabledState == true) {
            //$('#state').val(resources.enabledString);
        } else {
            //$('#state').val(resources.disabledString);
        }
        $('#RegionId').val(rule.RegionId);
        $('#RegionLatitude').val(rule.RegionLatitude);
        $('#RegionLongitude').val(rule.RegionLongitude);

        //set region here
        setLocationRegion(rule.Region);

        $('#RuleId').val(rule.RuleId);
        $('#VerticalThreshold').val(rule.VerticalThreshold);
        $('#LateralThreshold').val(rule.LateralThreshold);
        $('#ForwardThreshold').val(rule.ForwardThreshold);
        $('#RuleOutput').val(rule.RuleOutput);
    }    

    return {
        init: init,
        onBegin: onBegin,
        onSuccess: onSuccess,
        onFailure: onFailure,
        onComplete: onComplete
    }

}, [jQuery, resources]);

$(function () {
    "use strict";

    IoTApp.EditLocationRuleProperties.init();
});
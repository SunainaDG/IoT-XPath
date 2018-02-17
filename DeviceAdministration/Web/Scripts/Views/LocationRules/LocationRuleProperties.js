IoTApp.createModule('IoTApp.LocationRuleProperties', function () {
    "use strict";

    var self = this;

    var init = function (regionId, ruleId, updateCallback) {
        self.regionId = regionId;
        self.ruleId = ruleId;
        self.updateCallback = updateCallback;
        getRulePropertiesView();
    }

    var getRulePropertiesView = function () {
        $('#loadingElement').show();

        $.ajaxSetup({ cache: false });
        $.get('/LocationRules/GetRuleProperties', { regionId: self.regionId, ruleId: self.ruleId }, function (response) {
            if (!$(".details_grid").is(':visible')) {
                IoTApp.LocationRulesIndex.toggleProperties();
            }
            onRulePropertiesDone(response);
        }).fail(function (response) {
            $('#loadingElement').hide();
            IoTApp.Helpers.RenderRetryError(resources.unableToRetrieveRuleFromService, $('#details_grid_container'), function () { getRulePropertiesView(); });
        });
    }

    var onRulePropertiesDone = function (html) {
        $('#loadingElement').hide();
        $('#details_grid_container').empty();
        $('#details_grid_container').html(html);

        var removeButton = $('#remove_rule_button');
        if (removeButton !== null) {
            removeButton.on("click", function () {
                location.href = "/LocationRules/DeleteLocationRule?regionId=" + self.regionId + "&ruleId=" + self.ruleId;
            });
        }

        setDetailsPaneLoaderHeight();
    }

    var setDetailsPaneLoaderHeight = function () {
        /* Set the height of the Rule Details progress animation background to accommodate scrolling */
        var progressAnimationHeight = $("#details_grid_container").height() + $(".details_grid__grid_subhead.button_details_grid").outerHeight();

        $(".loader_container_details").height(progressAnimationHeight);
    };

    var onBegin = function () {
        $('#button_rule_status').attr("disabled", "disabled");
    }

    var onSuccess = function (result) {
        $('#button_rule_status').removeAttr("disabled");
        if (result.success) {
            self.updateCallback();
        } else if (result.error) {
            IoTApp.Helpers.Dialog.displayError(result.error);
        } else {
            IoTApp.Helpers.Dialog.displayError(resources.ruleUpdateError);
        }
    }

    var onFailure = function (result) {
        $('#button_rule_status').removeAttr("disabled");
        IoTApp.Helpers.Dialog.displayError(resources.ruleUpdateError);
    }

    var onComplete = function () {
        $('#loadingElement').hide();
    }

    return {
        init: init,
        onBegin: onBegin,
        onSuccess: onSuccess,
        onFailure: onFailure,
        onComplete: onComplete
    }
}, [jQuery, resources]);
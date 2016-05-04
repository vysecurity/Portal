$(document).ready(function () {

    // Clean quick button class
    $('body:not(#quickstartbutton)').click(function (e) {
        $('#quickstartbutton').removeClass('open');
    });

    // Displayed button array
    var nextStageIDsCond = [];
    var nextStageIDsMain = [];

    // First and last stage buttons
    var nextButton = null, prevButton = null, firstButton = null, lastButton = null;

    // Posted stage id
    var postedStageID;

    Init = function () {

        // Create stage buttons
        var stageButtonContainer = $('#stageButtons');

        $(bpfData.StageList).each(function (index, value) {

            var stageID = value['StageID'];
            var nextstageID = value['NextStageID'];
            var stageName = value['LabelDescription'];
            var btn = $('<a>').appendTo($(stageButtonContainer));
            $(btn).addClass('btn btn-default showButton');
            $(btn).attr({ 'id': 'stage_' + stageID, 'data-id': stageID, 'data-nextstageid': nextstageID });
            $(btn).css({ 'font-size': '11px', 'min-width':'250px' })
            $(btn).text(stageName);


            if (value['IsMainStage'] != 'true') {
                $(btn).hide();
                $(btn).removeClass('showButton');
            }

            $(btn).on("click", function () {
                GetFirstAndLastButtons();
                SetGrid(stageID);
            });
        });

        // Create navigation buttons
        // Create previous stage button
        var previousStageBtn = $('<a>').appendTo($('#navigationButtons'));
        previousStageBtn.addClass("btn btn-default btn-primary previousstagebutton");
        previousStageBtn.css({ 'width': '35px;', 'font-size': '11px' })
        previousStageBtn.html('<i class="fa fa-arrow-left" style="color:#fff; font-size:11px;"></i>');
        previousStageBtn.attr('disabled', true);

        $(previousStageBtn).on('click', function () {

            GetFirstAndLastButtons();

            var previousStageID = $(prevButton).data('id');

            if (previousStageID == $(firstButton).data('id')) {
                //$('.previousstagebutton').hide();
                $('.previousstagebutton').attr("disabled", true);
                //$('#navigationButtons').css({ 'margin-left': '50px' });
            } else {
                //$('#navigationButtons').css({ 'margin-left': '10px' });
            }
            
            var tmpCurrentStageID = currentStageID;
            SetGrid(previousStageID, true);

            if (IsEditForm()) {
                UpdateBpf('#grid_' + tmpCurrentStageID);
            }

            //$('.nextstagebutton').show();
            $('.nextstagebutton').attr("disabled", false);
        });

        
        // Create next stage button
        var nextStageBtn = $('<a>').appendTo($('#navigationButtons'));
        nextStageBtn.addClass("btn btn-default btn-primary nextstagebutton");
        nextStageBtn.html('<i class="fa fa-arrow-right" style="color:#fff; font-size:11px"></i> Next Stage');
        nextStageBtn.css({ 'width': '105px;', 'font-size': '11px' })

        $(nextStageBtn).on('click', function () {

            GetFirstAndLastButtons();

            // Check required field
            if (RequiredControl() == true) {

                var nextStageID = $(nextButton).data('id');

                if (nextStageID == $(lastButton).data('id')) {
                    //$('.nextstagebutton').hide();
                    $('.nextstagebutton').attr("disabled", true);
                    //$('#navigationButtons').css({ 'margin-left': '105px' });
                } else {
                    //$('#navigationButtons').css({ 'margin-left': '10px' });
                }

                var tmpCurrentStageID = currentStageID;
                SetGrid(nextStageID, true);

                if (IsEditForm()) {
                    UpdateBpf('#grid_' + tmpCurrentStageID);
                }


                //$('.previousstagebutton').show();
                $('.previousstagebutton').attr("disabled", false);

            }

        });


        // Selected first stage button
        var currentButton = $('#stageButtons').find('a').first();
        currentButton.addClass('btn-primary');
        currentStageID = currentButton.attr('data-id');
        SetCurrentIcon($(currentButton));

        postedStageID = currentStageID;

        // For page reloaded - Reinit
        $('[data-attribute="yes"]').each(function (index, value) {

            var dataType = $(value).attr('data-type');

            //if (dataType == "datetime") {

            //    var datePart = $(this).attr('data-timepicker');
            //    var dateFormat = $(this).attr('data-dateformat');
            //    var timeFormat = $(this).attr('data-timeformat');

            //    $(this).datetimepicker({
            //        timepicker: datePart == "dateandtime" ? true : false,
            //        format: datePart == "dateandtime" ? dateFormat + " " + timeFormat : dateFormat,
            //        step: 60

            //    });
            //}

            if (dataType == "lookup" || dataType == "customer") {

                var btn = $(this).parent().find('span').find('button.clicker');

                $(btn).unbind("click");
                $(btn).on("click", function (event, datas) {
                    MakeLookup(this, datas, false);
                });
            }


            // Set process id field
            $('#processid').val(bpfData.ProcessID);
            // Set stageid
            $('#stageid').val(postedStageID);


        });


        //Selected first stage template for onload
        $('#stageGridContainer').find('.stage').first().addClass('currentStage').removeClass('invisible');

        //Set Gear button in bpf list
        $('#quickbpfselector').remove();
        var li = $('<li id="quickbpfselector">').appendTo($('#quickstartelements'));
        li.text('Business Process Flow');
        var select = $('<select>').appendTo($(li));
        select.attr({ 'id': 'bpfSelector' });
        select.addClass('theme-setting theme-setting-style form-control input-sm input-small input-inline tooltips');
        $('#tmpbpfselectlist option').clone().appendTo($(select));

        // Reload selected bpf
        $(select).on("change", function () {
            var selectedBPF = $("#bpfSelector option:selected").text();

            if (selectedBPF != null && selectedBPF != "") {
                $('#tmpbpfselectlist option').each(function (index, value) {
                    $(value).removeAttr('selected');
                    if (value['value'] == selectedBPF) {
                        $(value).attr({ 'selected': 'selected' })
                    }
                });
                SwitchBusinessProcessFlow(selectedBPF);
            }
        });

        // All inputs change events
        
        $('.bpftrigger, .bpfformtrigger').on('change', function () {
            BPFValuesCopyToForm($(this));
            ConditionControl();
        });
        
        DatetimeInit();

    }

    // Change to stage view method
    SetGrid = function (stageID, isNavigation) {

        var nextButton = $('.nextstagebutton');
        var previousButton = $('.previousstagebutton');

        if (stageID == postedStageID) {
            $(nextButton).attr("disabled", false);
            $(previousButton).attr("disabled", false);

            if (postedStageID == $(firstButton).data('id')) {
                $(previousButton).attr("disabled", true);
            }

            if (postedStageID == $(lastButton).data('id')) {
                $(nextButton).attr("disabled", true);
            }
        } else {
            $(nextButton).attr("disabled", true);
            $(previousButton).attr("disabled", true);
        }


        var currentStageButton = $('#stageButtons').find('a.btn-primary').first();
        var currentStageGrid = $('.currentStage');

        if (currentStageButton.length != 0) {
            $(currentStageButton).removeClass('btn-primary').addClass('btn-default');
        }

        if (currentStageGrid.length != 0) {
            $(currentStageGrid).removeClass('currentStage').addClass('invisible');
        }

        $('#stage_' + stageID).removeClass('btn-default').addClass('btn-primary');
        $('#grid_' + stageID).removeClass('invisible').addClass('currentStage');

        currentStageID = $('#stage_' + stageID).attr('data-id');


        if (isNavigation) {

            postedStageID = stageID;

            $('#currentIcon').remove();
            SetCurrentIcon($('#stage_' + stageID));

            $(nextButton).attr("disabled", false);
            $(previousButton).attr("disabled", false);

            if (postedStageID == $(firstButton).data('id')) {
                $(previousButton).attr("disabled", true);
            }

            if (postedStageID == $(lastButton).data('id')) {
                $(nextButton).attr("disabled", true);
            }

            PrepareTraversedPath();
        }

        // Bind Unbind inputs
        if (postedStageID == stageID) {
            $('.bpftrigger').on('change', function () {
                BPFValuesCopyToForm($(this));
                ConditionControl();
            });
        } else {
            $(".bpftrigger").unbind("change");
        }

        $('#stageid').val(postedStageID);
    }

    // Load method for selected bpf
    SwitchBusinessProcessFlow = function (selectedBPF) {

        $.ajax({
            url: '/Page/GetBusinessProcessFlowHTML',
            type: "POST",
            data: {
                DataWidgetID: datawidgetid,
                BusinessProcessFlowList: bpfList,
                SelectedBPF: selectedBPF
            },
            dataType: "html",
            async: true,
            success: function (result) {
                $('#collapse_bpf').html(result);
                $('#bpfTitle').text(bpfTitle);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log(XMLHttpRequest + ' - ' + textStatus + ' - ' + errorThrown);
            }
        });
    }

    // Main condition control method
    ConditionControl = function (stageIDParameter) {

        //Clear error tooltips
        $('.tipserror').remove();

        var forConditionStageID;

        if (stageIDParameter != null) {
            forConditionStageID = stageIDParameter;
        } else {
            forConditionStageID = currentStageID;
        }

        var currentStageConditions = $('#grid_' + forConditionStageID).data('conditions');

        // All conditions grouped
        var groupData = currentStageConditions.reduce(function (result, current) {
            result[current.NextStageID] = result[current.NextStageID] || [];
            result[current.NextStageID].push(current);
            return result;
        }, {});

        if (groupData != null && groupData.length != 0) {

            for (branch in groupData) {

                var condBranch = groupData[branch];
                var conditionSize = condBranch.length;
                var result = 0;
                var anorOperator = condBranch[0].AndOr;
                var nextStageID = condBranch[0].NextStageID;
                var containsElseBranch = condBranch[0].ContainsElsebranch;

                var elseBranch;
                if (containsElseBranch == 'true') {
                    elseBranch = currentStageConditions[currentStageConditions.length - 1].NextStageID;
                }

                // Condition checked
                $(condBranch).each(function (index, value) {

                    var attributeName = null, operator = null, conditionType = null, tValue = null;

                    attributeName = value['AttributeName'];
                    operator = value['ConditionOperator'];
                    conditionType = value['ConditionType'];
                    tValue = value['Value'];

                    if (value['ConditionOperator'] == 0) {
                        result += bpfDoesNotContainData(attributeName, operator, conditionType);

                    } else if (value['ConditionOperator'] == 1) {
                        result += bpfContainData(attributeName, operator, conditionType);

                    } else if (value['ConditionOperator'] == 6) {
                        result += bpfEqual(attributeName, operator, conditionType, tValue);

                    } else if (value['ConditionOperator'] == 8) {
                        result += bpfContains(attributeName, operator, conditionType, tValue);

                    } else if (value['ConditionOperator'] == 9) {
                        result += bpfDoesNotContain(attributeName, operator, conditionType, tValue);

                    } else if (value['ConditionOperator'] == 10) {
                        result += bpfBeginWith(attributeName, operator, conditionType, tValue);

                    } else if (value['ConditionOperator'] == 11) {
                        result += bpfDoesNotBeginWith(attributeName, operator, conditionType, tValue);

                    } else if (value['ConditionOperator'] == 12) {
                        result += bpfEndWith(attributeName, operator, conditionType, tValue);

                    } else if (value['ConditionOperator'] == 13) {
                        result += bpfDoesNotEndWith(attributeName, operator, conditionType, tValue);

                    } else if (value['ConditionOperator'] == 14) {
                        result += bpfGreaterThan(attributeName, operator, conditionType, tValue);

                    } else if (value['ConditionOperator'] == 15) {
                        result += bpfGreaterThanOrEqual(attributeName, operator, conditionType, tValue);

                    } else if (value['ConditionOperator'] == 16) {
                        result += bpfLessThan(attributeName, operator, conditionType, tValue);

                    } else if (value['ConditionOperator'] == 17) {
                        result += bpfLessThanOrEqual(attributeName, operator, conditionType, tValue);
                    }


                });

                var button = $('#stageButtons').find('a[data-id="' + nextStageID + '"]');
                var elseBranchButton = $('#stageButtons').find('a[data-id="' + elseBranch + '"]');

                nextStageIDsCond = [];
                PrepareDisplayedButtonWithConditionNextStage(nextStageID);


                var currentStage;
                for (var z = 0; z < bpfData.StageList.length; z++) {
                    if (bpfData.StageList[z].StageID == forConditionStageID) {
                        currentStage = bpfData.StageList[z];
                    }
                }

                nextStageIDsMain = [];
                PrepareDisplayedButtonWithMainStage(currentStage['NextStageID']);



                if (anorOperator == 2 || anorOperator == null) {
                    if (result == conditionSize) {

                        $('.tipserror').remove();
                        $(button).addClass('showButton').show();

                        $('#stageButtons').find('a.showButton.btn-primary').nextUntil().hide();

                        for (var i = 0; i < nextStageIDsCond.length; i++) {
                            var showedButton = $('#stageButtons').find('a[data-id="' + nextStageIDsCond[i] + '"]');
                            $(showedButton).addClass('showButton').show();
                        }

                        $(elseBranchButton).removeClass('showButton').hide();

                        return false;

                    } else {
                        $(button).removeClass('showButton').hide();

                        $('#stageButtons').find('a.showButton.btn-primary').nextUntil().hide().removeClass('showButton');

                        for (var i = 0; i < nextStageIDsMain.length; i++) {
                            var showedButton = $('#stageButtons').find('a[data-id="' + nextStageIDsMain[i] + '"]');
                            $(showedButton).addClass('showButton').show();
                        }

                        if (containsElseBranch == 'true') {
                            $(elseBranchButton).addClass('showButton').show();
                        }


                    }

                } else if (anorOperator == 3) {
                    if (result > 0) {

                        $('.tipserror').remove();
                        $(button).addClass('showButton').show();

                        $('#stageButtons').find('a.showButton.btn-primary').nextUntil().hide();

                        for (var i = 0; i < nextStageIDsCond.length; i++) {
                            var showedButton = $('#stageButtons').find('a[data-id="' + nextStageIDsCond[i] + '"]');
                            $(showedButton).addClass('showButton').show();
                        }


                        $(elseBranchButton).removeClass('showButton').hide();

                        return false;
                    } else {
                        $(button).removeClass('showButton').hide();

                        $('#stageButtons').find('a.showButton.btn-primary').nextUntil().hide().removeClass('showButton');

                        for (var i = 0; i < nextStageIDsMain.length; i++) {
                            var showedButton = $('#stageButtons').find('a[data-id="' + nextStageIDsMain[i] + '"]');
                            $(showedButton).addClass('showButton').show();
                        }

                        if (containsElseBranch == 'true') {
                            $(elseBranchButton).addClass('showButton').show();
                        }

                    }
                }






            }
        }


        //Set tooltip labels
        $('[data-toggle="tooltip"]').tooltip();

    }

    // Add tooltips text method
    PrepareTooltipText = function (value, label) {

        var infoLabel = label.find('i');

        // İf exist i tag, append to err text
        if (infoLabel != null && infoLabel.length != 0) {
            var errTitle1;
            if (infoLabel.attr('data-original-title') != null && infoLabel.attr('data-original-title') != 'undefined') { errTitle1 = infoLabel.attr('data-original-title'); } else { errTitle1 = '' };
            var errTitle2;
            if (infoLabel.attr('title') != null && infoLabel.attr('title') != 'undefined') { errTitle2 = infoLabel.attr('title'); } else { errTitle2 = '' };

            errText = errTitle1 + errTitle2 + ' - ' + value;

            infoLabel.attr({ 'title': errText });
        } else {
            //if does not i tag, add new i tag
            var text = $('<i>');
            text.addClass('fa fa-warning tipserror');
            text.attr({ 'data-placement': 'right', 'data-toggle': 'tooltip', 'title': value });
            text.css({ 'color': 'red', 'z-index': '1', 'position': 'relative' });
            text.appendTo(label);
        }
    }

    // Required field control
    RequiredControl = function () {

        var requiredInputs = $('#grid_' + currentStageID).find('input, select');
        var requiredResult = 0;
        $(requiredInputs).each(function (index, value) {
            var isRequired = ($(value).attr('data-required'));
            if (isRequired == 'true') {

                var val = $(value).val();

                if (val == null || val == '' || val == 'null') {
                    var label = $(value).parent().parent().find('label');

                    // Add error tips to label
                    PrepareTooltipText('This step is required by the process', label);
                    requiredResult = 1;
                }

            }
        });


        //Has required field error
        if (requiredResult == 1) {
            $('[data-toggle="tooltip"]').tooltip();
            return false;
        } else {
            return true;
        }
    }

    // Next stage calculated with conditions obj
    PrepareDisplayedButtonWithConditionNextStage = function (nextStageID) {


        for (var i = 0; i < bpfData.StageList.length; i++) {

            //Last main stage
            if (bpfData.StageList[i + 1] != null && bpfData.StageList[i + 1].NextStageID == '') {

                var exist = $.inArray(bpfData.StageList[i + 1].StageID, nextStageIDsCond);

                if (exist == '-1') {
                    nextStageIDsCond.push(bpfData.StageList[i + 1].StageID);
                }

            }


            //Add main stages
            if (bpfData.StageList[i + 1] != null && bpfData.StageList[i + 1].StageID == nextStageID) {

                nextStageIDsCond.push(bpfData.StageList[i + 1].StageID)

                PrepareDisplayedButtonWithConditionNextStage(bpfData.StageList[i + 1].NextStageID);

            }
        }

    }

    // Next stage calculated with stage obj 
    PrepareDisplayedButtonWithMainStage = function (nextStageID) {


        for (var i = 0; i < bpfData.StageList.length; i++) {

            //Last main stage
            if (bpfData.StageList[i + 1] != null && bpfData.StageList[i + 1].NextStageID == '') {

                var exist = $.inArray(bpfData.StageList[i + 1].StageID, nextStageIDsMain);

                if (exist == '-1') {
                    nextStageIDsMain.push(bpfData.StageList[i + 1].StageID);
                }

            }


            //Add main stages
            if (bpfData.StageList[i + 1] != null && bpfData.StageList[i + 1].StageID == nextStageID) {

                nextStageIDsMain.push(bpfData.StageList[i + 1].StageID)

                PrepareDisplayedButtonWithMainStage(bpfData.StageList[i + 1].NextStageID);

            }
        }

    }

    // Get showed buttons
    GetFirstAndLastButtons = function () {

        var btn = $('#stageButtons .showButton')

        firstButton = $('#stageButtons .showButton').first();
        lastButton = $('#stageButtons .showButton').last();

        for (var i = 0; i < btn.length; i++) {
            if ($(btn[i]).data('id') == postedStageID) {
                nextButton = $(btn[i + 1]);
                prevButton = $(btn[i - 1]);
                return true;
            }
        }

    };

    // Bpf values clone to form
    BPFValuesCopyToForm = function (el) {

 
        if ($(el).hasClass('bpftrigger brtrigger')) {

            var type = el.data('type');
            
            if (type == 'string' || type == 'memo' || type == 'decimal' || type == 'double' || type == 'integer' || type == 'money' || type == 'metronicdate') {

                var value = $(el).val();
                var attrName = $(el).attr("name");
                $('[name=' + attrName + ']').val(value);

            } else if (type == 'picklist' || type == 'status' || type == 'boolean') {

                var attrName = $(el).attr("name");
                var attrType = $('[name = ' + attrName + ']').last().attr('type');

                if (attrType == 'radio') {
                    //Radio / option
                    var value = $(el).val();
                    $('[name="' + attrName + '"][type="radio"][value="' + value + '"]').parent().parent().parent().siblings().find('input').removeAttr("checked");
                    $('[name="' + attrName + '"][type="radio"][value="' + value + '"]').parent().parent().parent().siblings().find('span').removeClass('checked');
                    $('[name="' + attrName + '"][type="radio"][value="' + value + '"]').attr("checked", "checked");
                    $('[name="' + attrName + '"][type="radio"][value="' + value + '"]').parent().addClass('checked');

                } else {
                    //Select / option
                    $('[name = ' + attrName + ']').val($(el).val());
                }


            } else if (type == 'customer' || type == 'lookup') {
                var attrName = $(el).attr("name");
                $('[name = ' + attrName + ']').val($(el).val());
                $('[name = ' + attrName + ']').attr('data-id', $(el).attr('data-id'));
            }

        } else if ($(el).hasClass('bpfformtrigger brformtrigger')) {
            var type = el.data('type');

            if (type == 'string' || type == 'memo' || type == 'decimal' || type == 'double' || type == 'integer' || type == 'money' || type == 'metronicdate') {

                var value = $(el).val();
                var attrName = $(el).attr("name");
                $('[name=' + attrName + ']').val(value);

            } else if (type == 'customer' || type == 'lookup') {
                var attrName = $(el).attr("name");
                $('[name = ' + attrName + ']').val($(el).val());
                $('[name = ' + attrName + ']').attr('data-id', $(el).attr('data-id'));

            } else if (type == 'picklist' || type == 'status' || type == 'boolean') {

                var attrName = $(el).attr("name");
                var attrType = $('[name = ' + attrName + ']').last().attr('type');

                if (attrType == 'radio') {
                    //Radio / option
                    $('select[name="' + attrName + '"]').val($(el).val());
                } else {
                    //Select / option
                    $('[name = ' + attrName + ']').val($(el).val());
                }



            }
        }

        
    }

    // Add flag icon
    SetCurrentIcon = function (btn) {
        $('#stageButtons a').removeClass('postStage');
        $('#currentIcon').remove();

        $(btn).addClass('postStage btn-primary');
        var currentIcon = $('<span>').appendTo($(btn));
        currentIcon.attr({ 'id': 'currentIcon' });
        var icon = $('<i>').appendTo($(currentIcon));
        icon.addClass('fa fa-map-marker');
        icon.css({ 'font-size': '30px', 'color': '#26a69a', 'position': 'absolute', 'left': '12px', 'top': '8px' });
    }

    // Prepare traversedpath
    PrepareTraversedPath = function () {

        var traversed = "";

        var buttons = $('#stageButtons a.showButton');

        $.each($(buttons), function (index, value) {

            traversed += $(value).data('id');

            if ($(value).hasClass('postStage'))
                return false;

            traversed += ',';
        });


        $('#traversedpath').val(traversed);

    }

    IsEditForm = function () {
        var selectedProcessID = bpfData.SelectedProcessID;
        var selectedStageID = bpfData.SelectedStageID;
        var selectedTraversedPath = bpfData.SelectedTraversedPath;

        if (selectedProcessID != '' && selectedStageID != '' && selectedTraversedPath != '') {
            return true;
        } else {
            return false;
        }
    }

    // Init edit form
    PrepareEditFormStage = function () {

        var selectedProcessID = bpfData.SelectedProcessID;
        var selectedStageID = bpfData.SelectedStageID;
        var selectedTraversedPath = bpfData.SelectedTraversedPath;

        if (IsEditForm()) {

            var buttonArr = selectedTraversedPath.split(',');
            $('#stageButtons a').removeClass('btn-primary');

            var stageButtons;
            stageButtons = $('#stageButtons a').first().nextUntil($('#stageButtons a[data-id="' + buttonArr[buttonArr.length - 1] + '"]').next())

            if (stageButtons.length == 0) {
                stageButtons = $('#stageButtons a[data-id="' + buttonArr[buttonArr.length - 1] + '"]')
            }

            for (var i = 0; i < buttonArr.length; i++) {

                for (var z = 0; z < $(stageButtons).length; z++) {

                    if ($(stageButtons[z]).data('id') == selectedStageID) {
                        $(stageButtons[z]).addClass('postStage btn-primary');
                        $(stageButtons[z]).show();

                        postedStageID = selectedStageID;
                        currentStageID = selectedStageID;
                        SetGrid(postedStageID, true);
                    }

                }

            }


            for (var i = 0; i < buttonArr.length; i++) {
                ConditionControl(buttonArr[i]);
            }

            GetFirstAndLastButtons();

            if (selectedStageID == $(lastButton).data('id')) {
                $('.nextstagebutton').attr('disabled', true);
                //$('.nextstagebutton').hide();
                //$('#navigationButtons').css({ 'margin-left': '105' });
            } else {
                $('.nextstagebutton').attr('disabled', false);
                //$('#navigationButtons').css({ 'margin-left': '10px' });
                //$('.nextstagebutton').show();
            }


            if (selectedStageID == $(firstButton).data('id')) {
                $('.previousstagebutton').attr('disabled', true);
                //$('.previousstagebutton').hide();
                //$('#navigationButtons').css({ 'margin-left': '50px' });
            } else {
                $('.previousstagebutton').attr('disabled', false);
                //$('#navigationButtons').css({ 'margin-left': '10px' });
                //$('.previousstagebutton').show();
            }

        }

    }

    // Update current bpf field method
    UpdateBpf = function (grid) {

        var smodalid = $('.formwidgetclass').data('widgetid');

        Metronic.blockUI();
        Metronic.blockUI({
            target: $("#responsive_" + smodalid)
        });


        var senderarray = [];
        var entityname = "";

        $(grid).find("[data-attribute=yes]").each(function () {
            var senderobject = {};
            entityname = bpfData.PrimaryEntityName;

            var type = $(this).attr("data-type");

            if (type == "string") {
                if ($(this).val() != "") {
                    if ($(this).attr("data-formatted") == 1) {
                        senderobject.logicalname = $(this).attr("name");
                        senderobject.type = type;
                        senderobject.value = $(this).val().replace("(", "").replace(")", "").replace(/ /g, '');
                        senderarray.push(senderobject);
                    }
                    else {
                        senderobject.logicalname = $(this).attr("name");
                        senderobject.type = type;
                        senderobject.value = $(this).val();
                        senderarray.push(senderobject);
                    }
                }
            }
            else if (type == "lookup") {
                if ($(this).attr("data-id") != "" && $(this).attr("data-id") != undefined) {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).attr("data-id");
                    senderobject.entityname = $(this).attr("data-target");

                    senderarray.push(senderobject);
                }
            }
            else if (type == "customer") {
                if ($(this).attr("data-id") != "" && $(this).attr("data-id") != undefined) {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).attr("data-id");
                    senderobject.entityname = $(this).attr("data-target");
                    senderarray.push(senderobject);
                }
            }
            else if (type == "ınteger" || type == "integer") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
            else if (type == "money") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
            else if (type == "decimal") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
            else if (type == "double") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
            else if ($(this).attr("data-type") == "metronicdatetime") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = "datetime";
                    senderobject.value = $(this).val();
                    senderobject.datetimepicker = $(this).attr("data-timepicker");
                    senderobject.dateformat = $(this).attr("data-beforedateformat");
                    senderobject.timeformat = $(this).attr("data-beforetimeformat");
                    senderarray.push(senderobject);
                }

            }
            else if ($(this).attr("data-type") == "metronicdate") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = "datetime";
                    senderobject.value = $(this).val();
                    senderobject.datetimepicker = $(this).attr("data-timepicker");
                    senderobject.dateformat = $(this).attr("data-beforedateformat");
                    senderobject.timeformat = $(this).attr("data-beforetimeformat");
                    senderarray.push(senderobject);
                }

            }
            else if (type == "datetime") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderobject.datetimepicker = $(this).attr("data-timepicker");
                    senderobject.dateformat = $(this).attr("data-beforedateformat");
                    senderobject.timeformat = $(this).attr("data-beforetimeformat");
                    senderarray.push(senderobject);
                }
            }
            else if (type == "picklist") {

                if ($(this).val() != null) {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).find(":selected").val();
                    senderarray.push(senderobject);
                }
                else {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = null;
                    senderarray.push(senderobject);
                }
            }
            else if (type == "memo") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
            else if (type == "boolean") {
                senderobject.logicalname = $(this).attr("name");
                senderobject.type = type;
                senderobject.value = null;
                senderarray.push(senderobject);

            } else if (type == "guid") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
        });

        $('#bpfFormValue').find("[data-attribute=yes]").each(function () {

            var senderobject = {};

            var type = $(this).attr("data-type");

            if (type == "guid" || type == "string") {
                if ($(this).val() != "") {
                    senderobject.logicalname = $(this).attr("name");
                    senderobject.type = type;
                    senderobject.value = $(this).val();
                    senderarray.push(senderobject);
                }
            }
        });


        var DataID = $('.formwidgetclass').data('entityid');

        $.ajax({
            url: "/Page/UpdateFormDataToCrm",
            type: "POST",
            data: JSON.stringify({ 'FormData': JSON.stringify(senderarray), 'EntityName': entityname, 'Id': DataID, 'Ownership': $('.formwidgetclass').data('ownership') }),
            dataType: "json",
            async: true,
            contentType: "application/json; charset=utf-8",
            beforeSend: function (XMLHttpRequest) {
                XMLHttpRequest.setRequestHeader("Accept", "application/json");
            },
            success: function (data) {
                if (data != "") {
                    toastr.options = {
                        "closeButton": true,
                        "debug": false,
                        "positionClass": "toast-top-right",
                        "onclick": null
                    }
                    toastr["error"](data, ErrorMessageHeader)
                    Metronic.unblockUI();
                    Metronic.unblockUI($("#responsive_" + smodalid));
                }
                else {

                    Metronic.unblockUI();
                    Metronic.unblockUI($("#responsive_" + smodalid));

                    toastr.options = {
                        "closeButton": true,
                        "debug": false,
                        "positionClass": "toast-top-right",
                        "onclick": null
                    }
                    toastr["success"](UpdateMessage, SuccessMessageHeader);
                    GetInitializeFormValuesForArray();
                }
            },

            error: function (XMLHttpRequest, textStatus, errorThrown) {
                Metronic.unblockUI();
                Metronic.unblockUI($("#responsive_" + smodalid));
                toastr.options = {
                    "closeButton": true,
                    "debug": false,
                    "positionClass": "toast-top-right",
                    "onclick": null
                }
                if (XMLHttpRequest.responseText.indexOf("<title>") != -1) {
                    toastr["error"](GetMeaningfulErrorMessage(XMLHttpRequest.responseText), ErrorMessageHeader);
                }
                else {
                    toastr["error"](UpdateMessageError, ErrorMessageHeader);
                }
                return false;
            }
        });
    }

    DatetimeInit = function () {

        $.getScript("/assets/global/plugins/bootstrap-datepicker/js/locales/bootstrap-datepicker." + LangIdNativeName.split("-")[0] + ".js", function () {


            if ($(".datetimefield").length > 0) {

                $(".datetimefield").each(function () {

                    var dFormat = $(this).attr("data-beforedateformat").replace(/M/g, 'm');

                    $(this).datepicker({
                        rtl: Metronic.isRTL(),
                        orientation: "right top",
                        autoclose: true,
                        format: dFormat,
                        language: LangIdNativeName.split("-")[0],
                        scrollInput: false
                    });
                });

            }

        });
    }


    // ! Condition functions

    // 0
    bpfDoesNotContainData = function (attributeName, operator, conditionType) {

        // Operator != equal
        if (operator != '0') {
            return null;
        }

        // Find step label
        var label = $('#bpf_' + attributeName).parent().parent().find('label');

        var source = $('#bpf_' + attributeName).val();

        if (source == "") {
            return 1;
        } else {
            //Add error tip
            //PrepareTooltipText('This field must be not contains data!', label);
            return 0;
        }
    }

    // 1
    bpfContainData = function (attributeName, operator, conditionType) {

        if (operator != '1') {
            return null;
        }

        var label = $('#bpf_' + attributeName).parent().parent().find('label');

        var source = $('#bpf_' + attributeName).val();

        if (source != "") {
            return 1;
        } else {
            //PrepareTooltipText('This field must be contains data', label);
            return 0;
        }
    }

    // 6
    bpfEqual = function (attributeName, operator, conditionType, value) {

        if (operator != '6') {
            return null;
        }

        var label = $('#bpf_' + attributeName).parent().parent().find('label');

        var source = $('#bpf_' + attributeName).val();
        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $('#bpf_' + value).val();
        }

        if (source == target) {
            return 1;
        } else {
            //PrepareTooltipText('This field must be equal to \' ' + target + ' \'', label);
            return 0;
        }

    }

    // 8
    bpfContains = function (attributeName, operator, conditionType, value) {

        if (operator != '8') {
            return null;
        }

        var label = $('#bpf_' + attributeName).parent().parent().find('label');

        var source = $('#bpf_' + attributeName).val();
        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $('#bpf_' + value).val();
        }

        if (source.indexOf(target) != -1) {
            return 1;
        } else {
            //PrepareTooltipText('This field must be contains to \' ' + target + ' \'', label);
            return 0;
        }
    }

    // 9
    bpfDoesNotContain = function (attributeName, operator, conditionType, value) {

        if (operator != '9') {
            return null;
        }

        var label = $('#bpf_' + attributeName).parent().parent().find('label');

        var source = $('#bpf_' + attributeName).val();
        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $('#bpf_' + value).val();
        }

        if (source.indexOf(target) == -1) {
            return 1;
        } else {
            //PrepareTooltipText('This field must be not contains to \' ' + target + ' \'', label);
            return 0;
        }
    }

    // 10
    bpfBeginWith = function (attributeName, operator, conditionType, value) {

        if (operator != '10') {
            return null;
        }

        var label = $('#bpf_' + attributeName).parent().parent().find('label');

        var source = $('#bpf_' + attributeName).val();
        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $('#bpf_' + value).val();
        }

        if (source.indexOf(target) == 0) {
            return 1;
        } else {
            //PrepareTooltipText('This field must be begin with to \' ' + target + ' \'', label);
            return 0;
        }
    }

    // 11    
    bpfDoesNotBeginWith = function (attributeName, operator, conditionType, value) {

        if (operator != '11') {
            return null;
        }

        var label = $('#bpf_' + attributeName).parent().parent().find('label');

        var source = $('#bpf_' + attributeName).val();
        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $('#bpf_' + value).val();
        }

        if (source.indexOf(target) != 0) {
            return 1;
        } else {
            //PrepareTooltipText('This field must be not begin with to \' ' + target + ' \'', label);
            return 0;
        }
    }

    // 12
    bpfEndWith = function (attributeName, operator, conditionType, value) {

        if (operator != '12') {
            return null;
        }

        var label = $('#bpf_' + attributeName).parent().parent().find('label');

        var source = $('#bpf_' + attributeName).val();
        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $('#bpf_' + value).val();
        }

        if (source.endsWith(target)) {
            return 1;
        } else {
            //PrepareTooltipText('This field must be end with to \' ' + target + ' \'', label);
            return 0;
        }
    }

    // 13
    bpfDoesNotEndWith = function (attributeName, operator, conditionType, value) {

        if (operator != '13') {
            return null;
        }

        var label = $('#bpf_' + attributeName).parent().parent().find('label');

        var source = $('#bpf_' + attributeName).val();
        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $('#bpf_' + value).val();
        }

        if (source.endsWith(target)) {
            //PrepareTooltipText('This field must be not end with to \' ' + target + ' \'', label);
            return 0;
        } else {
            return 1;
        }
    }

    // 14
    bpfGreaterThan = function (attributeName, operator, conditionType, value) {

        if (operator != '14') {
            return null;
        }

        var label = $('#bpf_' + attributeName).parent().parent().find('label');

        var source = $('#bpf_' + attributeName).val();
        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $('#bpf_' + value).val();
        }

        if (source > target) {
            return 1;
        } else {
            //PrepareTooltipText('This field must be greater than to \' ' + target + ' \'', label);
            return 0;
        }
    }

    // 15
    bpfGreaterThanOrEqual = function (attributeName, operator, conditionType, value) {

        if (operator != '15') {
            return null;
        }

        var label = $('#bpf_' + attributeName).parent().parent().find('label');

        var source = $('#bpf_' + attributeName).val();
        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $('#bpf_' + value).val();
        }

        if (source >= target) {
            return 1;
        } else {
            //PrepareTooltipText('This field must be greater than or equal to \' ' + target + ' \'', label);
            return 0;
        }
    }

    // 16
    bpfLessThan = function (attributeName, operator, conditionType, value) {

        if (operator != '16') {
            return null;
        }

        var label = $('#bpf_' + attributeName).parent().parent().find('label');

        var source = $('#bpf_' + attributeName).val();
        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $('#bpf_' + value).val();
        }

        if (source < target) {
            return 1;
        } else {
            //PrepareTooltipText('This field must be less than to \' ' + target + ' \'', label);
            return 0;
        }
    }

    // 17
    bpfLessThanOrEqual = function (attributeName, operator, conditionType, value) {

        if (operator != '17') {
            return null;
        }

        var label = $('#bpf_' + attributeName).parent().parent().find('label');

        var source = $('#bpf_' + attributeName).val();
        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $('#bpf_' + value).val();
        }

        if (source <= target) {
            return 1;
        } else {
            //PrepareTooltipText('This field must be less than or equal to \' ' + target + ' \'', label);
            return 0;
        }
    }

    // !! Condition functions

    Init();
    ConditionControl();
    //PrepareEditFormStage(); -> Moved to GeneralFunction.js in BindDataToEditForm function
    PrepareTraversedPath();

});



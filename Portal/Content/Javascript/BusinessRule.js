$(document).ready(function () {

    // Global variables
    var BusinessRules = businessRules.BusinessRules;
    var bindControlElementList = [];

    Init = function () {

        bindControlElementList = [];
        // Main br list
        $(BusinessRules).each(function (index, value) {

            var businessRuleName = value.BusinessRuleName;
            // Sub br list
            $(value.BRList).each(function (index, value) {

                var branchID = value.BranchID;
                var conditionList = value.ConditionList;
                var actionList = value.ActionList;
                var logicalOperator = value.LogicalOperator;

                // for else condition
                if (conditionList == null || conditionList.length == 0) {
                    
                    if (actionList != null && actionList.length > 0) {
                        BRActionControl(actionList);
                    }
                    return false;
                } else {

                    var conditionResult = BRConditionControl(conditionList);

                    if (logicalOperator == 'and' || logicalOperator == null) {

                        if (conditionResult == conditionList.length) {
                            
                            if (actionList != null && actionList.length > 0) {
                                BRActionControl(actionList);
                            }
                            return false;
                        }

                    } else if (logicalOperator == 'or') {

                        if (conditionResult > 0) {
                            
                            if (actionList != null && actionList.length > 0) {
                                BRActionControl(actionList);
                            }
                            return false;
                        }
                    }

                }



            });


        });
    }

    ClearError = function () {
        $('input').attr({ 'has-data-error': false, 'data-error-text': '' }).css({ 'border-color': '#e5e5e5' });
    }

    // Run br actions function
    BRActionControl = function (actionList) {

        $(actionList).each(function (index, value) {

            var attributeName = null, operator = null, conditionType = null, tValue = null;

            attributeName = value['AttributeName'];
            operation = value['Operation'];
            valueType = value['ValueType'];
            tValue = value['Value'];

            if (operation == 'setvalue') {
                brSetValue(attributeName, operation, valueType, tValue);
            }

            if (operation == 'showerrormessage') {
                brShowErrorMessage(attributeName, tValue);
            }

            if (operation == 'setbusinessrequired') {
                brSetBusinessRequired(attributeName, tValue);
            }

            if (operation == 'setdefaultvalue') {
                brSetDefaultValue(attributeName, operation, valueType, tValue);
            }

            if (operation == 'setvisiblity') {
                brSetVisiblity(attributeName, tValue);
            }

            if (operation == 'setlockmode') {
                brSetLockMode(attributeName, tValue);
            }

        });

    }

    // Conditions control function
    BRConditionControl = function (conditionList) {

        var result = 0;

        $(conditionList).each(function (index, value) {

            var attributeName = null, operator = null, conditionType = null, tValue = null;

            attributeName = value['AttributeName'];
            operator = value['Operator'];
            conditionType = value['ValueType'];
            tValue = value['Value'];

            // For binding control elements
            bindControlElementList.push(attributeName);

            attributeName = '[name="' + attributeName + '"]';
            if (conditionType == 'primitive') {
                tValue = tValue;
            } else if (conditionType == 'entityattribute') {
                tValue = '[name = "' + tValue + '"]';
            }


            if (operator == 'null') {
                result += brDoesNotContainData(attributeName, operator, conditionType);

            } else if (operator == 'notnull') {
                result += brContainData(attributeName, operator, conditionType);

            } else if (operator == 'equal') {
                result += brEqual(attributeName, operator, conditionType, tValue);

            } else if (operator == 'notequal') {
                result += brDoesNotEqual(attributeName, operator, conditionType, tValue);

            } else if (operator == 'contains') {
                result += brContains(attributeName, operator, conditionType, tValue);

            } else if (operator == 'doesnotcontain') {
                result += brDoesNotContain(attributeName, operator, conditionType, tValue);

            } else if (operator == 'beginswith') {
                result += brBeginWith(attributeName, operator, conditionType, tValue);

            } else if (operator == 'doesnotbeginwith') {
                result += brDoesNotBeginWith(attributeName, operator, conditionType, tValue);

            } else if (operator == 'endswith') {
                result += brEndWith(attributeName, operator, conditionType, tValue);

            } else if (operator == 'doesnotendwith') {
                result += brDoesNotEndWith(attributeName, operator, conditionType, tValue);

            } else if (operator == 'greaterthan') {
                result += brGreaterThan(attributeName, operator, conditionType, tValue);

            } else if (operator == 'greaterequal') {
                result += brGreaterThanOrEqual(attributeName, operator, conditionType, tValue);

            } else if (operator == 'lessthan') {
                result += brLessThan(attributeName, operator, conditionType, tValue);

            } else if (operator == 'lessequal') {
                result += brLessThanOrEqual(attributeName, operator, conditionType, tValue);
            }


        });


        return result;
    }

    // Set input change event handler
    BindBRChange = function () {
        for (var i = 0; i < bindControlElementList.length; i++) {
            $('.brtrigger[name="' + bindControlElementList[i] + '"], .brformtrigger[name="' + bindControlElementList[i] + '"]').unbind('change');

            $('.brtrigger[name="' + bindControlElementList[i] + '"], .brformtrigger[name="' + bindControlElementList[i] + '"]').on('change', function () {

                // If this page have bpf, run to bpfvaluescopu method
                if (typeof bpfList !== 'undefined') {
                    // This condition control method and copy method run in to businessprocessflow.js
                    BPFValuesCopyToForm($(this));
                    ConditionControl();
                }
                
                ClearError();
                Init();
            });
        }
    }

    // ! Action functions

    brSetValue = function (attributeName, operation, valueType, value) {

        var source = $('[name="' + attributeName + '"]');
        if (source == undefined)
            return false;

        var type = $(source).data('type');
        
        if (type == 'string' || type == 'memo' || type == 'decimal' || type == 'double' || type == 'integer' || type == 'money' || type == 'metronicdate' || type == 'picklist') {

            var tValue;

            if (valueType == 'primitive') {
                tValue = value;
            } else if (valueType == 'entityattribute') {
                tValue = $('[name = ' + value + ']').val();
            }

            $('[name=' + attributeName + ']').val(tValue);



        } else if (type == 'boolean' || type == 'status') {
            var tValue;

            if (valueType == 'primitive') {
                tValue = value;
            } else if (valueType == 'entityattribute') {
                tValue = $('[name = ' + value + ']').val();
            }

            $('[name="' + attributeName + '"][type="radio"][value="' + tValue + '"]').parent().parent().parent().siblings().find('input').removeAttr("checked");
            $('[name="' + attributeName + '"][type="radio"][value="' + tValue + '"]').parent().parent().parent().siblings().find('span').removeClass('checked');
            $('[name="' + attributeName + '"][type="radio"][value="' + tValue + '"]').attr("checked", "checked");
            $('[name="' + attributeName + '"][type="radio"][value="' + tValue + '"]').parent().addClass('checked');

            //For bpf
            $('select[name=' + attributeName + ']').val(tValue);

        } else if (type == 'lookup' || type == 'customer') {
            var lookupID = value.split('&')[0];
            var lookupText = value.split('&')[1];
            
            $('[name="' + attributeName + '"]').attr("data-id", lookupID);
            $('[name="' + attributeName + '"]').val(lookupText);
        }
    }

    brShowErrorMessage = function (attributeName, errorText) {

        $('[name = "' + attributeName + '"]').attr({ 'has-data-error': true, 'data-error-text': errorText }).css({ 'border-color': 'red' });
        
        toastr.options = {
            "closeButton": true,
            "debug": false,
            "positionClass": "toast-top-right",
            "onclick": null
        }

        toastr["error"](errorText, ErrorMessageHeader);
    }

    brSetBusinessRequired = function (attributeName, requiredType) {

        // Set form element
        $.each($('.portlet.light').find('[name="' + attributeName + '"]').parent().prev(), function (index, value) {

            if ($(value).attr('data-required')) {
                if (requiredType == 'required') {
                    $(value).attr({"data-required":"applicationrequired"});

                    if ($(value).find('span').length == 0 ) {
                        var requiredSpan = $('<span>').addClass('required').attr({ 'aria-required': 'true' }).text('*');
                        requiredSpan.appendTo($(value));
                    }

                } else {
                    $(value).attr({"data-required":"none"});

                    if ($(value).find('span').length != 0) {
                        $(value).find('span').remove();
                    }

                }
            }

        });

        // Set bpf element
        $.each($('#stageGridContainer').find('[name="' + attributeName + '"]'), function (index, value) {
            if (requiredType == 'required') {
                $(value).attr({ "data-required": "true" });
                if ($(value).parent().prev().find('span').length == 0) {
                    var requiredSpan = $('<span>').addClass('required').attr({ 'aria-required': 'true' }).text('*');
                    requiredSpan.appendTo($(value).parent().prev());
                }
            } else {
                $(value).attr({ "data-required": "false" });
                if ($(value).parent().prev().find('span').length != 0) {
                    $(value).parent().prev().find('span').remove();
                }
            }
        });

    }

    brSetDefaultValue = function (attributeName, operation, valueType, value) {

        var source = $('[name="' + attributeName + '"]');
        if (source == undefined)
            return false;

        var type = $(source).data('type');

        if (type == 'string' || type == 'memo' || type == 'decimal' || type == 'double' || type == 'integer' || type == 'money' || type == 'metronicdate') {

            var tValue;

            if (valueType == 'primitive') {
                tValue = value;
            } else if (valueType == 'entityattribute') {
                tValue = $('[name = ' + value + ']').val();
            }

            $('[name=' + attributeName + ']').val(tValue);

        }
    }

    brSetVisiblity = function (attributeName, visibleType) {

        // Set form element
        $.each($('.portlet.light').find('[name="' + attributeName + '"]').parent().prev(), function (index, value) {

            if ($(value).attr('data-required')) {
                if (visibleType == 'true') {
                    $(value).parent().show();
                } else {
                    $(value).parent().hide();
                }
            }

        });

        // Set bpf element
        $.each($('#stageGridContainer').find('[name="' + attributeName + '"]'), function (index, value) {
            if (visibleType == 'true') {
                $(value).parent().parent().parent().show();
            } else {
                $(value).parent().parent().parent().hide();
            }
        });

    }

    brSetLockMode = function (attributeName, lockType) {

        if (lockType == 'true') {
            $('[name="' + attributeName + '"]').prop('disabled', true);
        } else {
            $('[name="' + attributeName + '"]').prop('disabled', false);
        }
        

    }

    // !! Action functions




    // ! Condition functions

    brDoesNotContainData = function (attributeName, operator, conditionType) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        if (source == "") {
            return 1;
        } else {
            return 0;
        }
    }

    brContainData = function (attributeName, operator, conditionType) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        if (source != "") {
            return 1;
        } else {
            return 0;
        }
    }

    brEqual = function (attributeName, operator, conditionType, value) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $(value).val();
        }

        if (source == target) {
            return 1;
        } else {
            return 0;
        }

    }

    brDoesNotEqual = function (attributeName, operator, conditionType, value) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $(value).val();
        }

        if (source != target) {
            return 1;
        } else {
            return 0;
        }

    }

    brContains = function (attributeName, operator, conditionType, value) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $(value).val();
        }

        if (source.indexOf(target) != -1) {
            return 1;
        } else {
            return 0;
        }
    }

    brDoesNotContain = function (attributeName, operator, conditionType, value) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $(value).val();
        }

        if (source.indexOf(target) == -1) {
            return 1;
        } else {
            return 0;
        }
    }

    brBeginWith = function (attributeName, operator, conditionType, value) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $(value).val();
        }

        if (source.indexOf(target) == 0) {
            return 1;
        } else {
            return 0;
        }
    }

    brDoesNotBeginWith = function (attributeName, operator, conditionType, value) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $(value).val();
        }

        if (source.indexOf(target) != 0) {
            return 1;
        } else {
            return 0;
        }
    }

    brEndWith = function (attributeName, operator, conditionType, value) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $(value).val();
        }

        if (source.endsWith(target)) {
            return 1;
        } else {
            return 0;
        }
    }

    brDoesNotEndWith = function (attributeName, operator, conditionType, value) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $(value).val();
        }

        if (source.endsWith(target)) {
            return 0;
        } else {
            return 1;
        }
    }

    brGreaterThan = function (attributeName, operator, conditionType, value) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $(value).val();
        }

        if (source > target) {
            return 1;
        } else {
            return 0;
        }
    }

    brGreaterThanOrEqual = function (attributeName, operator, conditionType, value) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $(value).val();
        }

        if (source >= target) {
            return 1;
        } else {
            return 0;
        }
    }

    brLessThan = function (attributeName, operator, conditionType, value) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $(value).val();
        }

        if (source < target) {
            return 1;
        } else {
            return 0;
        }
    }

    brLessThanOrEqual = function (attributeName, operator, conditionType, value) {

        var source = $(attributeName).val();
        if (source == undefined)
            return 0;

        var target = null;

        if (conditionType == 'primitive') {
            target = value;
        } else if (conditionType == 'entityattribute') {
            target = $(value).val();
        }

        if (source <= target) {
            return 1;
        } else {
            return 0;
        }
    }

    // !! Condition functions


    Init();
    BindBRChange();

});
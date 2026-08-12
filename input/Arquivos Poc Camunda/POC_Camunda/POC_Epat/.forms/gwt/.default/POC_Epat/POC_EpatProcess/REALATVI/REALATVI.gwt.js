

if (typeof(tibcoforms) == 'undefined') tibcoforms = new Object();
if (typeof(tibcoforms.formCode) == 'undefined') tibcoforms.formCode = new Object();
tibcoforms.formCode['_Q7QYQGQwEfGUo9jzQD7qQQ'] = new Object();
tibcoforms.formCode['_Q7QYQGQwEfGUo9jzQD7qQQ']['defineActions'] = function() {
var fc = tibcoforms.formCode['_Q7QYQGQwEfGUo9jzQD7qQQ'];
	fc['rule_cancel'] = function(formId, context, thisObj) {
	   try {
    var form = tibcoforms.formCache[formId];
	var data = new tibcoforms.formCode[form.jsForm.formModelId]['DataModel'](formId);
	var pane = form.paneMap;
	var control = form.controlMap;
	var factory = form._factory;
	var pkg = form._package;
	var f = form.f;
	var p = form.p;
		try {
		fc['action_cancel'].call(thisObj, context, data, pane, control, factory, pkg, f , p);
		} catch(e) {
           tibcoforms.bridge.log_error("Rule(cancel) Action(cancel) Script Error: " + e);
           throw e;
        }
	   } catch(e) {
	       tibcoforms.bridge.log_error("Rule(cancel) Action Script Error: " + e);
	       throw e;
	   }
	}

	fc['rule_close'] = function(formId, context, thisObj) {
	   try {
    var form = tibcoforms.formCache[formId];
	var data = new tibcoforms.formCode[form.jsForm.formModelId]['DataModel'](formId);
	var pane = form.paneMap;
	var control = form.controlMap;
	var factory = form._factory;
	var pkg = form._package;
	var f = form.f;
	var p = form.p;
		try {
		fc['action_close'].call(thisObj, context, data, pane, control, factory, pkg, f , p);
		} catch(e) {
           tibcoforms.bridge.log_error("Rule(close) Action(close) Script Error: " + e);
           throw e;
        }
	   } catch(e) {
	       tibcoforms.bridge.log_error("Rule(close) Action Script Error: " + e);
	       throw e;
	   }
	}

	fc['rule_submit'] = function(formId, context, thisObj) {
	   try {
    var form = tibcoforms.formCache[formId];
	var data = new tibcoforms.formCode[form.jsForm.formModelId]['DataModel'](formId);
	var pane = form.paneMap;
	var control = form.controlMap;
	var factory = form._factory;
	var pkg = form._package;
	var f = form.f;
	var p = form.p;
		try {
		fc['action_submit'].call(thisObj, context, data, pane, control, factory, pkg, f , p);
		} catch(e) {
           tibcoforms.bridge.log_error("Rule(submit) Action(submit) Script Error: " + e);
           throw e;
        }
	   } catch(e) {
	       tibcoforms.bridge.log_error("Rule(submit) Action Script Error: " + e);
	       throw e;
	   }
	}

	fc['action_cancel'] = function(context, data, pane, control, factory, pkg, f , p) {
		context.form.invokeAction('cancel');
	}

	fc['action_apply'] = function(context, data, pane, control, factory, pkg, f , p) {
		context.form.invokeAction('apply');
	}
	
	fc['action_close'] = function(context, data, pane, control, factory, pkg, f , p) {
		context.form.invokeAction('close');
	}

	fc['action_submit'] = function(context, data, pane, control, factory, pkg, f , p) {
		context.form.invokeAction('submit');
	}
	
	fc['action_validate'] = function(context, data, pane, control, factory, pkg, f , p) {
		context.form.invokeAction('validate');
    }
    
    fc['action_reset'] = function(context, data, pane, control, factory, pkg, f , p) {
    	context.form.invokeAction('reset');
    }    
    
};
tibcoforms.formCode['_Q7QYQGQwEfGUo9jzQD7qQQ']['defineActions']();

tibcoforms.formCode['_Q7QYQGQwEfGUo9jzQD7qQQ']['defineValidations'] = function() {
var fc = tibcoforms.formCode['_Q7QYQGQwEfGUo9jzQD7qQQ'];
	
fc['validation_DOCCONTROL_DOCCONTROL__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 10;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(DOCCONTROL: DOCCONTROL__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_SFPECA3_SFPECA3__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 3;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SFPECA3: SFPECA3__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_SFDIASREPRESENT_SFDIASREPRESENT__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 2;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SFDIASREPRESENT: SFDIASREPRESENT__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_CNTPECA2_CNTPECA2__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 3;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CNTPECA2: CNTPECA2__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_CNTPECA4_CNTPECA4__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 3;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CNTPECA4: CNTPECA4__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_REGRAINSDOC_REGRAINSDOC__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(REGRAINSDOC: REGRAINSDOC__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_REGRAINSDOC_REGRAINSDOC__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(REGRAINSDOC: REGRAINSDOC__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_REGRAINSDOC_REGRAINSDOC__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(REGRAINSDOC: REGRAINSDOC__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_ISAPPERROR_ISAPPERROR__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 1;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(ISAPPERROR: ISAPPERROR__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_SW_PARENTPROC_SW_PARENTPROC__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 8;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_PARENTPROC: SW_PARENTPROC__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_OUTCOME_OUTCOME__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 10;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(OUTCOME: OUTCOME__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_DOCSPERMITIDOS_DOCSPERMITIDOS__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 50;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(DOCSPERMITIDOS: DOCSPERMITIDOS__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_EMAILRELATOR_EMAILRELATOR__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 250;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(EMAILRELATOR: EMAILRELATOR__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_ARRAYINT_ARRAYINT__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 50;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(ARRAYINT: ARRAYINT__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_SERVICE_NAME_SERVICE_NAME__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 50;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SERVICE_NAME: SERVICE_NAME__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_CTL_RETIRAT_CTL_RETIRAT__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 10;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CTL_RETIRAT: CTL_RETIRAT__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_IDPECASCNT_IDPECASCNT__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 50;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDPECASCNT: IDPECASCNT__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_LINKIPE_LINKIPE__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 255;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(LINKIPE: LINKIPE__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_IDAIIM_IDAIIM__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 11, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDAIIM: IDAIIM__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_IDAIIM_IDAIIM__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDAIIM: IDAIIM__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_IDAIIM_IDAIIM__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDAIIM: IDAIIM__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_QTPECASCNT_QTPECASCNT__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 2, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(QTPECASCNT: QTPECASCNT__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_QTPECASCNT_QTPECASCNT__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(QTPECASCNT: QTPECASCNT__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_QTPECASCNT_QTPECASCNT__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(QTPECASCNT: QTPECASCNT__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_SFPECA1_SFPECA1__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 3;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SFPECA1: SFPECA1__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_INTIMACAOCARTA_INTIMACAOCARTA__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INTIMACAOCARTA: INTIMACAOCARTA__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_INTIMACAOCARTA_INTIMACAOCARTA__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INTIMACAOCARTA: INTIMACAOCARTA__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_INTIMACAOCARTA_INTIMACAOCARTA__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INTIMACAOCARTA: INTIMACAOCARTA__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_DESCREGRA_DESCREGRA__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 250;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(DESCREGRA: DESCREGRA__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_RESPOSTACQ_RESPOSTACQ__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 10;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(RESPOSTACQ: RESPOSTACQ__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_NR_RATORIG_NR_RATORIG__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 11, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NR_RATORIG: NR_RATORIG__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_NR_RATORIG_NR_RATORIG__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NR_RATORIG: NR_RATORIG__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_NR_RATORIG_NR_RATORIG__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NR_RATORIG: NR_RATORIG__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_CD_DRT_CD_DRT__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 8, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CD_DRT: CD_DRT__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CD_DRT_CD_DRT__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CD_DRT: CD_DRT__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CD_DRT_CD_DRT__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CD_DRT: CD_DRT__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_STSADMTITCNT_STSADMTITCNT__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 2, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSADMTITCNT: STSADMTITCNT__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STSADMTITCNT_STSADMTITCNT__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSADMTITCNT: STSADMTITCNT__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STSADMTITCNT_STSADMTITCNT__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSADMTITCNT: STSADMTITCNT__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_CODMUNAIIM_CODMUNAIIM__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 8, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CODMUNAIIM: CODMUNAIIM__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CODMUNAIIM_CODMUNAIIM__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CODMUNAIIM: CODMUNAIIM__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CODMUNAIIM_CODMUNAIIM__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CODMUNAIIM: CODMUNAIIM__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_IDTIPOIMPUGNACA_IDTIPOIMPUGNACA__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 50;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDTIPOIMPUGNACA: IDTIPOIMPUGNACA__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_EMAILVISTAS_EMAILVISTAS__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 255;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(EMAILVISTAS: EMAILVISTAS__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
	
fc['validation_BCCRELATORIO_BCCRELATORIO__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 150;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(BCCRELATORIO: BCCRELATORIO__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_CRCONTRIBUINTE_CRCONTRIBUINTE__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CRCONTRIBUINTE: CRCONTRIBUINTE__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CRCONTRIBUINTE_CRCONTRIBUINTE__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CRCONTRIBUINTE: CRCONTRIBUINTE__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CRCONTRIBUINTE_CRCONTRIBUINTE__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CRCONTRIBUINTE: CRCONTRIBUINTE__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_IDAIIMORIGINAL_IDAIIMORIGINAL__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 11, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDAIIMORIGINAL: IDAIIMORIGINAL__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_IDAIIMORIGINAL_IDAIIMORIGINAL__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDAIIMORIGINAL: IDAIIMORIGINAL__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_IDAIIMORIGINAL_IDAIIMORIGINAL__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDAIIMORIGINAL: IDAIIMORIGINAL__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_ORIGEM_ORIGEM__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 5;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(ORIGEM: ORIGEM__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_IDPECASSF_IDPECASSF__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 50;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDPECASSF: IDPECASSF__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_ISTECHERROR_ISTECHERROR__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 1;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(ISTECHERROR: ISTECHERROR__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_FIMSTRING_FIMSTRING__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 10, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(FIMSTRING: FIMSTRING__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_FIMSTRING_FIMSTRING__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(FIMSTRING: FIMSTRING__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_FIMSTRING_FIMSTRING__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(FIMSTRING: FIMSTRING__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_VRAIIM_VRAIIM__fixed'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 15, 3);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(VRAIIM: VRAIIM__fixed) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_DEAT0050_DEAT0050__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 8;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(DEAT0050: DEAT0050__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_DAYSOVER_DAYSOVER__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(DAYSOVER: DAYSOVER__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_DAYSOVER_DAYSOVER__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(DAYSOVER: DAYSOVER__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_DAYSOVER_DAYSOVER__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(DAYSOVER: DAYSOVER__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_DOCSREQUERIDOS_DOCSREQUERIDOS__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 50;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(DOCSREQUERIDOS: DOCSREQUERIDOS__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
	
fc['validation_STERRORDESC_STERRORDESC__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 100;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STERRORDESC: STERRORDESC__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_PROCESS_ID_PROCESS_ID__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 35;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(PROCESS_ID: PROCESS_ID__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_SFPECA2_SFPECA2__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 3;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SFPECA2: SFPECA2__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_TROCATPNOTIFICA_TROCATPNOTIFICA__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(TROCATPNOTIFICA: TROCATPNOTIFICA__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_TROCATPNOTIFICA_TROCATPNOTIFICA__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(TROCATPNOTIFICA: TROCATPNOTIFICA__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_TROCATPNOTIFICA_TROCATPNOTIFICA__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(TROCATPNOTIFICA: TROCATPNOTIFICA__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_CRCASA_CRCASA__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CRCASA: CRCASA__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CRCASA_CRCASA__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CRCASA: CRCASA__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CRCASA_CRCASA__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CRCASA: CRCASA__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_STSADMTITDRF_STSADMTITDRF__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 2, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSADMTITDRF: STSADMTITDRF__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STSADMTITDRF_STSADMTITDRF__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSADMTITDRF: STSADMTITDRF__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STSADMTITDRF_STSADMTITDRF__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSADMTITDRF: STSADMTITDRF__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_SW_CASENUM_SW_CASENUM__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 15, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_CASENUM: SW_CASENUM__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_SW_CASENUM_SW_CASENUM__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_CASENUM: SW_CASENUM__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_SW_CASENUM_SW_CASENUM__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_CASENUM: SW_CASENUM__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_NRAIIM_NRAIIM__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 11, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NRAIIM: NRAIIM__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_NRAIIM_NRAIIM__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NRAIIM: NRAIIM__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_NRAIIM_NRAIIM__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NRAIIM: NRAIIM__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_SW_MAINPROC_SW_MAINPROC__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 8;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_MAINPROC: SW_MAINPROC__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_IDMOTIVOINTIMAC_IDMOTIVOINTIMAC__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDMOTIVOINTIMAC: IDMOTIVOINTIMAC__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_IDMOTIVOINTIMAC_IDMOTIVOINTIMAC__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDMOTIVOINTIMAC: IDMOTIVOINTIMAC__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_IDMOTIVOINTIMAC_IDMOTIVOINTIMAC__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDMOTIVOINTIMAC: IDMOTIVOINTIMAC__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_CODTEXTO_CODTEXTO__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 4;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CODTEXTO: CODTEXTO__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_CCRELATORIO_CCRELATORIO__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 150;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CCRELATORIO: CCRELATORIO__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_STSRESPCNT_STSRESPCNT__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 2, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSRESPCNT: STSRESPCNT__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STSRESPCNT_STSRESPCNT__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSRESPCNT: STSRESPCNT__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STSRESPCNT_STSRESPCNT__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSRESPCNT: STSRESPCNT__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_IDPROCESSO_IDPROCESSO__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 9, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDPROCESSO: IDPROCESSO__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_IDPROCESSO_IDPROCESSO__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDPROCESSO: IDPROCESSO__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_IDPROCESSO_IDPROCESSO__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDPROCESSO: IDPROCESSO__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_NR_RAT_NR_RAT__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 11, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NR_RAT: NR_RAT__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_NR_RAT_NR_RAT__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NR_RAT: NR_RAT__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_NR_RAT_NR_RAT__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NR_RAT: NR_RAT__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_FORMACORRECAO_FORMACORRECAO__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 20;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(FORMACORRECAO: FORMACORRECAO__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_INTIMACAODE_INTIMACAODE__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INTIMACAODE: INTIMACAODE__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_INTIMACAODE_INTIMACAODE__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INTIMACAODE: INTIMACAODE__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_INTIMACAODE_INTIMACAODE__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INTIMACAODE: INTIMACAODE__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_STSRESPSF_STSRESPSF__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 2, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSRESPSF: STSRESPSF__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STSRESPSF_STSRESPSF__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSRESPSF: STSRESPSF__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STSRESPSF_STSRESPSF__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSRESPSF: STSRESPSF__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_TIPOVISTAS_TIPOVISTAS__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 50;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(TIPOVISTAS: TIPOVISTAS__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_SW_PARENTCASE_SW_PARENTCASE__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 10, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_PARENTCASE: SW_PARENTCASE__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_SW_PARENTCASE_SW_PARENTCASE__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_PARENTCASE: SW_PARENTCASE__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_SW_PARENTCASE_SW_PARENTCASE__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_PARENTCASE: SW_PARENTCASE__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_STATUSRECURSOS_STATUSRECURSOS__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 2, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STATUSRECURSOS: STATUSRECURSOS__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STATUSRECURSOS_STATUSRECURSOS__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STATUSRECURSOS: STATUSRECURSOS__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STATUSRECURSOS_STATUSRECURSOS__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STATUSRECURSOS: STATUSRECURSOS__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
	
fc['validation_CNTPECA1_CNTPECA1__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 3;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CNTPECA1: CNTPECA1__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_QTPECASSEFAZ_QTPECASSEFAZ__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 2, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(QTPECASSEFAZ: QTPECASSEFAZ__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_QTPECASSEFAZ_QTPECASSEFAZ__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(QTPECASSEFAZ: QTPECASSEFAZ__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_QTPECASSEFAZ_QTPECASSEFAZ__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(QTPECASSEFAZ: QTPECASSEFAZ__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_MAXRETRIES_MAXRETRIES__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 10, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(MAXRETRIES: MAXRETRIES__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_MAXRETRIES_MAXRETRIES__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(MAXRETRIES: MAXRETRIES__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_MAXRETRIES_MAXRETRIES__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(MAXRETRIES: MAXRETRIES__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_NOMEETAPA_NOMEETAPA__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 8;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NOMEETAPA: NOMEETAPA__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
	
fc['validation_SW_CASENUMPOC_SW_CASENUMPOC__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 15, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_CASENUMPOC: SW_CASENUMPOC__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_SW_CASENUMPOC_SW_CASENUMPOC__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_CASENUMPOC: SW_CASENUMPOC__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_SW_CASENUMPOC_SW_CASENUMPOC__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_CASENUMPOC: SW_CASENUMPOC__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_CDIMPOSTO_CDIMPOSTO__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 10, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CDIMPOSTO: CDIMPOSTO__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CDIMPOSTO_CDIMPOSTO__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CDIMPOSTO: CDIMPOSTO__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CDIMPOSTO_CDIMPOSTO__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CDIMPOSTO: CDIMPOSTO__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_NUMAPPRETRIES_NUMAPPRETRIES__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 10, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NUMAPPRETRIES: NUMAPPRETRIES__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_NUMAPPRETRIES_NUMAPPRETRIES__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NUMAPPRETRIES: NUMAPPRETRIES__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_NUMAPPRETRIES_NUMAPPRETRIES__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NUMAPPRETRIES: NUMAPPRETRIES__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_SW_HOSTNAME_SW_HOSTNAME__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 24;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_HOSTNAME: SW_HOSTNAME__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_NRSUBPRO_NRSUBPRO__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 50;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NRSUBPRO: NRSUBPRO__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_INSTANCIA_INSTANCIA__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INSTANCIA: INSTANCIA__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_INSTANCIA_INSTANCIA__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INSTANCIA: INSTANCIA__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_INSTANCIA_INSTANCIA__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INSTANCIA: INSTANCIA__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_CNTINSTANCIASUF_CNTINSTANCIASUF__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 2, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CNTINSTANCIASUF: CNTINSTANCIASUF__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CNTINSTANCIASUF_CNTINSTANCIASUF__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CNTINSTANCIASUF: CNTINSTANCIASUF__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CNTINSTANCIASUF_CNTINSTANCIASUF__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CNTINSTANCIASUF: CNTINSTANCIASUF__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_STSPRMSF_STSPRMSF__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 2, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSPRMSF: STSPRMSF__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STSPRMSF_STSPRMSF__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSPRMSF: STSPRMSF__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STSPRMSF_STSPRMSF__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSPRMSF: STSPRMSF__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_TIPODILIGENCIA_TIPODILIGENCIA__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 6;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(TIPODILIGENCIA: TIPODILIGENCIA__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_STATUSPRJ_STATUSPRJ__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 2, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STATUSPRJ: STATUSPRJ__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STATUSPRJ_STATUSPRJ__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STATUSPRJ: STATUSPRJ__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STATUSPRJ_STATUSPRJ__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STATUSPRJ: STATUSPRJ__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_RECAPIT_RECAPIT__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(RECAPIT: RECAPIT__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_RECAPIT_RECAPIT__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(RECAPIT: RECAPIT__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_RECAPIT_RECAPIT__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(RECAPIT: RECAPIT__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_INDICESUBDIN_INDICESUBDIN__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 10, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INDICESUBDIN: INDICESUBDIN__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_INDICESUBDIN_INDICESUBDIN__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INDICESUBDIN: INDICESUBDIN__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_INDICESUBDIN_INDICESUBDIN__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INDICESUBDIN: INDICESUBDIN__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_POSICAOFIM_POSICAOFIM__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 10, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(POSICAOFIM: POSICAOFIM__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_POSICAOFIM_POSICAOFIM__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(POSICAOFIM: POSICAOFIM__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_POSICAOFIM_POSICAOFIM__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(POSICAOFIM: POSICAOFIM__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_POSICAOINICIO_POSICAOINICIO__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 10, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(POSICAOINICIO: POSICAOINICIO__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_POSICAOINICIO_POSICAOINICIO__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(POSICAOINICIO: POSICAOINICIO__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_POSICAOINICIO_POSICAOINICIO__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(POSICAOINICIO: POSICAOINICIO__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_SFPECA4_SFPECA4__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 3;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SFPECA4: SFPECA4__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_DILIGENCIADESTI_DILIGENCIADESTI__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 20;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(DILIGENCIADESTI: DILIGENCIADESTI__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_TEMPORESPOSTA_TEMPORESPOSTA__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 2;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(TEMPORESPOSTA: TEMPORESPOSTA__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
	
fc['validation_INDRESPPRM_INDRESPPRM__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 2, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INDRESPPRM: INDRESPPRM__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_INDRESPPRM_INDRESPPRM__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INDRESPPRM: INDRESPPRM__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_INDRESPPRM_INDRESPPRM__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INDRESPPRM: INDRESPPRM__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_STATUS_CODE_STATUS_CODE__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 50;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STATUS_CODE: STATUS_CODE__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_SW_MAINCASE_SW_MAINCASE__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 10, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_MAINCASE: SW_MAINCASE__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_SW_MAINCASE_SW_MAINCASE__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_MAINCASE: SW_MAINCASE__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_SW_MAINCASE_SW_MAINCASE__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_MAINCASE: SW_MAINCASE__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_CNTPECA3_CNTPECA3__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 3;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CNTPECA3: CNTPECA3__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_STSPETICAO_STSPETICAO__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 2, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSPETICAO: STSPETICAO__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STSPETICAO_STSPETICAO__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSPETICAO: STSPETICAO__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_STSPETICAO_STSPETICAO__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STSPETICAO: STSPETICAO__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_AUX_AUX__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 255;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(AUX: AUX__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_CODMUNICIPIO_CODMUNICIPIO__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 8, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CODMUNICIPIO: CODMUNICIPIO__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CODMUNICIPIO_CODMUNICIPIO__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CODMUNICIPIO: CODMUNICIPIO__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_CODMUNICIPIO_CODMUNICIPIO__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(CODMUNICIPIO: CODMUNICIPIO__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
	
	
	
fc['validation_SW_CASEDESC_SW_CASEDESC__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 24;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_CASEDESC: SW_CASEDESC__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_STERRORCODE_STERRORCODE__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 50;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(STERRORCODE: STERRORCODE__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_INTIMACAOCOUNT_INTIMACAOCOUNT__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INTIMACAOCOUNT: INTIMACAOCOUNT__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_INTIMACAOCOUNT_INTIMACAOCOUNT__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INTIMACAOCOUNT: INTIMACAOCOUNT__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_INTIMACAOCOUNT_INTIMACAOCOUNT__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(INTIMACAOCOUNT: INTIMACAOCOUNT__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_SW_MAINCASEPOC_SW_MAINCASEPOC__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 10, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_MAINCASEPOC: SW_MAINCASEPOC__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_SW_MAINCASEPOC_SW_MAINCASEPOC__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_MAINCASEPOC: SW_MAINCASEPOC__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_SW_MAINCASEPOC_SW_MAINCASEPOC__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SW_MAINCASEPOC: SW_MAINCASEPOC__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_SITUACAOCARREGA_SITUACAOCARREGA__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 10;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(SITUACAOCARREGA: SITUACAOCARREGA__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_IDSINTIMADOS_IDSINTIMADOS__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 250;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDSINTIMADOS: IDSINTIMADOS__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_VOLTARSEGINSTAN_VOLTARSEGINSTAN__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(VOLTARSEGINSTAN: VOLTARSEGINSTAN__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_VOLTARSEGINSTAN_VOLTARSEGINSTAN__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(VOLTARSEGINSTAN: VOLTARSEGINSTAN__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_VOLTARSEGINSTAN_VOLTARSEGINSTAN__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(VOLTARSEGINSTAN: VOLTARSEGINSTAN__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_QTDINTIMADOS_QTDINTIMADOS__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 10, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(QTDINTIMADOS: QTDINTIMADOS__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_QTDINTIMADOS_QTDINTIMADOS__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(QTDINTIMADOS: QTDINTIMADOS__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_QTDINTIMADOS_QTDINTIMADOS__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(QTDINTIMADOS: QTDINTIMADOS__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_DATETIME_DATETIME__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'context.value.length <= 50;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(DATETIME: DATETIME__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	
fc['validation_NR_AIIM_NR_AIIM__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 11, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NR_AIIM: NR_AIIM__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_NR_AIIM_NR_AIIM__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NR_AIIM: NR_AIIM__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_NR_AIIM_NR_AIIM__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(NR_AIIM: NR_AIIM__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_IDDECISAODEBITO_IDDECISAODEBITO__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDDECISAODEBITO: IDDECISAODEBITO__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_IDDECISAODEBITO_IDDECISAODEBITO__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDDECISAODEBITO: IDDECISAODEBITO__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_IDDECISAODEBITO_IDDECISAODEBITO__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDDECISAODEBITO: IDDECISAODEBITO__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
fc['validation_IDDECISAOAIIM_IDDECISAOAIIM__length'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = '!isNaN(context.value) && this.getForm().numberFormat(context.value.valueOf(), 1, 0);';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDDECISAOAIIM: IDDECISAOAIIM__length) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_IDDECISAOAIIM_IDDECISAOAIIM__lowerLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() >= -2147483648;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDDECISAOAIIM: IDDECISAOAIIM__lowerLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
fc['validation_IDDECISAOAIIM_IDDECISAOAIIM__upperLimit'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
	var valScr = 'isNaN(context.value) || context.value.valueOf() <= 2147483647;';
	try {
	   return eval(valScr);
	} catch(e) {
	   tibcoforms.bridge.log_error("Validation(IDDECISAOAIIM: IDDECISAOAIIM__upperLimit) Script Error: " + e + ", for script: " + valScr);
	   throw e;
	}
}
	
	fc['validate_required'] = function(formId, controlName, cloneUID, listIndex) {
	var context = new Object();
    var form = tibcoforms.formCache[formId];
	var logger = tibcoforms.bridge.log_logger();
	context.control = form.controlMap[controlName]._getProxy(cloneUID);
    if (listIndex == -1)
        context.value = context.control.getValue();
    else
        context.value = context.control.getValue()[listIndex];
    if (context.value == null)
        context.value = '';
		var controlType = context.control.getControlType();
		if (listIndex >= 0 && context.control.getValue() instanceof Array) {
			if (context.control.getRequired()) {
				if (tibcoforms.bridge.log_isTraceEnabled())
					tibcoforms.bridge.log_trace('Form2GWTJS: validate_required: calling context.control.getValue() for array.');
				var contxtControlValue = context.control.getValue();
				if (tibcoforms.bridge.log_isTraceEnabled())
					tibcoforms.bridge.log_trace('Form2GWTJS: validate_required: contxtControlValue.length: ' + contxtControlValue.length);
				for (var i1=0; i1<contxtControlValue.length; i1++) {
					if (tibcoforms.bridge.log_isTraceEnabled()) {
						tibcoforms.bridge.log_trace('Form2GWTJS: validate_required: contxtControlValue[i1]: '+ contxtControlValue[i1]);
						tibcoforms.bridge.log_trace('Form2GWTJS: validate_required: typeof contxtControlValue[i1]: '+ (typeof contxtControlValue[i1]));
					}
					var strContxtControlValue = '';
					if (contxtControlValue[i1] instanceof Date) {
			        	strContxtControlValue = contxtControlValue[i1].toString();
				    } else if (contxtControlValue[i1] instanceof Boolean || contxtControlValue[i1] instanceof Number || contxtControlValue[i1] instanceof Duration) {
				    	strContxtControlValue = contxtControlValue[i1].toString();
				    } else if (contxtControlValue[i1] instanceof Object) {
				    	strContxtControlValue = contxtControlValue[i1].toString();
				    } else if (typeof contxtControlValue[i1] == 'number') {
				    	strContxtControlValue = new String(contxtControlValue[i1]).toString();
				    } else {
				    	strContxtControlValue = contxtControlValue[i1];
				    }
				    if (tibcoforms.bridge.log_isTraceEnabled())
						tibcoforms.bridge.log_trace('Form2GWTJS: validate_required: strContxtControlValue: '+ strContxtControlValue);
				    if (strContxtControlValue != null && strContxtControlValue.length > 0)
				        return !("com.tibco.forms.controls.checkbox" == controlType) || 'true' === strContxtControlValue.toLowerCase();
				}
				return false;
			} else
				return true;
		} else {
		    var strContxtControlValue = context.control.getValue();
			return !(context.control.getRequired() && 
			             (strContxtControlValue == null || strContxtControlValue.toString().length == 0) ||
			             (("com.tibco.forms.controls.checkbox" == controlType) && 'true' != strContxtControlValue.toString().toLowerCase()));
	    }
	}
	fc['register_pkgs_and_fcts'] = function(formId) {
	   var form = tibcoforms.formCache[formId];
	   form.registerPackages([]);
       form.registerFactories([]);
	}
	fc['DataModel']=function(formId) {
		this.form = tibcoforms.formCache[formId];
		this.getIDAIIM = function() {
			return this.form.dataMap['IDAIIM'].getValue();
		};
		this.setIDAIIM = function(value) {
			return this.form.dataMap['IDAIIM'].setValue(value);
		};
		this.getNRAIIM = function() {
			return this.form.dataMap['NRAIIM'].getValue();
		};
		this.setNRAIIM = function(value) {
			return this.form.dataMap['NRAIIM'].setValue(value);
		};
		this.getIDPROCESSO = function() {
			return this.form.dataMap['IDPROCESSO'].getValue();
		};
		this.setIDPROCESSO = function(value) {
			return this.form.dataMap['IDPROCESSO'].setValue(value);
		};
		this.getEMAILVISTAS = function() {
			return this.form.dataMap['EMAILVISTAS'].getValue();
		};
		this.setEMAILVISTAS = function(value) {
			return this.form.dataMap['EMAILVISTAS'].setValue(value);
		};
		this.getCNTPECA2 = function() {
			return this.form.dataMap['CNTPECA2'].getValue();
		};
		this.setCNTPECA2 = function(value) {
			return this.form.dataMap['CNTPECA2'].setValue(value);
		};
		this.getTIPODILIGENCIA = function() {
			return this.form.dataMap['TIPODILIGENCIA'].getValue();
		};
		this.setTIPODILIGENCIA = function(value) {
			return this.form.dataMap['TIPODILIGENCIA'].setValue(value);
		};
		this.getSTSRESPSF = function() {
			return this.form.dataMap['STSRESPSF'].getValue();
		};
		this.setSTSRESPSF = function(value) {
			return this.form.dataMap['STSRESPSF'].setValue(value);
		};
		this.getSTSADMTITDRF = function() {
			return this.form.dataMap['STSADMTITDRF'].getValue();
		};
		this.setSTSADMTITDRF = function(value) {
			return this.form.dataMap['STSADMTITDRF'].setValue(value);
		};
		this.getQTPECASCNT = function() {
			return this.form.dataMap['QTPECASCNT'].getValue();
		};
		this.setQTPECASCNT = function(value) {
			return this.form.dataMap['QTPECASCNT'].setValue(value);
		};
		this.getIDTIPOIMPUGNACA = function() {
			return this.form.dataMap['IDTIPOIMPUGNACA'].getValue();
		};
		this.setIDTIPOIMPUGNACA = function(value) {
			return this.form.dataMap['IDTIPOIMPUGNACA'].setValue(value);
		};
		this.getSFPECA1 = function() {
			return this.form.dataMap['SFPECA1'].getValue();
		};
		this.setSFPECA1 = function(value) {
			return this.form.dataMap['SFPECA1'].setValue(value);
		};
		this.getSTSRESPCNT = function() {
			return this.form.dataMap['STSRESPCNT'].getValue();
		};
		this.setSTSRESPCNT = function(value) {
			return this.form.dataMap['STSRESPCNT'].setValue(value);
		};
		this.getFLAGCONTRARAZAO = function() {
			return this.form.dataMap['FLAGCONTRARAZAO'].getValue();
		};
		this.setFLAGCONTRARAZAO = function(value) {
			return this.form.dataMap['FLAGCONTRARAZAO'].setValue(value);
		};
		this.getEXCLUSAOSOLIDAR = function() {
			return this.form.dataMap['EXCLUSAOSOLIDAR'].getValue();
		};
		this.setEXCLUSAOSOLIDAR = function(value) {
			return this.form.dataMap['EXCLUSAOSOLIDAR'].setValue(value);
		};
		this.getINDNAORECORRER = function() {
			return this.form.dataMap['INDNAORECORRER'].getValue();
		};
		this.setINDNAORECORRER = function(value) {
			return this.form.dataMap['INDNAORECORRER'].setValue(value);
		};
		this.getDESCREGRA = function() {
			return this.form.dataMap['DESCREGRA'].getValue();
		};
		this.setDESCREGRA = function(value) {
			return this.form.dataMap['DESCREGRA'].setValue(value);
		};
		this.getIDMOTIVOINTIMAC = function() {
			return this.form.dataMap['IDMOTIVOINTIMAC'].getValue();
		};
		this.setIDMOTIVOINTIMAC = function(value) {
			return this.form.dataMap['IDMOTIVOINTIMAC'].setValue(value);
		};
		this.getSFDIASREPRESENT = function() {
			return this.form.dataMap['SFDIASREPRESENT'].getValue();
		};
		this.setSFDIASREPRESENT = function(value) {
			return this.form.dataMap['SFDIASREPRESENT'].setValue(value);
		};
		this.getCODTEXTO = function() {
			return this.form.dataMap['CODTEXTO'].getValue();
		};
		this.setCODTEXTO = function(value) {
			return this.form.dataMap['CODTEXTO'].setValue(value);
		};
		this.getRECURSOOFICIO = function() {
			return this.form.dataMap['RECURSOOFICIO'].getValue();
		};
		this.setRECURSOOFICIO = function(value) {
			return this.form.dataMap['RECURSOOFICIO'].setValue(value);
		};
		this.getQTPECASSEFAZ = function() {
			return this.form.dataMap['QTPECASSEFAZ'].getValue();
		};
		this.setQTPECASSEFAZ = function(value) {
			return this.form.dataMap['QTPECASSEFAZ'].setValue(value);
		};
		this.getFLGALC = function() {
			return this.form.dataMap['FLGALC'].getValue();
		};
		this.setFLGALC = function(value) {
			return this.form.dataMap['FLGALC'].setValue(value);
		};
		this.getVICIOREPRESENTA = function() {
			return this.form.dataMap['VICIOREPRESENTA'].getValue();
		};
		this.setVICIOREPRESENTA = function(value) {
			return this.form.dataMap['VICIOREPRESENTA'].setValue(value);
		};
		this.getCNTPECA3 = function() {
			return this.form.dataMap['CNTPECA3'].getValue();
		};
		this.setCNTPECA3 = function(value) {
			return this.form.dataMap['CNTPECA3'].setValue(value);
		};
		this.getSFPECA2 = function() {
			return this.form.dataMap['SFPECA2'].getValue();
		};
		this.setSFPECA2 = function(value) {
			return this.form.dataMap['SFPECA2'].setValue(value);
		};
		this.getIDDECISAODEBITO = function() {
			return this.form.dataMap['IDDECISAODEBITO'].getValue();
		};
		this.setIDDECISAODEBITO = function(value) {
			return this.form.dataMap['IDDECISAODEBITO'].setValue(value);
		};
		this.getSTSPETICAO = function() {
			return this.form.dataMap['STSPETICAO'].getValue();
		};
		this.setSTSPETICAO = function(value) {
			return this.form.dataMap['STSPETICAO'].setValue(value);
		};
		this.getDILIGENCIA = function() {
			return this.form.dataMap['DILIGENCIA'].getValue();
		};
		this.setDILIGENCIA = function(value) {
			return this.form.dataMap['DILIGENCIA'].setValue(value);
		};
		this.getDEFESAADMITIDA = function() {
			return this.form.dataMap['DEFESAADMITIDA'].getValue();
		};
		this.setDEFESAADMITIDA = function(value) {
			return this.form.dataMap['DEFESAADMITIDA'].setValue(value);
		};
		this.getIDDECISAOAIIM = function() {
			return this.form.dataMap['IDDECISAOAIIM'].getValue();
		};
		this.setIDDECISAOAIIM = function(value) {
			return this.form.dataMap['IDDECISAOAIIM'].setValue(value);
		};
		this.getANULACAODTJ = function() {
			return this.form.dataMap['ANULACAODTJ'].getValue();
		};
		this.setANULACAODTJ = function(value) {
			return this.form.dataMap['ANULACAODTJ'].setValue(value);
		};
		this.getRECAPIT = function() {
			return this.form.dataMap['RECAPIT'].getValue();
		};
		this.setRECAPIT = function(value) {
			return this.form.dataMap['RECAPIT'].setValue(value);
		};
		this.getORIGEM = function() {
			return this.form.dataMap['ORIGEM'].getValue();
		};
		this.setORIGEM = function(value) {
			return this.form.dataMap['ORIGEM'].setValue(value);
		};
		this.getCNTPECA4 = function() {
			return this.form.dataMap['CNTPECA4'].getValue();
		};
		this.setCNTPECA4 = function(value) {
			return this.form.dataMap['CNTPECA4'].setValue(value);
		};
		this.getSTATUSRECURSOS = function() {
			return this.form.dataMap['STATUSRECURSOS'].getValue();
		};
		this.setSTATUSRECURSOS = function(value) {
			return this.form.dataMap['STATUSRECURSOS'].setValue(value);
		};
		this.getINDRESPPRM = function() {
			return this.form.dataMap['INDRESPPRM'].getValue();
		};
		this.setINDRESPPRM = function(value) {
			return this.form.dataMap['INDRESPPRM'].setValue(value);
		};
		this.getCNTPECA1 = function() {
			return this.form.dataMap['CNTPECA1'].getValue();
		};
		this.setCNTPECA1 = function(value) {
			return this.form.dataMap['CNTPECA1'].setValue(value);
		};
		this.getTEMPORESPOSTA = function() {
			return this.form.dataMap['TEMPORESPOSTA'].getValue();
		};
		this.setTEMPORESPOSTA = function(value) {
			return this.form.dataMap['TEMPORESPOSTA'].setValue(value);
		};
		this.getINSTANCIA = function() {
			return this.form.dataMap['INSTANCIA'].getValue();
		};
		this.setINSTANCIA = function(value) {
			return this.form.dataMap['INSTANCIA'].setValue(value);
		};
		this.getSTSADMTITCNT = function() {
			return this.form.dataMap['STSADMTITCNT'].getValue();
		};
		this.setSTSADMTITCNT = function(value) {
			return this.form.dataMap['STSADMTITCNT'].setValue(value);
		};
		this.getVRAIIM = function() {
			return this.form.dataMap['VRAIIM'].getValue();
		};
		this.setVRAIIM = function(value) {
			return this.form.dataMap['VRAIIM'].setValue(value);
		};
		this.getSFPECA4 = function() {
			return this.form.dataMap['SFPECA4'].getValue();
		};
		this.setSFPECA4 = function(value) {
			return this.form.dataMap['SFPECA4'].setValue(value);
		};
		this.getSTATUSPRJ = function() {
			return this.form.dataMap['STATUSPRJ'].getValue();
		};
		this.setSTATUSPRJ = function(value) {
			return this.form.dataMap['STATUSPRJ'].setValue(value);
		};
		this.getDILIGENCIADESTI = function() {
			return this.form.dataMap['DILIGENCIADESTI'].getValue();
		};
		this.setDILIGENCIADESTI = function(value) {
			return this.form.dataMap['DILIGENCIADESTI'].setValue(value);
		};
		this.getSTSPRMSF = function() {
			return this.form.dataMap['STSPRMSF'].getValue();
		};
		this.setSTSPRMSF = function(value) {
			return this.form.dataMap['STSPRMSF'].setValue(value);
		};
		this.getINTIMACAOCOUNT = function() {
			return this.form.dataMap['INTIMACAOCOUNT'].getValue();
		};
		this.setINTIMACAOCOUNT = function(value) {
			return this.form.dataMap['INTIMACAOCOUNT'].setValue(value);
		};
		this.getSFPECA3 = function() {
			return this.form.dataMap['SFPECA3'].getValue();
		};
		this.setSFPECA3 = function(value) {
			return this.form.dataMap['SFPECA3'].setValue(value);
		};
		this.getCHEFEUJ = function() {
			return this.form.dataMap['CHEFEUJ'].getValue();
		};
		this.setCHEFEUJ = function(value) {
			return this.form.dataMap['CHEFEUJ'].setValue(value);
		};
		this.getCRCASA = function() {
			return this.form.dataMap['CRCASA'].getValue();
		};
		this.setCRCASA = function(value) {
			return this.form.dataMap['CRCASA'].setValue(value);
		};
		this.getCODMUNICIPIO = function() {
			return this.form.dataMap['CODMUNICIPIO'].getValue();
		};
		this.setCODMUNICIPIO = function(value) {
			return this.form.dataMap['CODMUNICIPIO'].setValue(value);
		};
		this.getNR_RATORIG = function() {
			return this.form.dataMap['NR_RATORIG'].getValue();
		};
		this.setNR_RATORIG = function(value) {
			return this.form.dataMap['NR_RATORIG'].setValue(value);
		};
		this.getDOCCONTROL = function() {
			return this.form.dataMap['DOCCONTROL'].getValue();
		};
		this.setDOCCONTROL = function(value) {
			return this.form.dataMap['DOCCONTROL'].setValue(value);
		};
		this.getDTCIENCIA = function() {
			return this.form.dataMap['DTCIENCIA'].getValue();
		};
		this.setDTCIENCIA = function(value) {
			return this.form.dataMap['DTCIENCIA'].setValue(value);
		};
		this.getCRCONTRIBUINTE = function() {
			return this.form.dataMap['CRCONTRIBUINTE'].getValue();
		};
		this.setCRCONTRIBUINTE = function(value) {
			return this.form.dataMap['CRCONTRIBUINTE'].setValue(value);
		};
		this.getIDSINTIMADOS = function() {
			return this.form.dataMap['IDSINTIMADOS'].getValue();
		};
		this.setIDSINTIMADOS = function(value) {
			return this.form.dataMap['IDSINTIMADOS'].setValue(value);
		};
		this.getCODUADTJ = function() {
			return this.form.dataMap['CODUADTJ'].getValue();
		};
		this.setCODUADTJ = function(value) {
			return this.form.dataMap['CODUADTJ'].setValue(value);
		};
		this.getQTDINTIMADOS = function() {
			return this.form.dataMap['QTDINTIMADOS'].getValue();
		};
		this.setQTDINTIMADOS = function(value) {
			return this.form.dataMap['QTDINTIMADOS'].setValue(value);
		};
		this.getCODUADRT = function() {
			return this.form.dataMap['CODUADRT'].getValue();
		};
		this.setCODUADRT = function(value) {
			return this.form.dataMap['CODUADRT'].setValue(value);
		};
		this.getDEAT0050 = function() {
			return this.form.dataMap['DEAT0050'].getValue();
		};
		this.setDEAT0050 = function(value) {
			return this.form.dataMap['DEAT0050'].setValue(value);
		};
		this.getEXISTENOTIFICAC = function() {
			return this.form.dataMap['EXISTENOTIFICAC'].getValue();
		};
		this.setEXISTENOTIFICAC = function(value) {
			return this.form.dataMap['EXISTENOTIFICAC'].setValue(value);
		};
		this.getTROCATPNOTIFICA = function() {
			return this.form.dataMap['TROCATPNOTIFICA'].getValue();
		};
		this.setTROCATPNOTIFICA = function(value) {
			return this.form.dataMap['TROCATPNOTIFICA'].setValue(value);
		};
		this.getFLAGRETIRATE = function() {
			return this.form.dataMap['FLAGRETIRATE'].getValue();
		};
		this.setFLAGRETIRATE = function(value) {
			return this.form.dataMap['FLAGRETIRATE'].setValue(value);
		};
		this.getBCCRELATORIO = function() {
			return this.form.dataMap['BCCRELATORIO'].getValue();
		};
		this.setBCCRELATORIO = function(value) {
			return this.form.dataMap['BCCRELATORIO'].setValue(value);
		};
		this.getPRAZORETIRADAVI = function() {
			return this.form.dataMap['PRAZORETIRADAVI'].getValue();
		};
		this.setPRAZORETIRADAVI = function(value) {
			return this.form.dataMap['PRAZORETIRADAVI'].setValue(value);
		};
		this.getVOLTARSEGINSTAN = function() {
			return this.form.dataMap['VOLTARSEGINSTAN'].getValue();
		};
		this.setVOLTARSEGINSTAN = function(value) {
			return this.form.dataMap['VOLTARSEGINSTAN'].setValue(value);
		};
		this.getTIPOVISTAS = function() {
			return this.form.dataMap['TIPOVISTAS'].getValue();
		};
		this.setTIPOVISTAS = function(value) {
			return this.form.dataMap['TIPOVISTAS'].setValue(value);
		};
		this.getHORAFINAL = function() {
			return this.form.dataMap['HORAFINAL'].getValue();
		};
		this.setHORAFINAL = function(value) {
			return this.form.dataMap['HORAFINAL'].setValue(value);
		};
		this.getPRAZOVISTA = function() {
			return this.form.dataMap['PRAZOVISTA'].getValue();
		};
		this.setPRAZOVISTA = function(value) {
			return this.form.dataMap['PRAZOVISTA'].setValue(value);
		};
		this.getFLAGCRZ = function() {
			return this.form.dataMap['FLAGCRZ'].getValue();
		};
		this.setFLAGCRZ = function(value) {
			return this.form.dataMap['FLAGCRZ'].setValue(value);
		};
		this.getSW_CASENUMPOC = function() {
			return this.form.dataMap['SW_CASENUMPOC'].getValue();
		};
		this.setSW_CASENUMPOC = function(value) {
			return this.form.dataMap['SW_CASENUMPOC'].setValue(value);
		};
		this.getNR_AIIM = function() {
			return this.form.dataMap['NR_AIIM'].getValue();
		};
		this.setNR_AIIM = function(value) {
			return this.form.dataMap['NR_AIIM'].setValue(value);
		};
		this.getSW_MAINCASEPOC = function() {
			return this.form.dataMap['SW_MAINCASEPOC'].getValue();
		};
		this.setSW_MAINCASEPOC = function(value) {
			return this.form.dataMap['SW_MAINCASEPOC'].setValue(value);
		};
		this.getCNTINSTANCIASUF = function() {
			return this.form.dataMap['CNTINSTANCIASUF'].getValue();
		};
		this.setCNTINSTANCIASUF = function(value) {
			return this.form.dataMap['CNTINSTANCIASUF'].setValue(value);
		};
		this.getAFR = function() {
			return this.form.dataMap['AFR'].getValue();
		};
		this.setAFR = function(value) {
			return this.form.dataMap['AFR'].setValue(value);
		};
		this.getCDIMPOSTO = function() {
			return this.form.dataMap['CDIMPOSTO'].getValue();
		};
		this.setCDIMPOSTO = function(value) {
			return this.form.dataMap['CDIMPOSTO'].setValue(value);
		};
		this.getSITUACAOCARREGA = function() {
			return this.form.dataMap['SITUACAOCARREGA'].getValue();
		};
		this.setSITUACAOCARREGA = function(value) {
			return this.form.dataMap['SITUACAOCARREGA'].setValue(value);
		};
		this.getDOCSREQUERIDOS = function() {
			return this.form.dataMap['DOCSREQUERIDOS'].getValue();
		};
		this.setDOCSREQUERIDOS = function(value) {
			return this.form.dataMap['DOCSREQUERIDOS'].setValue(value);
		};
		this.getNR_RAT = function() {
			return this.form.dataMap['NR_RAT'].getValue();
		};
		this.setNR_RAT = function(value) {
			return this.form.dataMap['NR_RAT'].setValue(value);
		};
		this.getDOCSPERMITIDOS = function() {
			return this.form.dataMap['DOCSPERMITIDOS'].getValue();
		};
		this.setDOCSPERMITIDOS = function(value) {
			return this.form.dataMap['DOCSPERMITIDOS'].setValue(value);
		};
		this.getREGRAINSDOC = function() {
			return this.form.dataMap['REGRAINSDOC'].getValue();
		};
		this.setREGRAINSDOC = function(value) {
			return this.form.dataMap['REGRAINSDOC'].setValue(value);
		};
		this.getSW_CASEDESC = function() {
			return this.form.dataMap['SW_CASEDESC'].getValue();
		};
		this.setSW_CASEDESC = function(value) {
			return this.form.dataMap['SW_CASEDESC'].setValue(value);
		};
		this.getCODMUNAIIM = function() {
			return this.form.dataMap['CODMUNAIIM'].getValue();
		};
		this.setCODMUNAIIM = function(value) {
			return this.form.dataMap['CODMUNAIIM'].setValue(value);
		};
		this.getCD_DRT = function() {
			return this.form.dataMap['CD_DRT'].getValue();
		};
		this.setCD_DRT = function(value) {
			return this.form.dataMap['CD_DRT'].setValue(value);
		};
		this.getFORMACORRECAO = function() {
			return this.form.dataMap['FORMACORRECAO'].getValue();
		};
		this.setFORMACORRECAO = function(value) {
			return this.form.dataMap['FORMACORRECAO'].setValue(value);
		};
		this.getCTL_RETIRAT = function() {
			return this.form.dataMap['CTL_RETIRAT'].getValue();
		};
		this.setCTL_RETIRAT = function(value) {
			return this.form.dataMap['CTL_RETIRAT'].setValue(value);
		};
		this.getNOMEETAPA = function() {
			return this.form.dataMap['NOMEETAPA'].getValue();
		};
		this.setNOMEETAPA = function(value) {
			return this.form.dataMap['NOMEETAPA'].setValue(value);
		};
		this.getRESPOSTACQ = function() {
			return this.form.dataMap['RESPOSTACQ'].getValue();
		};
		this.setRESPOSTACQ = function(value) {
			return this.form.dataMap['RESPOSTACQ'].setValue(value);
		};
		this.getLINKIPE = function() {
			return this.form.dataMap['LINKIPE'].getValue();
		};
		this.setLINKIPE = function(value) {
			return this.form.dataMap['LINKIPE'].setValue(value);
		};
		this.getDAYSOVER = function() {
			return this.form.dataMap['DAYSOVER'].getValue();
		};
		this.setDAYSOVER = function(value) {
			return this.form.dataMap['DAYSOVER'].setValue(value);
		};
		this.getDTFIMCQ = function() {
			return this.form.dataMap['DTFIMCQ'].getValue();
		};
		this.setDTFIMCQ = function(value) {
			return this.form.dataMap['DTFIMCQ'].setValue(value);
		};
		this.getHRFIMCQ = function() {
			return this.form.dataMap['HRFIMCQ'].getValue();
		};
		this.setHRFIMCQ = function(value) {
			return this.form.dataMap['HRFIMCQ'].setValue(value);
		};
		this.getCOORDENADOR = function() {
			return this.form.dataMap['COORDENADOR'].getValue();
		};
		this.setCOORDENADOR = function(value) {
			return this.form.dataMap['COORDENADOR'].setValue(value);
		};
		this.getCORRECAO = function() {
			return this.form.dataMap['CORRECAO'].getValue();
		};
		this.setCORRECAO = function(value) {
			return this.form.dataMap['CORRECAO'].setValue(value);
		};
		this.getNOTIFICACAO = function() {
			return this.form.dataMap['NOTIFICACAO'].getValue();
		};
		this.setNOTIFICACAO = function(value) {
			return this.form.dataMap['NOTIFICACAO'].setValue(value);
		};
		this.getPRAZORELATO = function() {
			return this.form.dataMap['PRAZORELATO'].getValue();
		};
		this.setPRAZORELATO = function(value) {
			return this.form.dataMap['PRAZORELATO'].setValue(value);
		};
		this.getEMAILRELATOR = function() {
			return this.form.dataMap['EMAILRELATOR'].getValue();
		};
		this.setEMAILRELATOR = function(value) {
			return this.form.dataMap['EMAILRELATOR'].setValue(value);
		};
		this.getCCRELATORIO = function() {
			return this.form.dataMap['CCRELATORIO'].getValue();
		};
		this.setCCRELATORIO = function(value) {
			return this.form.dataMap['CCRELATORIO'].setValue(value);
		};
		this.getNRSUBPRO = function() {
			return this.form.dataMap['NRSUBPRO'].getValue();
		};
		this.setNRSUBPRO = function(value) {
			return this.form.dataMap['NRSUBPRO'].setValue(value);
		};
		this.getARRAYINT = function() {
			return this.form.dataMap['ARRAYINT'].getValue();
		};
		this.setARRAYINT = function(value) {
			return this.form.dataMap['ARRAYINT'].setValue(value);
		};
		this.getINTIMACAOCARTA = function() {
			return this.form.dataMap['INTIMACAOCARTA'].getValue();
		};
		this.setINTIMACAOCARTA = function(value) {
			return this.form.dataMap['INTIMACAOCARTA'].setValue(value);
		};
		this.getDTPUBLICACAODE = function() {
			return this.form.dataMap['DTPUBLICACAODE'].getValue();
		};
		this.setDTPUBLICACAODE = function(value) {
			return this.form.dataMap['DTPUBLICACAODE'].setValue(value);
		};
		this.getINTIMACAODE = function() {
			return this.form.dataMap['INTIMACAODE'].getValue();
		};
		this.setINTIMACAODE = function(value) {
			return this.form.dataMap['INTIMACAODE'].setValue(value);
		};
		this.getNOVOMODELO = function() {
			return this.form.dataMap['NOVOMODELO'].getValue();
		};
		this.setNOVOMODELO = function(value) {
			return this.form.dataMap['NOVOMODELO'].setValue(value);
		};
		this.getINDICESUBDIN = function() {
			return this.form.dataMap['INDICESUBDIN'].getValue();
		};
		this.setINDICESUBDIN = function(value) {
			return this.form.dataMap['INDICESUBDIN'].setValue(value);
		};
		this.getCTRLIATV = function() {
			return this.form.dataMap['CTRLIATV'].getValue();
		};
		this.setCTRLIATV = function(value) {
			return this.form.dataMap['CTRLIATV'].setValue(value);
		};
		this.getAUX = function() {
			return this.form.dataMap['AUX'].getValue();
		};
		this.setAUX = function(value) {
			return this.form.dataMap['AUX'].setValue(value);
		};
		this.getPOSICAOINICIO = function() {
			return this.form.dataMap['POSICAOINICIO'].getValue();
		};
		this.setPOSICAOINICIO = function(value) {
			return this.form.dataMap['POSICAOINICIO'].setValue(value);
		};
		this.getIDPECASSF = function() {
			return this.form.dataMap['IDPECASSF'].getValue();
		};
		this.setIDPECASSF = function(value) {
			return this.form.dataMap['IDPECASSF'].setValue(value);
		};
		this.getIDPECASCNT = function() {
			return this.form.dataMap['IDPECASCNT'].getValue();
		};
		this.setIDPECASCNT = function(value) {
			return this.form.dataMap['IDPECASCNT'].setValue(value);
		};
		this.getFIMSTRING = function() {
			return this.form.dataMap['FIMSTRING'].getValue();
		};
		this.setFIMSTRING = function(value) {
			return this.form.dataMap['FIMSTRING'].setValue(value);
		};
		this.getPOSICAOFIM = function() {
			return this.form.dataMap['POSICAOFIM'].getValue();
		};
		this.setPOSICAOFIM = function(value) {
			return this.form.dataMap['POSICAOFIM'].setValue(value);
		};
		this.getDATAENCPREPNOT = function() {
			return this.form.dataMap['DATAENCPREPNOT'].getValue();
		};
		this.setDATAENCPREPNOT = function(value) {
			return this.form.dataMap['DATAENCPREPNOT'].setValue(value);
		};
		this.getIDAIIMORIGINAL = function() {
			return this.form.dataMap['IDAIIMORIGINAL'].getValue();
		};
		this.setIDAIIMORIGINAL = function(value) {
			return this.form.dataMap['IDAIIMORIGINAL'].setValue(value);
		};
		this.getSW_PARENTPROC = function() {
			return this.form.dataMap['SW_PARENTPROC'].getValue();
		};
		this.setSW_PARENTPROC = function(value) {
			return this.form.dataMap['SW_PARENTPROC'].setValue(value);
		};
		this.getSW_CASENUM = function() {
			return this.form.dataMap['SW_CASENUM'].getValue();
		};
		this.setSW_CASENUM = function(value) {
			return this.form.dataMap['SW_CASENUM'].setValue(value);
		};
		this.getSW_PARENTCASE = function() {
			return this.form.dataMap['SW_PARENTCASE'].getValue();
		};
		this.setSW_PARENTCASE = function(value) {
			return this.form.dataMap['SW_PARENTCASE'].setValue(value);
		};
		this.getSW_HOSTNAME = function() {
			return this.form.dataMap['SW_HOSTNAME'].getValue();
		};
		this.setSW_HOSTNAME = function(value) {
			return this.form.dataMap['SW_HOSTNAME'].setValue(value);
		};
		this.getSW_MAINCASE = function() {
			return this.form.dataMap['SW_MAINCASE'].getValue();
		};
		this.setSW_MAINCASE = function(value) {
			return this.form.dataMap['SW_MAINCASE'].setValue(value);
		};
		this.getISAPPERROR = function() {
			return this.form.dataMap['ISAPPERROR'].getValue();
		};
		this.setISAPPERROR = function(value) {
			return this.form.dataMap['ISAPPERROR'].setValue(value);
		};
		this.getSW_MAINPROC = function() {
			return this.form.dataMap['SW_MAINPROC'].getValue();
		};
		this.setSW_MAINPROC = function(value) {
			return this.form.dataMap['SW_MAINPROC'].setValue(value);
		};
		this.getMAXRETRIES = function() {
			return this.form.dataMap['MAXRETRIES'].getValue();
		};
		this.setMAXRETRIES = function(value) {
			return this.form.dataMap['MAXRETRIES'].setValue(value);
		};
		this.getISTECHERROR = function() {
			return this.form.dataMap['ISTECHERROR'].getValue();
		};
		this.setISTECHERROR = function(value) {
			return this.form.dataMap['ISTECHERROR'].setValue(value);
		};
		this.getSTATUS_CODE = function() {
			return this.form.dataMap['STATUS_CODE'].getValue();
		};
		this.setSTATUS_CODE = function(value) {
			return this.form.dataMap['STATUS_CODE'].setValue(value);
		};
		this.getDATETIME = function() {
			return this.form.dataMap['DATETIME'].getValue();
		};
		this.setDATETIME = function(value) {
			return this.form.dataMap['DATETIME'].setValue(value);
		};
		this.getOUTCOME = function() {
			return this.form.dataMap['OUTCOME'].getValue();
		};
		this.setOUTCOME = function(value) {
			return this.form.dataMap['OUTCOME'].setValue(value);
		};
		this.getDUMP = function() {
			return this.form.dataMap['DUMP'].getValue();
		};
		this.setDUMP = function(value) {
			return this.form.dataMap['DUMP'].setValue(value);
		};
		this.getSTERRORDESC = function() {
			return this.form.dataMap['STERRORDESC'].getValue();
		};
		this.setSTERRORDESC = function(value) {
			return this.form.dataMap['STERRORDESC'].setValue(value);
		};
		this.getNUMAPPRETRIES = function() {
			return this.form.dataMap['NUMAPPRETRIES'].getValue();
		};
		this.setNUMAPPRETRIES = function(value) {
			return this.form.dataMap['NUMAPPRETRIES'].setValue(value);
		};
		this.getPROCESS_ID = function() {
			return this.form.dataMap['PROCESS_ID'].getValue();
		};
		this.setPROCESS_ID = function(value) {
			return this.form.dataMap['PROCESS_ID'].setValue(value);
		};
		this.getSTERRORCODE = function() {
			return this.form.dataMap['STERRORCODE'].getValue();
		};
		this.setSTERRORCODE = function(value) {
			return this.form.dataMap['STERRORCODE'].setValue(value);
		};
		this.getSERVICE_NAME = function() {
			return this.form.dataMap['SERVICE_NAME'].getValue();
		};
		this.setSERVICE_NAME = function(value) {
			return this.form.dataMap['SERVICE_NAME'].setValue(value);
		};
		this.getPARTICIPANTE = function() {
			return this.form.dataMap['PARTICIPANTE'].getValue();
		};
		this.setPARTICIPANTE = function(value) {
			return this.form.dataMap['PARTICIPANTE'].setValue(value);
		};
	}
};
tibcoforms.formCode['_Q7QYQGQwEfGUo9jzQD7qQQ']['defineValidations']();



if (typeof(tibcoforms) == 'undefined') tibcoforms = new Object();
if (typeof(tibcoforms.formCode) == 'undefined') tibcoforms.formCode = new Object();
tibcoforms.formCode['_Q8r7oGQwEfGUo9jzQD7qQQ'] = new Object();
tibcoforms.formCode['_Q8r7oGQwEfGUo9jzQD7qQQ']['defineActions'] = function() {
var fc = tibcoforms.formCode['_Q8r7oGQwEfGUo9jzQD7qQQ'];
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
tibcoforms.formCode['_Q8r7oGQwEfGUo9jzQD7qQQ']['defineActions']();

tibcoforms.formCode['_Q8r7oGQwEfGUo9jzQD7qQQ']['defineValidations'] = function() {
var fc = tibcoforms.formCode['_Q8r7oGQwEfGUo9jzQD7qQQ'];
	
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
		this.getDUMP = function() {
			return this.form.dataMap['DUMP'].getValue();
		};
		this.setDUMP = function(value) {
			return this.form.dataMap['DUMP'].setValue(value);
		};
		this.getSTERRORCODE = function() {
			return this.form.dataMap['STERRORCODE'].getValue();
		};
		this.setSTERRORCODE = function(value) {
			return this.form.dataMap['STERRORCODE'].setValue(value);
		};
		this.getDATETIME = function() {
			return this.form.dataMap['DATETIME'].getValue();
		};
		this.setDATETIME = function(value) {
			return this.form.dataMap['DATETIME'].setValue(value);
		};
		this.getISAPPERROR = function() {
			return this.form.dataMap['ISAPPERROR'].getValue();
		};
		this.setISAPPERROR = function(value) {
			return this.form.dataMap['ISAPPERROR'].setValue(value);
		};
		this.getPROCESS_ID = function() {
			return this.form.dataMap['PROCESS_ID'].getValue();
		};
		this.setPROCESS_ID = function(value) {
			return this.form.dataMap['PROCESS_ID'].setValue(value);
		};
		this.getSTATUS_CODE = function() {
			return this.form.dataMap['STATUS_CODE'].getValue();
		};
		this.setSTATUS_CODE = function(value) {
			return this.form.dataMap['STATUS_CODE'].setValue(value);
		};
		this.getMAXRETRIES = function() {
			return this.form.dataMap['MAXRETRIES'].getValue();
		};
		this.setMAXRETRIES = function(value) {
			return this.form.dataMap['MAXRETRIES'].setValue(value);
		};
		this.getSERVICE_NAME = function() {
			return this.form.dataMap['SERVICE_NAME'].getValue();
		};
		this.setSERVICE_NAME = function(value) {
			return this.form.dataMap['SERVICE_NAME'].setValue(value);
		};
		this.getSW_MAINPROC = function() {
			return this.form.dataMap['SW_MAINPROC'].getValue();
		};
		this.setSW_MAINPROC = function(value) {
			return this.form.dataMap['SW_MAINPROC'].setValue(value);
		};
		this.getSW_MAINCASE = function() {
			return this.form.dataMap['SW_MAINCASE'].getValue();
		};
		this.setSW_MAINCASE = function(value) {
			return this.form.dataMap['SW_MAINCASE'].setValue(value);
		};
		this.getISTECHERROR = function() {
			return this.form.dataMap['ISTECHERROR'].getValue();
		};
		this.setISTECHERROR = function(value) {
			return this.form.dataMap['ISTECHERROR'].setValue(value);
		};
		this.getSTERRORDESC = function() {
			return this.form.dataMap['STERRORDESC'].getValue();
		};
		this.setSTERRORDESC = function(value) {
			return this.form.dataMap['STERRORDESC'].setValue(value);
		};
		this.getOUTCOME = function() {
			return this.form.dataMap['OUTCOME'].getValue();
		};
		this.setOUTCOME = function(value) {
			return this.form.dataMap['OUTCOME'].setValue(value);
		};
	}
};
tibcoforms.formCode['_Q8r7oGQwEfGUo9jzQD7qQQ']['defineValidations']();

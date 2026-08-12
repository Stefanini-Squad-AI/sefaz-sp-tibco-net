

if (typeof(tibcoforms) == 'undefined') tibcoforms = new Object();
if (typeof(tibcoforms.formCode) == 'undefined') tibcoforms.formCode = new Object();
tibcoforms.formCode['_hl5-YF9TEfG6Lfb98zsREQ'] = new Object();
tibcoforms.formCode['_hl5-YF9TEfG6Lfb98zsREQ']['defineActions'] = function() {
var fc = tibcoforms.formCode['_hl5-YF9TEfG6Lfb98zsREQ'];
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
tibcoforms.formCode['_hl5-YF9TEfG6Lfb98zsREQ']['defineActions']();

tibcoforms.formCode['_hl5-YF9TEfG6Lfb98zsREQ']['defineValidations'] = function() {
var fc = tibcoforms.formCode['_hl5-YF9TEfG6Lfb98zsREQ'];
	
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
		this.getFLAGCONTRARAZAO = function() {
			return this.form.dataMap['FLAGCONTRARAZAO'].getValue();
		};
		this.setFLAGCONTRARAZAO = function(value) {
			return this.form.dataMap['FLAGCONTRARAZAO'].setValue(value);
		};
		this.getCNTPECA4 = function() {
			return this.form.dataMap['CNTPECA4'].getValue();
		};
		this.setCNTPECA4 = function(value) {
			return this.form.dataMap['CNTPECA4'].setValue(value);
		};
		this.getSFPECA4 = function() {
			return this.form.dataMap['SFPECA4'].getValue();
		};
		this.setSFPECA4 = function(value) {
			return this.form.dataMap['SFPECA4'].setValue(value);
		};
		this.getQTPECASSEFAZ = function() {
			return this.form.dataMap['QTPECASSEFAZ'].getValue();
		};
		this.setQTPECASSEFAZ = function(value) {
			return this.form.dataMap['QTPECASSEFAZ'].setValue(value);
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
		this.getSFPECA3 = function() {
			return this.form.dataMap['SFPECA3'].getValue();
		};
		this.setSFPECA3 = function(value) {
			return this.form.dataMap['SFPECA3'].setValue(value);
		};
		this.getTIPOVISTAS = function() {
			return this.form.dataMap['TIPOVISTAS'].getValue();
		};
		this.setTIPOVISTAS = function(value) {
			return this.form.dataMap['TIPOVISTAS'].setValue(value);
		};
		this.getSFPECA1 = function() {
			return this.form.dataMap['SFPECA1'].getValue();
		};
		this.setSFPECA1 = function(value) {
			return this.form.dataMap['SFPECA1'].setValue(value);
		};
		this.getCNTPECA1 = function() {
			return this.form.dataMap['CNTPECA1'].getValue();
		};
		this.setCNTPECA1 = function(value) {
			return this.form.dataMap['CNTPECA1'].setValue(value);
		};
		this.getSFDIASREPRESENT = function() {
			return this.form.dataMap['SFDIASREPRESENT'].getValue();
		};
		this.setSFDIASREPRESENT = function(value) {
			return this.form.dataMap['SFDIASREPRESENT'].setValue(value);
		};
		this.getQTPECASCNT = function() {
			return this.form.dataMap['QTPECASCNT'].getValue();
		};
		this.setQTPECASCNT = function(value) {
			return this.form.dataMap['QTPECASCNT'].setValue(value);
		};
		this.getCNTPECA2 = function() {
			return this.form.dataMap['CNTPECA2'].getValue();
		};
		this.setCNTPECA2 = function(value) {
			return this.form.dataMap['CNTPECA2'].setValue(value);
		};
		this.getDESCREGRA = function() {
			return this.form.dataMap['DESCREGRA'].getValue();
		};
		this.setDESCREGRA = function(value) {
			return this.form.dataMap['DESCREGRA'].setValue(value);
		};
		this.getTEMPORESPOSTA = function() {
			return this.form.dataMap['TEMPORESPOSTA'].getValue();
		};
		this.setTEMPORESPOSTA = function(value) {
			return this.form.dataMap['TEMPORESPOSTA'].setValue(value);
		};
		this.getCODTEXTO = function() {
			return this.form.dataMap['CODTEXTO'].getValue();
		};
		this.setCODTEXTO = function(value) {
			return this.form.dataMap['CODTEXTO'].setValue(value);
		};
	}
};
tibcoforms.formCode['_hl5-YF9TEfG6Lfb98zsREQ']['defineValidations']();

<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet exclude-result-prefixes="xsl xsi java ap sso" xmlns:ap="http://tibco.com/bpm/actionprocessor" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:sso="http://tibco.com/bpm/sso/types" xmlns="http://www.w3.org/1999/xhtml">
<xsl:param name="skin"/>
<xsl:param name="username"/>
<xsl:param name="password"/>
<xsl:param name="workQueueTag"/>
<xsl:param name="workItemTag"/>
<xsl:param name="workItemSubmitUrl">Workspace</xsl:param>
<xsl:param name="locale"/>
<xsl:param name="baseHRef"/>
<xsl:param name="sessionTimeoutWarning"/>
<xsl:param name="messages"/>
<xsl:variable name="skinFile" select="concat(concat('skins/', $skin), '.xml')"/>
<xsl:variable name="skinDoc" select="document($skinFile)"/>
<xsl:variable name="customForm">true</xsl:variable>
<xsl:output method="xml" indent="yes"/>
<xsl:include href="xslt/workspace.xslt"/>
<xsl:include href="xslt/WorkItem2HtmlCommon.xslt"/>
<xsl:template name="formApi">
<script type="text/javascript" src="./scripts/wl_form_api.js"/>
<script type="text/javascript">//This script is ignored</script>
<script type="text/javascript" src="./scripts/wl_form_ext.js"/>
<script type="text/javascript">//This script is ignored</script>
<script type="text/javascript" src="./scripts/">
</script>
<script type="text/javascript">//This script is ignored</script>
<script type="text/javascript" src="./scripts/">
</script>
<script type="text/javascript">//This script is ignored</script>
</xsl:template>
<xsl:template match="ap:ActionResult" mode="form">
<xsl:if test="$hideStatus != 'true'">
<xsl:apply-templates select="//ap:Status"/>
</xsl:if>
<br/>
<script type="text/javascript">
var context = new Object();
context.form = new com.tibco.forms.workspacelite.Form();
context.form._addControl("control.DUMP", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.STERRORCODE", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.DATETIME", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.ISAPPERROR", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.PROCESS_ID", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.STATUS_CODE", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.MAXRETRIES", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SERVICE_NAME", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SW_MAINPROC", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SW_MAINCASE", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.ISTECHERROR", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.STERRORDESC", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.OUTCOME", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.cancel", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.close", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.submit", "com.tibco.forms.controls.textinput");&#x0A;var anchorRefs = [];
var anchorText = [];
</script>
<script type="text/javascript">
function onFormLoad() {
}
function onFormCancel() {
/**** Script for action action.cancel ****/
		
		}
function onFormClose() {
/**** Script for action action.close ****/
		
		}
function onFormSubmit() {
/**** Script for action action.submit ****/
		
		}
</script>
<!-- Renders the work item data for this form. -->
<br/>
<xsl:element name="form">
<xsl:attribute name="method">post</xsl:attribute>
<xsl:attribute name="action">
<xsl:value-of select="$workItemSubmitUrl"/>
</xsl:attribute>
<xsl:call-template name="hiddenFields"/>
<xsl:call-template name="root"/>
<xsl:call-template name="buttons">
<xsl:with-param name="forwardable">
<xsl:value-of select="//sso:IsForwardable"/>
</xsl:with-param>
</xsl:call-template>
</xsl:element>
</xsl:template>
<xsl:template name="onLoad">
<xsl:if test="$isPageBusAvailable = 'true'">
<xsl:text>onFormLoad();</xsl:text>
</xsl:if>
</xsl:template>
<xsl:template name="root">
<div class="vertPane" style="" id="root">
<a name="root">
</a>
<fieldset>
<legend>Manipular Excecao</legend>
    
    
    
    
    
    <div class="item">
<label for="control.DUMP" class="vert" style="display: block; ">DUMP: </label>
<textarea id="control.DUMP" name="fieldDUMP" rows="10" cols="25" readonly="readonly">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DUMP']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DUMP']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DUMP']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:when>
<xsl:otherwise>
<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>
</xsl:otherwise>
</xsl:choose>
</textarea>
<input type="hidden" name="fieldDUMPType" value="swText"/>
</div>
    <div class="item">
<label for="control.STERRORCODE" class="vert" style="display: block; ">STErrorCode: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.STERRORCODE</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldSTERRORCODE</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STERRORCODE']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STERRORCODE']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSTERRORCODEType" value="swText"/>
</div>
    <div class="item">
<label for="control.DATETIME" class="vert" style="display: block; ">DATETIME: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DATETIME</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldDATETIME</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DATETIME']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DATETIME']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldDATETIMEType" value="swText"/>
</div>
    <div class="item">
<label for="control.ISAPPERROR" class="vert" style="display: block; ">IsAppError: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.ISAPPERROR</xsl:attribute>
<xsl:attribute name="maxlength">1</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldISAPPERROR</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='ISAPPERROR']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='ISAPPERROR']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldISAPPERRORType" value="swText"/>
</div>
    <div class="item">
<label for="control.PROCESS_ID" class="vert" style="display: block; ">PROCESS_ID: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.PROCESS_ID</xsl:attribute>
<xsl:attribute name="maxlength">35</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldPROCESS_ID</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='PROCESS_ID']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='PROCESS_ID']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldPROCESS_IDType" value="swText"/>
</div>
    <div class="item">
<label for="control.STATUS_CODE" class="vert" style="display: block; ">STATUS_CODE: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.STATUS_CODE</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldSTATUS_CODE</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STATUS_CODE']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STATUS_CODE']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSTATUS_CODEType" value="swText"/>
</div>
    <div class="item">
<label for="control.MAXRETRIES" class="vert" style="display: block; ">MaxRetries: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.MAXRETRIES</xsl:attribute>
<xsl:attribute name="maxlength">11</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldMAXRETRIES</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='MAXRETRIES']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='MAXRETRIES']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldMAXRETRIESType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.SERVICE_NAME" class="vert" style="display: block; ">SERVICE_NAME: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SERVICE_NAME</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldSERVICE_NAME</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SERVICE_NAME']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SERVICE_NAME']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSERVICE_NAMEType" value="swText"/>
</div>
    <div class="item">
<label for="control.SW_MAINPROC" class="vert" style="display: block; ">SW_MAINPROC: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SW_MAINPROC</xsl:attribute>
<xsl:attribute name="maxlength">8</xsl:attribute>
<xsl:attribute name="size">10</xsl:attribute>
<xsl:attribute name="name">fieldSW_MAINPROC</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_MAINPROC']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_MAINPROC']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSW_MAINPROCType" value="swText"/>
</div>
    <div class="item">
<label for="control.SW_MAINCASE" class="vert" style="display: block; ">SW_MAINCASE: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SW_MAINCASE</xsl:attribute>
<xsl:attribute name="maxlength">11</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldSW_MAINCASE</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_MAINCASE']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_MAINCASE']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSW_MAINCASEType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.ISTECHERROR" class="vert" style="display: block; ">IsTechError: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.ISTECHERROR</xsl:attribute>
<xsl:attribute name="maxlength">1</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldISTECHERROR</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='ISTECHERROR']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='ISTECHERROR']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldISTECHERRORType" value="swText"/>
</div>
    <div class="item">
<label for="control.STERRORDESC" class="vert" style="display: block; ">STErrorDesc: </label>
<textarea id="control.STERRORDESC" name="fieldSTERRORDESC" rows="10" cols="25" readonly="readonly">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STERRORDESC']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STERRORDESC']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STERRORDESC']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:when>
<xsl:otherwise>
<xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>
</xsl:otherwise>
</xsl:choose>
</textarea>
<input type="hidden" name="fieldSTERRORDESCType" value="swText"/>
</div>
    <div class="item">
<label for="control.OUTCOME" class="vert" style="display: block; ">Outcome: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.OUTCOME</xsl:attribute>
<xsl:attribute name="maxlength">10</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldOUTCOME</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='OUTCOME']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='OUTCOME']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldOUTCOMEType" value="swText"/>
</div>
  </fieldset>
</div>
</xsl:template>
<xsl:template name="toolbar">
<div class="horizPane" style="" id="toolbar">
<a name="toolbar">
</a>
<fieldset>
    
    
    
    
     <xsl:element name="input">
<xsl:attribute name="type">button</xsl:attribute>
<xsl:attribute name="id">Cancel</xsl:attribute>
<xsl:attribute name="name">fieldCANCEL</xsl:attribute>
<xsl:attribute name="onclick"/>
<xsl:attribute name="value">Cancel</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCANCELType" value="swText"/>
     <xsl:element name="input">
<xsl:attribute name="type">button</xsl:attribute>
<xsl:attribute name="id">Close</xsl:attribute>
<xsl:attribute name="name">fieldCLOSE</xsl:attribute>
<xsl:attribute name="onclick"/>
<xsl:attribute name="value">Close</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCLOSEType" value="swText"/>
     <xsl:element name="input">
<xsl:attribute name="type">button</xsl:attribute>
<xsl:attribute name="id">Submit</xsl:attribute>
<xsl:attribute name="name">fieldSUBMIT</xsl:attribute>
<xsl:attribute name="onclick"/>
<xsl:attribute name="value">Submit</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSUBMITType" value="swText"/>
  </fieldset>
</div>
</xsl:template>
<xsl:template name="messages">
<div class="horizPane" style="" id="messages">
<a name="messages">
</a>
<fieldset>
    
    
    
    
  </fieldset>
</div>
</xsl:template>
</xsl:stylesheet>

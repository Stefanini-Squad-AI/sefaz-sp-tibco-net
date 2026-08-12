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
context.form._addControl("control.FLAGCONTRARAZAO", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.CNTPECA4", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SFPECA4", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.QTPECASSEFAZ", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CNTPECA3", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SFPECA2", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SFPECA3", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.TIPOVISTAS", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SFPECA1", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CNTPECA1", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SFDIASREPRESENT", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.QTPECASCNT", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CNTPECA2", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.DESCREGRA", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.TEMPORESPOSTA", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CODTEXTO", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.cancel", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.close", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.submit", "com.tibco.forms.controls.textinput");&#x0A;var anchorRefs = [];
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
<legend>Verificar Retorno Decisions</legend>
    
    
    
    
    
    <div class="item">
<label for="control.FLAGCONTRARAZAO" class="vert" style="display: block; ">flagContraRazao: </label>
<input type="checkbox" id="control.FLAGCONTRARAZAO" name="fieldFLAGCONTRARAZAO" readonly="readonly">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='FLAGCONTRARAZAO']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldFLAGCONTRARAZAOType" value="swText"/>
</div>
    <div class="item">
<label for="control.CNTPECA4" class="vert" style="display: block; ">cntPeca4: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CNTPECA4</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldCNTPECA4</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CNTPECA4']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CNTPECA4']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCNTPECA4Type" value="swText"/>
</div>
    <div class="item">
<label for="control.SFPECA4" class="vert" style="display: block; ">sfPeca4: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SFPECA4</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldSFPECA4</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SFPECA4']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SFPECA4']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSFPECA4Type" value="swText"/>
</div>
    <div class="item">
<label for="control.QTPECASSEFAZ" class="vert" style="display: block; ">qtPecasSefaz: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.QTPECASSEFAZ</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldQTPECASSEFAZ</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='QTPECASSEFAZ']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='QTPECASSEFAZ']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldQTPECASSEFAZType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.CNTPECA3" class="vert" style="display: block; ">cntPeca3: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CNTPECA3</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldCNTPECA3</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CNTPECA3']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CNTPECA3']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCNTPECA3Type" value="swText"/>
</div>
    <div class="item">
<label for="control.SFPECA2" class="vert" style="display: block; ">sfPeca2: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SFPECA2</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldSFPECA2</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SFPECA2']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SFPECA2']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSFPECA2Type" value="swText"/>
</div>
    <div class="item">
<label for="control.SFPECA3" class="vert" style="display: block; ">sfPeca3: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SFPECA3</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldSFPECA3</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SFPECA3']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SFPECA3']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSFPECA3Type" value="swText"/>
</div>
    <div class="item">
<label for="control.TIPOVISTAS" class="vert" style="display: block; ">tipoVistas: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.TIPOVISTAS</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldTIPOVISTAS</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='TIPOVISTAS']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='TIPOVISTAS']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldTIPOVISTASType" value="swText"/>
</div>
    <div class="item">
<label for="control.SFPECA1" class="vert" style="display: block; ">sfPeca1: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SFPECA1</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldSFPECA1</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SFPECA1']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SFPECA1']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSFPECA1Type" value="swText"/>
</div>
    <div class="item">
<label for="control.CNTPECA1" class="vert" style="display: block; ">cntPeca1: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CNTPECA1</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldCNTPECA1</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CNTPECA1']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CNTPECA1']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCNTPECA1Type" value="swText"/>
</div>
    <div class="item">
<label for="control.SFDIASREPRESENT" class="vert" style="display: block; ">sfDiasRepresentacao: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SFDIASREPRESENT</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldSFDIASREPRESENT</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SFDIASREPRESENT']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SFDIASREPRESENT']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSFDIASREPRESENTType" value="swText"/>
</div>
    <div class="item">
<label for="control.QTPECASCNT" class="vert" style="display: block; ">qtPecasCNT: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.QTPECASCNT</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldQTPECASCNT</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='QTPECASCNT']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='QTPECASCNT']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldQTPECASCNTType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.CNTPECA2" class="vert" style="display: block; ">cntPeca2: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CNTPECA2</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldCNTPECA2</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CNTPECA2']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CNTPECA2']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCNTPECA2Type" value="swText"/>
</div>
    <div class="item">
<label for="control.DESCREGRA" class="vert" style="display: block; ">DescricaoRegra: </label>
<textarea id="control.DESCREGRA" name="fieldDESCREGRA" rows="10" cols="25" readonly="readonly">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DESCREGRA']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DESCREGRA']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DESCREGRA']/sso:Value"/>
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
<input type="hidden" name="fieldDESCREGRAType" value="swText"/>
</div>
    <div class="item">
<label for="control.TEMPORESPOSTA" class="vert" style="display: block; ">tempoResposta: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.TEMPORESPOSTA</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldTEMPORESPOSTA</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='TEMPORESPOSTA']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='TEMPORESPOSTA']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldTEMPORESPOSTAType" value="swText"/>
</div>
    <div class="item">
<label for="control.CODTEXTO" class="vert" style="display: block; ">codTexto: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CODTEXTO</xsl:attribute>
<xsl:attribute name="maxlength">4</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldCODTEXTO</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CODTEXTO']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CODTEXTO']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCODTEXTOType" value="swText"/>
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

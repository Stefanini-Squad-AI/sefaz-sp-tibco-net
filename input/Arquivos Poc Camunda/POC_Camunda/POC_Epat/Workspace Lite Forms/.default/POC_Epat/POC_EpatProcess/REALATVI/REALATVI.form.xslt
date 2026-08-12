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
context.form._addControl("control.IDAIIM", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.NRAIIM", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.IDPROCESSO", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.EMAILVISTAS", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.CNTPECA2", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.TIPODILIGENCIA", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.STSRESPSF", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.STSADMTITDRF", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.QTPECASCNT", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.IDTIPOIMPUGNACA", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SFPECA1", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.STSRESPCNT", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.FLAGCONTRARAZAO", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.EXCLUSAOSOLIDAR", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.INDNAORECORRER", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.DESCREGRA", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.IDMOTIVOINTIMAC", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SFDIASREPRESENT", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CODTEXTO", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.RECURSOOFICIO", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.QTPECASSEFAZ", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.FLGALC", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.VICIOREPRESENTA", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.CNTPECA3", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SFPECA2", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.IDDECISAODEBITO", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.STSPETICAO", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.DILIGENCIA", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.DEFESAADMITIDA", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.IDDECISAOAIIM", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.ANULACAODTJ", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.RECAPIT", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.ORIGEM", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CNTPECA4", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.STATUSRECURSOS", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.INDRESPPRM", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CNTPECA1", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.TEMPORESPOSTA", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.INSTANCIA", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.STSADMTITCNT", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.VRAIIM", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SFPECA4", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.STATUSPRJ", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.DILIGENCIADESTI", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.STSPRMSF", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.INTIMACAOCOUNT", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SFPECA3", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CHEFEUJ", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.CRCASA", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CODMUNICIPIO", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.NR_RATORIG", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.DOCCONTROL", "com.tibco.forms.controls.textinput");&#x0A;<xsl:choose>
<xsl:when test="$use3PartDate = 'true'">context.form._addControl("control.DTCIENCIA"dd, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.DTCIENCIA"mm, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.DTCIENCIA"yyyy, "com.tibco.forms.controls.text");&#x0A;</xsl:when>
<xsl:otherwise>context.form._addControl("control.DTCIENCIA", "com.tibco.forms.controls.date");&#x0A;</xsl:otherwise>
</xsl:choose>context.form._addControl("control.CRCONTRIBUINTE", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.IDSINTIMADOS", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.CODUADTJ", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.QTDINTIMADOS", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CODUADRT", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.DEAT0050", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.EXISTENOTIFICAC", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.TROCATPNOTIFICA", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.FLAGRETIRATE", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.BCCRELATORIO", "com.tibco.forms.controls.textarea");&#x0A;<xsl:choose>
<xsl:when test="$use3PartDate = 'true'">context.form._addControl("control.PRAZORETIRADAVI"dd, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.PRAZORETIRADAVI"mm, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.PRAZORETIRADAVI"yyyy, "com.tibco.forms.controls.text");&#x0A;</xsl:when>
<xsl:otherwise>context.form._addControl("control.PRAZORETIRADAVI", "com.tibco.forms.controls.date");&#x0A;</xsl:otherwise>
</xsl:choose>context.form._addControl("control.VOLTARSEGINSTAN", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.TIPOVISTAS", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.HORAFINAL", "com.tibco.forms.controls.textinput");&#x0A;<xsl:choose>
<xsl:when test="$use3PartDate = 'true'">context.form._addControl("control.PRAZOVISTA"dd, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.PRAZOVISTA"mm, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.PRAZOVISTA"yyyy, "com.tibco.forms.controls.text");&#x0A;</xsl:when>
<xsl:otherwise>context.form._addControl("control.PRAZOVISTA", "com.tibco.forms.controls.date");&#x0A;</xsl:otherwise>
</xsl:choose>context.form._addControl("control.FLAGCRZ", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.SW_CASENUMPOC", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.NR_AIIM", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SW_MAINCASEPOC", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CNTINSTANCIASUF", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.AFR", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.CDIMPOSTO", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SITUACAOCARREGA", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.DOCSREQUERIDOS", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.NR_RAT", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.DOCSPERMITIDOS", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.REGRAINSDOC", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SW_CASEDESC", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CODMUNAIIM", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CD_DRT", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.FORMACORRECAO", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CTL_RETIRAT", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.NOMEETAPA", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.RESPOSTACQ", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.LINKIPE", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.DAYSOVER", "com.tibco.forms.controls.textinput");&#x0A;<xsl:choose>
<xsl:when test="$use3PartDate = 'true'">context.form._addControl("control.DTFIMCQ"dd, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.DTFIMCQ"mm, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.DTFIMCQ"yyyy, "com.tibco.forms.controls.text");&#x0A;</xsl:when>
<xsl:otherwise>context.form._addControl("control.DTFIMCQ", "com.tibco.forms.controls.date");&#x0A;</xsl:otherwise>
</xsl:choose>context.form._addControl("control.HRFIMCQ", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.COORDENADOR", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.CORRECAO", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.NOTIFICACAO", "com.tibco.forms.controls.textarea");&#x0A;<xsl:choose>
<xsl:when test="$use3PartDate = 'true'">context.form._addControl("control.PRAZORELATO"dd, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.PRAZORELATO"mm, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.PRAZORELATO"yyyy, "com.tibco.forms.controls.text");&#x0A;</xsl:when>
<xsl:otherwise>context.form._addControl("control.PRAZORELATO", "com.tibco.forms.controls.date");&#x0A;</xsl:otherwise>
</xsl:choose>context.form._addControl("control.EMAILRELATOR", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.CCRELATORIO", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.NRSUBPRO", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.ARRAYINT", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.INTIMACAOCARTA", "com.tibco.forms.controls.textinput");&#x0A;<xsl:choose>
<xsl:when test="$use3PartDate = 'true'">context.form._addControl("control.DTPUBLICACAODE"dd, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.DTPUBLICACAODE"mm, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.DTPUBLICACAODE"yyyy, "com.tibco.forms.controls.text");&#x0A;</xsl:when>
<xsl:otherwise>context.form._addControl("control.DTPUBLICACAODE", "com.tibco.forms.controls.date");&#x0A;</xsl:otherwise>
</xsl:choose>context.form._addControl("control.INTIMACAODE", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.NOVOMODELO", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.INDICESUBDIN", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.CTRLIATV", "com.tibco.forms.controls.checkbox");&#x0A;context.form._addControl("control.AUX", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.POSICAOINICIO", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.IDPECASSF", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.IDPECASCNT", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.FIMSTRING", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.POSICAOFIM", "com.tibco.forms.controls.textinput");&#x0A;<xsl:choose>
<xsl:when test="$use3PartDate = 'true'">context.form._addControl("control.DATAENCPREPNOT"dd, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.DATAENCPREPNOT"mm, "com.tibco.forms.controls.text");&#x0A;context.form._addControl("control.DATAENCPREPNOT"yyyy, "com.tibco.forms.controls.text");&#x0A;</xsl:when>
<xsl:otherwise>context.form._addControl("control.DATAENCPREPNOT", "com.tibco.forms.controls.date");&#x0A;</xsl:otherwise>
</xsl:choose>context.form._addControl("control.IDAIIMORIGINAL", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SW_PARENTPROC", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SW_CASENUM", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SW_PARENTCASE", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SW_HOSTNAME", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SW_MAINCASE", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.ISAPPERROR", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SW_MAINPROC", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.MAXRETRIES", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.ISTECHERROR", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.STATUS_CODE", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.DATETIME", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.OUTCOME", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.DUMP", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.STERRORDESC", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.NUMAPPRETRIES", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.PROCESS_ID", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.STERRORCODE", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.SERVICE_NAME", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.PARTICIPANTE", "com.tibco.forms.controls.textarea");&#x0A;context.form._addControl("control.cancel", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.close", "com.tibco.forms.controls.textinput");&#x0A;context.form._addControl("control.submit", "com.tibco.forms.controls.textinput");&#x0A;var anchorRefs = [];
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
<legend>Realizar Atividade Vista Mista</legend>
    
    
    
    
    
    <div class="item">
<label for="control.IDAIIM" class="vert" style="display: block; ">idAIIM: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.IDAIIM</xsl:attribute>
<xsl:attribute name="maxlength">12</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldIDAIIM</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDAIIM']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDAIIM']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldIDAIIMType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.NRAIIM" class="vert" style="display: block; ">nrAiim: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.NRAIIM</xsl:attribute>
<xsl:attribute name="maxlength">12</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldNRAIIM</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NRAIIM']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NRAIIM']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldNRAIIMType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.IDPROCESSO" class="vert" style="display: block; ">idProcesso: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.IDPROCESSO</xsl:attribute>
<xsl:attribute name="maxlength">10</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldIDPROCESSO</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDPROCESSO']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDPROCESSO']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldIDPROCESSOType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.EMAILVISTAS" class="vert" style="display: block; ">email Vistas: </label>
<textarea id="control.EMAILVISTAS" name="fieldEMAILVISTAS" rows="10" cols="25">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='EMAILVISTAS']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='EMAILVISTAS']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='EMAILVISTAS']/sso:Value"/>
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
<input type="hidden" name="fieldEMAILVISTASType" value="swText"/>
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
<label for="control.TIPODILIGENCIA" class="vert" style="display: block; ">tipoDiligencia: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.TIPODILIGENCIA</xsl:attribute>
<xsl:attribute name="maxlength">6</xsl:attribute>
<xsl:attribute name="size">10</xsl:attribute>
<xsl:attribute name="name">fieldTIPODILIGENCIA</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='TIPODILIGENCIA']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='TIPODILIGENCIA']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldTIPODILIGENCIAType" value="swText"/>
</div>
    <div class="item">
<label for="control.STSRESPSF" class="vert" style="display: block; ">statusRESPFazenda: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.STSRESPSF</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldSTSRESPSF</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STSRESPSF']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STSRESPSF']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSTSRESPSFType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.STSADMTITDRF" class="vert" style="display: block; ">StatusAdmissaoTITDRF: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.STSADMTITDRF</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldSTSADMTITDRF</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STSADMTITDRF']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STSADMTITDRF']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSTSADMTITDRFType" value="swNumeric"/>
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
<label for="control.IDTIPOIMPUGNACA" class="vert" style="display: block; ">idTipoImpugnacao: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.IDTIPOIMPUGNACA</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldIDTIPOIMPUGNACA</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDTIPOIMPUGNACA']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDTIPOIMPUGNACA']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldIDTIPOIMPUGNACAType" value="swText"/>
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
<label for="control.STSRESPCNT" class="vert" style="display: block; ">statusRESPCnt: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.STSRESPCNT</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldSTSRESPCNT</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STSRESPCNT']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STSRESPCNT']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSTSRESPCNTType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.FLAGCONTRARAZAO" class="vert" style="display: block; ">flagContraRazao: </label>
<input type="checkbox" id="control.FLAGCONTRARAZAO" name="fieldFLAGCONTRARAZAO">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='FLAGCONTRARAZAO']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldFLAGCONTRARAZAOType" value="swText"/>
</div>
    <div class="item">
<label for="control.EXCLUSAOSOLIDAR" class="vert" style="display: block; ">ExclusaoSolidarios: </label>
<input type="checkbox" id="control.EXCLUSAOSOLIDAR" name="fieldEXCLUSAOSOLIDAR" readonly="readonly">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='EXCLUSAOSOLIDAR']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldEXCLUSAOSOLIDARType" value="swText"/>
</div>
    <div class="item">
<label for="control.INDNAORECORRER" class="vert" style="display: block; ">indNaoRecorrer: </label>
<input type="checkbox" id="control.INDNAORECORRER" name="fieldINDNAORECORRER">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='INDNAORECORRER']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldINDNAORECORRERType" value="swText"/>
</div>
    <div class="item">
<label for="control.DESCREGRA" class="vert" style="display: block; ">DescricaoRegra: </label>
<textarea id="control.DESCREGRA" name="fieldDESCREGRA" rows="10" cols="25">
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
<label for="control.IDMOTIVOINTIMAC" class="vert" style="display: block; ">idMotivoIntimacao: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.IDMOTIVOINTIMAC</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldIDMOTIVOINTIMAC</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDMOTIVOINTIMAC']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDMOTIVOINTIMAC']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldIDMOTIVOINTIMACType" value="swNumeric"/>
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
<label for="control.CODTEXTO" class="vert" style="display: block; ">codTexto: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CODTEXTO</xsl:attribute>
<xsl:attribute name="maxlength">4</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldCODTEXTO</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
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
    <div class="item">
<label for="control.RECURSOOFICIO" class="vert" style="display: block; ">recursoOficio: </label>
<input type="checkbox" id="control.RECURSOOFICIO" name="fieldRECURSOOFICIO" readonly="readonly">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='RECURSOOFICIO']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldRECURSOOFICIOType" value="swText"/>
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
<label for="control.FLGALC" class="vert" style="display: block; ">FlagAlcada: </label>
<input type="checkbox" id="control.FLGALC" name="fieldFLGALC">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='FLGALC']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldFLGALCType" value="swText"/>
</div>
    <div class="item">
<label for="control.VICIOREPRESENTA" class="vert" style="display: block; ">vicioRepresentacao: </label>
<input type="checkbox" id="control.VICIOREPRESENTA" name="fieldVICIOREPRESENTA">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='VICIOREPRESENTA']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldVICIOREPRESENTAType" value="swText"/>
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
<label for="control.IDDECISAODEBITO" class="vert" style="display: block; ">idDecisaoDebitoFiscal: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.IDDECISAODEBITO</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldIDDECISAODEBITO</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDDECISAODEBITO']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDDECISAODEBITO']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldIDDECISAODEBITOType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.STSPETICAO" class="vert" style="display: block; ">StatusPeticao: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.STSPETICAO</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldSTSPETICAO</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STSPETICAO']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STSPETICAO']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSTSPETICAOType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.DILIGENCIA" class="vert" style="display: block; ">diligencia: </label>
<input type="checkbox" id="control.DILIGENCIA" name="fieldDILIGENCIA" readonly="readonly">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DILIGENCIA']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldDILIGENCIAType" value="swText"/>
</div>
    <div class="item">
<label for="control.DEFESAADMITIDA" class="vert" style="display: block; ">defesaAdmitida: </label>
<input type="checkbox" id="control.DEFESAADMITIDA" name="fieldDEFESAADMITIDA">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DEFESAADMITIDA']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldDEFESAADMITIDAType" value="swText"/>
</div>
    <div class="item">
<label for="control.IDDECISAOAIIM" class="vert" style="display: block; ">idDecisaoAIIM: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.IDDECISAOAIIM</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldIDDECISAOAIIM</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDDECISAOAIIM']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDDECISAOAIIM']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldIDDECISAOAIIMType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.ANULACAODTJ" class="vert" style="display: block; ">AnulacaoDTJ: </label>
<input type="checkbox" id="control.ANULACAODTJ" name="fieldANULACAODTJ" readonly="readonly">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='ANULACAODTJ']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldANULACAODTJType" value="swText"/>
</div>
    <div class="item">
<label for="control.RECAPIT" class="vert" style="display: block; ">Recapitulacao: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.RECAPIT</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldRECAPIT</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='RECAPIT']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='RECAPIT']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldRECAPITType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.ORIGEM" class="vert" style="display: block; ">origem: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.ORIGEM</xsl:attribute>
<xsl:attribute name="maxlength">5</xsl:attribute>
<xsl:attribute name="size">10</xsl:attribute>
<xsl:attribute name="name">fieldORIGEM</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='ORIGEM']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='ORIGEM']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldORIGEMType" value="swText"/>
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
<label for="control.STATUSRECURSOS" class="vert" style="display: block; ">statusRecursos: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.STATUSRECURSOS</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldSTATUSRECURSOS</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STATUSRECURSOS']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STATUSRECURSOS']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSTATUSRECURSOSType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.INDRESPPRM" class="vert" style="display: block; ">indRespPRM: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.INDRESPPRM</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldINDRESPPRM</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='INDRESPPRM']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='INDRESPPRM']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldINDRESPPRMType" value="swNumeric"/>
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
<label for="control.TEMPORESPOSTA" class="vert" style="display: block; ">tempoResposta: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.TEMPORESPOSTA</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldTEMPORESPOSTA</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
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
<label for="control.INSTANCIA" class="vert" style="display: block; ">Instancia: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.INSTANCIA</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldINSTANCIA</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='INSTANCIA']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='INSTANCIA']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldINSTANCIAType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.STSADMTITCNT" class="vert" style="display: block; ">StatusAdmissaoTITCNT: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.STSADMTITCNT</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldSTSADMTITCNT</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STSADMTITCNT']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STSADMTITCNT']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSTSADMTITCNTType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.VRAIIM" class="vert" style="display: block; ">vrAIIM: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.VRAIIM</xsl:attribute>
<xsl:attribute name="maxlength">17</xsl:attribute>
<xsl:attribute name="size">20</xsl:attribute>
<xsl:attribute name="name">fieldVRAIIM</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='VRAIIM']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='VRAIIM']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldVRAIIMType" value="swNumeric"/>
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
<label for="control.STATUSPRJ" class="vert" style="display: block; ">statusPrj: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.STATUSPRJ</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldSTATUSPRJ</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STATUSPRJ']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STATUSPRJ']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSTATUSPRJType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.DILIGENCIADESTI" class="vert" style="display: block; ">diligenciaDestino: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DILIGENCIADESTI</xsl:attribute>
<xsl:attribute name="maxlength">20</xsl:attribute>
<xsl:attribute name="size">25</xsl:attribute>
<xsl:attribute name="name">fieldDILIGENCIADESTI</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DILIGENCIADESTI']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DILIGENCIADESTI']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldDILIGENCIADESTIType" value="swText"/>
</div>
    <div class="item">
<label for="control.STSPRMSF" class="vert" style="display: block; ">statusPRMFazenda: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.STSPRMSF</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldSTSPRMSF</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STSPRMSF']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='STSPRMSF']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSTSPRMSFType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.INTIMACAOCOUNT" class="vert" style="display: block; ">intimacaoCount: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.INTIMACAOCOUNT</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldINTIMACAOCOUNT</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='INTIMACAOCOUNT']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='INTIMACAOCOUNT']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldINTIMACAOCOUNTType" value="swNumeric"/>
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
<label for="control.CHEFEUJ" class="vert" style="display: block; ">ChefeUJ: </label>
<textarea id="control.CHEFEUJ" name="fieldCHEFEUJ" rows="10" cols="25" readonly="readonly">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CHEFEUJ']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CHEFEUJ']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CHEFEUJ']/sso:Value"/>
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
<input type="hidden" name="fieldCHEFEUJType" value="swText"/>
</div>
    <div class="item">
<label for="control.CRCASA" class="vert" style="display: block; ">crCasa: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CRCASA</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldCRCASA</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CRCASA']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CRCASA']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCRCASAType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.CODMUNICIPIO" class="vert" style="display: block; ">CodMunicipio: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CODMUNICIPIO</xsl:attribute>
<xsl:attribute name="maxlength">9</xsl:attribute>
<xsl:attribute name="size">10</xsl:attribute>
<xsl:attribute name="name">fieldCODMUNICIPIO</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CODMUNICIPIO']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CODMUNICIPIO']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCODMUNICIPIOType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.NR_RATORIG" class="vert" style="display: block; ">NR_RAT: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.NR_RATORIG</xsl:attribute>
<xsl:attribute name="maxlength">12</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldNR_RATORIG</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NR_RATORIG']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NR_RATORIG']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldNR_RATORIGType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.DOCCONTROL" class="vert" style="display: block; ">docControl: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DOCCONTROL</xsl:attribute>
<xsl:attribute name="maxlength">10</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldDOCCONTROL</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DOCCONTROL']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DOCCONTROL']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldDOCCONTROLType" value="swText"/>
</div>
    <div class="item">
<label for="control.DTCIENCIA" class="vert" style="display: block; ">dtCiencia <xsl:call-template name="datePrototype">
<xsl:with-param name="locale">
<xsl:value-of select="$locale"/>
</xsl:with-param>
</xsl:call-template>: </label>
<xsl:choose>
<xsl:when test="$use3PartDate='true'">
<xsl:element name="div">
<xsl:attribute name="id">control.DTCIENCIA</xsl:attribute>
<xsl:attribute name="name">control.DTCIENCIA</xsl:attribute>
<xsl:attribute name="class">date</xsl:attribute>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DTCIENCIAdd</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datedd</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DTCIENCIAmm</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datemm</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DTCIENCIAyyyy</xsl:attribute>
<xsl:attribute name="size">2</xsl:attribute>
<xsl:attribute name="class">dateyyyy</xsl:attribute>
</xsl:element>
</xsl:element>
</xsl:when>
<xsl:otherwise>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DTCIENCIA</xsl:attribute>
<xsl:attribute name="maxlength"/>
<xsl:attribute name="size"/>
<xsl:attribute name="name">fieldDTCIENCIA</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DTCIENCIA']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DTCIENCIA']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
</xsl:otherwise>
</xsl:choose>
<input type="hidden" name="fieldDTCIENCIAType" value="swDate"/>
</div>
    <div class="item">
<label for="control.CRCONTRIBUINTE" class="vert" style="display: block; ">crContribuinte: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CRCONTRIBUINTE</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldCRCONTRIBUINTE</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CRCONTRIBUINTE']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CRCONTRIBUINTE']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCRCONTRIBUINTEType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.IDSINTIMADOS" class="vert" style="display: block; ">idsIntimados: </label>
<textarea id="control.IDSINTIMADOS" name="fieldIDSINTIMADOS" rows="10" cols="25">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDSINTIMADOS']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDSINTIMADOS']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDSINTIMADOS']/sso:Value"/>
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
<input type="hidden" name="fieldIDSINTIMADOSType" value="swText"/>
</div>
    <div class="item">
<label for="control.CODUADTJ" class="vert" style="display: block; ">cod UA DTJ: </label>
<textarea id="control.CODUADTJ" name="fieldCODUADTJ" rows="10" cols="25">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CODUADTJ']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CODUADTJ']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CODUADTJ']/sso:Value"/>
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
<input type="hidden" name="fieldCODUADTJType" value="swText"/>
</div>
    <div class="item">
<label for="control.QTDINTIMADOS" class="vert" style="display: block; ">qtdeIntimados: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.QTDINTIMADOS</xsl:attribute>
<xsl:attribute name="maxlength">11</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldQTDINTIMADOS</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='QTDINTIMADOS']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='QTDINTIMADOS']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldQTDINTIMADOSType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.CODUADRT" class="vert" style="display: block; ">codUADRT: </label>
<textarea id="control.CODUADRT" name="fieldCODUADRT" rows="10" cols="25" readonly="readonly">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CODUADRT']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CODUADRT']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CODUADRT']/sso:Value"/>
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
<input type="hidden" name="fieldCODUADRTType" value="swText"/>
</div>
    <div class="item">
<label for="control.DEAT0050" class="vert" style="display: block; ">DEAT0050: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DEAT0050</xsl:attribute>
<xsl:attribute name="maxlength">8</xsl:attribute>
<xsl:attribute name="size">10</xsl:attribute>
<xsl:attribute name="name">fieldDEAT0050</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DEAT0050']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DEAT0050']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldDEAT0050Type" value="swText"/>
</div>
    <div class="item">
<label for="control.EXISTENOTIFICAC" class="vert" style="display: block; ">existeNotificacao: </label>
<input type="checkbox" id="control.EXISTENOTIFICAC" name="fieldEXISTENOTIFICAC">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='EXISTENOTIFICAC']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldEXISTENOTIFICACType" value="swText"/>
</div>
    <div class="item">
<label for="control.TROCATPNOTIFICA" class="vert" style="display: block; ">Trocar Tipo Notificacao: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.TROCATPNOTIFICA</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldTROCATPNOTIFICA</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='TROCATPNOTIFICA']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='TROCATPNOTIFICA']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldTROCATPNOTIFICAType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.FLAGRETIRATE" class="vert" style="display: block; ">flagretirate: </label>
<input type="checkbox" id="control.FLAGRETIRATE" name="fieldFLAGRETIRATE">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='FLAGRETIRATE']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldFLAGRETIRATEType" value="swText"/>
</div>
    <div class="item">
<label for="control.BCCRELATORIO" class="vert" style="display: block; ">BCCRELATORIO: </label>
<textarea id="control.BCCRELATORIO" name="fieldBCCRELATORIO" rows="10" cols="25">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='BCCRELATORIO']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='BCCRELATORIO']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='BCCRELATORIO']/sso:Value"/>
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
<input type="hidden" name="fieldBCCRELATORIOType" value="swText"/>
</div>
    <div class="item">
<label for="control.PRAZORETIRADAVI" class="vert" style="display: block; ">PrazoRetiradaVista <xsl:call-template name="datePrototype">
<xsl:with-param name="locale">
<xsl:value-of select="$locale"/>
</xsl:with-param>
</xsl:call-template>: </label>
<xsl:choose>
<xsl:when test="$use3PartDate='true'">
<xsl:element name="div">
<xsl:attribute name="id">control.PRAZORETIRADAVI</xsl:attribute>
<xsl:attribute name="name">control.PRAZORETIRADAVI</xsl:attribute>
<xsl:attribute name="class">date</xsl:attribute>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.PRAZORETIRADAVIdd</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datedd</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.PRAZORETIRADAVImm</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datemm</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.PRAZORETIRADAVIyyyy</xsl:attribute>
<xsl:attribute name="size">2</xsl:attribute>
<xsl:attribute name="class">dateyyyy</xsl:attribute>
</xsl:element>
</xsl:element>
</xsl:when>
<xsl:otherwise>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.PRAZORETIRADAVI</xsl:attribute>
<xsl:attribute name="maxlength"/>
<xsl:attribute name="size"/>
<xsl:attribute name="name">fieldPRAZORETIRADAVI</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='PRAZORETIRADAVI']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='PRAZORETIRADAVI']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
</xsl:otherwise>
</xsl:choose>
<input type="hidden" name="fieldPRAZORETIRADAVIType" value="swDate"/>
</div>
    <div class="item">
<label for="control.VOLTARSEGINSTAN" class="vert" style="display: block; ">voltarSegInstancia: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.VOLTARSEGINSTAN</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldVOLTARSEGINSTAN</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='VOLTARSEGINSTAN']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='VOLTARSEGINSTAN']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldVOLTARSEGINSTANType" value="swNumeric"/>
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
<label for="control.HORAFINAL" class="vert" style="display: block; ">horafinal <xsl:call-template name="datePrototype">
<xsl:with-param name="locale">
<xsl:value-of select="$locale"/>
</xsl:with-param>
</xsl:call-template>
<xsl:text/>
<xsl:call-template name="timePrototype">
<xsl:with-param name="locale">
<xsl:value-of select="$locale"/>
</xsl:with-param>
</xsl:call-template>: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.HORAFINAL</xsl:attribute>
<xsl:attribute name="maxlength"/>
<xsl:attribute name="size"/>
<xsl:attribute name="name">fieldHORAFINAL</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='HORAFINAL']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='HORAFINAL']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldHORAFINALType" value="swTime"/>
</div>
    <div class="item">
<label for="control.PRAZOVISTA" class="vert" style="display: block; ">PrazoVista <xsl:call-template name="datePrototype">
<xsl:with-param name="locale">
<xsl:value-of select="$locale"/>
</xsl:with-param>
</xsl:call-template>: </label>
<xsl:choose>
<xsl:when test="$use3PartDate='true'">
<xsl:element name="div">
<xsl:attribute name="id">control.PRAZOVISTA</xsl:attribute>
<xsl:attribute name="name">control.PRAZOVISTA</xsl:attribute>
<xsl:attribute name="class">date</xsl:attribute>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.PRAZOVISTAdd</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datedd</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.PRAZOVISTAmm</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datemm</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.PRAZOVISTAyyyy</xsl:attribute>
<xsl:attribute name="size">2</xsl:attribute>
<xsl:attribute name="class">dateyyyy</xsl:attribute>
</xsl:element>
</xsl:element>
</xsl:when>
<xsl:otherwise>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.PRAZOVISTA</xsl:attribute>
<xsl:attribute name="maxlength"/>
<xsl:attribute name="size"/>
<xsl:attribute name="name">fieldPRAZOVISTA</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='PRAZOVISTA']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='PRAZOVISTA']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
</xsl:otherwise>
</xsl:choose>
<input type="hidden" name="fieldPRAZOVISTAType" value="swDate"/>
</div>
    <div class="item">
<label for="control.FLAGCRZ" class="vert" style="display: block; ">FlagCrz: </label>
<input type="checkbox" id="control.FLAGCRZ" name="fieldFLAGCRZ">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='FLAGCRZ']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldFLAGCRZType" value="swText"/>
</div>
    <div class="item">
<label for="control.SW_CASENUMPOC" class="vert" style="display: block; ">SW_CASENUMPOC: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SW_CASENUMPOC</xsl:attribute>
<xsl:attribute name="maxlength">16</xsl:attribute>
<xsl:attribute name="size">20</xsl:attribute>
<xsl:attribute name="name">fieldSW_CASENUMPOC</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_CASENUMPOC']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_CASENUMPOC']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSW_CASENUMPOCType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.NR_AIIM" class="vert" style="display: block; ">Número do AIIM: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.NR_AIIM</xsl:attribute>
<xsl:attribute name="maxlength">12</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldNR_AIIM</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NR_AIIM']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NR_AIIM']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldNR_AIIMType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.SW_MAINCASEPOC" class="vert" style="display: block; ">SW_MAINCASEPOC: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SW_MAINCASEPOC</xsl:attribute>
<xsl:attribute name="maxlength">11</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldSW_MAINCASEPOC</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_MAINCASEPOC']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_MAINCASEPOC']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSW_MAINCASEPOCType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.CNTINSTANCIASUF" class="vert" style="display: block; ">cntInstanciasUFC: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CNTINSTANCIASUF</xsl:attribute>
<xsl:attribute name="maxlength">3</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldCNTINSTANCIASUF</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CNTINSTANCIASUF']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CNTINSTANCIASUF']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCNTINSTANCIASUFType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.AFR" class="vert" style="display: block; ">Agente Fiscal de Renda: </label>
<textarea id="control.AFR" name="fieldAFR" rows="10" cols="25">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='AFR']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='AFR']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='AFR']/sso:Value"/>
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
<input type="hidden" name="fieldAFRType" value="swText"/>
</div>
    <div class="item">
<label for="control.CDIMPOSTO" class="vert" style="display: block; ">Código Imposto: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CDIMPOSTO</xsl:attribute>
<xsl:attribute name="maxlength">11</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldCDIMPOSTO</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CDIMPOSTO']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CDIMPOSTO']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCDIMPOSTOType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.SITUACAOCARREGA" class="vert" style="display: block; ">Situação do carregamento do AIIM: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SITUACAOCARREGA</xsl:attribute>
<xsl:attribute name="maxlength">10</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldSITUACAOCARREGA</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SITUACAOCARREGA']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SITUACAOCARREGA']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSITUACAOCARREGAType" value="swText"/>
</div>
    <div class="item">
<label for="control.DOCSREQUERIDOS" class="vert" style="display: block; ">DocsRequeridos: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DOCSREQUERIDOS</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldDOCSREQUERIDOS</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DOCSREQUERIDOS']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DOCSREQUERIDOS']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldDOCSREQUERIDOSType" value="swText"/>
</div>
    <div class="item">
<label for="control.NR_RAT" class="vert" style="display: block; ">Versão do AIIM: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.NR_RAT</xsl:attribute>
<xsl:attribute name="maxlength">12</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldNR_RAT</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NR_RAT']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NR_RAT']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldNR_RATType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.DOCSPERMITIDOS" class="vert" style="display: block; ">DocsPermitidos: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DOCSPERMITIDOS</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldDOCSPERMITIDOS</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DOCSPERMITIDOS']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DOCSPERMITIDOS']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldDOCSPERMITIDOSType" value="swText"/>
</div>
    <div class="item">
<label for="control.REGRAINSDOC" class="vert" style="display: block; ">Regra de inserção de documentos: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.REGRAINSDOC</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldREGRAINSDOC</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='REGRAINSDOC']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='REGRAINSDOC']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldREGRAINSDOCType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.SW_CASEDESC" class="vert" style="display: block; ">SW_CASEDESC: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SW_CASEDESC</xsl:attribute>
<xsl:attribute name="maxlength">24</xsl:attribute>
<xsl:attribute name="size">25</xsl:attribute>
<xsl:attribute name="name">fieldSW_CASEDESC</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_CASEDESC']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_CASEDESC']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSW_CASEDESCType" value="swText"/>
</div>
    <div class="item">
<label for="control.CODMUNAIIM" class="vert" style="display: block; ">Código do Município do AIIM: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CODMUNAIIM</xsl:attribute>
<xsl:attribute name="maxlength">9</xsl:attribute>
<xsl:attribute name="size">10</xsl:attribute>
<xsl:attribute name="name">fieldCODMUNAIIM</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CODMUNAIIM']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CODMUNAIIM']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCODMUNAIIMType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.CD_DRT" class="vert" style="display: block; ">Código da DRT: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CD_DRT</xsl:attribute>
<xsl:attribute name="maxlength">9</xsl:attribute>
<xsl:attribute name="size">10</xsl:attribute>
<xsl:attribute name="name">fieldCD_DRT</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CD_DRT']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CD_DRT']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCD_DRTType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.FORMACORRECAO" class="vert" style="display: block; ">Forma de Correção: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.FORMACORRECAO</xsl:attribute>
<xsl:attribute name="maxlength">20</xsl:attribute>
<xsl:attribute name="size">25</xsl:attribute>
<xsl:attribute name="name">fieldFORMACORRECAO</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='FORMACORRECAO']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='FORMACORRECAO']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldFORMACORRECAOType" value="swText"/>
</div>
    <div class="item">
<label for="control.CTL_RETIRAT" class="vert" style="display: block; ">CTL_RETIRAT: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.CTL_RETIRAT</xsl:attribute>
<xsl:attribute name="maxlength">10</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldCTL_RETIRAT</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CTL_RETIRAT']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CTL_RETIRAT']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldCTL_RETIRATType" value="swText"/>
</div>
    <div class="item">
<label for="control.NOMEETAPA" class="vert" style="display: block; ">Nome da Etapa: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.NOMEETAPA</xsl:attribute>
<xsl:attribute name="maxlength">8</xsl:attribute>
<xsl:attribute name="size">10</xsl:attribute>
<xsl:attribute name="name">fieldNOMEETAPA</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NOMEETAPA']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NOMEETAPA']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldNOMEETAPAType" value="swText"/>
</div>
    <div class="item">
<label for="control.RESPOSTACQ" class="vert" style="display: block; ">Resposta do Contorle de Qualidade: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.RESPOSTACQ</xsl:attribute>
<xsl:attribute name="maxlength">10</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldRESPOSTACQ</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='RESPOSTACQ']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='RESPOSTACQ']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldRESPOSTACQType" value="swText"/>
</div>
    <div class="item">
<label for="control.LINKIPE" class="vert" style="display: block; ">LinkIpe: </label>
<textarea id="control.LINKIPE" name="fieldLINKIPE" rows="10" cols="25">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='LINKIPE']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='LINKIPE']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='LINKIPE']/sso:Value"/>
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
<input type="hidden" name="fieldLINKIPEType" value="swText"/>
</div>
    <div class="item">
<label for="control.DAYSOVER" class="vert" style="display: block; ">daysover: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DAYSOVER</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldDAYSOVER</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DAYSOVER']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DAYSOVER']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldDAYSOVERType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.DTFIMCQ" class="vert" style="display: block; ">DTFIMCQ <xsl:call-template name="datePrototype">
<xsl:with-param name="locale">
<xsl:value-of select="$locale"/>
</xsl:with-param>
</xsl:call-template>: </label>
<xsl:choose>
<xsl:when test="$use3PartDate='true'">
<xsl:element name="div">
<xsl:attribute name="id">control.DTFIMCQ</xsl:attribute>
<xsl:attribute name="name">control.DTFIMCQ</xsl:attribute>
<xsl:attribute name="class">date</xsl:attribute>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DTFIMCQdd</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datedd</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DTFIMCQmm</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datemm</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DTFIMCQyyyy</xsl:attribute>
<xsl:attribute name="size">2</xsl:attribute>
<xsl:attribute name="class">dateyyyy</xsl:attribute>
</xsl:element>
</xsl:element>
</xsl:when>
<xsl:otherwise>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DTFIMCQ</xsl:attribute>
<xsl:attribute name="maxlength"/>
<xsl:attribute name="size"/>
<xsl:attribute name="name">fieldDTFIMCQ</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DTFIMCQ']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DTFIMCQ']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
</xsl:otherwise>
</xsl:choose>
<input type="hidden" name="fieldDTFIMCQType" value="swDate"/>
</div>
    <div class="item">
<label for="control.HRFIMCQ" class="vert" style="display: block; ">HRFIMCQ <xsl:call-template name="timePrototype">
<xsl:with-param name="locale">
<xsl:value-of select="$locale"/>
</xsl:with-param>
</xsl:call-template>: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.HRFIMCQ</xsl:attribute>
<xsl:attribute name="maxlength"/>
<xsl:attribute name="size"/>
<xsl:attribute name="name">fieldHRFIMCQ</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='HRFIMCQ']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='HRFIMCQ']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldHRFIMCQType" value="swTime"/>
</div>
    <div class="item">
<label for="control.COORDENADOR" class="vert" style="display: block; ">Coordenador: </label>
<textarea id="control.COORDENADOR" name="fieldCOORDENADOR" rows="10" cols="25">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='COORDENADOR']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='COORDENADOR']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='COORDENADOR']/sso:Value"/>
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
<input type="hidden" name="fieldCOORDENADORType" value="swText"/>
</div>
    <div class="item">
<label for="control.CORRECAO" class="vert" style="display: block; ">correcao: </label>
<input type="checkbox" id="control.CORRECAO" name="fieldCORRECAO">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CORRECAO']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldCORRECAOType" value="swText"/>
</div>
    <div class="item">
<label for="control.NOTIFICACAO" class="vert" style="display: block; ">notificacao: </label>
<textarea id="control.NOTIFICACAO" name="fieldNOTIFICACAO" rows="10" cols="25">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NOTIFICACAO']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NOTIFICACAO']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NOTIFICACAO']/sso:Value"/>
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
<input type="hidden" name="fieldNOTIFICACAOType" value="swText"/>
</div>
    <div class="item">
<label for="control.PRAZORELATO" class="vert" style="display: block; ">PrazoRelato <xsl:call-template name="datePrototype">
<xsl:with-param name="locale">
<xsl:value-of select="$locale"/>
</xsl:with-param>
</xsl:call-template>: </label>
<xsl:choose>
<xsl:when test="$use3PartDate='true'">
<xsl:element name="div">
<xsl:attribute name="id">control.PRAZORELATO</xsl:attribute>
<xsl:attribute name="name">control.PRAZORELATO</xsl:attribute>
<xsl:attribute name="class">date</xsl:attribute>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.PRAZORELATOdd</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datedd</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.PRAZORELATOmm</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datemm</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.PRAZORELATOyyyy</xsl:attribute>
<xsl:attribute name="size">2</xsl:attribute>
<xsl:attribute name="class">dateyyyy</xsl:attribute>
</xsl:element>
</xsl:element>
</xsl:when>
<xsl:otherwise>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.PRAZORELATO</xsl:attribute>
<xsl:attribute name="maxlength"/>
<xsl:attribute name="size"/>
<xsl:attribute name="name">fieldPRAZORELATO</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='PRAZORELATO']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='PRAZORELATO']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
</xsl:otherwise>
</xsl:choose>
<input type="hidden" name="fieldPRAZORELATOType" value="swDate"/>
</div>
    <div class="item">
<label for="control.EMAILRELATOR" class="vert" style="display: block; ">email Relator: </label>
<textarea id="control.EMAILRELATOR" name="fieldEMAILRELATOR" rows="10" cols="25">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='EMAILRELATOR']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='EMAILRELATOR']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='EMAILRELATOR']/sso:Value"/>
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
<input type="hidden" name="fieldEMAILRELATORType" value="swText"/>
</div>
    <div class="item">
<label for="control.CCRELATORIO" class="vert" style="display: block; ">CCRELATORIO: </label>
<textarea id="control.CCRELATORIO" name="fieldCCRELATORIO" rows="10" cols="25">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CCRELATORIO']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CCRELATORIO']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CCRELATORIO']/sso:Value"/>
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
<input type="hidden" name="fieldCCRELATORIOType" value="swText"/>
</div>
    <div class="item">
<label for="control.NRSUBPRO" class="vert" style="display: block; ">nrSubProc: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.NRSUBPRO</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldNRSUBPRO</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NRSUBPRO']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NRSUBPRO']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldNRSUBPROType" value="swText"/>
</div>
    <div class="item">
<label for="control.ARRAYINT" class="vert" style="display: block; ">arrayIntimados: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.ARRAYINT</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldARRAYINT</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='ARRAYINT']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='ARRAYINT']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldARRAYINTType" value="swText"/>
</div>
    <div class="item">
<label for="control.INTIMACAOCARTA" class="vert" style="display: block; ">intimacaoCarta: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.INTIMACAOCARTA</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldINTIMACAOCARTA</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='INTIMACAOCARTA']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='INTIMACAOCARTA']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldINTIMACAOCARTAType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.DTPUBLICACAODE" class="vert" style="display: block; ">dtPublicacaoDE <xsl:call-template name="datePrototype">
<xsl:with-param name="locale">
<xsl:value-of select="$locale"/>
</xsl:with-param>
</xsl:call-template>: </label>
<xsl:choose>
<xsl:when test="$use3PartDate='true'">
<xsl:element name="div">
<xsl:attribute name="id">control.DTPUBLICACAODE</xsl:attribute>
<xsl:attribute name="name">control.DTPUBLICACAODE</xsl:attribute>
<xsl:attribute name="class">date</xsl:attribute>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DTPUBLICACAODEdd</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datedd</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DTPUBLICACAODEmm</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datemm</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DTPUBLICACAODEyyyy</xsl:attribute>
<xsl:attribute name="size">2</xsl:attribute>
<xsl:attribute name="class">dateyyyy</xsl:attribute>
</xsl:element>
</xsl:element>
</xsl:when>
<xsl:otherwise>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DTPUBLICACAODE</xsl:attribute>
<xsl:attribute name="maxlength"/>
<xsl:attribute name="size"/>
<xsl:attribute name="name">fieldDTPUBLICACAODE</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DTPUBLICACAODE']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DTPUBLICACAODE']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
</xsl:otherwise>
</xsl:choose>
<input type="hidden" name="fieldDTPUBLICACAODEType" value="swDate"/>
</div>
    <div class="item">
<label for="control.INTIMACAODE" class="vert" style="display: block; ">intimacaoDE: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.INTIMACAODE</xsl:attribute>
<xsl:attribute name="maxlength">2</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldINTIMACAODE</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='INTIMACAODE']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='INTIMACAODE']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldINTIMACAODEType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.NOVOMODELO" class="vert" style="display: block; ">novoModelo: </label>
<input type="checkbox" id="control.NOVOMODELO" name="fieldNOVOMODELO">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NOVOMODELO']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldNOVOMODELOType" value="swText"/>
</div>
    <div class="item">
<label for="control.INDICESUBDIN" class="vert" style="display: block; ">indiceSubDin: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.INDICESUBDIN</xsl:attribute>
<xsl:attribute name="maxlength">11</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldINDICESUBDIN</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='INDICESUBDIN']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='INDICESUBDIN']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldINDICESUBDINType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.CTRLIATV" class="vert" style="display: block; ">CTRLIATV: </label>
<input type="checkbox" id="control.CTRLIATV" name="fieldCTRLIATV">
<xsl:if test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='CTRLIATV']/sso:Value = '0.0'">
<xsl:attribute name="checked">checked</xsl:attribute>
</xsl:if>
</input>
<input type="hidden" name="fieldCTRLIATVType" value="swText"/>
</div>
    <div class="item">
<label for="control.AUX" class="vert" style="display: block; ">aux: </label>
<textarea id="control.AUX" name="fieldAUX" rows="10" cols="25">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='AUX']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='AUX']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='AUX']/sso:Value"/>
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
<input type="hidden" name="fieldAUXType" value="swText"/>
</div>
    <div class="item">
<label for="control.POSICAOINICIO" class="vert" style="display: block; ">posicaoInicio: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.POSICAOINICIO</xsl:attribute>
<xsl:attribute name="maxlength">11</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldPOSICAOINICIO</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='POSICAOINICIO']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='POSICAOINICIO']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldPOSICAOINICIOType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.IDPECASSF" class="vert" style="display: block; ">idPecasSf: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.IDPECASSF</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldIDPECASSF</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDPECASSF']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDPECASSF']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldIDPECASSFType" value="swText"/>
</div>
    <div class="item">
<label for="control.IDPECASCNT" class="vert" style="display: block; ">idPecasCnt: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.IDPECASCNT</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldIDPECASCNT</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDPECASCNT']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDPECASCNT']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldIDPECASCNTType" value="swText"/>
</div>
    <div class="item">
<label for="control.FIMSTRING" class="vert" style="display: block; ">fimString: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.FIMSTRING</xsl:attribute>
<xsl:attribute name="maxlength">11</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldFIMSTRING</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='FIMSTRING']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='FIMSTRING']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldFIMSTRINGType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.POSICAOFIM" class="vert" style="display: block; ">posicaoFim: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.POSICAOFIM</xsl:attribute>
<xsl:attribute name="maxlength">11</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldPOSICAOFIM</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='POSICAOFIM']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='POSICAOFIM']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldPOSICAOFIMType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.DATAENCPREPNOT" class="vert" style="display: block; ">Data Encerramento Preparar Notifica <xsl:call-template name="datePrototype">
<xsl:with-param name="locale">
<xsl:value-of select="$locale"/>
</xsl:with-param>
</xsl:call-template>: </label>
<xsl:choose>
<xsl:when test="$use3PartDate='true'">
<xsl:element name="div">
<xsl:attribute name="id">control.DATAENCPREPNOT</xsl:attribute>
<xsl:attribute name="name">control.DATAENCPREPNOT</xsl:attribute>
<xsl:attribute name="class">date</xsl:attribute>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DATAENCPREPNOTdd</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datedd</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DATAENCPREPNOTmm</xsl:attribute>
<xsl:attribute name="size">1</xsl:attribute>
<xsl:attribute name="class">datemm</xsl:attribute>
</xsl:element>/<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DATAENCPREPNOTyyyy</xsl:attribute>
<xsl:attribute name="size">2</xsl:attribute>
<xsl:attribute name="class">dateyyyy</xsl:attribute>
</xsl:element>
</xsl:element>
</xsl:when>
<xsl:otherwise>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DATAENCPREPNOT</xsl:attribute>
<xsl:attribute name="maxlength"/>
<xsl:attribute name="size"/>
<xsl:attribute name="name">fieldDATAENCPREPNOT</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DATAENCPREPNOT']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='DATAENCPREPNOT']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
</xsl:otherwise>
</xsl:choose>
<input type="hidden" name="fieldDATAENCPREPNOTType" value="swDate"/>
</div>
    <div class="item">
<label for="control.IDAIIMORIGINAL" class="vert" style="display: block; ">idAiimOriginal: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.IDAIIMORIGINAL</xsl:attribute>
<xsl:attribute name="maxlength">12</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldIDAIIMORIGINAL</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDAIIMORIGINAL']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='IDAIIMORIGINAL']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldIDAIIMORIGINALType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.SW_PARENTPROC" class="vert" style="display: block; ">SW_PARENTPROC: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SW_PARENTPROC</xsl:attribute>
<xsl:attribute name="maxlength">8</xsl:attribute>
<xsl:attribute name="size">10</xsl:attribute>
<xsl:attribute name="name">fieldSW_PARENTPROC</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_PARENTPROC']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_PARENTPROC']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSW_PARENTPROCType" value="swText"/>
</div>
    <div class="item">
<label for="control.SW_CASENUM" class="vert" style="display: block; ">SW_CASENUM: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SW_CASENUM</xsl:attribute>
<xsl:attribute name="maxlength">16</xsl:attribute>
<xsl:attribute name="size">20</xsl:attribute>
<xsl:attribute name="name">fieldSW_CASENUM</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_CASENUM']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_CASENUM']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSW_CASENUMType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.SW_PARENTCASE" class="vert" style="display: block; ">SW_PARENTCASE: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SW_PARENTCASE</xsl:attribute>
<xsl:attribute name="maxlength">11</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldSW_PARENTCASE</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_PARENTCASE']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_PARENTCASE']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSW_PARENTCASEType" value="swNumeric"/>
</div>
    <div class="item">
<label for="control.SW_HOSTNAME" class="vert" style="display: block; ">SW_HOSTNAME: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SW_HOSTNAME</xsl:attribute>
<xsl:attribute name="maxlength">24</xsl:attribute>
<xsl:attribute name="size">25</xsl:attribute>
<xsl:attribute name="name">fieldSW_HOSTNAME</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="readonly">readonly</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_HOSTNAME']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='SW_HOSTNAME']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldSW_HOSTNAMEType" value="swText"/>
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
<label for="control.ISAPPERROR" class="vert" style="display: block; ">IsAppError: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.ISAPPERROR</xsl:attribute>
<xsl:attribute name="maxlength">1</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldISAPPERROR</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
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
<label for="control.SW_MAINPROC" class="vert" style="display: block; ">SW_MAINPROC: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SW_MAINPROC</xsl:attribute>
<xsl:attribute name="maxlength">8</xsl:attribute>
<xsl:attribute name="size">10</xsl:attribute>
<xsl:attribute name="name">fieldSW_MAINPROC</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
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
<label for="control.MAXRETRIES" class="vert" style="display: block; ">MaxRetries: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.MAXRETRIES</xsl:attribute>
<xsl:attribute name="maxlength">11</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldMAXRETRIES</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
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
<label for="control.ISTECHERROR" class="vert" style="display: block; ">IsTechError: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.ISTECHERROR</xsl:attribute>
<xsl:attribute name="maxlength">1</xsl:attribute>
<xsl:attribute name="size">5</xsl:attribute>
<xsl:attribute name="name">fieldISTECHERROR</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
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
<label for="control.STATUS_CODE" class="vert" style="display: block; ">STATUS_CODE: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.STATUS_CODE</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldSTATUS_CODE</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
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
<label for="control.DATETIME" class="vert" style="display: block; ">DATETIME: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.DATETIME</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldDATETIME</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
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
    <div class="item">
<label for="control.DUMP" class="vert" style="display: block; ">DUMP: </label>
<textarea id="control.DUMP" name="fieldDUMP" rows="10" cols="25">
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
<label for="control.STERRORDESC" class="vert" style="display: block; ">STErrorDesc: </label>
<textarea id="control.STERRORDESC" name="fieldSTERRORDESC" rows="10" cols="25">
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
<label for="control.NUMAPPRETRIES" class="vert" style="display: block; ">NumAppRetries: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.NUMAPPRETRIES</xsl:attribute>
<xsl:attribute name="maxlength">11</xsl:attribute>
<xsl:attribute name="size">15</xsl:attribute>
<xsl:attribute name="name">fieldNUMAPPRETRIES</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
<xsl:attribute name="value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NUMAPPRETRIES']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='NUMAPPRETRIES']/sso:Value"/>
</xsl:when>
<xsl:otherwise>
<xsl:text/>
</xsl:otherwise>
</xsl:choose>
</xsl:attribute>
</xsl:element>
<input type="hidden" name="fieldNUMAPPRETRIESType" value="swNumeric"/>
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
<label for="control.STERRORCODE" class="vert" style="display: block; ">STErrorCode: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.STERRORCODE</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldSTERRORCODE</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
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
<label for="control.SERVICE_NAME" class="vert" style="display: block; ">SERVICE_NAME: </label>
<xsl:element name="input">
<xsl:attribute name="type">text</xsl:attribute>
<xsl:attribute name="id">control.SERVICE_NAME</xsl:attribute>
<xsl:attribute name="maxlength">50</xsl:attribute>
<xsl:attribute name="size">40</xsl:attribute>
<xsl:attribute name="name">fieldSERVICE_NAME</xsl:attribute>
<xsl:attribute name="class">textControl</xsl:attribute>
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
<label for="control.PARTICIPANTE" class="vert" style="display: block; ">participante: </label>
<textarea id="control.PARTICIPANTE" name="fieldPARTICIPANTE" rows="10" cols="25">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='PARTICIPANTE']/sso:Value">
<xsl:choose>
<xsl:when test="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='PARTICIPANTE']/sso:Value">
<xsl:value-of select="//sso:vResult[@Id='LockItems']//sso:vField[sso:Name='PARTICIPANTE']/sso:Value"/>
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
<input type="hidden" name="fieldPARTICIPANTEType" value="swText"/>
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

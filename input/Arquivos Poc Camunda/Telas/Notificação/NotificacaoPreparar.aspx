<%@ Page Language="C#" MasterPageFile="~/Internet.Master" AutoEventWireup="true" CodeBehind="NotificacaoPreparar.aspx.cs" Inherits="Br.Gov.Sp.Fazenda.ePAT.WebApp.WebPages.PAT.PrimeiraInstancia.NotificacaoPreparar" ValidateRequest="false" MaintainScrollPositionOnPostback="true" %>

<%@ Register Src="../../../Controls/Pecas.ascx" TagName="Pecas" TagPrefix="uc1" %>
<%@ Register Src="../../../Controls/ListaNotificaveis.ascx" TagName="Notificaveis" TagPrefix="uc3" %>
<%@ Register Src="../../../Controls/Cabecalho_AIIM_DEAT.ascx" TagName="Cabecalho_AIIM" TagPrefix="uc2" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TituloPagina" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Menu" runat="server"></asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ConteudoPagina" runat="server">

    <script src="../../../Scripts/Util.js" type="text/javascript"></script>
    
    <script type="text/javascript">

        function disableButton(_btn) {
            _btn.disabled = true;
        }

        function Cancelar(mensagem) {
            if (confirm(mensagem)) {
                document.getElementById("infoAguarde").style.visibility = "";
                var obj = document.getElementById("<%=undoHidden.ClientID%>");
                obj.click();
            }
        }

        function SalvarRascunho(mensagem) {
            if (confirm(mensagem)) {
                document.getElementById("infoAguarde").style.visibility = "";
                var obj = document.getElementById("<%=keepHidden.ClientID%>");
                obj.click();
            }
        }

        function FecharAposDesbloquear(mensagem) {
            alert(mensagem);
            closeWindow();
        }

        function FecharComMsgmErro(titulo, mensagem) {
            alert(titulo + "\n" + mensagem);
            closeWindow();
        }

        function alertaMensagem(mensagem) {
            alert(mensagem);
        }

        function fndDEC() {
            window.open("https://spointrades01/DEC");
        }

        function AIIMConsultarDocsJanela(AiimNumero) {
            var btn = document.getElementById("<%=btnIntegraDoc.ClientID%>");
            btn.disabled = true;
            var endPagina = 'AIIMConsultaDocs.aspx?AIIMNumero=' + AiimNumero;
            //window.open(endPagina, '', 'top=300,left=100,height=350,width=900,resizable=0,scrollbars=0,location=0,menubar=0,toolbar=0');
            var sFeatures = "dialogHeight:500px;dialogWidth:980px;resizable:1;scroll:1;status:0;edge:sunken ;dialogHide:0;center:yes";
            window.showModalDialog(endPagina, "", sFeatures);
            btn.disabled = false;
        }
    </script>
    
    <div id="todo">
        <div class="divContainerUCs" >
            <uc2:Cabecalho_AIIM ID="Cabecalho_AIIM" runat="server"  />
        </div>
        <div style="margin-left: 3px; text-align:center;width: 1000px;">
            <hr />
            <table style="width:100%;">
                <tr align="center">
                <td>
                 <table style="width: 125px; height: 60px; border-style:  outset">
                <tr><td style="height: 17px"><label  style=" font-weight:bold;">Decisão</label><br /></td></tr>
                <tr><td align="center" style="height: 81px" valign="top">
                    <asp:DropDownList ID="ddlDecisao" runat="server" 
                    onselectedindexchanged="ddlDecisao_SelectedIndexChanged" AutoPostBack="true" 
                        Font-Size="Small" Height="25px" Width="109px">
                    <asp:ListItem Text="Selecione..." Selected="True" Value="-1" ></asp:ListItem>
                    <asp:ListItem Text="Corrigir"  Value="0" ></asp:ListItem>
                    <asp:ListItem Text="Notificar"  Value="1" ></asp:ListItem>
                </asp:DropDownList></td></tr>
                </table>
                </td>
                </tr>
            </table>
            <table style="width:100%;" >
                <tr>
                    <td class="label">&nbsp;</td>
                    <td align="right">
                        <table style="border-color:Black;" border="1">
                            <tr>
                                <td><asp:Button ID="btnIntegraDoc" runat="server" Text="Exibir Integra do AIIM" OnClientClick="disableButton(this);" UseSubmitBehavior="false"   /></td>
                            </tr>
                        </table>
                    </td>
                    </tr>
            </table>
        </div>

        <div class="divContainerUC2" style="margin-left: 3px;width: 1000px;" >
            <hr />  

            <asp:Panel ID="Panel1" runat="server">
                <asp:GridView ID="gvLista" runat="server" CssClass="TABLE1" Style="width: 1000px;"        
                    EmptyDataText="Não foi encontrado Autuado/Solidário notificado"
                    DataKeyNames="meioNotificacao,credenciadoDEC,cnpjcpf" 
                                ondatabound="gvLista_DataBound" 
                        onrowcommand="gvLista_RowCommand" 
                        onrowcreated="gvLista_RowCreated"
                        OnRowDataBound="gvLista_RowDataBound"
                         AutoGenerateColumns="False">
                    <RowStyle CssClass="linha_grid" />
                    <HeaderStyle CssClass="cabecalho" />
                    <AlternatingRowStyle CssClass="linha_grid_alt" />
                    <Columns>
                        <asp:BoundField DataField="nomeAutuadoSolidario" HeaderText="Autuado / Solidário" ItemStyle-HorizontalAlign="Center"
                            ItemStyle-VerticalAlign="Middle">
                            <ItemStyle HorizontalAlign="left" VerticalAlign="Middle"></ItemStyle>
                        </asp:BoundField>
                        <asp:BoundField DataField="tipoNotificado" HeaderText="Tipo Notificado" ItemStyle-HorizontalAlign="Center"
                            ItemStyle-VerticalAlign="Middle">
                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle"></ItemStyle>
                        </asp:BoundField>
                        <asp:BoundField DataField="credenciadoDEC"  HeaderText="Habilitado no DEC?" ItemStyle-HorizontalAlign="Center"
                            ItemStyle-VerticalAlign="Middle">
                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle"></ItemStyle>
                        </asp:BoundField>            
                        <asp:BoundField DataField="credenciadoEPAT" HeaderText="Credenciado no ePAT?" ItemStyle-HorizontalAlign="Center"
                            ItemStyle-VerticalAlign="Middle">
                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle"></ItemStyle>
                        </asp:BoundField>              
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:Label ID="lblHeader" runat="server" Text="Meio de Notificação" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:RadioButton ID="rdoMeioNotificacaoDEC" Enabled="false" GroupName="tipoNotificacao" Checked="false" Text="DEC" runat="server" AutoPostBack="true" />
                                <asp:RadioButton ID="rdoMeioNotificacaoPessoal" Enabled="false" GroupName="tipoNotificacao" Checked="false" Text="Outros"  runat="server" AutoPostBack="true" />
                                <asp:Button ID="btnAssinarXml" Enabled="true" Text="Assinar" runat="server" CommandName="cmdAssinarXml" CommandArgument="<%# ((GridViewRow) Container).RowIndex %>" />                                
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center"/>
                        </asp:TemplateField>
                    </Columns>
                     <EmptyDataTemplate>
                        <table class="TABLE1" cellspacing="0" rules="cols" border="1" id="tabela" style="width: 100%;
                            border-collapse: collapse;">
                            <tr class="cabecalho">
                                <th scope="col">
                        
                                </th>
                                <th scope="col">
                                    Autuado / Solidário
                                </th>
                                <th scope="col">
                                    Tipo Notificado
                                </th>
                                <th scope="col">
                                    Habilitado no DEC?
                                </th>
                                <th scope="col">
                                    Credenciado no EPAT?
                                </th>  
                                <th scope="col">
                                    Meio de Notificação
                                </th>                                       
                            </tr>
                            <tr>
                                <td colspan="4">
                                    <b>Não foram encontrados autuados/solidários.</b>
                                </td>
                            </tr>
                        </table>
                    </EmptyDataTemplate>
                </asp:GridView>
            </asp:Panel>
        </div>
        
        <div class="divContainerUCs">
        <hr />
            <span class="subTituloPagina">Observações</span>
            <asp:UpdatePanel ID="UpdatePaneOBS" runat="server" UpdateMode="Conditional" RenderMode="Inline">
                <ContentTemplate>
                    <asp:Panel ID="pnlOBS" runat="server" Width="870px" Font-Bold="True">
                        <asp:GridView ID="gvObsrvc" runat="server" CssClass="tabela" AutoGenerateColumns="false"
                            AllowPaging="True" PageSize="5" OnPageIndexChanging="gvObsrvc_PageIndexChanging"
                            OnRowDataBound="gvObsrvc_RowDataBound" Width="996px">
                            <RowStyle CssClass="linha_grid_larger" />
                            <HeaderStyle CssClass="cabecalho" />
                            <AlternatingRowStyle CssClass="linha_grid_larger_alt" />
                            <Columns>
                                <asp:BoundField DataField="data" HeaderText="Data" HtmlEncode="False" ItemStyle-Width="140px">
                                    <ItemStyle Width="140px" />
                                </asp:BoundField>
                                <asp:BoundField DataField="descricaoEtapa" HeaderText="Etapa" HtmlEncode="False"
                                    ItemStyle-Width="150px">
                                    <ItemStyle Width="150px" />
                                </asp:BoundField>
                                <asp:BoundField DataField="descricaoUsuario" HeaderText="Usuário" HtmlEncode="False"
                                    ItemStyle-Width="130px">
                                    <ItemStyle Width="130px" />
                                </asp:BoundField>
                                <asp:TemplateField HeaderText="Observação">
                                    <ItemTemplate>
                                        <asp:Label ID="lblObservacao" ToolTip='<%# Bind("observacao2") %>' runat="server" Text='<%# Bind("observacao1") %>'></asp:Label>
                                    </ItemTemplate>
                                    <ItemStyle Width="545px" Wrap="True" />
                                </asp:TemplateField>
                                <asp:BoundField DataField="observacao2" HeaderText="observacaoOculta" Visible="false"
                                    HtmlEncode="False" ItemStyle-Width="545px" ItemStyle-Wrap="true">
                                    <ItemStyle Width="545px" Wrap="True" />
                                </asp:BoundField>
                            </Columns>
                            <EmptyDataTemplate>
                                <table class="TABLE1" cellspacing="0" rules="cols" border="1" id="tabela" style="width: 100%;
                                    border-collapse: collapse;">
                                    <tr class="cabecalho">
                                        <th scope="col" style="display:"" ; width: 91px; visibility: " align="center">
                                            Data
                                        </th>
                                        <th scope="col" style="display:"" ; width: 103px; visibility: " class="style8" align="center">
                                            Etapa
                                        </th>
                                        <th scope="col" style="display:"" ; width: 110px; visibility: " align="center">
                                            Usuário
                                        </th>
                                        <th scope="col" style="width: 545px;" align="center">
                                            Observação
                                        </th>
                                    </tr>
                                    <tr>
                                        <td colspan="5">
                                            <b>Não há registros.</b>
                                        </td>
                                    </tr>
                                </table>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
            <br />
            <div>
                <asp:TextBox ID="ftbObsrvc" runat="server" TextMode="MultiLine" Rows="6" Height="101px" Width="996px" Font-Size="12px" />
            </div>
            <div align="center">
                <asp:Button ID="undoHidden" runat="server" Text="undoHidden" CausesValidation="False"  Style="position: static; display: none" OnClick="undoHidden_Click" />
                <asp:Button ID="btnUndo" runat="server" Text="Cancelar" OnClick="btnUndo_Click"  CausesValidation="False" />&nbsp;
                <asp:Button ID="keepHidden" runat="server" Text="keepHidden" CausesValidation="False"  Style="position: static; display: none" OnClick="keepHidden_Click" />
                <asp:Button ID="btnKeep" runat="server" Text="Salvar Rascunho" OnClick="btnKeep_Click" CausesValidation="False" Visible="false" Enabled="false" />&nbsp;
                <asp:Button ID="btnIniciarCorrecao" Enabled="false" runat="server" Text="Iniciar Correção" OnClick="btnIniciarCorrecao_Click" CausesValidation="False" />&nbsp;
                <asp:Button ID="btnRelease" Enabled="false" runat="server" Text="Iniciar Notificação" OnClick="btnRelease_Click" />
            </div>
        </div>
    </div>
    

    <!-- ===================== -->
    <!-- Controle de progresso -->
    <!-- ===================== -->
    <div id="infoAguarde" style="visibility: hidden">
        <table style="width:100%">
            <tr style="text-align: center; vertical-align: middle;">
                <td style="text-align: center; vertical-align: middle; height: 100%; font-family: Verdana;
                    font-size: xx-large; font-weight: bold;">
                    <br />
                    <br />
                    <br />
                    <br />
                    <br />
                    <br />
                    <br />
                    Processando... 
                    <br />
                    Por favor, aguarde!
                    <br />
                    <br />
                    <br />
                    <img alt="Processando" src="../../../images/loading.gif" />
                    <br />
                    <br />
                    <br />
                    <br />
                    <br />
                </td>
            </tr>
        </table>
    </div>

    <asp:Panel ID="pnlPorFavorAguarde" runat="server" Visible="False">
        <div style="text-align:center;font-size:large;font-weight:bold">
            Por favor aguarde...
        </div>
    </asp:Panel>

</asp:Content>

<%@ Page Title="" Language="C#" MasterPageFile="~/Internet.Master" AutoEventWireup="true"
    CodeBehind="FechamentoAIIM.aspx.cs" Inherits="Fazenda.ePAT.WebApp.WebPages.PAT.PrimeiraInstancia.FechamentoAIIM"
    ValidateRequest="false" %>

<%@ Register Src="../../../Controls/Pecas.ascx" TagName="Pecas" TagPrefix="uc1" %>
<%@ Register Src="../../../Controls/Cabecalho_AIIM.ascx" TagName="Cabecalho_AIIM"
    TagPrefix="uc2" %>
<%@ Register Src="../../../Controls/AdicionarPecas.ascx" TagName="AdicionarPecas"
    TagPrefix="uc3" %>
<%@ Register Src="../../../Controls/Cabecalho_AIIM_DEAT.ascx" TagName="Cabecalho_AIIM_DEAT"
    TagPrefix="uc4" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TituloPagina" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Menu" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ConteudoPagina" runat="server">
    <script language="javascript" type="text/javascript">

        function Cancelar(mensagem) {
            if (confirm(mensagem)) {
                //document.getElementById("infoAguarde").style.visibility = "";
                statusProcessando(true);
                var obj = document.getElementById("<%=undoHidden.ClientID%>");
                obj.click();
            }
        }

        function SalvarRascunho(mensagem) {
            if (confirm(mensagem)) {
                //document.getElementById("infoAguarde").style.visibility = "";
                statusProcessando(true);
                var obj = document.getElementById("<%=keepHidden.ClientID%>");
                obj.click();
            }
        }

        function DesbloquearAIIM(mensagem) {
            if (confirm(mensagem)) {
                //document.getElementById("infoAguarde").style.visibility = "";
                statusProcessando(true);
                var obj = document.getElementById("<%=DesbloquearHidden.ClientID%>");
                obj.click();
            }
        }

        function RecarregarAIIM(mensagem) {
            if (confirm(mensagem)) {
                //document.getElementById("infoAguarde").style.visibility = "";
                statusProcessando(true);
                var obj = document.getElementById("<%=RecarregarHidden.ClientID%>");
                obj.click();
            }
        }

        

        function FecharAposDesbloquear(mensagem) {

            alert(mensagem);

            closeWindow();

        }


        function AIIMConsultarDocsJanela(AiimNumero) {

            var btn = document.getElementById("<%=btnIntegraDoc.ClientID%>");
            btn.disabled = true;
            var endPagina = 'AIIMConsultaDocs.aspx?AIIMNumero=' + AiimNumero;
            statusProcessando(true);

            window.open(endPagina, '', 'top=300,left=100,height=350,width=900,resizable=0,scrollbars=0,location=0,menubar=0,toolbar=0');
            //var sFeatures = "dialogHeight:500px;dialogWidth:980px;resizable:1;scroll:1;status:0;edge:sunken ;dialogHide:0;center:yes";
            //window.showModalDialog(endPagina, "", sFeatures);
            btn.disabled = false;

        }

    </script>
    <div id="todo">
        <div class="divContainerUCs">
            <uc4:Cabecalho_AIIM_DEAT ID="Cabecalho_AIIM_DEAT1" runat="server" />
            <asp:Button ID="btnDesbloquear" runat="server" Text="Desbloquear" OnClientClick="this.disabled = true;"
                UseSubmitBehavior="false" OnClick="btnDesbloquear_Click" />&nbsp;
            <asp:Button ID="btnRecarregar" runat="server" Text="Recarregar" OnClientClick="this.disabled = true;"
                UseSubmitBehavior="false" OnClick="btnRecarregar_Click" />
        </div>

        <div align="left" style="width:997px">
            <table style="width:996px;">
                <tr>
                    <td style="text-align:left; padding-right:10px;">
                        <asp:Button ID="btnIntegraDoc" runat="server" Text="Exibir Íntegra do AIIM" Width="150px"   />
                        &nbsp;<asp:Button ID="btnOrdenarPaginas" runat="server" Text="Ordenar Paginas" 
                    OnClick="btnOrdenarPaginas_Click" Width="129px" Visible="False"/>
                &nbsp;<asp:Button ID="btnRenumrarPagina" runat="server" Text="Renumera Paginas" 
                    OnClick="btnRenumrarPagina_Click" Width="122px" Visible="False"/>
                        </td>
                    </tr>
            </table>
        </div>

        <div class="divContainerUCs">
            <span class="subTituloPagina">Anexar Documentos</span>
            <table border="0" width="100%">
                <tr>
                    <td>
                        Documento:
                    </td>
                    <td>
                        <div class="fileinputs">
                            <asp:FileUpload ID="uploadClient" runat="server" class="file" Width="312px" /></div>
                    </td>
                    <td class="label">
                        Tipo:
                    </td>
                    <td>
                        <asp:DropDownList ID="ddlTpDocmnt" runat="server" Width="400px">
                        </asp:DropDownList>
                    </td>
                    <td>
                        <asp:Button ID="btnInserir" runat="server" Text="Inserir" OnClientClick="this.disabled = true;"
                            UseSubmitBehavior="false" OnClick="btnInserirArquivoHidden_Click"  />
                    </td>
                </tr>
            </table>
        </div>

        <div align="left" style="width:997px">
            <table style="width:996px;">
                <tr><td><span class="subTituloPagina">Documentos</span></td></tr>
                <tr>
                    <td>
                        <div class="divContainerUCs">
                            <uc3:AdicionarPecas ID="AdicionarPecas" runat="server" />
                        </div>
                    </td>
                </tr>
            </table>
        </div>

        <div class="divContainerUCs">
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
                <asp:TextBox ID="ftbObsrvc" runat="server" TextMode="MultiLine" Rows="6" Height="101px"
                    Width="996px" Font-Size="12px" />
            </div>
            <div align="center">
                <asp:Button ID="undoHidden" runat="server" Text="undoHidden" CausesValidation="False"
                    Style="position: static; display: none" OnClick="undoHidden_Click" />
                <asp:Button ID="keepHidden" runat="server" Text="keepHidden" CausesValidation="False"
                    Style="position: static; display: none" OnClick="keepHidden_Click" />
                <asp:Button ID="DesbloquearHidden" runat="server" Text="DesbloquearHidden" 
                    CausesValidation="False" Style="position: static; display: none" OnClick="DesbloquearHidden_Click" />
                <asp:Button ID="RecarregarHidden" runat="server" Text="RecarregarHidden" 
                    CausesValidation="False" Style="position: static; display: none" OnClick="RecarregarHidden_Click" />
                <asp:Button ID="btnUndo" runat="server" Text="Cancelar" OnClick="btnUndo_Click"
                    CausesValidation="False" Width="100px"/>&nbsp;
                <asp:Button ID="btnKeep" runat="server" Text="Salvar Rascunho" OnClick="btnKeep_Click"
                    CausesValidation="False" Width="100px"/>&nbsp;
                <asp:Button ID="btnRelease" runat="server" Text="Finalizar AIIM" OnClick="btnRelease_Click" Width="100px"/>
            </div>
        </div>
    </div>
    <!-- ===================== -->
    <!-- Controle de progresso -->
    <!-- ===================== -->
    <div id="infoAguarde" style="visibility: hidden">
        <table width="100%">
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
                    Processando...<br />
                    <br />
                    <img alt="Processando" src="../../../images/loading.gif" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>

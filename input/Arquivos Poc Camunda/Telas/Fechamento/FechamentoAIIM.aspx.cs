using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Br.Gov.Sp.Fazenda.ePAT.Facade.Comum ;
using Br.Gov.Sp.Fazenda.ePAT.Entities;
using Br.Gov.Sp.Fazenda.ePAT.Facade;
using Br.Gov.Sp.Fazenda.ePAT.Entities.Comum;
using System.Xml;
using Fazenda.ePAT.Entities.TIBCO.General;
using wrkItem = Fazenda.ePAT.Entities.TIBCO.WorkItem;
using System.Data;
using Fazenda.ePAT.Entities.TIBCO;


namespace Fazenda.ePAT.WebApp.WebPages.PAT.PrimeiraInstancia
{
    public partial class FechamentoAIIM : System.Web.UI.Page
    {

        AiimEntity aiim = new AiimEntity();
        enum PROBLEMA_ARQUIVO { FORMATO, NOME_UNICO, TIPO_UNICO, TAMANHO, TIPO_INDEFINIDO, ARQUIVO_INDEFINIDO, OK, ARQUIVO_SEM_CABECALHO, ARQUIVO_COM_CRIPTOGRAFIA };
        private List<string> listaErrosPerfilUsuario = new List<string>();

        public void HabilitaEnviar(bool b)
        {
            //if (btnDesbloquear.Enabled == false && b == true)
            //{ btnRelease.Enabled = false; }
            //else
            //{
            //    btnRelease.Enabled = b;
            //}
        }
        public void HabilitaInserir(bool b)
        {
            btnInserir.Enabled = b;
        }

        //objeto usado para repassar os documentos não paginados quando usuario apetar botão Renumerar Paginas.
        private List<DocNaoPaginadoEntity> listaDocsNaoPaginados = new List<DocNaoPaginadoEntity>();
        //Variavel usada para analisar retorno da lista  listaDocsNaoPaginados  carreagada dentro do metodo:
        //ExistemPaginasSendoCalculadas
        private bool DocsNaoPaginados;

        protected void Page_Load(object sender, EventArgs e)
        {
            Label mlblErro = (Label)Master.FindControl("lblErro");
            if (mlblErro.Text != "Botão &quot;Finalizar AIIM&quot; bloqueado.<br>Há documentos neste AIIM com o número de páginas sendo calculado.<br>Por favor atualize a página.<br>Se o problema persistir, verifique quais documentos estão com número de páginas zero ou &quot;calculando&quot; e os substitua.")
                mlblErro.Text = string.Empty;
            try
            {
                if (!Page.IsPostBack)
                {
                    //throw new ArgumentException("teste de erro.");

                    ((Label)this.Master.FindControl("nomeModulo")).Text = "Fechamento AIIM";
                    ((Label)this.Master.FindControl("lblSubprocesso")).Text = "Finalizar AIIM";
                    Load_WorkItem();
                    buscarDadosCabecAIIM();
                    Load_ListaTipoDocumentos();
                    buscaObservacoes();


                    //Ativando "carregando" para as seguintes ações da página
                    btnRenumrarPagina.Attributes.Add("onclick", "statusProcessando(true)");
                    btnOrdenarPaginas.Attributes.Add("onclick", "statusProcessando(true)");
                    var url = Request.Url.AbsoluteUri;
                    url = url.Substring(0, url.LastIndexOf('/')) + "/AIIMConsultaDocs.aspx?AIIMNumero=" + Cabecalho_AIIM_DEAT1.NrAiim.Substring(0, Cabecalho_AIIM_DEAT1.NrAiim.IndexOf("-")).Replace(".", "");

                    //Ativando "carregando" para as seguintes ações da página                        
                    btnIntegraDoc.Attributes.Add("onclick", "statusProcessando(true);" + "window.open('" + url + "', '_blank', 'titlebar=yes,toolbar=yes,location=yes,status=no,menubar=yes,scrollbars=no,resizable=yes,width=300px,Height=auto,left=300,top=100');");

                    btnInserir.Attributes.Add("onclick", "statusProcessando(true)");





                    string usuario = IdentificaUsuarioLogado();
                    if (PermissaoVerificar(usuario, 99))
                    {
                        btnOrdenarPaginas.Visible = true;
                        btnRenumrarPagina.Visible = true;
                    }

                    FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];

                    if (fieldsIProcess["SITUACAOCARREGA"].Value != null && fieldsIProcess["SITUACAOCARREGA"].Value == "A")
                    {   //aiim desbloqueado no aiimWeb.Desabilitar botoes cancelar, salvar Rascunho e Finalizar AIIM

                        btnKeep.Enabled = false;
                        btnInserir.Enabled = false;
                        uploadClient.Enabled = false;
                        ddlTpDocmnt.Enabled = false;

                        btnDesbloquear.Enabled = false;
                        btnRecarregar.Enabled = true;
                    }
                    else
                    {   //aiim bloqueado no aiimWeb desabilitar botao Recarregar.
                        btnDesbloquear.Enabled = true;
                        btnRecarregar.Enabled = false;
                        if (fieldsIProcess["FORMACORRECAO"].Value == "RETIRRATI")
                        {
                            btnDesbloquear.Enabled = false;
                            btnRecarregar.Enabled = true;
                        }
                    }

                    //if (new AIIM_Facade().existeVersaoParaCarregar(usuario, Cabecalho_AIIM_DEAT1.NrAiim.ToString()))
                    //{
                    //    btnDesbloquear.Enabled = false;
                    //    btnRecarregar.Enabled = true;
                    //}
                    

                    if (ExistemPaginasSendoCalculadas(long.Parse(fieldsIProcess["IDAIIM"].Value))) {
                        btnRelease.Enabled = false;
                        mlblErro.Text = "Botão &quot;Finalizar AIIM&quot; bloqueado.<br>Há documentos neste AIIM com o número de páginas sendo calculado.<br>Por favor atualize a página.<br>Se o problema persistir, verifique quais documentos estão com número de páginas zero ou &quot;calculando&quot; e os substitua.";
                    } else {
                        btnRelease.Enabled = true;
                        mlblErro.Text = string.Empty;
                    }
                }
                if (!string.IsNullOrEmpty((string)Session["Message"]))
                {
                    string mensagem;

                    mensagem = Session["Message"].ToString();
                    MostrarMensagemOrdenarRenumerar(mensagem);
                    Session.Remove("Message");

                }
                AdicionarPecas.HabilitarDelegate = this.HabilitaEnviar;

                FieldColletion<string, WorkItemLockField> fieldsIProcess2 = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];
                ExistemPaginasSendoCalculadas(long.Parse(fieldsIProcess2["IDAIIM"].Value));
            }
            catch (Exception ex)
            {

                mlblErro.Text = ex.Message;
                string msg = ex.Message.Replace('\n','|');
                //Response.Redirect("errPagina.aspx?errDescricao=Problemas para carregar página. Entre em contato com administrador do sistema.&errDetalhe=" + msg  ); 


            }
        }

        private bool PermissaoVerificar(string UsuarioLogin, long permissaoNumero)
        {
            List<long> listaPerfis = new List<long>(1); listaPerfis.Add(permissaoNumero);
            return new UsuarioDistribuicaoFacade().BuscarListaPerfisUsuario(listaPerfis, UsuarioLogin, true).ListaAreasUsuario.Count() > 0;
        }

 

        private string IdentificaUsuarioLogado()
        {
            if (Request["ffp"] != null)
            {
                Session["ffp"] = Request["ffp"];
            }

            if (Session["ffp"] != null)
            {
                string ffpTag = Session["ffp"].ToString();

                XmlDocument document = new XmlDocument();
                document.LoadXml(ffpTag);

                string usuario = document.SelectSingleNode("Params/NodeCtx/UserName").InnerText;
                return usuario;
            }
            else
            {
                listaErrosPerfilUsuario.Add("Falha na comunicação com o iProcess: Dados do usuário não informados.");
                return null;
            }
        }



        private bool ExistemPaginasSendoCalculadas(long idAiim)
        {
            bool retorno;
            string transactionID = System.Configuration.ConfigurationManager.AppSettings["NomeSistema"] + Page.Session.SessionID.ToString();
            //List<DocNaoPaginadoEntity> listaDocsNaoPaginados = new List<DocNaoPaginadoEntity>();
            DocPaginadoEntity docPaginado = new DocPaginadoEntity();

            new DocPaginadoFacade().BuscarListaArquivos(transactionID, idAiim, out docPaginado, out listaDocsNaoPaginados);

            retorno = listaDocsNaoPaginados.Exists(docnaopaginado => docnaopaginado.NroPaginas == 0);

            DocsNaoPaginados = retorno;

            return retorno;
            // return listaDocsNaoPaginados.Exists(docnaopaginado => docnaopaginado.NroPaginas == 0);
        }

        protected void btnDesbloquear_Click(object sender, EventArgs e)
        {
            try
            {
                if (VerificarSessaoAtiva())
                {
                    return;
                }

                FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];
                long idAIIM = long.Parse(fieldsIProcess["IDAIIM"].Value);
                ClientScript.RegisterStartupScript(GetType(), "Desbloqueio AIIM", "DesbloquearAIIM('O desbloqueio permitirá que o AIIM " + Cabecalho_AIIM_DEAT1.NrAiim.ToString() + " seja cancelado e retransmitido no AiimWeb. Confirma esta operação?');", true);
            }
            catch (Exception ex)
            {
                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }

        }

        protected virtual void MostrarMensagemOrdenarRenumerar(string message)
        {
            ClientScript.RegisterStartupScript(
                                  this.GetType(),
                                  Guid.NewGuid().ToString(),
                                  string.Format("alert('{0}');", message),
                                  true
                              );
        }



        protected void DesbloquearHidden_Click(object sender, EventArgs e)
        {
            Label mlblErro = (Label)Master.FindControl("lblErro");
            try
            {

                IProcess iProcess = new IProcess();
                WorkItem wrkI = (WorkItem)Session["WorkItem"];
                iProcess.getSessionStatus(wrkI);

                btnUndo.Enabled = false;
                btnKeep.Enabled = false;
                btnRelease.Enabled = false;

                FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];

                long idProcesso = long.Parse(fieldsIProcess["IDAIIM"].Value);
                string transactionID = fieldsIProcess["SW_CASENUM"].Value; //somente nesta situação (fechamento de AIIM, transactionID = caseNUm)
                AIIM_Facade facade = new AIIM_Facade();
                XmlDocument oXmlFFP = new XmlDocument();
                oXmlFFP.LoadXml(Session["ffp"].ToString());

                wrkItem.FormFlowParameters oFFP = iProcess.getFFPWorkItem(oXmlFFP);
                
                bool retorno = facade.Reabertura(idProcesso, transactionID, oFFP.NodeCtx.UserName, this.Page.Session.SessionID);

                wrkI = (WorkItem)Session["WorkItem"];
                FieldColletion<string, WorkItemKeepField> fieldsIProcessKeep = new FieldColletion<string, WorkItemKeepField>();

                fieldsIProcessKeep.Add("SITUACAOCARREGA",new WorkItemKeepField("SITUACAOCARREGA","swText","A"));

                fndSalvarArquivos();
                inserirObservacao();
                iProcess.keepWorkItem(wrkI, fieldsIProcessKeep);
                destruirSession();

                ClientScript.RegisterStartupScript(GetType(), "Fechar", "FecharAposDesbloquear('Este AIIM foi desbloqueado no AiimWeb. Para recarrega-lo, abra a atividade AIIM: " + Cabecalho_AIIM_DEAT1.NrAiim + " em sua fila de trabalho.');", true);

            }
            catch (Exception ex)
            {
                btnUndo.Enabled = true;
                mlblErro.Text = ex.Message;
                string msg = ex.Message.Replace('\n', '|');
            }
        }

        protected void btnRecarregar_Click(object sender, EventArgs e)
        {
            Label mlblErro = (Label)Master.FindControl("lblErro");

            try
            {

                if (VerificarSessaoAtiva())
                {
                    return;
                }

                FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];
                long idAIIM = long.Parse(fieldsIProcess["IDAIIM"].Value);
                ClientScript.RegisterStartupScript(GetType(), "Recarregar AIIM", "RecarregarAIIM('Confirma a recarga do AIIM " + Cabecalho_AIIM_DEAT1.NrAiim.ToString() + " ?');", true);

            }
            catch (Exception ex)
            {
                mlblErro.Text = ex.Message;
            }

        }

        protected void RecarregarHidden_Click(object sender, EventArgs e)
        {
            Label mlblErro = (Label)Master.FindControl("lblErro");
            try
            {

                IProcess iProcess = new IProcess();
                WorkItem wrkI = (WorkItem)Session["WorkItem"];
                iProcess.getSessionStatus(wrkI);

                FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];

                long idProcesso = long.Parse(fieldsIProcess["IDAIIM"].Value);
                string transactionID = fieldsIProcess["SW_CASENUM"].Value; //somente nesta situação (fechamento de AIIM, transactionID = caseNUm)
                AIIM_Facade facade = new AIIM_Facade();
                XmlDocument oXmlFFP = new XmlDocument();
                oXmlFFP.LoadXml(Session["ffp"].ToString());

                wrkItem.FormFlowParameters oFFP = iProcess.getFFPWorkItem(oXmlFFP);

                wrkI = (WorkItem)Session["WorkItem"];

                //AiimCabecalhoFacade aiimCabecFacade = new AiimCabecalhoFacade();
                //AiimEntity oAiim = aiimCabecFacade.RetornarAiim(idProcesso,transactionID);
                AiimEntity oAiim = new AiimEntity();
                oAiim.IdAIIM = idProcesso;
                string nrAiimTela = Cabecalho_AIIM_DEAT1.NrAiim;
                oAiim.NumeroAiimDV = nrAiimTela.Substring(nrAiimTela.IndexOf('-') + 1);
                oAiim.NumeroAIIM = nrAiimTela.Replace(".", "").Substring(0, nrAiimTela.IndexOf('-') - 2);
                //if (Cabecalho_AIIM_DEAT1.VersaoAIIM=="Original") {oAiim.VersaoAIIM ="0";} else {oAiim.VersaoAIIM =Cabecalho_AIIM_DEAT1.VersaoAIIM;}

                string tipoCorrecao = "CORRECAO";

                string[] retorno = facade.Recarregar(oAiim, wrkI, transactionID,tipoCorrecao);

                FieldColletion<string, WorkItemKeepField> keepFields = new FieldColletion<string, WorkItemKeepField>();
                WorkItemKeepField keepField1 = new WorkItemKeepField("IDAIIM","swText",retorno[0]);
                WorkItemKeepField keepField2 = new WorkItemKeepField("SITUACAOCARREGA","swText",retorno[1]);
                keepFields.Add("IDAIIM",keepField1);
                keepFields.Add("SITUACAOCARREGA",keepField2);

                iProcess.keepWorkItem(wrkI, keepFields);


                //FieldColletion<string, WorkItemLockField> fieldsIProcessLock = new FieldColletion<string, WorkItemLockField>();

                //fieldsIProcessLock.Add("SITUACAOCARREGA", new WorkItemLockField("SITUACAOCARREGA", "swText", ""));


                inserirObservacao();

                destruirSession();

                ClientScript.RegisterStartupScript(GetType(), "Fechar", "FecharComMensagem('Este AIIM foi recarregado do AiimWeb com sucesso.');", true);



            }
            catch (Exception ex)
            {
                mlblErro.Text = ex.Message;
                string msg = ex.Message.Replace('\n', '|');
            }
        }
        /// <summary>
        /// Processa o caso (Keep ou Release)
        /// </summary>
        /// <param name="tipoAcao">TRUE indica Release, FALSE indica Keep</param>
        /// <param name="caseDescription">Descrição do caso, caso não haja, pode ser passado um String.Empty</param>
        protected void processarCaso(string tipoAcao)
        {
            try
            {

                //SalvarFieldsSaida();

                IProcess iProcess = new IProcess();
                WorkItem wrkI = (WorkItem)Session["WorkItem"];


                switch (tipoAcao)
                {

                    case "keep":
                        iProcess.keepWorkItem(wrkI);
                        break;
                    case "release":
                        iProcess.releaseWorkItem(wrkI);
                        break;
                    case "undo":
                        iProcess.undoWorkItem(wrkI);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }
        }

        /// <summary>
        /// inseri a observação
        /// </summary>
        private Boolean inserirObservacao()
        {

            bool retorno = false;
            try
            {
                //--\/-- Verifica se a sessao nao esta ativa...
                IProcess oIProcess = new IProcess();
                WorkItem wrkItem = (WorkItem)Session["WorkItem"];
                oIProcess.getSessionStatus(wrkItem);
                //--/\-- Verifica se a sessao nao esta ativa...

                FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];
                AIIM_Facade facade = new AIIM_Facade();

                long idProcesso = long.Parse(fieldsIProcess["IDAIIM"].Value);
                string transactionID = fieldsIProcess["SW_CASENUM"].Value; //somente nesta situação (fechamento de AIIM, transactionID = caseNUm)
                WorkItem wrkI = (WorkItem)Session["WorkItem"];


                String nomeEtapa = wrkI.WorkItemTag.StepName;
                String nomeUsuario = wrkI.NodeCtx.UserName;
                String nomeProcesso = wrkI.WorkItemTag.ProcedureName;

                retorno = facade.InserirObservacoes(idProcesso, transactionID, nomeEtapa, nomeProcesso, nomeUsuario, ftbObsrvc.Text);

            }
            catch (Exception ex)
            {
                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }

            return retorno;
        }

        /// <summary>
        /// Metodo que renumera as paginas caso haja paginas não enumeradas.
        /// </summary>
        /// <param name="listaNaoPaginados"></param>
        private void RenumeraPaginas(List<DocNaoPaginadoEntity> listaNaoPaginados)
        {

            try
            {
                //--\/-- Verifica se a sessao nao esta ativa...
                IProcess oIProcess = new IProcess();
                WorkItem wrkItem = (WorkItem)Session["WorkItem"];
                oIProcess.getSessionStatus(wrkItem);
                //--/\-- Verifica se a sessao nao esta ativa... 
                FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];

                long idProcesso = long.Parse(fieldsIProcess["IDAIIM"].Value);
                string transactionID = fieldsIProcess["SW_CASENUM"].Value; //somente nesta situação (fechamento de AIIM, transactionID = caseNUm)
                AIIM_Facade facade = new AIIM_Facade();

                bool retorno = facade.RenumeraArquivos(idProcesso, transactionID, transactionID, this.Page.Session.SessionID, listaNaoPaginados);

            }
            catch (Exception ex)
            {

                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }


        }

        /// <summary>
        /// Método Usado para ordenar documentos
        /// Obs: Mesma rotina de salvar: requisitado pela Task: 237779
        /// </summary>
        private void OrdenaPaginas()
        {

            try
            {
                //--\/-- Verifica se a sessao nao esta ativa...
                IProcess oIProcess = new IProcess();
                WorkItem wrkItem = (WorkItem)Session["WorkItem"];
                oIProcess.getSessionStatus(wrkItem);
                //--/\-- Verifica se a sessao nao esta ativa... 
                FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];

                long idProcesso = long.Parse(fieldsIProcess["IDAIIM"].Value);
                string transactionID = fieldsIProcess["SW_CASENUM"].Value; //somente nesta situação (fechamento de AIIM, transactionID = caseNUm)
                AIIM_Facade facade = new AIIM_Facade();

                List<DocumentoAIIMEntity> listaDocumentos = (List<DocumentoAIIMEntity>)Session["lista"];

                bool retorno = facade.SalvarEordenarArquivos(idProcesso, transactionID, transactionID, this.Page.Session.SessionID, listaDocumentos);

            }
            catch (Exception ex)
            {

                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }


        }


        private void fndSalvarArquivos()
        {

            try
            {
                //--\/-- Verifica se a sessao nao esta ativa...
                IProcess oIProcess = new IProcess();
                WorkItem wrkItem = (WorkItem)Session["WorkItem"];
                oIProcess.getSessionStatus(wrkItem);
                //--/\-- Verifica se a sessao nao esta ativa... 
                FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];

                long idProcesso = long.Parse(fieldsIProcess["IDAIIM"].Value);
                string transactionID = fieldsIProcess["SW_CASENUM"].Value; //somente nesta situação (fechamento de AIIM, transactionID = caseNUm)
                AIIM_Facade facade = new AIIM_Facade();

                List<DocumentoAIIMEntity> listaDocumentos = (List<DocumentoAIIMEntity>)Session["lista"];

                bool retorno = facade.SalvarEordenarArquivos(idProcesso, transactionID,transactionID, this.Page.Session.SessionID, listaDocumentos);

            }
            catch (Exception ex)
            {

                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }

        }

        /// <summary>
        /// Carrega a Lista de Observações com page index
        /// </summary>
        /// <param name="index">index da pagina</param>
        private void buscaObservacoes(int index)
        {
            try
            {
                buscaObservacoes();
                gvObsrvc.PageIndex = index;
                gvObsrvc.DataBind();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Carrega a Lista de Observações 
        /// </summary>


        private void buscaObservacoes()
        {
            try
            {
                FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];

                long idProcesso = long.Parse(fieldsIProcess["IDAIIM"].Value);
                string transactionID = fieldsIProcess["SW_CASENUM"].Value; //somente nesta situação (fechamento de AIIM, transactionID = caseNUm)

                AIIM_Facade facade = new AIIM_Facade();

                DataTable dt = facade.BuscarObservacoes(idProcesso, transactionID);

                if (dt.Rows.Count > 0)
                {
                    gvObsrvc.DataSource = dt;
                    gvObsrvc.DataBind();
                }
                else
                {
                    List<ObservacaoEntity> observacoes = new List<ObservacaoEntity>();

                    gvObsrvc.DataSource = observacoes;
                    gvObsrvc.DataBind();
                }
            }
            catch (Exception ex)
            {
                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }
        }

        protected void gvObsrvc_PageIndexChanging(Object sender, GridViewPageEventArgs e)
        {
            try
            {
                buscaObservacoes(e.NewPageIndex);
            }
            catch (Exception ex)
            {
                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }
        }

        /// <summary>
        /// Mostra a observação
        /// </summary>
        protected void gvObsrvc_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Função foi desabilitada devido a falhas  do javascript em tempo de execução 
            //Foi modificado para usar o tooltip do próprio dot net 

            //if (e.Row.RowType == DataControlRowType.DataRow)
            //{
            //    if (e.Row.DataItem != null)
            //    {
            //        string obs = DataBinder.Eval(e.Row.DataItem, "observacao2").ToString().Replace("\n", "\\n").Replace("'", "\\'");

            //        if (obs.Length > 56)
            //        {
            //            e.Row.Attributes.Add("onmouseover", "ShowTooltip('Observação','" + obs + "');");
            //            e.Row.Attributes.Add("onmouseout", "HideTooltip();");
            //        }
            //    }
            //}
        }

        private void Load_ListaTipoDocumentos()
        {
            try
            {
                FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];

                AIIM_Facade facade = new AIIM_Facade();
                string transactionID = fieldsIProcess["SW_CASENUM"].Value; //somente nesta situação (fechamento de AIIM, transactionID = caseNUm)
                string processID = "";
                string docsPermitidos = fieldsIProcess["DOCSPERMITIDOS"].Value;

                List<TipoDocumento> documentos = facade.ObterTiposDocumentoAIIM(transactionID, processID, docsPermitidos);

                if ((documentos != null) && (documentos.Count > 0))
                {
                    ddlTpDocmnt.DataSource = documentos;
                    ddlTpDocmnt.DataTextField = "Descricao";
                    ddlTpDocmnt.DataValueField = "Id";
                    ddlTpDocmnt.DataBind();
                }

                ddlTpDocmnt.Items.Insert(0, new ListItem("Selecione tipo documento", "0"));
            }
            catch (Exception ex)
            {

                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }

        }

        private void Load_WorkItem()
        {

            XmlDocument oXmlFFP = new XmlDocument();
            oXmlFFP.LoadXml(Session["ffp"].ToString());
            IProcess iProcess = new IProcess();
            wrkItem.FormFlowParameters oFFP = iProcess.getFFPWorkItem(oXmlFFP);

            //if (oFFP.WorkItemTag.StepName=="SOLICITA")
            //{
            //    btnDesbloquear.Enabled = false;
            //    btnRecarregar.Enabled = true;
            //}

            Label lblIdentificacao = (Label)Master.FindControl("identificacao");
            lblIdentificacao.Text = "Usuário: " + oFFP.NodeCtx.UserName;
            Session["UserName"] = oFFP.NodeCtx.UserName;
            Label lblDtUltimoAcesso = (Label)Master.FindControl("dataUltimoAcesso");
            lblDtUltimoAcesso.Text = "Data/hora de acesso: " + DateTime.Now.ToString();
            lblIdentificacao.Visible = true;
            lblDtUltimoAcesso.Visible = true;

            WorkItem wrkI = iProcess.getWorkItemByFFP(oFFP);
            Session["workItem"] = wrkI;

            FieldColletion<string,WorkItemLockField> lstWrkItemLockField = new FieldColletion<string,WorkItemLockField>();
            lstWrkItemLockField.Add("SW_CASENUM", new WorkItemLockField("SW_CASENUM"));//transactionID (BW)
            lstWrkItemLockField.Add("idAIIM",new WorkItemLockField("idAIIM"));
            lstWrkItemLockField.Add("DocsPermitidos",new WorkItemLockField("DocsPermitidos"));
            lstWrkItemLockField.Add("DocsRequeridos",new WorkItemLockField("DocsRequeridos"));
            lstWrkItemLockField.Add("NR_AIIM",new WorkItemLockField("NR_AIIM"));
            lstWrkItemLockField.Add("FormaCorrecao",new WorkItemLockField("FormaCorrecao"));
            lstWrkItemLockField.Add("NR_RAT",new WorkItemLockField("NR_RAT"));
            lstWrkItemLockField.Add("REGRAINSDOC", new WorkItemLockField("REGRAINSDOC"));
            lstWrkItemLockField.Add("SITUACAOCARREGA", new WorkItemLockField("SITUACAOCARREGA")); //situacaoCarregamentoAiim



            FieldColletion<string, WorkItemLockField> lstField = iProcess.lockWorkItem(wrkI, lstWrkItemLockField);

            Session["FieldsIProcess"] = lstField;

        }

        private void buscarDadosCabecAIIM()
        {
            try
            {
                AiimEntity aiim = new AiimEntity();
                AIIM_Facade facade = new AIIM_Facade();
                FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];
                long idAIIM = long.Parse(fieldsIProcess["IDAIIM"].Value);
                string transactionID = fieldsIProcess["SW_CASENUM"].Value;
                aiim = facade.buscarCabecAIIM(idAIIM,transactionID);

                if (fieldsIProcess["REGRAINSDOC"].Value == "1")
                {
                    Cabecalho_AIIM_DEAT1.Obs = "AIIM lavrado pelo AIIM2003.(1)";
                }

                if (fieldsIProcess["REGRAINSDOC"].Value == "2")
                {
                    Cabecalho_AIIM_DEAT1.Obs = "AIIM não lavrado pelo AIIM2003.(2)";
                }

                if (fieldsIProcess["REGRAINSDOC"].Value == "3")
                {
                    Cabecalho_AIIM_DEAT1.Obs = "Cadastramento Manual de AIIM.(3)";
                }

                Cabecalho_AIIM_DEAT1.NrAiim = aiim.NumeroAIIM.Replace("-","");

                Cabecalho_AIIM_DEAT1.DataLavratura = aiim.DataLavratura;
                Cabecalho_AIIM_DEAT1.ValorAIIM = Convert.ToDecimal(aiim.ValorAiim);

                if (aiim.VersaoAIIM == "0")
                { Cabecalho_AIIM_DEAT1.VersaoAIIM = "Original"; } else { Cabecalho_AIIM_DEAT1.VersaoAIIM = Convert.ToString(aiim.VersaoAIIM); }

                if (string.IsNullOrWhiteSpace(aiim.OrdemServicoFiscal))
                {   Cabecalho_AIIM_DEAT1.OSF = ""; }
                else
                {   Cabecalho_AIIM_DEAT1.OSF = aiim.OrdemServicoFiscal; }

                Cabecalho_AIIM_DEAT1.NomeTributo = aiim.NomeTributo;

                Cabecalho_AIIM_DEAT1.AfrAtuante = aiim.IdFuncionalAFR + " - " + aiim.NomeAFRautuante;


                if (aiim.TipoAutuada == "0")
                {
                    Cabecalho_AIIM_DEAT1.Cpf = aiim.CpfAutuada;
                    Cabecalho_AIIM_DEAT1.Cnpj = "";
                }
                else
                {

                    Cabecalho_AIIM_DEAT1.Cnpj = aiim.CnpjAutuada;
                    Cabecalho_AIIM_DEAT1.Cpf = "";
                }

                Cabecalho_AIIM_DEAT1.NomeAutuado = aiim.NomeAutuada;
                Cabecalho_AIIM_DEAT1.Municipio = aiim.CidadeAutuada;
                Cabecalho_AIIM_DEAT1.UF = aiim.UFAutuada;
                Cabecalho_AIIM_DEAT1.Cep = aiim.CEPAutuada;
                Cabecalho_AIIM_DEAT1.IE = aiim.IEAutuada;

            }
            catch (Exception ex)
            {
                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }

        }


        private bool VerificarSessaoAtiva()
        {
            try
            {
                //--\/-- Verifica se a sessao nao esta ativa...
                IProcess oIProcess = new IProcess();
                WorkItem wrkItem = (WorkItem)Session["WorkItem"];
                oIProcess.getSessionStatus(wrkItem);
                //--/\-- Verifica se a sessao nao esta ativa...
            }
            catch
            {
                ClientScript.RegisterStartupScript(GetType(), "Fechar", "FecharComMensagem('Sessão expirou. Realize novamente o acesso ao sistema!');", true);
                return true;
            }

            return false;
        }

        protected void btnInserirArquivoHidden_Click(object sender, EventArgs e)
        {
            try
            {
                if (VerificarSessaoAtiva())
                {
                    return;
                }

                PROBLEMA_ARQUIVO prob = PROBLEMA_ARQUIVO.OK;
                List<DocumentoAIIMEntity> lista = (List<DocumentoAIIMEntity>)Session["lista"];

                if (ddlTpDocmnt.SelectedIndex == 0)
                {
                    prob = PROBLEMA_ARQUIVO.TIPO_INDEFINIDO;
                }

                if (uploadClient.HasFile)
                {
                    if ((uploadClient.PostedFile.ContentType != "application/pdf") || (uploadClient.FileName.ToUpper().IndexOf(".PDF") == 0))
                        prob = PROBLEMA_ARQUIVO.FORMATO;

                    if (!Br.Gov.Sp.Fazenda.ePAT.Facade.DocumentoPdfFacade.ExisteCabecalhoPDF(uploadClient.FileBytes))
                        prob = PROBLEMA_ARQUIVO.ARQUIVO_SEM_CABECALHO;

                    if (Br.Gov.Sp.Fazenda.ePAT.Facade.DocumentoPdfFacade.VerificarSePDFehCriptografado(uploadClient.FileBytes))
                        prob = PROBLEMA_ARQUIVO.ARQUIVO_COM_CRIPTOGRAFIA;

                    FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];
                    DocumentoAIIMFacade docAimFacede = new DocumentoAIIMFacade();
                    docAimFacede.DocsRequeridos = fieldsIProcess["DOCSREQUERIDOS"].Value;

                    if ((lista != null) && (lista.Count > 0))
                    {
                        if (docAimFacede.ValidaUnicidadeDoNome(uploadClient.FileName, (List<DocumentoAIIMEntity>)Session["lista"]) == false)
                        {
                            prob = PROBLEMA_ARQUIVO.NOME_UNICO;
                        }
                    }

                    if (uploadClient.FileBytes.LongLength > 8971520)
                    {
                        prob = PROBLEMA_ARQUIVO.TAMANHO;
                    }

                    if ((lista != null) && (lista.Count > 0))
                    {

                        if (fieldsIProcess["REGRAINSDOC"].Value == "3")
                        {
                            if (docAimFacede.ValidaUnicidadeDoTipo(Convert.ToInt32(ddlTpDocmnt.SelectedValue), (List<DocumentoAIIMEntity>)Session["lista"]) == false)
                            {
                                prob = PROBLEMA_ARQUIVO.TIPO_UNICO;
                            }
                        }
                    }

                }
                else
                    prob = PROBLEMA_ARQUIVO.ARQUIVO_INDEFINIDO;

                switch (prob)
                {
                    case PROBLEMA_ARQUIVO.OK:

                        btnInserir.Enabled = false;
                        FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];

                        long idProcesso = long.Parse(fieldsIProcess["IDAIIM"].Value);
                        string transactionID = fieldsIProcess["SW_CASENUM"].Value; //somente nesta situação (fechamento de AIIM, transactionID = caseNUm)

                        WorkItem wrkI = (WorkItem)Session["WorkItem"];

                        string mainCase = transactionID;
                        string userName = wrkI.NodeCtx.UserName;

                        string SessionId = this.Page.Session.SessionID;
                        string descricaoArquivo = uploadClient.FileName;
                        int idTipoArquivo = Convert.ToInt32(ddlTpDocmnt.SelectedValue);
                        string descricaoTipoArquivo = ddlTpDocmnt.SelectedItem.Text;

                        InserirArquivo(idProcesso, long.Parse(transactionID), mainCase, userName, SessionId, idTipoArquivo, descricaoTipoArquivo,
                            descricaoArquivo, uploadClient.FileName, uploadClient.FileBytes);
                        ddlTpDocmnt.SelectedIndex = 0;

                        DocumentoAIIMFacade facade = new DocumentoAIIMFacade();
                        facade.DocsRequeridos = fieldsIProcess["DOCSREQUERIDOS"].Value;

                        if ((lista != null) && (lista.Count > 0))
                        {
                            HabilitaEnviar(facade.PodeEnviar(lista));
                        }
                        else
                        {
                            HabilitaEnviar(false);
                        }

                        btnInserir.Enabled = true;
                        break;
                    case PROBLEMA_ARQUIVO.TAMANHO:
                        ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('Tamanho do arquivo excede o máximo permitido de 8 MB.');", true);
                        break;
                    case PROBLEMA_ARQUIVO.TIPO_INDEFINIDO:
                        ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('Selecione o tipo de documento.');", true);
                        break;
                    case PROBLEMA_ARQUIVO.FORMATO:
                        ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('Arquivo Inválido. Arquivo deve estar no formato PDF.');", true);
                        break;
                    case PROBLEMA_ARQUIVO.NOME_UNICO:
                        ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('O arquivo já existe.');", true);
                        break;
                    case PROBLEMA_ARQUIVO.TIPO_UNICO:
                        ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('Tipo do arquivo já existe.');", true);
                        break;
                    case PROBLEMA_ARQUIVO.ARQUIVO_INDEFINIDO:
                        ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('Selecione um arquivo.');", true);
                        break;
                    case PROBLEMA_ARQUIVO.ARQUIVO_SEM_CABECALHO:
                        ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('Cabeçalho PDF não encontrado no arquivo. Possivelmente é um arquivo PDF inválido.');", true);
                        break;
                    case PROBLEMA_ARQUIVO.ARQUIVO_COM_CRIPTOGRAFIA:
                        ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('Arquivo PDF protegido com senha.');", true);
                        break;
                }

            }
            catch (Exception ex)
            {
                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }
        }

        private void InserirArquivo(long IdAIIM, long caseNumber, string mainCase, string userName,
                                            string SessionId, int idTipoArquivo,string descricaoTipoArquivo, string descricaoArquivo, string fileName, byte[] conteudo)
        {

            try
            {
                //--\/-- Verifica se a sessao nao esta ativa...
                IProcess oIProcess = new IProcess();
                WorkItem wrkItem = (WorkItem)Session["WorkItem"];
                oIProcess.getSessionStatus(wrkItem);
                //--/\-- Verifica se a sessao nao esta ativa...

                AIIM_Facade facade = new AIIM_Facade();
                DocumentoAIIMEntity documentoAIIM = facade.SalvarArquivosTemporarios(IdAIIM, caseNumber, mainCase, userName, SessionId, idTipoArquivo, fileName, conteudo);
                documentoAIIM.DescricaoTipoArquivo = descricaoTipoArquivo;
                documentoAIIM.IdTipoArquivo = idTipoArquivo;
                documentoAIIM.Login = userName;
                documentoAIIM.NomeDoArquivo = fileName;

                AdicionarPecas.AdicionarArquivo(documentoAIIM);
            }
            catch (Exception ex)
            {
                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }

        }

        protected void btnUndo_Click(object sender, EventArgs e)
        {

            try
            {
                if (VerificarSessaoAtiva())
                {
                    return;
                }

                ClientScript.RegisterStartupScript(GetType(), "Cancelar", "Cancelar('Todas as alterações serão perdidas. Confirma?');", true);
            }
            catch (Exception ex)
            {
                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }

        }




        private void destruirSession()
        {
            Session.RemoveAll();
        }

        protected void btnKeep_Click(object sender, EventArgs e)
        {
            try
            {
                if (VerificarSessaoAtiva())
                {
                    return;
                }

                ClientScript.RegisterStartupScript(GetType(), "SalvarRascunho", "SalvarRascunho('Deseja que o AIIM " + Cabecalho_AIIM_DEAT1.NrAiim + " seja salvo como rascunho?');", true);
            }
            catch (Exception ex)
            {
                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }
        }




        protected void btnRelease_Click(object sender, EventArgs e)
        {

            try
            {

                if (VerificarSessaoAtiva())
                {
                    return;
                }

                FieldColletion<string, WorkItemLockField> fieldsIProcess = (FieldColletion<string, WorkItemLockField>)Session["FieldsIProcess"];


                if (fieldsIProcess["SITUACAOCARREGA"].Value != null && fieldsIProcess["SITUACAOCARREGA"].Value == "A")
                {   //aiim desbloqueado no aiimWeb.Desabilitar botoes cancelar, salvar Rascunho e Finalizar AIIM
                    ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('AIIM desbloqueado. Necessário recarregá-lo antes de finalizá-lo.');", true);
                }
                else
                {
                    //DocumentoAIIMFacade docAiimFacade = new DocumentoAIIMFacade();

                    //docAiimFacade.DocsRequeridos = fieldsIProcess["DOCSREQUERIDOS"].Value;

                    //if (AdicionarPecas.Lista != null && docAiimFacade.PodeEnviar(AdicionarPecas.Lista) == true)
                    bool relato = false, quadro1 = false, quadro2 = false;

                    if (AdicionarPecas.Lista != null)
                    {
                        relato = AdicionarPecas.Lista.Exists(f => f.IdTipoArquivo == 1);
                        quadro1 = AdicionarPecas.Lista.Exists(f => f.IdTipoArquivo == 58);
                        quadro2 = AdicionarPecas.Lista.Exists(f => f.IdTipoArquivo == 59);
                    }

                    if (relato && quadro1 && quadro2)
                    {

                        fndSalvarArquivos();
                        inserirObservacao();
                        processarCaso("release");
                        destruirSession();
                        ClientScript.RegisterStartupScript(GetType(), "Fechar", "FecharComMensagem('AIIM Finalizado com sucesso!');", true);
                    }
                    else
                    {

                        ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('É necessário anexar os documentos AIIM-Relato e/ou AIIM-Quadro1 e/ou Quadro2');", true);

                        //if (fieldsIProcess["REGRAINSDOC"].Value == "1")
                        //{
                        //    ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('É necessário anexar o documento AIIM-Quadro2');", true);
                        //}

                        //if (fieldsIProcess["REGRAINSDOC"].Value == "2")
                        //{
                        //    ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('É necessário anexar os documentos AIIM-Relato e/ou AIIM-Quadro1');", true);
                        //}

                        //if (fieldsIProcess["REGRAINSDOC"].Value == "3")
                        //{
                        //    ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('É necessário anexar os documentos AIIM-Relato e/ou AIIM-Quadro1 e/ou Quadro2');", true);

                        //}

                    }
                }


            }
            catch (Exception ex)
            {
                Label mlblErro = (Label)Master.FindControl("lblErro");
                mlblErro.Text = ex.Message;
            }
        }

        protected void undoHidden_Click(object sender, EventArgs e)
        {
            processarCaso("undo");
            destruirSession();

            ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "Fechar();", true);
        }

        protected void keepHidden_Click(object sender, EventArgs e)
        {

            fndSalvarArquivos();
            inserirObservacao();
            processarCaso("keep");
            destruirSession();
            ClientScript.RegisterStartupScript(GetType(), "Fechar", "FecharComMensagem('AIIM " + Cabecalho_AIIM_DEAT1.NrAiim + " salvo como rascunho.');", true);

        }

        protected void btnOrdenarPaginas_Click(object sender, EventArgs e)
        {


            OrdenaPaginas();

            Session["Message"] = "Documentos ordenados com sucesso. A pagina foi carregada novamente.";
            
            Response.Redirect("FechamentoAIIM.aspx");



        }

        protected void btnRenumrarPagina_Click(object sender, EventArgs e)
        {

            if (DocsNaoPaginados == true)
            {
                RenumeraPaginas(listaDocsNaoPaginados);
                Session["Message"] = "Documentos renumerados com sucesso. A pagina foi carregada novamente.";
                

                Response.Redirect("FechamentoAIIM.aspx");

            }
            else
            {                
                MostrarMensagemOrdenarRenumerar("Não há documentos a serem renumerados.");                
            }

        }

    }
}
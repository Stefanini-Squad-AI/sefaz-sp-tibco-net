#region References
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using Br.Gov.Sp.Fazenda.ePAT.Facade.Comum;
using Br.Gov.Sp.Fazenda.ePAT.Entities;
using Br.Gov.Sp.Fazenda.ePAT.Facade;
using Br.Gov.Sp.Fazenda.ePAT.Entities.Comum;
using Br.Gov.Sp.Fazenda.ePAT.WebApp.Dados;
using System.Xml;
using Fazenda.ePAT.Entities.TIBCO.General;
using wrkItem = Fazenda.ePAT.Entities.TIBCO.WorkItem;
using System.Data;
using Fazenda.ePAT.Entities.TIBCO;
using System.Configuration;
using Br.Gov.Sp.Fazenda.ePAT.Facade.AIIM.Notificacao;
using System.Globalization;
using Br.Gov.Sp.Fazenda.ePAT.WebApp.Util;
using Br.Gov.Sp.Fazenda.ePAT.Business.AIIM.Notificacao;
using System.Security.Cryptography.X509Certificates;
using Br.Gov.Sp.Fazenda.ePAT.Business;
using LOG = Br.Gov.Sp.Fazenda.ePAT.Facade.Util;
using System.Text;
#endregion

namespace Br.Gov.Sp.Fazenda.ePAT.WebApp.WebPages.PAT.PrimeiraInstancia
{
	public partial class NotificacaoPreparar : System.Web.UI.Page
	{
		#region Variaveis Globais

		AiimEntity aiim = new AiimEntity();
		LOG.LogMessageFacade log = new LOG.LogMessageFacade();
		StringBuilder logMessage = new StringBuilder();

		enum PROBLEMA_ARQUIVO { FORMATO, NOME_UNICO, TIPO_UNICO, TAMANHO, TIPO_INDEFINIDO, ARQUIVO_INDEFINIDO, NUMERO_ARQUIVOS, OK };
		private DataTable dt;

        //Armazena os GUIDs dos arquivos enviados para portal assinaturas caso seja necessária futura remoção 
        private List<string> guids_arquivos_assinados 
        {
            get
            {
                if (ViewState["guids_arquivos_assinados"] != null)
                    return (List<string>)ViewState["guids_arquivos_assinados"];
                else
                    return new List<string>();
            }
            set
            {
                ViewState["guids_arquivos_assinados"] = value;
            }
        }

        #endregion

        #region Propriedades

        ParametrosPagina_NotificacaoPreparar parametros = new ParametrosPagina_NotificacaoPreparar();

        private string urlServicos = ConfigurationManager.AppSettings["HostServicos"];
        private string urlPortal = ConfigurationManager.AppSettings["UrlPortal"];


        private UsuarioSIAPEntity usuarioSIAP
        {
            get
            {
                return (UsuarioSIAPEntity)Session["usuarioSIAP"];
            }
            set
            {
                Session["usuarioSIAP"] = value;
            }
        }

        public ParametrosPagina_NotificacaoPreparar ParametrosPaginaType 
		{
			get
			{
				if (Session[idSupSession + "ParametrosPaginaPrepNot"] == null)
				{
					return new ParametrosPagina_NotificacaoPreparar();
				}
				else
				{
					return Session[idSupSession + "ParametrosPaginaPrepNot"] as ParametrosPagina_NotificacaoPreparar;
				}
			}
			set 
			{
				if (value.GetType() == typeof(ParametrosPagina_NotificacaoPreparar))
				{
					Session[idSupSession + "ParametrosPaginaPrepNot"] = value;
				}
			}
		}

		public string idSupSession
		{
			get
			{
				if (Session["idSupSession"] == null)
				{
					return "";
				}
				else
				{
					return Session["idSupSession"].ToString();
				}
			}
			set
			{
				Session["idSupSession"] = value;
			}
		}

		#endregion
		
		#region Carregamento da Página

		protected void Page_Load(object sender, EventArgs e)
		{
			Label mlblErro = (Label)Master.FindControl("lblErro");
			mlblErro.Text = "";
			pnlPorFavorAguarde.Visible = false;
			try
			{
				this.parametros = ParametrosPaginaType;
                string gamb = Session["PostBack"] != null ? Session["PostBack"].ToString() : "";

                if (!Page.IsPostBack && !(gamb=="true"))
				{
					log.setOrigem("PrepararNotificação");
					logMessage.AppendLine("<br /><br />Início do carregamento da página...");

					VerificaPrecisaLimpaVariaveisSessaoAntigas();

					logMessage.AppendLine("<br /><br />Chamando método Load_WorkItem");
					Load_WorkItem();
					logMessage.AppendLine("<br />Finalizada chamanda do método Load_WorkItem");

					CarregaParametrosPagina();

					logMessage.AppendLine("<br /><br />Chamando método CarregaParametrosPagina");
					buscarDadosCabecAIIM();
					logMessage.AppendLine("<br />Finalizada chamanda do método buscarDadosCabecAIIM");

					logMessage.AppendLine("<br /><br />Chamando método buscaObservacoes");
					buscaObservacoes();
					logMessage.AppendLine("<br />Finalizada chamanda do método buscaObservacoes");

					logMessage.AppendLine("<br /><br />Chamando método buscarMensagens");
					buscarMensagens();
					logMessage.AppendLine("<br />Finalizada chamanda do método buscarMensagens");

					((Label)this.Master.FindControl("nomeModulo")).Text = "Notificação";
					((Label)this.Master.FindControl("lblSubprocesso")).Text = "Preparar Notificação";
					Label lblIdentificacao = (Label)Master.FindControl("identificacao");
					lblIdentificacao.Text = "Usuário: " + parametros.Username;
					Label lblDtUltimoAcesso = (Label)Master.FindControl("dataUltimoAcesso");
					lblDtUltimoAcesso.Text = "Data/hora de acesso: " + DateTime.Now.ToString();
					lblIdentificacao.Visible = true;
					lblDtUltimoAcesso.Visible = true;

					btnKeep.Enabled = false;
					logMessage.AppendLine(string.Format(
						"Page_Load completo! <br /> IdAiim: {0}, NroAiim: {1}, Nome Etapa: {2}, Nome Processo: {3}",
						parametros.IdAiim, parametros.NumeroAiim, parametros.NomeEtapa, parametros.NomeProcesso));

                    CarregarUsuarioPortalAssinatura();

					var url = Request.Url.AbsoluteUri;
					url = url.Substring(0, url.LastIndexOf('/')) + "/AIIMConsultaDocs.aspx?AIIMNumero=" + aiim.NumeroAiimSemDV.ToString();

					//Ativando "carregando" para as seguintes ações da página                        
					btnIntegraDoc.Attributes.Add("onclick", "statusProcessando(true);" + "window.open('" + url + "', '_blank', 'titlebar=yes,toolbar=yes,location=yes,status=no,menubar=yes,scrollbars=no,resizable=yes,width=300px,Height=auto,left=300,top=100');");

					if (HttpContext.Current.Session["OrigemLog"] == null)
                    {
                        ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarReleaseNulo", "alertaMensagem('O contribuinte não é credenciado no DEC. Erro ou Indisponibilidade no Sistema DEC')", true);
                    }

                    logMessage = log.addLogMsg(logMessage);
				}
			}
			catch (Exception ex)
			{
				mostraErro(ex, " Erro ao realizar o PageLoad... ", logMessage);
			}
		}

        public void CarregarUsuarioPortalAssinatura()
        {
            try
            {
                this.usuarioSIAP = new UsuarioSIAPFacade().BuscarServidorPorLogin(string.Empty, string.Empty, this.parametros.Username);
                this.usuarioSIAP.nome = this.parametros.Username;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("85"))
                {
                    var usuarioEPAT = new UsuarioEPATFacade().BuscarUsuariosEPAT(parametros.TransactionID, "", this.parametros.Username, "", "", "").FirstOrDefault();
                    this.usuarioSIAP = new UsuarioSIAPEntity()
                    {

                        RG = usuarioEPAT.Rg,
                        digitoRG = usuarioEPAT.DigitoRg,
                        nome = usuarioEPAT.Login,
                        CPF = usuarioEPAT.Cpf

                    };

                }

            }

        }

        protected void Page_PreRender(object sender, EventArgs e)
		{
			try
			{
				int i = 0;
				bool habilitaEnviar = false;
                bool PermitirNotificacao = false;

                if (Session["ffp"] != null && !ClientScript.IsStartupScriptRegistered(GetType(), "Fechar")) 
                {    
				    PermitirNotificacao = !ExistemPaginasSendoCalculadas(parametros.IdAiim);

                }

                gvLista.Visible = ddlDecisao.SelectedItem.Text == "Notificar";

                string gamb = Session["PostBack"] != null ? Session["PostBack"].ToString() : "";

                if (Page.IsPostBack || gamb=="true")
				{
					if (gvLista.Rows.Count > 0)
					{
						habilitaEnviar = true;
						foreach (GridViewRow row in gvLista.Rows)
						{
							RadioButton rbt1 = (RadioButton)row.FindControl("rdoMeioNotificacaoDEC");
							RadioButton rbt2 = (RadioButton)row.FindControl("rdoMeioNotificacaoPessoal");
							Button btn = (Button)row.FindControl("btnAssinarXml");

							if (btn.Enabled) {
								if (rbt2.Checked)
									btn.Enabled = false;
							}
							else {
								if (rbt1.Checked) {
									if (rbt2.Enabled)
										btn.Enabled = true;
									else {
										btn.Enabled = false;
										i++;
									}
								}
							}

							if (btn.Enabled)
								habilitaEnviar = false;

						}

						if (!PermitirNotificacao)
							habilitaEnviar = false;
					}
				}

				if (habilitaEnviar)
				{
					btnRelease.Enabled = true;
				}
				else
				{
					btnRelease.Enabled = false;
				}

                
                if (HttpContext.Current.Session["OrigemLog"] == null)
                {
                    bool mostrarAlerta = false;

                    foreach (GridViewRow row in gvLista.Rows)
                    {
                        RadioButton rbt1 = (RadioButton)row.FindControl("rdoMeioNotificacaoDEC");
                        RadioButton rbt2 = (RadioButton)row.FindControl("rdoMeioNotificacaoPessoal");
                        Button btn = (Button)row.FindControl("btnAssinarXml");

                        rbt1.Enabled = true;
                        rbt2.Enabled = true;
                        btn.Enabled = true;

                        if (rbt1.Checked && !ClientScript.IsStartupScriptRegistered(GetType(), "Fechar")) 
                        {
                            mostrarAlerta = true; 
                        }
                    }

                    if (mostrarAlerta) 
                    {
                        ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarReleaseNulo", "alertaMensagem('O contribuinte não é credenciado no DEC ou ocorreu um erro ou indisponibilidade no Sistema DEC. Aguarde o problema ser resolvido ou utilize o outro meio de notificação.')", true);
                    }
                    
                }

                if (!ClientScript.IsStartupScriptRegistered(GetType(),"Fechar")) 
                {
                    ((Label)Master.FindControl("lblErro")).Text = PermitirNotificacao ? string.Empty : "Decisão &quot;Notificar&quot; bloqueada.<br>Há documentos neste AIIM com o número de páginas sendo calculado.<br>Por favor atualize a página.<br>Se o problema persistir, verifique quais documentos estão com número de páginas zero ou &quot;calculando&quot; e os substitua.";
                }
				

                
			}
			catch (Exception ex)
			{

				mostraErro(ex, " Erro ao realizar o Page Pre Render... ", logMessage);
			}
		}

		private void Load_WorkItem()
		{
			try
			{
				XmlDocument oXmlFFP = new XmlDocument();
				logMessage.AppendLine("<br /><br />carregamendo da OFFP: <br />" + Session["ffp"].ToString());
				oXmlFFP.LoadXml(Session["ffp"].ToString());

				IProcess iProcess = new IProcess();
				wrkItem.FormFlowParameters oFFP = iProcess.getFFPWorkItem(oXmlFFP);
				parametros.Username = oFFP.NodeCtx.UserName;
				log.setNomeUsuario(parametros.Username);

                Session["UserName"] = oFFP.NodeCtx.UserName;

                WorkItem wrkI = iProcess.getWorkItemByFFP(oFFP);
				FieldColletion<string, WorkItemLockField> lstWrkItemLockField = new FieldColletion<string, WorkItemLockField>();
				lstWrkItemLockField.Add("SW_CASENUM", new WorkItemLockField("SW_CASENUM"));
				lstWrkItemLockField.Add("SW_PARENTCASE", new WorkItemLockField("SW_PARENTCASE"));
				lstWrkItemLockField.Add("IDAIIM", new WorkItemLockField("IDAIIM"));
				lstWrkItemLockField.Add("DocsPermitidos", new WorkItemLockField("DocsPermitidos"));
				lstWrkItemLockField.Add("DocsRequeridos", new WorkItemLockField("DocsRequeridos"));
				lstWrkItemLockField.Add("CPFCNPJNOTIFICA", new WorkItemLockField("CPFCNPJNOTIFICA"));
				lstWrkItemLockField.Add("REGRAINSDOC", new WorkItemLockField("REGRAINSDOC"));
				FieldColletion<string, WorkItemLockField> lstField = iProcess.lockWorkItem(wrkI, lstWrkItemLockField);
				
				idSupSession = lstField[campoIProcess.IDAIIM].Value.ToString();
				parametros.wrkI = wrkI;
				parametros.fieldsIProcess = lstField;
			}
			catch
			{
				throw;
			}
		}


		private void CarregaParametrosPagina()
		{
			try
			{
				if (String.IsNullOrEmpty(parametros.wrkI.ToString()))
				{
					try
					{
						Load_WorkItem();
					}
					catch
					{
						throw;
					}
				}
				else
				{
					parametros.NomeEtapa = parametros.wrkI.WorkItemTag.StepName;
					parametros.TransactionID = parametros.fieldsIProcess[campoIProcess.SW_CASENUM].Value;
					parametros.IdAiim = long.Parse(parametros.fieldsIProcess[campoIProcess.IDAIIM].Value);
					parametros.NomeEtapa = parametros.wrkI.WorkItemTag.StepName;
					parametros.Username = parametros.wrkI.NodeCtx.UserName;
					parametros.NomeProcesso = parametros.wrkI.WorkItemTag.ProcedureName;
					//parametros.MainCase = parametros.fieldsIProcess[campoIProcess.SW_MAINCASE].Value;

					AIIM_Facade facade = new AIIM_Facade();
					parametros.aiim = facade.buscarCabecAIIM(parametros.IdAiim, parametros.TransactionID);
					parametros.NumeroAiim = parametros.aiim.NumeroAIIM;

					log.setNumeroAiim(parametros.NumeroAiim);
					this.ParametrosPaginaType = parametros;

					this.logMessage.AppendLine("<br />IdAiim: " + this.parametros.IdAiim.ToString());
					this.logMessage.AppendLine("<br />NumeroAiim: " + this.parametros.NumeroAiim.ToString());
					this.logMessage.AppendLine("<br />TransactionID: " + this.parametros.TransactionID.ToString());
					this.logMessage.AppendLine("<br />NomeEtapa: " + this.parametros.NomeEtapa.ToString());
					this.logMessage.AppendLine("<br />NomeProcesso: " + this.parametros.NomeProcesso.ToString());
					this.logMessage.AppendLine("<br />Username: " +  this.parametros.Username.ToString());
					this.logMessage.AppendLine("<br /><br />Fields do iProcess...");
					foreach (var item in parametros.fieldsIProcess)
					{
						if (item.Value.Value != null)
							this.logMessage.AppendLine(string.Format("<br />Key iProcess: {0} , FieldIProcess: {1}", item.Key.ToString(), item.Value.Value.ToString()));    
						else
							this.logMessage.AppendLine(string.Format("<br />Key iProcess: {0} , FieldIProcess: null", item.Key.ToString()));    
					}
				}
			}
			catch
			{
				throw;
			}
		}

		private void buscarDadosCabecAIIM()
		{
			try
			{
				if (parametros.fieldsIProcess["REGRAINSDOC"].Value == "1")
					Cabecalho_AIIM.Obs = "AIIM lavrado pelo AIIM2003.(1)";

				if (parametros.fieldsIProcess["REGRAINSDOC"].Value == "2")
					Cabecalho_AIIM.Obs = "AIIM não lavrado pelo AIIM2003.(2)";

				if (parametros.fieldsIProcess["REGRAINSDOC"].Value == "3")
					Cabecalho_AIIM.Obs = "Cadastramento Manual de AIIM.(3)";

				Cabecalho_AIIM.NrAiim = parametros.aiim.NumeroAIIM.Replace("-", "");
				Cabecalho_AIIM.DataLavratura = parametros.aiim.DataLavratura;
				Cabecalho_AIIM.ValorAIIM = Convert.ToDecimal(parametros.aiim.ValorAiim);
				Cabecalho_AIIM.NomeTributo = parametros.aiim.NomeTributo;
				Cabecalho_AIIM.AfrAtuante = parametros.aiim.IdFuncionalAFR + " - " + parametros.aiim.NomeAFRautuante;
				Cabecalho_AIIM.NomeAutuado = parametros.aiim.NomeAutuada;
				Cabecalho_AIIM.Municipio = parametros.aiim.CidadeAutuada;
				Cabecalho_AIIM.UF = parametros.aiim.UFAutuada;
				Cabecalho_AIIM.Cep = parametros.aiim.CEPAutuada;
				Cabecalho_AIIM.IE = parametros.aiim.IEAutuada;
				Cabecalho_AIIM.OSF = Convert.ToString(parametros.aiim.OrdemServicoFiscal);

				if (parametros.aiim.VersaoAIIM == "0") 
					Cabecalho_AIIM.VersaoAIIM = "Original"; 
				else
					Cabecalho_AIIM.VersaoAIIM = Convert.ToString(parametros.aiim.VersaoAIIM);

				if (parametros.aiim.OrdemServicoFiscal != null || parametros.aiim.OrdemServicoFiscal != "")
					Cabecalho_AIIM.OSF = parametros.aiim.OrdemServicoFiscal;
				else
					Cabecalho_AIIM.OSF = "";


				if (aiim.TipoAutuada == "0")
				{
					Cabecalho_AIIM.Cpf = parametros.aiim.CpfAutuada;
					Cabecalho_AIIM.Cnpj = "";
				}
				else
				{
					Cabecalho_AIIM.Cnpj = parametros.aiim.CnpjAutuada;
					Cabecalho_AIIM.Cpf = "";
				}
			}
			catch
			{
				throw;
			}

		}

		private void buscarMensagens()
		{
			try
			{
				AIIM_Facade facade = new AIIM_Facade();
				aiim = facade.buscarCabecAIIM(parametros.IdAiim, parametros.TransactionID);
				
				TextBox txtlMsgs = (TextBox)Master.FindControl("txtMsgs");
				txtlMsgs.Attributes.Add("readOnly", "true");
				txtlMsgs.Text = facade.buscarMensagens(long.Parse(parametros.aiim.NumeroAiimSemDV));
				
				if (txtlMsgs.Text == "" || txtlMsgs.Text == null)
					txtlMsgs.Visible = false;
				else 
					txtlMsgs.Visible = true;
			}
			catch
			{
				throw;
			}
		}

		#endregion

		#region Métodos

		protected void processarCaso(string tipoAcao)
		{
			try
			{
				logMessage.AppendLine("<br /><br />Método processar caso iniciado...");
				IProcess iProcess = new IProcess();

				switch (tipoAcao)
				{
					case "keep":
						logMessage.AppendLine("<br /><br />Ação Keep selecionada...");
						FieldColletion<string, WorkItemKeepField> lstWrkItemKeep = new FieldColletion<string, WorkItemKeepField>();
						iProcess.keepWorkItem(parametros.wrkI, lstWrkItemKeep);
						logMessage.AppendLine("<br /><br />Ação Keep Concluída com sucesso...");
						break;

					case "release":
						logMessage.AppendLine("<br /><br />Ação Release selecionada...");

						FieldColletion<string, WorkItemReleaseField> lstWrkReleaseKeep = new FieldColletion<string, WorkItemReleaseField>();
						if (this.ddlDecisao.SelectedItem.Text == "Corrigir")
						{
							logMessage.AppendLine("<br /><br />Decisão de Correção em andamento...");
							lstWrkReleaseKeep.Add("CORRECAO", new WorkItemReleaseField("CORRECAO", "swNumeric", "1"));
							iProcess.releaseWorkItem(parametros.wrkI, lstWrkReleaseKeep);
							ClientScript.RegisterStartupScript(GetType(), "Fechar", "FecharComMensagem('Correção iniciada com sucesso!')", true);
							logMessage.AppendLine("<br /><br />Ação Release (Correção) concluída...");
							return;
						}
						else //Notificar
						{
							logMessage.AppendLine("<br /><br />Decisão de Notificar em andamento...");
							bool consistencia = true;
							string vNotificacao = "";
							
							if (parametros.vNotificacao == null)
								vNotificacao = "";
							else
								vNotificacao = parametros.vNotificacao;

							logMessage.AppendLine("<br /><br />Conteúdo da vNotificação: " + vNotificacao);

							foreach (GridViewRow row in getDataTable().Rows)
							{
								Button btnAssinar = (Button)row.FindControl("btnAssinarXml");
								RadioButton addButton2 = (RadioButton)row.FindControl("rdoMeioNotificacaoPessoal");
								if (btnAssinar.Enabled == true)
								{
									consistencia = false;
								}
								else
								{
									if (addButton2.Checked == true)
									{
										vNotificacao += getDataTable().DataKeys[row.DataItemIndex].Values[2].ToString() + ";Outros;|";
										parametros.vNotificacao = vNotificacao;
										logMessage.AppendLine("<br /><br />Conteúdo parcial da vNotificação: " + vNotificacao);
									}
									else
									{
										string key_currentDataItem = getDataTable().DataKeys[row.DataItemIndex].Values[2].ToString();
										logMessage.AppendLine("<br />Conteudo da Key - nro doc contribuinte: " + key_currentDataItem);
										
                                        Dictionary<string, string> dicionario = (Dictionary<string,string>)Session[key_currentDataItem];

                                        XmlDocument xmlDoc = new XmlDocument();
                                        xmlDoc.LoadXml(dicionario["xmlDoc"]);

                                        try
                                        {
											int idDECResposta = new AIIM_Facade().DECSolicitarNotificacao(dicionario, parametros.Username, parametros.aiim.NomeTributo);
											if (idDECResposta != 0 && idDECResposta.ToString() != "")
											{
												vNotificacao += key_currentDataItem + ";DEC;" + idDECResposta.ToString() + ";|";
												parametros.vNotificacao = vNotificacao;
												logMessage.AppendLine("<br /><br />Conteúdo parcial da vNotificação atualizada: " + vNotificacao);
											}
											else
											{
												logMessage.AppendLine("<br /><br />ERRO de comunicação com o DEC... Fechando a Tela do Usuário...");
												ClientScript.RegisterStartupScript(GetType(), "FecharComMsgmErro", "FecharComMsgmErro('ERRO de comunicação com o DEC !', 'Por favor, tente novamente... Caso o problema persistir, contate o AIIM Suporte.')", true);
											}
										}
										catch
										{
											throw;
										}
									}
								}
							}

							if (consistencia == true)
							{
								if (vNotificacao == "" || vNotificacao == null)
								{
									ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarReleaseNulo", "alertaMensagem('Não pode ser enviado! Os parametros de notificação não foram retornados ao iProcess')", true);
									logMessage.AppendLine("<br /><br />Não pode ser enviado! Os parametros de notificação não foram retornados ao iProcess");
								}
								else
								{
									logMessage.AppendLine("<br /><br />Conteúdo FINAL da vNotificação: " + vNotificacao);
									lstWrkReleaseKeep.Add("CORRECAO", new WorkItemReleaseField("CORRECAO", "swNumeric", "0"));
									lstWrkReleaseKeep.Add("NOTIFICACAO", new WorkItemReleaseField("NOTIFICACAO", "swText", vNotificacao));
									iProcess.releaseWorkItem(parametros.wrkI, lstWrkReleaseKeep);
									logMessage.AppendLine("<br /><br />Ação Release concluída...");
									ClientScript.RegisterStartupScript(GetType(), "Fechar", "FecharComMensagem('Notificação enviada com sucesso!')", true);
								}
							}
							else
							{
								ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarRelease", "alertaMensagem('Não pode ser enviado! Favor Assinar todos os Itens')", true);
							}
						}

						break;

					case "undo":
						iProcess.undoWorkItem(parametros.wrkI);
						logMessage.AppendLine("<br /><br />Ação Undo concluída...");
						break;
				}
			}
			catch
			{
				throw;
			}
		}

		private void fndSalvarArquivos()
		{
			try
			{
				AIIM_Facade facade = new AIIM_Facade();
				List<DocumentoAIIMEntity> listaDocumentos = (List<DocumentoAIIMEntity>)Session["lista"];
				bool retorno = facade.SalvarEordenarArquivos(
								Convert.ToInt64(parametros.fieldsIProcess[campoIProcess.IDAIIM].Value), 
								parametros.TransactionID, parametros.TransactionID, 
								this.Page.Session.SessionID, listaDocumentos);
			}
			catch
			{
				throw;
			}
		}

		private UsuarioSIAPEntity BuscarDadosUsuarioeSIAP()
		{
			UsuarioSIAPEntity UsuarioSIAP = null;
			try
			{
				UsuarioSIAPFacade facade = new UsuarioSIAPFacade();
				UsuarioSIAP = facade.BuscarServidorPorLogin(
												parametros.TransactionID, 
												parametros.fieldsIProcess[campoIProcess.IDAIIM].Value.ToString(), 
												parametros.Username);
				if (UsuarioSIAP == null)
					throw new Exception("Dados do usuário não encontrados na base do SIAP.");

				return UsuarioSIAP;
			}
			catch
			{
				throw; 
			}
		}
		
		public int TipoTributos(string nomeTributo)
		{
			//0-ICMS 			//1-IPVA  			//2-ITCMD 			//3-TAXAS 			//4-SIMPLES
			int intTipoTributo = -1;
			try
			{
                parametros.NomeTributo = nomeTributo;
				switch (nomeTributo)
				{
					case "ICMS":
						intTipoTributo = 0;
						break;

					case "IPVA":
						intTipoTributo = 1;
						break;

					case "ITCMD":
						intTipoTributo = 2;
						break;

					case "TAXAS":
						intTipoTributo = 3;
						break;

					case "ICMS do Simples Nacional":
						intTipoTributo = 4;
						break;

                    case "AINF - SIMPLES NACIONAL":
						intTipoTributo = 5;
						break;
                    
					default:
						break;
				}
				return intTipoTributo;
			}
			catch
			{
				throw; 
			}
		}

		protected string verificaTipoTributo(long tipoTributo)
		{
			//0-ICMS			//1-IPVA			//2-ITCMD			//3-TAXAS			//4-SIMPLES
			try
			{
				string TipoDocXML = "0";
				switch (tipoTributo)
				{
					case 0:
						TipoDocXML = "1"; //0-ICMS
						break;
					case 1:
						TipoDocXML = "6"; //1-IPVA
						break;
					case 2:
						TipoDocXML = "11"; //2-ITCMD
						break;
					case 4:
						TipoDocXML = "16"; //4-SIMPLES
						break;
                    case 5:
						TipoDocXML = "20"; //5-EPAT - NOTIFICAÇÃO AINF SIMPLES DEC
						break;
					default:
						TipoDocXML = "0";
						break;

				}
                if (TipoDocXML == "0")
                {
                    pnlPorFavorAguarde.Visible = false;
                    throw new Exception(string.Format("Este AIIM {0} referente ao tipo de tributo {1} não possui modelo de notificação via DEC. " +
                                                      "Favor prosseguir com outro tipo de notificação", parametros.NumeroAiim, parametros.NomeTributo));
                }

				return TipoDocXML;
			}
			catch (Exception)
			{
                pnlPorFavorAguarde.Visible = false;
				throw; 
			}
		}

		public void iniciarNotificacoes(Double juros, DateTime dataLavratura, String responsavel, String numeroAIIM)
		{
			try
			{
				AIIM_Facade facade = new AIIM_Facade();
				facade.iniciarNotificacoes(parametros.listaDTableNotificaveis, "1234567890", juros, dataLavratura, responsavel, numeroAIIM);
			}
			catch
			{
				throw; 
			}
		}

		public void buscaNotificaveisByIdAIIM(long idAIIM, long idAiimRenotificacao)
		{
			try
			{
				AIIM_Facade facade = new AIIM_Facade();
                dt = facade.buscarNotificaveis(idAIIM, idAiimRenotificacao, parametros.aiim.NomeTributo);
				if (dt.Rows.Count > 0)
				{
					gvLista.DataSource = dt;
					gvLista.DataBind();
                    if (parametros.aiim.NomeTributo=="TAXAS")
                    {
                        mostraErro("Não existe modelo de notificação DEC para o tributo: TAXAS. Prosseguir com outros tipos de notificação");
                    }
				}
				else
				{
					gvLista.DataSource = null; // this.Lista;
					gvLista.DataBind();
				}
				parametros.listaDTableNotificaveis = dt;
			}
			catch
			{
				throw;
			}
		}

		protected void mostraErro(Exception ex, string MensagemErro = "", StringBuilder sb = null)
		{
			addExceptionLogMsg(ex, string.Format("{0} - {1}", MensagemErro, sb.ToString()));
			string MensagemErroUsuario = string.Format("ERRO: {0} - {1}", MensagemErro, ex.Message);
			Label mlblErro = (Label)Master.FindControl("lblErro");
			mlblErro.Text = MensagemErroUsuario;
			logMessage.Clear();
			ClientScript.RegisterStartupScript(GetType(), "generalError", "alertaMensagem('" + MensagemErroUsuario + "')", true);
		}

        protected void mostraErro(string MensagemErro = "")
        {
            string MensagemErroUsuario = string.Format("{0}", MensagemErro);
            Label mlblErro = (Label)Master.FindControl("lblErro");
            mlblErro.Text += "<br /><br />";
            mlblErro.Text += MensagemErroUsuario;
            mlblErro.Text += "<br /><br />";

            log.addLogMsg(MensagemErroUsuario);
            ClientScript.RegisterStartupScript(GetType(), "generalError", "alertaMensagem('" + MensagemErroUsuario + "')", true);
        }

		private void addExceptionLogMsg(Exception ex, string MensagemErro = "")
		{
			logMessage.AppendLine(string.Format(
				"<br /><br />Erro: {0}, <br /><strong>Exception Message:</strong> {1}, <br />TargetSite: {2}, <br />Source: {3}, <br />Data: {4}, <br />StackTrace: {5}, <br />Inner Exception: {6}",
				MensagemErro, ex.Message, ex.TargetSite, ex.Source, ex.Data, ex.StackTrace, ex.InnerException
				));
			if (ex.InnerException != null)
			{
				Exception exception = new Exception();
				ex = ex.InnerException;
				addExceptionLogMsg(exception, "");
			}
			else
			{
				log.addLogMsg(logMessage);
			}
		}

		private bool ExistemPaginasSendoCalculadas(long idAiim)
		{

            Load_WorkItem();
            CarregaParametrosPagina();


			string transactionID = System.Configuration.ConfigurationManager.AppSettings["NomeSistema"] + Page.Session.SessionID.ToString();
			List<DocNaoPaginadoEntity> listaDocsNaoPaginados = new List<DocNaoPaginadoEntity>();
			DocPaginadoEntity docPaginado = new DocPaginadoEntity();

            new DocPaginadoFacade().BuscarListaArquivos(transactionID, long.Parse(parametros.fieldsIProcess[campoIProcess.IDAIIM].Value), out docPaginado, out listaDocsNaoPaginados);

			return listaDocsNaoPaginados.Exists(docnaopaginado => docnaopaginado.NroPaginas == 0);
		}

		#region Helpers

		private void destruirSession()
		{
			Session.RemoveAll();
		}

		private bool VerificarSessaoExpirada()
		{
			try
			{
				IProcess oIProcess = new IProcess();
				oIProcess.getSessionStatus(this.parametros.wrkI); // Verifica se a sessao nao esta ativa...
			}
			catch
			{
				ClientScript.RegisterStartupScript(GetType(), "Fechar", "FecharComMensagem('Sessão expirou. Realize novamente o acesso ao sistema!');", true);
				logMessage.AppendLine("<br /><br />ERRO: Sessão expirou. Realize novamente o acesso ao sistema!");
               
                if (HttpContext.Current.Session["OrigemLog"] == null)
                {
                    ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarReleaseNulo", "alertaMensagem('O contribuinte não é credenciado no DE. Erro ou Indisponibilidade no Sistema DEC')", true);
                }

                logMessage = log.addLogMsg(logMessage);

				return true;
			}
			return false;
		}

		private void VerificaPrecisaLimpaVariaveisSessaoAntigas()
		{
			bool limpaSession = false;

			if (Session["i"] != null) { limpaSession = true; }
			if (Session["xmlNotificacaoDEC"] != null) { limpaSession = true; }
			if (Session["currentIProcess"] != null) { limpaSession = true;  }
			if (Session["vNotificacao"] != null) { limpaSession = true; }
            if (Session["dicionarioNotificacaoDEC"] != null) { limpaSession = true; }

            if (limpaSession == true)
			{
				XmlDocument oXmlFFP = new XmlDocument();
				oXmlFFP.LoadXml(Session["ffp"].ToString());

				destruirSession();
				log.setOrigem("PrepararNotificação");
				logMessage.AppendLine("<br /><br />Foi necessário destruir variáveis antigas da Session...");

				Session["ffp"] = oXmlFFP.InnerXml;
				logMessage.AppendLine("<br /><br />Carregando novamente a ffp para a Session... Continuando com carregamento da Página.");
			}
			else
			{
				logMessage.AppendLine("<br /><br />Não foi necessário destruir variáveis antigas da Session...");
			}
		}

		public GridView getDataTable()
		{
			try
			{
				return this.gvLista;
			}
			catch
			{
				throw;
			}
		}

		private void PopulaGrid()
		{
			try
			{
				gvLista.DataSource = null; // this.Lista;
				gvLista.DataBind();
			}
			catch
			{
				throw;
			}
		}

		private string srtformat_vCpf(GridViewRow row)
		{
			string vCnpjCpf = getDataTable().DataKeys[row.DataItemIndex].Values[2].ToString();
			if (vCnpjCpf.Length == 11)
				vCnpjCpf = string.Format("{0:CPF}", new Formatar(vCnpjCpf));
			else
				vCnpjCpf = string.Format("{0:CNPJ}", new Formatar(vCnpjCpf));
			return vCnpjCpf;
		}

		private string DataPorExtenso()
		{
			string dataExtenso = "";
			try
			{
				CultureInfo culture = new CultureInfo("pt-BR");
				DateTimeFormatInfo dtfi = culture.DateTimeFormat;
				int dia = DateTime.Now.Day;
				int ano = DateTime.Now.Year;
				string mes = culture.TextInfo.ToTitleCase(dtfi.GetMonthName(DateTime.Now.Month));
				string diasemana = culture.TextInfo.ToTitleCase(dtfi.GetDayName(DateTime.Now.DayOfWeek));
				dataExtenso = diasemana + ", " + dia + " de " + mes + " de " + ano;
			}
			catch
			{
				throw;
			}

			return dataExtenso;
		}

		#endregion

		#endregion

		#region Seção de Observações da Página

		private Boolean inserirObservacao()
		{
			bool retorno = false;
			try
			{
				AIIM_Facade facade = new AIIM_Facade();
				retorno = facade.InserirObservacoes(
									Convert.ToInt64(parametros.fieldsIProcess[campoIProcess.IDAIIM].Value), 
									parametros.TransactionID, parametros.NomeEtapa, 
									parametros.NomeProcesso, parametros.Username, ftbObsrvc.Text);

				return retorno;
			}
			catch
			{
				throw;
			}
		}

		private void buscaObservacoes(int index)
		{
			try
			{
				buscaObservacoes();
				gvObsrvc.PageIndex = index;
				gvObsrvc.DataBind();
			}
			catch
			{
				throw; 
			}
		}

		private void buscaObservacoes()
		{
			try
			{
				AIIM_Facade facade = new AIIM_Facade();
				DataTable dt = facade.BuscarObservacoes(parametros.IdAiim, parametros.TransactionID);
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
			catch
			{
				throw; 
			}
		}

		protected void gvObsrvc_PageIndexChanging(Object sender, GridViewPageEventArgs e)
		{
			try
			{
				if(!VerificarSessaoExpirada())
					buscaObservacoes(e.NewPageIndex);
			}
			catch (Exception ex)
			{
				mostraErro(ex, "", logMessage);
			}
		}

		protected void gvObsrvc_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (VerificarSessaoExpirada())
				return;
			
			try
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
			catch (Exception ex)
			{
				mostraErro(ex, "", logMessage);
			}
		}

		#endregion

		#region Eventos

		protected void btnUndo_Click(object sender, EventArgs e)
		{
			if (!VerificarSessaoExpirada())
			{
				try
				{
					logMessage.AppendLine("<br /><br />Usuário clicou no botão Undo...");
					if (VerificarSessaoExpirada())
					{
						return;
					}
					ClientScript.RegisterStartupScript(GetType(), "Cancelar", "Cancelar('Deseja fechar sem salvar as operações?');", true);

                    if (HttpContext.Current.Session["OrigemLog"] == null)
                    {
                        ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarReleaseNulo", "alertaMensagem('O contribuinte não é credenciado no DE. Erro ou Indisponibilidade no Sistema DEC')", true);
                    }

					logMessage = log.addLogMsg(logMessage);
				}
				catch (Exception ex)
				{
					mostraErro(ex , " Erro ao executar o botão Cancelar ... ", logMessage);
				}
			}
		}

		protected void btnKeep_Click(object sender, EventArgs e)
		{
			if (!VerificarSessaoExpirada())
			{
				try
				{
					logMessage.AppendLine("<br /><br />Usuário clicou no botão keep...");
					if (VerificarSessaoExpirada())
						return;
					ClientScript.RegisterStartupScript(GetType(), "SalvarRascunho", "SalvarRascunho('Deseja que o AIIM " + Cabecalho_AIIM.NrAiim + " seja salvo como rascunho?');", true);

                    //if (HttpContext.Current.Session["OrigemLog"] == null)
                    //{
                    //    ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarReleaseNulo", "alertaMensagem('O contribuinte não é credenciado no DE. Erro ou Indisponibilidade no Sistema DEC')", true);
                    //}

                    logMessage = log.addLogMsg(logMessage);
				}
				catch (Exception ex)
				{
					mostraErro(ex, " Erro ao Executar o botão Release (Enviar) ... ", logMessage);
				}
			}
		}

		protected void btnIniciarCorrecao_Click(object sender, EventArgs e)
		{
			if (!VerificarSessaoExpirada())
			{
				try
				{
					logMessage.AppendLine("<br /><br />Usuário clicou no botão Iniciar Correção...");
					if (VerificarSessaoExpirada())
						return;

                    //if (new TibcoIProcessFacade().VerificarPossibilidadeDevolucaoNotificacao(parametros.fieldsIProcess[campoIProcess.SW_CASENUM].Value))
                    if(new AIIM_Facade().VerificarPossibilidadeCorrecao(parametros.IdAiim,parametros.ProcessID,parametros.TransactionID))
                    {
                        inserirObservacao();
                        processarCaso("release");
                    }
                    else
                    {
                        ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarRelease", "alertaMensagem('AIIM não pode ser corrigido, pois já existe um autuado/solidário notificado.')", true);
                    }

                    //if (HttpContext.Current.Session["OrigemLog"] == null)
                    //{
                    //    ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarReleaseNulo", "alertaMensagem('O contribuinte não é credenciado no DE. Erro ou Indisponibilidade no Sistema DEC')", true);
                    //}

					logMessage = log.addLogMsg(logMessage);
				}
				catch (Exception ex)
				{
					mostraErro(ex , " Erro ao iniciar a correção ... ", logMessage);
				}
			}
		}

		protected void btnRelease_Click(object sender, EventArgs e)
		{
			if (!VerificarSessaoExpirada())
			{
				try
				{
					logMessage.AppendLine("<br /><br />Usuário clicou no botão Release...");
					inserirObservacao();
					logMessage.AppendLine("<br /><br />Sucesso ao inserir observação");
					processarCaso("release");
					logMessage.AppendLine("<br /><br />Sucesso ao processar caso com release");
                    
                    if (HttpContext.Current.Session["OrigemLog"] == null)
                    {
                        ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarReleaseNulo", "alertaMensagem('O contribuinte não é credenciado no DE. Erro ou Indisponibilidade no Sistema DEC')", true);
                    }

					logMessage = log.addLogMsg(logMessage);
				}
				catch (Exception ex)
				{
					mostraErro(ex, " Erro ao realizar o Relase (Enviar) do Caso.", logMessage);
				}
				finally
				{
					destruirSession();
				}
			}
		}

		protected void undoHidden_Click(object sender, EventArgs e)
		{
			if (!VerificarSessaoExpirada())
			{
				try
				{
					processarCaso("undo");

                    CancelarNotificacoesNaoAssinadasPortalAssinatura();

                    ClientScript.RegisterStartupScript(GetType(), "Fechar", "Fechar();", true);

                    if (HttpContext.Current.Session["OrigemLog"] == null)
                    {
                        ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarReleaseNulo", "alertaMensagem('O contribuinte não é credenciado no DEC. Erro ou Indisponibilidade no Sistema DEC')", true);
                    }

                    logMessage = log.addLogMsg(logMessage);
				}
				catch (Exception ex)
				{
					mostraErro(ex, " Erro ao clicar em undo (cancelar)... ", logMessage);
				}
				finally
				{
					destruirSession();
				}
			}
		}

		protected void keepHidden_Click(object sender, EventArgs e)
		{
			if (!VerificarSessaoExpirada())
			{
				try
				{
					logMessage.AppendLine("<br /><br />Usuário Clicou no botão release... ");
					inserirObservacao();
					logMessage.AppendLine("<br /><br />Sucesso ao executar Inserir Observações...");
					processarCaso("keep");
					logMessage.AppendLine("<br /><br />Sucesso ao processar caso com Keep");
					ClientScript.RegisterStartupScript(GetType(), "Fechar", "FecharComMensagem('AIIM " + parametros.NumeroAiim + " salvo como rascunho.');", true);

                    if (HttpContext.Current.Session["OrigemLog"] == null)
                    {
                        ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarReleaseNulo", "alertaMensagem('O contribuinte não é credenciado no DEC. Erro ou Indisponibilidade no Sistema DEC')", true);
                    }

                    logMessage = log.addLogMsg(logMessage);
				}
				catch (Exception ex)
				{
					mostraErro(ex, " Erro ao clicar em keep (salvar rascunho)... ", logMessage);
				}
				finally
				{
					destruirSession();
                    ClientScript.RegisterStartupScript(GetType(), "Fechar", "FecharComMensagem('Correção iniciada com sucesso!')", true);
				}
			}
		}

		protected void ddlDecisao_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				logMessage.AppendLine("<br /><br />Usuário clicou no Drop Down List Decisão...");
				parametros = this.ParametrosPaginaType;

				#region Verificar a sessão(Ativa ou não)
				if (VerificarSessaoExpirada())
				{
					return;
				}
				#endregion

				DropDownList ddl = (DropDownList)sender;

				switch (ddl.SelectedItem.Text) {
					case "Corrigir" :
						btnIniciarCorrecao.Enabled = true;
						btnRelease.Enabled = false;
						logMessage.AppendLine("<br /><br />Usuário selecionou a Decisão Correção...");
						break;
					case "Notificar" :
						btnIniciarCorrecao.Enabled = false;
						try
						{
							logMessage.AppendLine("<br />Usuário selecionou a Decisão Notificar...");
							string NumeroAIIMSemDigito = parametros.NumeroAiim;
							int posicaoTraco = NumeroAIIMSemDigito.IndexOf('-');
							NumeroAIIMSemDigito = NumeroAIIMSemDigito.Substring(0, posicaoTraco);
							buscaNotificaveisByIdAIIM(long.Parse(NumeroAIIMSemDigito), parametros.IdAiim);

							logMessage.AppendLine("<br />Lista Notificaveis: ");
							for (int i = 0; i < parametros.listaDTableNotificaveis.Rows.Count; i++)
							{
								logMessage.AppendLine(string.Format("<br />{0}: {1} ",
									i.ToString(), parametros.listaDTableNotificaveis.Rows[i].ItemArray.GetValue(0).ToString()
									));
							}
						}
						catch
						{
							throw;
						}
						break;
					default : //case "Selecione..."
						btnIniciarCorrecao.Enabled = false;
						btnRelease.Enabled = false;
						logMessage.AppendLine("<br /><br />Usuário deselecionou a Decisão...");
						break;
					}

                if (HttpContext.Current.Session["OrigemLog"] == null)
                {
                    ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarReleaseNulo", "alertaMensagem('O contribuinte não é credenciado no DEC. Erro ou Indisponibilidade no Sistema DEC')", true);
                }

				logMessage = log.addLogMsg(logMessage);
			}
			catch (Exception ex)
			{
				mostraErro(ex, " Erro ao Selecionar a Decisão da Notificação... ", logMessage);
			}
		}


        #endregion

        #region GridView gvLista

        protected void gvLista_DataBound(object sender, EventArgs e)
		{
            

		}

		protected void gvLista_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			try
			{
				switch (e.CommandName)
				{
					case "cmdAssinarXml":				                        //gambi (provisorio), ao chamar o assinador esta sendo feito um postback = false
                        Session["PostBack"] = "true";

						logMessage.AppendLine("<br />Usuário clicou no botão assinar do Indice: " + e.CommandArgument.ToString());

						string Assunto = "";
						bool sendXmlToDEC = true;

						int i = Convert.ToInt32(e.CommandArgument);
						GridViewRow row = gvLista.Rows[i];
						Session["i"] = i.ToString();
							
						RadioButton addButton1 = (RadioButton)row.FindControl("rdoMeioNotificacaoDEC");
						RadioButton addButton2 = (RadioButton)row.FindControl("rdoMeioNotificacaoPessoal");
						Button btnAssinar = (Button)row.FindControl("btnAssinarXml");
						btnAssinar.Enabled = false;
						addButton1.Enabled = false;
						addButton2.Enabled = false;
							
						string nrDocumento = parametros.fieldsIProcess["CPFCNPJNOTIFICA"].Value;

						string vCpfAFR = BuscarDadosUsuarioeSIAP().CPF;
						long tipoTributo = TipoTributos(parametros.aiim.NomeTributo);

						#region Verificar data de lavratura maior que a data atual....

						DateTime now = DateTime.Now;
						int result = DateTime.Compare(now, parametros.aiim.DataLavratura);

						if (result < 0)
						{
							ScriptManager.RegisterStartupScript(this, this.Page.GetType(), "noFile", "alert('Data de lavratura maior que a data atual!');", true);
							return;
						}

						#endregion

						#region Notificacao DEC

						if (addButton1.Checked == true)
						{
							AIIM_Facade facade = new AIIM_Facade();
							this.aiim = facade.buscarCabecAIIM(parametros.IdAiim, parametros.TransactionID);

							logMessage.AppendLine("<br />Meio de notificação DEC selecionado...");

							string vCnpjCpf = srtformat_vCpf(row);

							AiimEntity ai = new AiimEntity();
							
							AIIMNotificadoEntity notificado = null;
							NotificacaoAIIMFacade notificacaoFacade = new NotificacaoAIIMFacade();
							notificado = notificacaoFacade.buscarDadosNotificavel(
										parametros.IdAiim, 
										vCnpjCpf.Replace(".", "").Replace("-", "").Replace("/", ""), 
										Convert.ToInt64(parametros.TransactionID));

							NotificacaoAIIMFacade notificacaoAIIMFacade = new NotificacaoAIIMFacade();
							AIIMNotificacaoEntity aiimNotificacaoEntity = notificacaoAIIMFacade.buscarDadosNotificacao(
										parametros.IdAiim, 
										vCnpjCpf.Replace(".", "").Replace("-", "").Replace("/", ""), 
										Convert.ToInt64(parametros.TransactionID));

							string tipoDoTributo = verificaTipoTributo(tipoTributo);
                            string numeroAinfComDv = "";
                            
                            if (tipoTributo == 5) //AINF - SIMPLES NACIONAL
                            {
                                AIIM_Facade aiim = new AIIM_Facade();

                                string numeroAIIM = parametros.aiim.NumeroAIIM.Replace("-", "").Replace(".", "");

                                numeroAinfComDv = aiim.BuscarAIIMAINFSimplesNacional(Convert.ToInt32(numeroAIIM));

                            }

							if (sendXmlToDEC)
							{
								try
								{
									logMessage.AppendLine("<br />Montando XML para enviar ao DEC...");
										
									Dictionary<string,string> dicionario = new AIIM_Facade().CreateXMLNotificacao(
											vCnpjCpf, vCpfAFR, aiimNotificacaoEntity.icms, aiimNotificacaoEntity.juros,
											aiimNotificacaoEntity.multa, aiimNotificacaoEntity.total, parametros.aiim.DataLavratura, 
											aiimNotificacaoEntity.NomeAFR, this.parametros.NumeroAiim, aiimNotificacaoEntity.drt, 
											aiimNotificacaoEntity.nf, aiimNotificacaoEntity.razaoSocial, string.Format("{0:IE}", 
											new Formatar(aiimNotificacaoEntity.ie)), aiimNotificacaoEntity.enderecoContribuinte, 
											aiimNotificacaoEntity.tributo, aiimNotificacaoEntity.nomePostoFiscalVinculacao, 
											aiimNotificacaoEntity.enderecoPostoFiscalVinculacao, aiimNotificacaoEntity.dtj, notificado.NomeNotificavel, 
											Assunto, tipoDoTributo, aiimNotificacaoEntity.municipioContribuite, aiimNotificacaoEntity.numeroFolhas,
                                            aiimNotificacaoEntity.funcionalAFR, DataPorExtenso(), parametros.aiim.NomeAutuada, numeroAinfComDv
									);

                                    dicionario.Add("Login", this.usuarioSIAP.nome);

                                    XmlDocument xmlDoc = new XmlDocument();

                                    xmlDoc.LoadXml(dicionario["xmlDoc"]);

                                    Session["xmlNotificacaoDEC"] = xmlDoc;
                                    Session["dicionarioNotificacaoDEC"] = dicionario;

                                    string key = getDataTable().DataKeys[row.DataItemIndex].Values[2].ToString();
                                    logMessage.AppendLine("<br />Conteudo da Key - nro doc contribuinte: " + key);
                                    Session[key] = dicionario;

                                    assinatura(xmlDoc, notificado.NomeNotificavel);                           

                                }
                                catch
								{
									throw;
								}
							}
							else
							{
								logMessage.AppendLine("<br />XML não foi montado para enviar ao DEC... Tipo do Tributo: " + tipoDoTributo.ToString());
							}
						}

						#endregion

						#region Notificacao OUTROS

						else
						{
							try
							{
								logMessage.AppendLine("<br />Meio de notificação OUTROS selecionado...");
									
								string vNotificacao = "";
								vNotificacao += getDataTable().DataKeys[row.DataItemIndex].Values[2].ToString() + ";Outros;|";
								logMessage.AppendLine("<br />Adicionando a vNotificação: " + vNotificacao);
								Session["vNotificacao"] += vNotificacao;
							}
							catch (Exception ex)
							{
								throw ex;
							}
						}
						#endregion

						break;

					default:
						ClientScript.RegisterStartupScript(GetType(), "alertaErrorCommandName", "alertaMensagem('Nenhum comando encontrado')");
						break;

				}

                //if (HttpContext.Current.Session["OrigemLog"] == null)
                //{
                //    ClientScript.RegisterStartupScript(GetType(), "NaoPodeDarReleaseNulo", "alertaMensagem('O contribuinte não é credenciado no DE. Erro ou Indisponibilidade no Sistema DEC')", true);
                //}

				logMessage = log.addLogMsg(logMessage);
			}
			catch (Exception ex)
			{
                pnlPorFavorAguarde.Visible = false;
				mostraErro(ex, " Erro ID:12589, Mensagem: ", logMessage);
			}    
		}

        // Adicionado método para o novo portal de assinaturas
        protected void assinatura(XmlDocument xmlDoc, String nomeNotificado)
        {
            Documento novoDoc = new Documento()
            {
                txNomeArquivo = this.parametros.Username + " " + nomeNotificado + " " + this.parametros.NumeroAiim + ".txt",
                binDocumento = Encoding.ASCII.GetBytes(xmlDoc.OuterXml),
                EnderecoIpAlteracao = HttpContext.Current.Request.UserHostAddress.Contains(":") ? "127.0.0.1" : HttpContext.Current.Request.UserHostAddress,
                ResponsavelAlteracao = this.parametros.Username,
                txDescricao1 = "AIIM: " + this.parametros.NumeroAiim,
                txDescricao2 = "XML - DEC"
            };

            novoDoc.AdicionarSignatarioArquivo(usuarioSIAP.nome, this.usuarioSIAP.CPF, this.parametros.Username + "@fazenda.sp.gov.br");
            Documento docRetorno = null;

            docRetorno = new PortalAssinatura().incluirDocumentoPortalAssinatura(novoDoc, urlServicos, urlPortal);

            //Adiciona GUID do arquivo na ViewState
            guids_arquivos_assinados.Add(docRetorno.txGuidDocumento);

            if (!string.IsNullOrEmpty(docRetorno.txGuidDocumento))
            {
                string url = Page.ResolveUrl("~/AssinaturaDocumentos/PortalAssinatura.aspx");
                String texto = String.Format("{0}/Documento/Details/guid/{1}", urlPortal, docRetorno.txGuidDocumento);
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "src", "window.open('" + url + "?url=" + texto + "', '_blank', 'width=1024,height=550');", true);
            }

            //Habilita o botão release ou mantém o mesmo desabilitado
            bool habilita = true;
            foreach (GridViewRow r in gvLista.Rows)
            {
                RadioButton addButton1 = (RadioButton)r.FindControl("rdoMeioNotificacaoDEC");
                if (addButton1.Enabled == true) { habilita = false; }
            }

            if (habilita == true) { btnRelease.Enabled = true; }

        }

        public void CancelarNotificacoesNaoAssinadasPortalAssinatura()
        {
            // fazer uma varredura no viewstate, caso haja algum não assinado, irá excluir do portal de assinaturas
            foreach (var l in this.guids_arquivos_assinados)
            {
                if (!new PortalAssinatura().verificarSeArquivoFoiAssinadoViaPortal(l, this.usuarioSIAP, this.urlServicos, this.urlPortal))
                {
                    new PortalAssinatura().cancelarDocumento(l, ConfigurationManager.AppSettings["HostServicos"], HttpContext.Current.Request.UserHostAddress.Contains(":") ? "127.0.0.1" : HttpContext.Current.Request.UserHostAddress, this.usuarioSIAP.nome);
                }
            }

        }

        protected void gvLista_RowCreated(object sender, GridViewRowEventArgs e)
		{
			try
			{
				if (e.Row.RowType == DataControlRowType.DataRow)
				{
					// Retrieve the LinkButton control from the first column.
					RadioButton addButton1 = (RadioButton)e.Row.FindControl("rdoMeioNotificacaoDEC");
					RadioButton addButton2 = (RadioButton)e.Row.FindControl("rdoMeioNotificacaoPessoal");
					Button btnAssinar = (Button)e.Row.FindControl("btnAssinarXml");

					if (this.gvLista.DataKeys[e.Row.DataItemIndex].Values[0].ToString() == "Outros")
					{
						addButton1.Checked = false; addButton2.Checked = true; btnAssinar.Enabled = false;
					}
					else
					{ 
						addButton1.Checked = true; addButton2.Checked = false; btnAssinar.Enabled = true;
					}

					if (this.gvLista.DataKeys[e.Row.DataItemIndex].Values[1].ToString() == "Sim")
					{ 
						addButton1.Enabled = true; addButton2.Enabled = true;
					}
					if (this.gvLista.DataKeys[e.Row.DataItemIndex].Values[1].ToString() == "Não")
					{ 
						addButton1.Enabled = false; addButton2.Enabled = false;
					}
				}
			}
			catch (Exception ex)
			{
				mostraErro(ex, "", null);
			}
		}

		protected void gvLista_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			
		}

		#endregion
		
	}
    
}
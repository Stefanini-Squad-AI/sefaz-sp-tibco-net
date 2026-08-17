#nullable enable

using System.Net;
using System.Net.Mail;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Porta de saída para a tarefa de e-mail "Email CQ Fechamento"
/// (nodo XPDL <c>_O-rPp1qUEfG5K7mY0I3I6w</c>, emailTask, passo 70 do POC_EpatProcess).
///
/// Declarada dentro de Integration.Soap porque a TIBCO trata tarefas de e-mail
/// como integracoes externas de transporte, da mesma forma que invocacoes SOAP.
/// </summary>
public interface IEmailCQFechamentoTask
{
    /// <summary>
    /// Envia o aviso ao coordenador de qualidade no fechamento do processo.
    /// </summary>
    /// <param name="parameters">Campos do caso necessarios para preencher o template TIBCO.</param>
    /// <param name="ct">Token de cancelamento.</param>
    Task SendAsync(EmailCQFechamentoParameters parameters, CancellationToken ct);
}

/// <summary>
/// Parametros extraidos do caso para a tarefa "Email CQ Fechamento".
/// Correspondem exactamente aos tokens declarados no modelo TIBCO:
/// AFR, COORDENADOR, LINKIPE, SW_CASEDESC.
/// </summary>
/// <param name="Afr">Numero/identificador do AFR (campo <c>AFR</c>).</param>
/// <param name="Coordenador">Login do coordenador; o TO e <c>{Coordenador}@fazenda.sp.gov.br</c>.</param>
/// <param name="LinkIpe">URL de accesso a tarefa no ePAT (campo <c>LINKIPE</c>).</param>
/// <param name="SwCaseDesc">Descricao do caso (campo <c>SW_CASEDESC</c>).</param>
public sealed record EmailCQFechamentoParameters(
    string Afr,
    string Coordenador,
    string LinkIpe,
    string SwCaseDesc);

/// <summary>
/// Opcoes de infraestrutura para a tarefa "Email CQ Fechamento".
/// Preenchidas por configuracao por ambiente — nunca literais no codigo.
/// Resolucao: rulings.SCRIPT-HARDCODED (config/glossary/POC_Epat.yaml).
/// </summary>
public sealed class EmailCQFechamentoOptions
{
    /// <summary>Hostname do servidor SMTP (obrigatorio).</summary>
    public string SmtpHost { get; init; } = string.Empty;

    /// <summary>Porta SMTP. Padrao: 25.</summary>
    public int SmtpPort { get; init; } = 25;

    /// <summary>Dominio do destinatario. Padrao: fazenda.sp.gov.br.</summary>
    public string RecipientDomain { get; init; } = "fazenda.sp.gov.br";

    /// <summary>Endereco do remetente (From).</summary>
    public string FromAddress { get; init; } = string.Empty;

    /// <summary>Nome do remetente (From display name). Opcional.</summary>
    public string FromDisplayName { get; init; } = "ePAT";

    /// <summary>
    /// Credencial SMTP, se necessaria. Null desactiva autenticacao (relay interno).
    /// Preenchida por configuracao por ambiente.
    /// </summary>
    public string? SmtpUser { get; init; }

    /// <summary>Palavra-passe SMTP. Preenchida por configuracao por ambiente.</summary>
    public string? SmtpPassword { get; init; }

    /// <summary>Activa SSL/TLS. Padrao: false (relay interno da SEFAZ).</summary>
    public bool EnableSsl { get; init; }
}

/// <summary>
/// Implementacao de <see cref="IEmailCQFechamentoTask"/> via SMTP.
///
/// O TO e construido a partir do campo <c>COORDENADOR</c> do caso
/// (<c>{Coordenador}@{RecipientDomain}</c>), em conformidade com o modelo TIBCO.
/// Nenhum endereco de e-mail e literal no codigo.
/// CC e BCC sao nulos para esta tarefa (conforme declarado em process-model.json).
///
/// Resolucao: rulings.SCRIPT-HARDCODED — CCRELATORIO/BCCRELATORIO de configuracao
/// por ambiente. Para esta tarefa especifica os campos CC/BCC estao a null no modelo
/// TIBCO; os parametros do relay SMTP (host, porta, from) vem de
/// <see cref="EmailCQFechamentoOptions"/>.
/// </summary>
public sealed class EmailCQFechamentoSmtpTask : IEmailCQFechamentoTask
{
    private readonly EmailCQFechamentoOptions _options;

    public EmailCQFechamentoSmtpTask(EmailCQFechamentoOptions options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public async Task SendAsync(EmailCQFechamentoParameters parameters, CancellationToken ct)
    {
        var to = BuildToAddress(parameters.Coordenador);
        var subject = BuildSubject(parameters.SwCaseDesc);
        var body = BuildBody(parameters);

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromDisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        message.To.Add(to);

        using var smtp = BuildSmtpClient();
        await smtp.SendMailAsync(message, ct);
    }

    // ── Template TIBCO ──────────────────────────────────────────────────────

    /// <summary>
    /// Constroi o TO: <c>{coordenador}@{RecipientDomain}</c>.
    /// O dominio vem de configuracao por ambiente (nunca literal no codigo).
    /// </summary>
    private string BuildToAddress(string coordenador) =>
        $"{coordenador}@{_options.RecipientDomain}";

    /// <summary>
    /// Template TIBCO: <c>[ePAT] - %SW_CASEDESC% - CQ Fechamento</c>
    /// </summary>
    private static string BuildSubject(string swCaseDesc) =>
        $"[ePAT] - {swCaseDesc} - CQ Fechamento";

    /// <summary>
    /// Template TIBCO (body HTML):
    /// <c>O %SW_CASEDESC% carregado pelo AFR %AFR% está em sua fila de trabalho no ePAT...</c>
    /// </summary>
    private static string BuildBody(EmailCQFechamentoParameters p) =>
        $"""
         O {p.SwCaseDesc} carregado pelo AFR {p.Afr} est&#225; em sua fila de trabalho no ePAT para que seja efetuado o controle de qualidade.
         <!DOCTYPE html>
         <html>
         <body>
         <p>
         <p>
         <a href="{p.LinkIpe}">Clique aqui</a> para verificar a tarefa.
         </body>
         </html>
         """;

    // ── Construcao do SmtpClient ────────────────────────────────────────────

    private SmtpClient BuildSmtpClient()
    {
        var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };

        if (_options.SmtpUser is { Length: > 0 } user &&
            _options.SmtpPassword is { Length: > 0 } password)
        {
            client.Credentials = new NetworkCredential(user, password);
        }

        return client;
    }
}

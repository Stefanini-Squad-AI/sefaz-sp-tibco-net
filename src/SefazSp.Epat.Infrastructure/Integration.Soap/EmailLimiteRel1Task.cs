#nullable enable

// Card: BUILD-POCEPATPROCESS-seg034
// AC5 — emailTask 'Email Limite Rel 1' (_6WNq-lqgEfG5K7mY0I3I6w, entrouPor=fluxo)
//
// Os destinatários TO/CC/BCC são calculados pelo passo anterior 'Define Destinatarios'
// e escritos em CCRELATORIO e BCCRELATORIO do caso (via DefineDestinatariosStep).
// Nenhum endereço de e-mail é literal no código (rulings.HARDCODED-VALUES).
// Os parâmetros de relay SMTP vêm de EmailLimiteRel1Options.

using System.Net;
using System.Net.Mail;
using SefazSp.Epat.Application.Workflows.PocEpatProcess;

namespace SefazSp.Epat.Infrastructure.Integration.Soap;

/// <summary>
/// Opções de infraestrutura para a tarefa 'Email Limite Rel 1'.
/// Preenchidas por configuração por ambiente — nunca literais no código.
/// Resolução: rulings.HARDCODED-VALUES (config/glossary/POC_Epat.yaml).
/// </summary>
public sealed class EmailLimiteRel1Options
{
    /// <summary>Hostname do servidor SMTP (obrigatório).</summary>
    public string SmtpHost { get; init; } = string.Empty;

    /// <summary>Porta SMTP. Padrão: 25.</summary>
    public int SmtpPort { get; init; } = 25;

    /// <summary>Endereço do remetente (From).</summary>
    public string FromAddress { get; init; } = string.Empty;

    /// <summary>Nome do remetente (From display name). Opcional.</summary>
    public string FromDisplayName { get; init; } = "ePAT";

    /// <summary>
    /// Credencial SMTP, se necessária. Null desactiva autenticação (relay interno).
    /// </summary>
    public string? SmtpUser { get; init; }

    /// <summary>Palavra-passe SMTP. Preenchida por configuração por ambiente.</summary>
    public string? SmtpPassword { get; init; }

    /// <summary>Activa SSL/TLS. Padrão: false (relay interno da SEFAZ).</summary>
    public bool EnableSsl { get; init; }

    /// <summary>
    /// Endereço TO principal do e-mail de limite.
    /// Preenchido por configuração por ambiente — nunca literal no código.
    /// </summary>
    public string ToAddress { get; init; } = string.Empty;
}

/// <summary>
/// Implementação de <see cref="IEmailLimiteRel1Task"/> via SMTP.
///
/// Os endereços CC e BCC vêm dos parâmetros (calculados por DefineDestinatariosStep
/// a partir de configuração por ambiente). O TO vem de <see cref="EmailLimiteRel1Options"/>.
/// Nenhum endereço de e-mail é literal no código.
///
/// Resolução: rulings.HARDCODED-VALUES — CCRELATORIO/BCCRELATORIO de configuração
/// por ambiente, decidido em card BUILD-POCEPATPROCESS-seg034, AC4.
/// </summary>
public sealed class EmailLimiteRel1SmtpTask : IEmailLimiteRel1Task
{
    private readonly EmailLimiteRel1Options _options;

    public EmailLimiteRel1SmtpTask(EmailLimiteRel1Options options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public async Task SendAsync(EmailLimiteRel1Parameters parameters, CancellationToken ct)
    {
        var subject = BuildSubject(parameters.SwCaseDesc);
        var body = BuildBody(parameters);

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromDisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        message.To.Add(_options.ToAddress);

        if (!string.IsNullOrWhiteSpace(parameters.CcRelatorio))
            message.CC.Add(parameters.CcRelatorio);

        if (!string.IsNullOrWhiteSpace(parameters.BccRelatorio))
            message.Bcc.Add(parameters.BccRelatorio);

        using var smtp = BuildSmtpClient();
        await smtp.SendMailAsync(message, ct);
    }

    // ── Template TIBCO ──────────────────────────────────────────────────────

    /// <summary>
    /// Template de assunto: <c>[ePAT] - %SW_CASEDESC% - Limite Rel 1</c>
    /// </summary>
    private static string BuildSubject(string swCaseDesc) =>
        $"[ePAT] - {swCaseDesc} - Limite Rel 1";

    /// <summary>
    /// Template HTML do corpo (baseado no padrão observado no pacote POC_Epat).
    /// </summary>
    private static string BuildBody(EmailLimiteRel1Parameters p) =>
        $"""
         <!DOCTYPE html>
         <html>
         <body>
         <p>O caso {p.SwCaseDesc} atingiu o limite de Rela&#231;&#227;o 1 no ePAT.</p>
         <p><a href="{p.LinkIpe}">Clique aqui</a> para verificar a tarefa.</p>
         </body>
         </html>
         """;

    // ── Construção do SmtpClient ────────────────────────────────────────────

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

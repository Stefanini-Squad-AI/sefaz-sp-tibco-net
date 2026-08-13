#nullable enable

using SefazSp.Epat.Application.Execution;

namespace SefazSp.Epat.Application.Execution;

/// <summary>
/// Envelope técnico do scriptTask HoraFimSC (_lrer3VqhEfG5K7mY0I3I6w).
///
/// AC3 — separação regra/envelope:
///   • A lógica de cálculo (regra de domínio) fica em Domain/Rules como função pura
///     (IHoraFimScRule, a criar pelo implementador-rules).
///   • Este executor lida APENAS com o envelope técnico: STATUS_CODE, contadores
///     de retentativa e mapeamento para ProcessExecutionContext.
///
/// Builtin iProcess relevante: IPEDateTimeUtil.CALCTIME — traduzido via
/// shim-tri-state (FieldValue&lt;T&gt;) conforme NOEQ-iprocess-builtin (ratificado 2026-08-06).
/// DATACONTROLE usa FieldValue&lt;DateOnly&gt; (sentinelaSwNa=true).
/// PRAZODEFESA usa DateOnly simples (sentinelaSwNa=false).
///
/// Rastreia: checklist ordem 3 (_lrer3VqhEfG5K7mY0I3I6w, entrouPor=fluxo, gap iprocess-builtin=decided)
/// Processo: DEAT0050 · Segmento: BUILD-DEAT0050-seg009
/// </summary>
public sealed class HoraFimScExecution
{
    private readonly IHoraFimScRule _rule;

    public HoraFimScExecution(IHoraFimScRule rule)
    {
        _rule = rule;
    }

    /// <summary>
    /// Executa o scriptTask HoraFimSC:
    /// 1. Delega o cálculo de prazo para a regra de domínio pura.
    /// 2. Aplica o resultado ao contexto técnico (STATUS_CODE, contadores).
    /// </summary>
    public HoraFimScResult Execute(HoraFimScInput input, ProcessExecutionContext ctx)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(ctx);

        // Delegar cálculo ao domínio (função pura, sem efeitos laterais).
        var ruleResult = _rule.Calculate(input.DataControleSemana, input.PrazoBase);

        // Actualizar envelope técnico com o resultado.
        ctx.STATUS_CODE = ruleResult.Success ? "0" : ruleResult.ErrorCode;

        return new HoraFimScResult(
            Prazodefesa: ruleResult.Prazodefesa,
            Prazodefesat: ruleResult.Prazodefesat,
            StatusCode: ctx.STATUS_CODE);
    }
}

/// <summary>
/// Entradas do scriptTask HoraFimSC, preparadas a partir do caso AIIM.
/// DATACONTROLE é tri-estado (FieldValue) — sentinelaSwNa=true.
/// </summary>
public sealed record HoraFimScInput(
    SefazSp.Epat.Domain.ValueObjects.FieldValue<DateOnly> DataControleSemana,
    DateOnly PrazoBase);

/// <summary>
/// Saída do scriptTask HoraFimSC após execução.
/// </summary>
public sealed record HoraFimScResult(
    DateOnly Prazodefesa,
    TimeOnly Prazodefesat,
    string? StatusCode);

/// <summary>
/// Contrato da regra de domínio pura para HoraFimSC.
/// Implementação concreta em Domain/Rules (implementador-rules).
/// Não contém lógica de envelope técnico.
/// </summary>
public interface IHoraFimScRule
{
    HoraFimScRuleResult Calculate(
        SefazSp.Epat.Domain.ValueObjects.FieldValue<DateOnly> dataControle,
        DateOnly prazoBase);
}

/// <summary>
/// Resultado da regra de domínio pura de HoraFimSC.
/// </summary>
public sealed record HoraFimScRuleResult(
    bool Success,
    DateOnly Prazodefesa,
    TimeOnly Prazodefesat,
    string? ErrorCode = null);

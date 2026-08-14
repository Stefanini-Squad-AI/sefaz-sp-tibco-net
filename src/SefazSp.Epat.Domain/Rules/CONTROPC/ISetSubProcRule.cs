#nullable enable

// Card: BUILD-CONTROPC-seg045
// AC4 — scriptTask 'ISetSubProc' (_-bkw-F6JEfGBBLgT-R5iuw, entrouPor=fluxo)
//
// Classificação (rule-catalogue.json · RI-script-CONTROPC-ISetSubProc):
//   eRegraDeNegocio=true · efeito=calcula-valor
//   → lógica de domínio pura em Domain/Rules.
//   → STATUS_CODE e contadores de retentativa permanecem em Application/Execution (não presentes
//     neste script).
//
// Script legado (POC_Epat.xpdl, linha 8554 / CONTROPC__MAIN.bpmn _-bkw-F6JEfGBBLgT-R5iuw):
//
//   Comentário legado do script:
//     idmotivos7
//     1 - Decisao  2 - Vicio  3 - Diligencia
//     iddeciaodebito: 1 mantido  2 reduzido  3 cancelado
//
//   Lógica transcrita (ver Apply abaixo).
//
// Campos lidos :  IDMOTIVOINTIMAC, IDDECISAODEBITO, DEFESAADMITIDA, RECURSOOFICIO,
//                 IDTIPOIMPUGNACA, CRCONTRIBUINTE, DILIGENCIA, DSTIPOINTIMACAO,
//                 DTCIENCIA (FieldValue — pode ser SW_NA), DTPUBLICACAODE, NOVOMODELO
// Campos escritos: AGUARDAR, PROCRETORNO, DTCIENCIA
//
// NOEQ-iprocess-builtin (shim-tri-state, ratificado 2026-08-06):
//   DTCIENCIA é comparada com SW_NA — terceiro estado distinto de null e de vazio.
//   Usa FieldValue<DateOnly>.IsNotAvailable; SW_NA nunca é mapeado para null.

using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Domain.Rules.CONTROPC;

/// <summary>
/// Regra de domínio para o scriptTask 'ISetSubProc'
/// (<c>_-bkw-F6JEfGBBLgT-R5iuw</c>) do processo CONTROPC.
///
/// Determina o subprocesso de retorno (<c>PROCRETORNO</c>) e a fila de espera
/// (<c>AGUARDAR</c>) com base nas condições de negócio do caso.
/// Função pura: não depende de relógio, I/O nem estado externo.
///
/// <para>
/// <b>Decisão NOEQ-iprocess-builtin (shim-tri-state):</b>
/// <c>DTCIENCIA</c> usa <see cref="FieldValue{T}"/> para distinguir o estado SW_NA
/// (não disponível) de null e de vazio. Colapsar SW_NA em null trocaria o ramo que
/// dispara sem erro de compilação nem teste vermelho.
/// </para>
/// </summary>
public static class ISetSubProcRule
{
    /// <summary>
    /// Identificador da regra de instância — invariante: não renomear.
    /// </summary>
    public const string RuleId = "RI-script-CONTROPC-ISetSubProc";

    /// <summary>
    /// Aplica a regra 'ISetSubProc' ao caso.
    ///
    /// <para>
    /// Comportamento transcrito do script legado (CONTROPC__MAIN.bpmn, nó <c>_-bkw-F6JEfGBBLgT-R5iuw</c>):
    /// <list type="bullet">
    ///   <item>
    ///     <b>IDMOTIVOINTIMAC == 1 (Decisão):</b> selecciona subprocesso com base em
    ///     IDDECISAODEBITO, DEFESAADMITIDA, RECURSOOFICIO e IDTIPOIMPUGNACA.
    ///   </item>
    ///   <item>
    ///     <b>IDMOTIVOINTIMAC == 2 (Vício):</b> subprocesso = "AgPecas".
    ///   </item>
    ///   <item>
    ///     <b>CRCONTRIBUINTE == 1:</b> subprocesso = "AgRCRaz" ou "AgCRaz" consoante
    ///     IDDECISAODEBITO e RECURSOOFICIO.
    ///   </item>
    ///   <item>
    ///     <b>DILIGENCIA == true:</b> subprocesso = "AgPetica".
    ///   </item>
    ///   <item>
    ///     Após selecção: <c>PROCRETORNO = AGUARDAR[0]</c>.
    ///   </item>
    ///   <item>
    ///     <b>DSTIPOINTIMACAO == "DE" ou DTCIENCIA == SW_NA:</b>
    ///     <c>DTCIENCIA = DTPUBLICACAODE</c> (ciência assume data de publicação).
    ///     Shim-tri-state: SW_NA é distinto de null/vazio — usa
    ///     <see cref="FieldValue{T}.IsNotAvailable"/>.
    ///   </item>
    ///   <item>
    ///     <b>NOVOMODELO == true:</b> substitui selecção anterior por "AgPecas".
    ///   </item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="aiimCase">Estado de negócio mutável do caso.</param>
    public static void Apply(AiimCase aiimCase)
    {
        // ── Selecção do subprocesso de retorno ─────────────────────────────────
        // AGUARDAR é uma lista; o legado escreve apenas no índice [0].
        // Usa uma variável local e escreve de volta no final de cada bloco.

        string? aguardar0 = null;

        // ── Bloco 1: IDMOTIVOINTIMAC == 1 (Decisão) ──────────────────────────
        if (aiimCase.IDMOTIVOINTIMAC == 1)
        {
            // Debitado cancelado (3) ou sem decisão (0) com defesa admitida → aguarda PRJ
            if ((aiimCase.IDDECISAODEBITO == 3 || aiimCase.IDDECISAODEBITO == 0)
                && aiimCase.DEFESAADMITIDA)
            {
                aguardar0 = "AgPRJ";
            }
            // Sem decisão (0) com defesa não admitida → aguarda recurso ao PRJ
            else if (aiimCase.IDDECISAODEBITO == 0 && !aiimCase.DEFESAADMITIDA)
            {
                aguardar0 = "AgRecPRJ";
            }

            // Mantido (1) ou reduzido (2): subprocesso depende de recurso de ofício
            if (aiimCase.IDDECISAODEBITO == 1 || aiimCase.IDDECISAODEBITO == 2)
            {
                if (aiimCase.RECURSOOFICIO)
                {
                    aguardar0 = "AgPRJ";
                }
                else
                {
                    aguardar0 = "AgRecPRJ";
                }
            }

            // Tipo de impugnação 4 → aguarda PRJ com rito próprio
            // Nota legado: "falta implementar regras para PRJ"
            if (aiimCase.IDTIPOIMPUGNACA == "4")
            {
                aguardar0 = "AgPRJR";
            }
        }

        // ── Bloco 2: IDMOTIVOINTIMAC == 2 (Vício) ────────────────────────────
        if (aiimCase.IDMOTIVOINTIMAC == 2)
        {
            aguardar0 = "AgPecas";
        }

        // ── Bloco 3: CRCONTRIBUINTE == 1 (contribuinte do CRaz) ──────────────
        if (aiimCase.CRCONTRIBUINTE == 1)
        {
            // Legado: IDDECISAODEBITO == 1 || IDDECISAODEBITO == 2 && RECURSOOFICIO==true
            // A precedência iProcess avalia && antes de ||; reproduzida aqui com parênteses
            // explícitos para legibilidade e correctude.
            if (aiimCase.IDDECISAODEBITO == 1
                || (aiimCase.IDDECISAODEBITO == 2 && aiimCase.RECURSOOFICIO))
            {
                aguardar0 = "AgRCRaz";
            }
            else
            {
                aguardar0 = "AgCRaz";
            }
        }

        // ── Bloco 4: DILIGENCIA == true ───────────────────────────────────────
        if (aiimCase.DILIGENCIA)
        {
            aguardar0 = "AgPetica";
        }

        // Escreve AGUARDAR[0] e PROCRETORNO com o valor seleccionado
        if (aguardar0 is not null)
        {
            aiimCase.AGUARDAR = [aguardar0];
            aiimCase.PROCRETORNO = aguardar0;
        }

        // ── DTCIENCIA: SW_NA → data de publicação ─────────────────────────────
        // Decisão NOEQ-iprocess-builtin (shim-tri-state, 2026-08-06):
        // DTCIENCIA == IPESystemValues.SW_NA é o terceiro estado IsNotAvailable;
        // não é null nem vazio — colapsar causaria troca silenciosa de ramo.
        bool dtCienciaIsSwNa = aiimCase.DTCIENCIA.IsNotAvailable;
        bool tipoIsDE = aiimCase.DSTIPOINTIMACAO == "DE";

        if (tipoIsDE || dtCienciaIsSwNa)
        {
            // Ciência assume data de publicação do Diário Eletrônico.
            aiimCase.DTCIENCIA = FieldValue<DateOnly>.Of(aiimCase.DTPUBLICACAODE);
        }

        // ── Bloco final: NOVOMODELO == true (substitui selecção anterior) ─────
        if (aiimCase.NOVOMODELO)
        {
            aiimCase.AGUARDAR = ["AgPecas"];
            aiimCase.PROCRETORNO = "AgPecas";
        }
    }
}

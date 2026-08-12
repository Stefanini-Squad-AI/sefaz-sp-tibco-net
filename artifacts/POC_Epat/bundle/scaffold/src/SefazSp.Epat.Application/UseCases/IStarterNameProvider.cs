// Shim para IPEStarterUtil.GETATTRIBUTE("Name").
// Decisão: shim-tri-state (gaps.iprocess-builtin = shim-tri-state, ratificado 2026-08-06).
#nullable enable

using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.UseCases;

/// <summary>
/// Shim para IPEStarterUtil.GETATTRIBUTE("Name") do iProcess.
/// Retorna o nome do utilizador que iniciou o caso como FieldValue&lt;string&gt;,
/// preservando o estado tri-estado: HasValue / IsNotAvailable (SW_NA) / Empty.
/// Decisão: shim-tri-state ratificado em gaps.iprocess-builtin (2026-08-06).
/// </summary>
public interface IStarterNameProvider
{
    FieldValue<string> GetStarterName();
}

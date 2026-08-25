#nullable enable

using Microsoft.EntityFrameworkCore;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Infrastructure.Persistence;
using SefazSp.Epat.Infrastructure.Persistence.Serialization;

namespace SefazSp.Epat.Infrastructure.Runtime;

/// <summary>
/// Estado do fluxo principal POC_EpatProcess que sobrevive às 5 suspensões do percurso SC-001.
/// <see cref="Path"/> acumula os ids de nó na ordem de travessia — comparado ao oráculo SC-001.
/// </summary>
public sealed class PocEpatMainSnapshot
{
    // Parameterless ctor for the JSON round-trip; the (idAiim, processId) ctor is used at runtime.
    public PocEpatMainSnapshot() { }

    public PocEpatMainSnapshot(long idAiim, string processId)
    {
        IdAiim = idAiim;
        ProcessId = processId;
    }

    public long IdAiim { get; set; }
    public string ProcessId { get; set; } = default!;
    public AiimCase Case { get; set; } = new();
    public List<string> Path { get; set; } = new();

    /// <summary>Nome do AFR submetido em 'Finalizar AIIM' (aplicado no callback como GETATTRIBUTE("Name")).</summary>
    public string? PendingAfrName { get; set; }

    /// <summary>Guarda da corrida DRF em 'Pedido de Vistas' (evento externo ⇄ timer de fronteira).</summary>
    public int RaceResolved;

    /// <summary>Ramo 'Existe Notificação? = Sim' tomado (curto-circuito SC-014).</summary>
    public bool ExisteNotificacaoSim;

    /// <summary>Ramo 'Corrigir? = No' tomado (Criar Notificacao → endEvent, SC-015).</summary>
    public bool CorrigirNo;

    /// <summary>Node 13 em modo graft-real (pai estaciona; filhos DEAT0050 anexam) em vez de descida única.</summary>
    public bool GraftMode;

    /// <summary>Node 18: PRPINTPC devolve erro de aplicação na 1ª tentativa (erro → operador → retry).</summary>
    public bool PrpintpcFails;

    /// <summary>Tentativa corrente de PRPINTPC (para o laço de operador).</summary>
    public int PrpintpcAttempt;

    /// <summary>Atributos de entrada do motor Decisions (CaptaParametros) — por nome normalizado.</summary>
    public Dictionary<string, string?> DecisionsSeed { get; set; } = new(StringComparer.Ordinal);
}

/// <summary>Guarda de forma durável o snapshot do fluxo principal POC_EpatProcess, por PROCESS_ID.</summary>
public sealed class PocEpatProcessState
{
    /// <summary>Store kind desta unidade no documento durável (StoreKind, PROCESS_ID).</summary>
    public const string StoreKind = "poc-epat-main";

    private readonly IDbContextFactory<EpatRuntimeDbContext> _factory;

    public PocEpatProcessState(IDbContextFactory<EpatRuntimeDbContext> factory) => _factory = factory;

    public void Save(string correlationKey, PocEpatMainSnapshot snapshot)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(snapshot, EpatJsonSerialization.Options);
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        if (row is null)
            db.Snapshots.Add(new EpatSnapshotRow
            {
                StoreKind = StoreKind,
                ProcessId = correlationKey,
                DocumentJson = json,
                Version = 1,
            });
        else
        {
            row.DocumentJson = json;
            row.Version++; // compare-and-swap durável (substitui os guards Interlocked em RAM)
        }
        db.SaveChanges();
    }

    public PocEpatMainSnapshot? Load(string correlationKey)
    {
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        return row is null
            ? null
            : System.Text.Json.JsonSerializer.Deserialize<PocEpatMainSnapshot>(row.DocumentJson, EpatJsonSerialization.Options);
    }

    public void Clear(string correlationKey)
    {
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        if (row is null) return;
        db.Snapshots.Remove(row);
        db.SaveChanges();
    }
}

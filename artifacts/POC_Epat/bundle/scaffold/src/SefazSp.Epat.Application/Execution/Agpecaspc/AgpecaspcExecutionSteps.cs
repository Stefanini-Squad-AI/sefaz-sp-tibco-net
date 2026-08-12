#nullable enable

using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Domain.ValueObjects;

namespace SefazSp.Epat.Application.Execution.Agpecaspc;

public sealed class SetPrazoStep
{
    public void Execute(AiimCase aiimCase)
    {
        ArgumentNullException.ThrowIfNull(aiimCase);
        aiimCase.DATACONTROLE = FieldValue<DateOnly>.Of(aiimCase.PRAZORECEBIMENT);
    }
}

public sealed class SetFlagDecursoStep
{
    public void Execute(AiimCase aiimCase)
    {
        ArgumentNullException.ThrowIfNull(aiimCase);
        aiimCase.FLGTERMODEC = true;
    }
}

public sealed class ControlaDatasStep
{
    private readonly IClock _clock;

    public ControlaDatasStep(IClock clock)
    {
        _clock = clock;
    }

    public void Execute(AiimCase aiimCase)
    {
        ArgumentNullException.ThrowIfNull(aiimCase);
        aiimCase.DATACONTROLE = FieldValue<DateOnly>.Of(DateOnly.FromDateTime(_clock.Now.LocalDateTime));
    }
}

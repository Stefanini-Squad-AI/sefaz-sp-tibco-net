#nullable enable

using SefazSp.Epat.Domain.Abstractions;

namespace SefazSp.Epat.Infrastructure.Runtime;

/// <summary>Relógio do sistema. Nunca se usa DateTime.Now directamente no domínio/aplicação.</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
    public TimeZoneInfo TimeZone => TimeZoneInfo.Local;
}

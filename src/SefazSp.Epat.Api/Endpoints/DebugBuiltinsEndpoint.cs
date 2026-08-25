#nullable enable

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SefazSp.Epat.Application.Abstractions.Legacy;

namespace SefazSp.Epat.Api.Endpoints;

/// <summary>
/// Debug-only endpoint: proves the anticorruption shim against the behavioral oracle
/// VEC-TOKENISE-PIPE-LIST (builtin-contract.json). Tokenizes '278713|278712|' using only
/// the base-1 SEARCH/SUBSTR/STRLEN primitives and asserts ['278713','278712'].
/// This is the oracle that authorizes the base-1 choice without TIBCO documentation.
/// </summary>
public static class DebugBuiltinsEndpoint
{
    public static IEndpointRouteBuilder MapDebugBuiltins(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/debug/builtins/tokenize", Handle)
              .WithName("Debug-Builtins-Tokenize")
              .WithTags("Debug")
              .WithSummary("Proves the iProcess builtins shim against VEC-TOKENISE-PIPE-LIST");
        return routes;
    }

    private static IResult Handle(DebugTokenizeRequest request, IProcessBuiltins builtins)
    {
        var input = string.IsNullOrEmpty(request.Value) ? "278713|278712|" : request.Value;

        var tokens = Tokenize(builtins, input, separator: "|");

        var expected = new[] { "278713", "278712" };
        var oraclePasses = input == "278713|278712|" && tokens.SequenceEqual(expected);

        return Results.Ok(new
        {
            Input = input,
            Tokens = tokens,
            OracleVector = "VEC-TOKENISE-PIPE-LIST",
            OraclePasses = oraclePasses,
            IndexBase = "1 (base-1, oracle-forced; pending rulings.BUILTIN-SEMANTICS)"
        });
    }

    // Classic iProcess pipe tokenizer built on the base-1 primitives.
    private static List<string> Tokenize(IProcessBuiltins b, string source, string separator)
    {
        var tokens = new List<string>();
        var rest = source;

        while (b.StrLen(rest) > 0)
        {
            var pos = b.Search(separator, rest);   // 1-based position of next separator
            if (pos == 0)
                break;                             // no more separators

            tokens.Add(b.Substr(rest, 1, pos - 1)); // token before the separator
            rest = b.Substr(rest, pos + 1, b.StrLen(rest) - pos); // remainder after it
        }

        return tokens;
    }
}

public sealed record DebugTokenizeRequest(string? Value);

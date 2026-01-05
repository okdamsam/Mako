using System.Linq;
using Content.Server.Administration;
using Content.Shared._Mako.Ranks;
using Content.Shared.Administration;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Errors;
using Robust.Shared.Toolshed.Syntax;

namespace Content.Server._Mako.Ranks.Commands;

/// <summary>
/// Toolshed commands for managing entity ranks.
/// </summary>
[ToolshedCommand, AdminCommand(AdminFlags.Admin)]
public sealed class RankCommand : ToolshedCommand
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [CommandImplementation("get")]
    public string? GetRank([PipedArgument] EntityUid entity)
    {
        if (!TryComp<RankComponent>(entity, out var rank))
            return null;

        return rank.RankId;
    }

    [CommandImplementation("set")]
    public EntityUid SetRank(
        [CommandInvocationContext] IInvocationContext ctx,
        [PipedArgument] EntityUid entity,
        [CommandArgument] string rankId)
    {
        if (!_prototypeManager.HasIndex<RankPrototype>(rankId))
        {
            ctx.WriteLine($"Invalid rank prototype: {rankId}");
            return entity;
        }

        var rankSystem = GetSys<RankSystem>();
        rankSystem.SetEntityRank(entity, rankId);
        return entity;
    }

    [CommandImplementation("remove")]
    public EntityUid RemoveRank([PipedArgument] EntityUid entity)
    {
        var rankSystem = GetSys<RankSystem>();
        rankSystem.RemoveEntityRank(entity);
        return entity;
    }

    [CommandImplementation("list")]
    public IEnumerable<string> ListRanks()
    {
        return _prototypeManager
            .EnumeratePrototypes<RankPrototype>()
            .Select(p => p.ID)
            .OrderBy(id => id);
    }

    [CommandImplementation("listdetail")]
    public IEnumerable<string> ListRanksDetailed()
    {
        return _prototypeManager
            .EnumeratePrototypes<RankPrototype>()
            .OrderBy(p => p.PayGrade)
            .Select(p => $"{p.ID} ({p.Prefix}) - {p.Name} [{p.Grade}] - {p.Category}");
    }

    [CommandImplementation("info")]
    public string? GetRankInfo(
        [CommandInvocationContext] IInvocationContext ctx,
        [CommandArgument] string rankId)
    {
        if (!_prototypeManager.TryIndex<RankPrototype>(rankId, out var rank))
        {
            ctx.WriteLine($"Invalid rank prototype: {rankId}");
            return null;
        }

        return $"Rank: {rank.Name}\n" +
               $"Prefix: {rank.Prefix}\n" +
               $"Grade: {rank.Grade}\n" +
               $"Pay Grade: {rank.PayGrade}\n" +
               $"Category: {rank.Category}\n" +
               $"Description: {rank.Description}";
    }
}

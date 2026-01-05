using Content.Shared.Eui;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._Mako.Ranks;

[Serializable, NetSerializable]
public sealed class RankPermissionsEuiState : EuiStateBase
{
    public List<PlayerRankInfo> Players { get; }

    public RankPermissionsEuiState(List<PlayerRankInfo> players)
    {
        Players = players;
    }
}

[Serializable, NetSerializable]
public sealed class PlayerRankInfo
{
    public NetUserId UserId { get; }
    public string Username { get; }
    public int? MaxRankPayGrade { get; }

    public PlayerRankInfo(NetUserId userId, string username, int? maxRankPayGrade)
    {
        UserId = userId;
        Username = username;
        MaxRankPayGrade = maxRankPayGrade;
    }
}

[Serializable, NetSerializable]
public sealed class SetMaxRankMessage : EuiMessageBase
{
    public NetUserId UserId { get; }
    public int? MaxRankPayGrade { get; }

    public SetMaxRankMessage(NetUserId userId, int? maxRankPayGrade)
    {
        UserId = userId;
        MaxRankPayGrade = maxRankPayGrade;
    }
}

using System.Threading;
using Content.Server.Database;
using Content.Server.EUI;
using Content.Server.Preferences.Managers;
using Content.Shared._Mako.Ranks;
using Content.Shared.Eui;
using Robust.Server.Player;
using Robust.Shared.Network;

namespace Content.Server._Mako.Ranks;

public sealed class RankPermissionsEui : BaseEui
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IServerPreferencesManager _prefsManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private List<PlayerRankInfo> _players = new();

    public RankPermissionsEui()
    {
        IoCManager.InjectDependencies(this);
    }

    public override void Opened()
    {
        base.Opened();
        RefreshState();
    }

    private async void RefreshState()
    {
        _players.Clear();
        
        // Get all preferences from the database
        var allPrefs = await _db.GetAllPreferences();
        
        foreach (var pref in allPrefs)
        {
            // Get actual username from player record
            var playerRecord = await _db.GetPlayerRecordByUserId(pref.Key, CancellationToken.None);
            var username = playerRecord?.LastSeenUserName ?? pref.Key.ToString();
            _players.Add(new PlayerRankInfo(pref.Key, username, pref.Value.MaxRankPayGrade));
        }

        // Sort by username
        _players.Sort((a, b) => string.Compare(a.Username, b.Username, StringComparison.OrdinalIgnoreCase));

        StateDirty();
    }

    public override EuiStateBase GetNewState()
    {
        return new RankPermissionsEuiState(_players);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is SetMaxRankMessage setMsg)
        {
            SetMaxRank(setMsg.UserId, setMsg.MaxRankPayGrade);
        }
    }

    private async void SetMaxRank(NetUserId userId, int? maxRankPayGrade)
    {
        await _db.SetMaxRankPayGrade(userId, maxRankPayGrade);
        
        // If the player is online, refresh their preferences to send the update
        if (_playerManager.TryGetSessionById(userId, out var session))
        {
            await _prefsManager.RefreshPreferencesAsync(session, CancellationToken.None);
        }
        
        RefreshState();
    }
}

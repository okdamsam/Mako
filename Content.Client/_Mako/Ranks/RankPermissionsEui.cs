using System.Linq;
using System.Numerics;
using Content.Client.Eui;
using Content.Shared._Mako.Ranks;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Client._Mako.Ranks;

[UsedImplicitly]
public sealed class RankPermissionsEui : BaseEui
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    
    private readonly RankPermissionsWindow _window;
    private BoxContainer? _playerList;

    public RankPermissionsEui()
    {
        IoCManager.InjectDependencies(this);
        _window = new RankPermissionsWindow(_prototypeManager);
        _window.OnClose += () => SendMessage(new CloseEuiMessage());
        _window.OnSetMaxRank += (userId, payGrade) => SendMessage(new SetMaxRankMessage(userId, payGrade));
    }

    public override void Opened()
    {
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is RankPermissionsEuiState rankState)
        {
            _window.UpdateState(rankState);
        }
    }
}

public sealed class RankPermissionsWindow : DefaultWindow
{
    private readonly IPrototypeManager _prototypeManager;
    private readonly BoxContainer _playerList;
    private readonly LineEdit _searchBox;
    private List<PlayerRankInfo> _allPlayers = new();
    
    public event Action<NetUserId, int?>? OnSetMaxRank;

    public RankPermissionsWindow(IPrototypeManager prototypeManager)
    {
        _prototypeManager = prototypeManager;
        
        Title = "Rank Permissions";
        SetSize = new Vector2(800, 600);

        var vbox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical
        };

        // Search box
        var searchBox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            Margin = new Thickness(4, 4, 4, 8)
        };
        searchBox.AddChild(new Label { Text = "Search: ", Margin = new Thickness(0, 0, 4, 0) });
        _searchBox = new LineEdit { HorizontalExpand = true, PlaceHolder = "Enter username..." };
        _searchBox.OnTextChanged += _ => FilterPlayers();
        searchBox.AddChild(_searchBox);
        vbox.AddChild(searchBox);

        var headerBox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            Margin = new Thickness(4)
        };

        headerBox.AddChild(new Label { Text = "Username", MinWidth = 200 });
        headerBox.AddChild(new Label { Text = "User ID", MinWidth = 250 });
        headerBox.AddChild(new Label { Text = "Max Rank", MinWidth = 150 });
        headerBox.AddChild(new Label { Text = "Actions", MinWidth = 150 });

        vbox.AddChild(headerBox);

        _playerList = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            VerticalExpand = true
        };

        var scrollContainer = new ScrollContainer
        {
            VerticalExpand = true,
            Children = { _playerList }
        };

        vbox.AddChild(scrollContainer);

        Contents.AddChild(vbox);
    }

    public void UpdateState(RankPermissionsEuiState state)
    {
        _allPlayers = state.Players.ToList();
        FilterPlayers();
    }

    private void FilterPlayers()
    {
        _playerList.RemoveAllChildren();
        
        var searchTerm = _searchBox.Text.Trim().ToLowerInvariant();
        var filteredPlayers = string.IsNullOrEmpty(searchTerm)
            ? _allPlayers
            : _allPlayers.Where(p => p.Username.ToLowerInvariant().Contains(searchTerm)).ToList();

        foreach (var player in filteredPlayers)
        {
            var hbox = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                Margin = new Thickness(2)
            };

            hbox.AddChild(new Label 
            { 
                Text = player.Username, 
                MinWidth = 200,
                ClipText = true
            });

            hbox.AddChild(new Label 
            { 
                Text = player.UserId.ToString(), 
                MinWidth = 250,
                ClipText = true 
            });

            var maxRankText = player.MaxRankPayGrade.HasValue 
                ? GetRankNameForPayGrade(player.MaxRankPayGrade.Value) 
                : "None";

            hbox.AddChild(new Label 
            { 
                Text = maxRankText, 
                MinWidth = 150 
            });

            var buttonBox = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                MinWidth = 150
            };

            var setButton = new Button { Text = "Set" };
            setButton.OnPressed += _ => OpenSetRankDialog(player.UserId, player.MaxRankPayGrade);
            buttonBox.AddChild(setButton);

            if (player.MaxRankPayGrade.HasValue)
            {
                var clearButton = new Button { Text = "Clear" };
                clearButton.OnPressed += _ => OnSetMaxRank?.Invoke(player.UserId, null);
                buttonBox.AddChild(clearButton);
            }

            hbox.AddChild(buttonBox);

            _playerList!.AddChild(hbox);
        }
    }

    private void OpenSetRankDialog(NetUserId userId, int? currentPayGrade)
    {
        var dialog = new DefaultWindow { Title = "Set Max Rank" };
        dialog.SetSize = new Vector2(400, 500);

        var vbox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new Thickness(8)
        };

        vbox.AddChild(new Label { Text = "Select maximum rank pay grade:" });

        var scrollContainer = new ScrollContainer { VerticalExpand = true };
        var rankList = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            VerticalExpand = true
        };

        var ranks = _prototypeManager.EnumeratePrototypes<RankPrototype>()
            .OrderBy(r => r.PayGrade)
            .ToList();

        foreach (var rank in ranks)
        {
            var button = new Button
            {
                Text = $"{rank.Name} ({rank.Grade}) - Pay Grade {rank.PayGrade}",
                HorizontalExpand = true
            };
            button.OnPressed += _ =>
            {
                OnSetMaxRank?.Invoke(userId, rank.PayGrade);
                dialog.Close();
            };
            rankList.AddChild(button);
        }

        scrollContainer.AddChild(rankList);
        vbox.AddChild(scrollContainer);

        dialog.Contents.AddChild(vbox);
        dialog.OpenCentered();
    }

    private string GetRankNameForPayGrade(int payGrade)
    {
        var rank = _prototypeManager.EnumeratePrototypes<RankPrototype>()
            .FirstOrDefault(r => r.PayGrade == payGrade);
        
        return rank != null ? $"{rank.Name} (PG{payGrade})" : $"Pay Grade {payGrade}";
    }
}

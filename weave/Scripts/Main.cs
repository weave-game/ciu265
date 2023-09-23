using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using weave.InputHandlers;
using weave.Utils;

namespace weave;

internal enum ControllerTypes
{
    Keyboard,
    Controller // TODO: implement
}

public partial class Main : Node2D
{
    private readonly ISet<Player> _players = new HashSet<Player>();
    private readonly int _nPlayers = 1;
    private const int LineWidth = 6;
    private ControllerTypes _controllerType = ControllerTypes.Keyboard;

    private readonly List<(Key, Key)> _keybindings =
        new() { (Key.Left, Key.Right), (Key.Key1, Key.Q), (Key.B, Key.N), (Key.Z, Key.X) };

    /// <summary>
    ///     How many players that have reached the goal during the current round.
    /// </summary>
    private int _roundCompletions;

    private readonly IList<CurveSpawner> _curveSpawners = new List<CurveSpawner>();

    public override void _Ready()
    {
        this.GetNodes();
        if (_keybindings.Count < _nPlayers)
            throw new ArgumentException(
                "More players than available keybindings",
                nameof(_nPlayers)
            );

        SpawnPlayers();
        InitiateCurveSpawners();
        ClearAndSpawnGoals();
    }

    public override void _PhysicsProcess(double delta)
    {
        CheckPlayerCollisions();
    }

    private void CheckPlayerCollisions()
    {
        foreach (var player in _players)
        {
            foreach (var curveSpawner in _curveSpawners)
            {
                if (IsIntersecting(player, curveSpawner.Segments))
                {
                    GD.Print("Player has collided");
                }
            }
        }
    }

    private void InitiateCurveSpawners()
    {
        foreach (var player in _players)
        {
            var curveSpawner = Instanter.Instantiate<CurveSpawner>();
            curveSpawner.Player = player;
            curveSpawner.LineWidth = LineWidth;
            _curveSpawners.Add(curveSpawner);
            AddChild(curveSpawner);
        }
    }

    private void SpawnPlayers()
    {
        var i = 0;
        _nPlayers.TimesDo(() =>
        {
            var playerId = UniqueId.Generate();
            var player = Instanter.Instantiate<Player>();
            if (_controllerType == ControllerTypes.Keyboard)
                player.Controller = new KeyboardController(_keybindings[i]);
            _players.Add(player);

            AddChild(player);
            player.GlobalPosition = GetRandomCoordinateInView(100);
            player.PlayerId = playerId;
            i++;
        });
    }

    private static bool IsIntersecting(Player player, IEnumerable<SegmentShape2D> segments)
    {
        var position = player.GlobalPosition;
        var circleShape = (CircleShape2D)player.CollisionShape2D.Shape;
        var radius = circleShape.Radius + LineWidth / 2f;

        return segments.Any(
            segment =>
                Geometry2D.SegmentIntersectsCircle(segment.A, segment.B, position, radius) != -1
        );
    }

    private void OnPlayerReachedGoal(Player player)
    {
        if (++_roundCompletions != _nPlayers)
            return;

        _roundCompletions = 0;
        ClearAndSpawnGoals();
    }

    private void ClearAndSpawnGoals()
    {
        // Remove existing goals
        GetTree()
            .GetNodesInGroup(GroupConstants.GoalGroup)
            .ToList()
            .ForEach(goal => goal.QueueFree());

        // Spawn new goals
        _players
            .ToList()
            .ForEach(player =>
            {
                var goal = Instanter.Instantiate<Goal>();
                CallDeferred("add_child", goal);
                goal.GlobalPosition = GetRandomCoordinateInView(100);
                goal.PlayerReachedGoal += OnPlayerReachedGoal;
                goal.CallDeferred("set", nameof(Player.PlayerId), player.PlayerId);
            });
    }

    private Vector2 GetRandomCoordinateInView(float margin)
    {
        var x = (float)GD.RandRange(margin, GetViewportRect().Size.X - margin);
        var y = (float)GD.RandRange(margin, GetViewportRect().Size.Y - margin);
        return new Vector2(x, y);
    }
}

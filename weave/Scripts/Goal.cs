using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;

namespace weave;

[Scene("res://Objects/Goal.tscn")]
public partial class Goal : Node2D
{
    /// <summary>
    ///     This is the signal that will be emitted when a player reaches the goal.
    ///     Will only be emitted once.
    /// </summary>
    /// <param name="player">The player who reached the goal.</param>
    [Signal]
    public delegate void PlayerReachedGoalEventHandler(Player player);

    private Color _color;

    private bool _reached;

    [GetNode("Sprite2D")]
    private Sprite2D _sprite;

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            Modulate = value;
        }
    }

    public override void _Ready()
    {
        this.GetNodes();
        Area2D area = GetNode<Area2D>("Area2D");
        area.BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_reached)
            return;

        if (body is not Player player)
            return;

        if (player.Color != Color)
            return;

        _reached = true;
        _sprite.Modulate = Colors.Black;
        EmitSignal(SignalName.PlayerReachedGoal, player);
        QueueFree();
    }
}

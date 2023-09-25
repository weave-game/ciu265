using System.Collections.Generic;
using Godot;
using weave.Utils;

namespace weave;

public partial class CurveSpawner : Node2D
{
    [Signal]
    public delegate void CreatedLineEventHandler(Line2D line);

    private const float TimeBetweenGaps = 5;
    private const float TimeForGaps = 0.5f;
    private Timer _drawTimer;
    private Timer _gapTimer;
    private bool _hasStarted;
    private Vector2 _lastPoint;
    private Color _lineColor = new(1, 0, 0);
    public bool IsDrawing { get; private set; } = true;
    public ISet<SegmentShape2D> Segments { get; } = new HashSet<SegmentShape2D>();

    public override void _Ready()
    {
        InitializeTimers();
    }

    public override void _Process(double delta)
    {
        Step();
    }

    private void Step()
    {
        // Don't draw line on first iteration (first line will otherwise originate from (0,0))
        if (_hasStarted && IsDrawing)
            SpawnLine(_lastPoint, GlobalPosition);
        else
            _hasStarted = true;

        _lastPoint = GlobalPosition;
    }

    private void InitializeTimers()
    {
        _drawTimer = new Timer { WaitTime = TimeBetweenGaps, OneShot = true };
        _drawTimer.Timeout += HandleDrawTimerTimeout;
        AddChild(_drawTimer);

        _gapTimer = new Timer { WaitTime = TimeForGaps, OneShot = true };
        _gapTimer.Timeout += HandleGapTimerTimeout;
        AddChild(_gapTimer);

        _drawTimer.Start();
    }

    private void HandleDrawTimerTimeout()
    {
        IsDrawing = false;
        _drawTimer.Stop();
        _gapTimer.Start();
    }

    private void HandleGapTimerTimeout()
    {
        IsDrawing = true;
        _gapTimer.Stop();
        _drawTimer.Start();
    }

    private void SpawnLine(Vector2 from, Vector2 to)
    {
        // Line that is drawn to screen
        var line = new Line2D { DefaultColor = _lineColor, Width = Constants.LineWidth };
        line.AddPoint(from);
        line.AddPoint(to);
        EmitSignal(SignalName.CreatedLine, line);

        // Create segment that is used to check for intersections
        Segments.Add(new SegmentShape2D { A = from, B = to });
    }
}
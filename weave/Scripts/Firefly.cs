using Godot;
using System;
using System.Linq;
using GodotSharper;
using GodotSharper.AutoGetNode;
using weave.Utils;

namespace weave;

public partial class Firefly : Path2D
{
    [GetNode("PathFollow2D")]
    private PathFollow2D _pathFollow;

    [GetNode("PathFollow2D/Area2D")]
    private Area2D _area;

    [GetNode("Line2D")]
    private Line2D _line;

    private const float MaxSpeed = 12;
    private const float MinSpeed = 3;
    private const int NrPoints = 30;
    private const float DistanceBetweenPoints = 5;
    private float _currentSpeed;
    private float _goalSpeed;
    private float _lastProgress;
    private bool _isWaiting = true;
    private Timer _animationTimer;

    public override void _Ready()
    {
        this.GetNodes();

        // TODO: Add non destructive one shot timer factory method
        var animationDelay = (GD.Randf() * 4) + 2;
        _animationTimer = TimerFactory.StartedRepeating(animationDelay, HandleTimerTimeout);
        _animationTimer.OneShot = true; 
        AddChild(_animationTimer);

        _line.Width = Constants.LineWidth;
        _line.DefaultColor = Unique.NewColor();

        for (var i = 0; i < NrPoints; i++)
        {
            _line.AddPoint(new Vector2());
        }
    }

    public override void _Process(double delta)
    {
        if (_isWaiting)
            return;

        // Reset line points when progress is done
        if (_lastProgress >= _pathFollow.Progress)
        {
            _line.Points = Enumerable.Repeat(_area.GlobalPosition, NrPoints).ToArray();
            _animationTimer.Start();
            _isWaiting = true;
        }

        // Reached goal speed, set new speed
        if (MathF.Abs(_currentSpeed - _goalSpeed) < (float)10e-5)
        {
            _goalSpeed = (GD.Randf() * MaxSpeed) + MinSpeed;
        }

        // Make line follow the leading point
        if (_line.Points[0].DistanceTo(_area.GlobalPosition) >= DistanceBetweenPoints)
        {
            var lastPoint = _area.GlobalPosition;
            var tempPoints = (Vector2[])_line.Points.Clone();

            for (var i = 0; i < NrPoints; i++)
            {
                tempPoints[i] = lastPoint;
                lastPoint = _line.Points[Math.Max(i - 1, 0)]; // Avoid index out of bounds
            }

            _line.Points = tempPoints;
        }

        _lastProgress = _pathFollow.Progress;
        _currentSpeed = Mathf.Lerp(_currentSpeed, _goalSpeed, 0.3f);
        _pathFollow.Progress += _currentSpeed;
    }

    private void HandleTimerTimeout()
    {
        _isWaiting = false;
        _animationTimer.Stop();
    }
}

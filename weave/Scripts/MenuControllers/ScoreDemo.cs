using Godot;
using GodotSharper.AutoGetNode;
using Weave.Scoring;
using Weave.Utils;

namespace Weave.MenuControllers;

public partial class ScoreDemo : Node2D
{
    private IScoreManager _scoreManager;
    private ScoreRecord _score;

    [GetNode("Button")]
    private Button _button;

    [GetNode("LineEdit")]
    private LineEdit _lineEdit;

    public override void _Ready()
    {
        this.GetNodes();
        _scoreManager = new JsonScoreManager(WeaveConstants.ScoreLogFileJsonPath);
        _score = new ScoreRecord(0, UniqueNameGenerator.Instance.New());
        _lineEdit.Text = _score.Name;

        _button.Pressed += OnButtonPressed;
    }

    private void OnButtonPressed()
    {
        _score.Name = _lineEdit.Text;
        _scoreManager.Save(_score);
    }
}
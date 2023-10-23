using Godot;
using GodotSharper.AutoGetNode;
using GodotSharper.Instancing;
using Weave.Scoring;

namespace Weave.MenuControllers;

public partial class GameOverOverlay : CanvasLayer
{
    [GetNode("ExplosionPlayer")]
    private AudioStreamPlayer _explosionPlayer;

    [GetUniqueNode("MenuButton")]
    private Button _menuButton;

    [GetUniqueNode("RetryButton")]
    private Button _retryButton;

    [GetUniqueNode("NameLineEdit")]
    private LineEdit _nameLineEdit;

    [GetUniqueNode("SaveNameButton")]
    private Button _save;

    [GetUniqueNode("SavedNotificationAnimationPlayer")]
    private AnimationPlayer _savedPlayer;

    private IScoreManager _scoreManager;
    private Score _sessionScore;

    public override void _Ready()
    {
        this.GetNodes();
        _nameLineEdit.Text = GameConfig.Lobby.Name;
        _scoreManager = new MongoDBScoreManager();

        _retryButton.Pressed += () => GetTree().ChangeSceneToFile(SceneGetter.GetPath<Main>());
        _menuButton.Pressed += () =>
        {
            GameConfig.MultiplayerManager.StopClientAsync();
            GetTree().ChangeSceneToFile(SceneGetter.GetPath<StartScreen>());
        };
        _save.Pressed += () => SaveScore(_sessionScore);

        // On game over, set process mode to idle to stop game, but keep overlays clickable
        ProcessMode = ProcessModeEnum.Always;

        _savedPlayer.Play("hide");
    }

    public void DisplayGameOver()
    {
        _explosionPlayer.Play();
        _retryButton.GrabFocus();
        Show();
    }

    public void SaveScore(Score score)
    {
        // First time saving score, save as session score to update later
        _sessionScore ??= score;

        // Dont save bad scores
        if (score.Points <= 0)
            return;

        // Players have filled in a new name, update lobby name
        var newName = _nameLineEdit.Text;
        if (!string.IsNullOrWhiteSpace(newName) && newName != score.Name)
        {
            GameConfig.Lobby.Name = newName;
            score.Name = newName;
            _nameLineEdit.Text = newName;
        }

        _scoreManager.Save(score);
        _savedPlayer.Play("hide");
        _savedPlayer.Play("saved");
    }
}

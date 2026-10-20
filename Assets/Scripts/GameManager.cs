using UnityEngine;
using UnityEngine.InputSystem;

// Controla a partida, o cronômetro e a pontuação por perigos desviados.
public class GameManager : MonoBehaviour
{
    public enum GameState { Menu, Playing, GameOver }
    public GameState State { get; private set; }
    public float SurvivalTime { get; private set; }
    public float BestTime { get; private set; }
    public int Lives { get; private set; } = 3;
    public int Score { get; private set; }
    public int BestScore { get; private set; }
    public bool Paused { get; private set; }
    public PlayerController player;
    public AsteroidSpawner spawner;
    public GameUI ui;
    public GameAudio audioPlayer;

    void Start()
    {
        Time.timeScale = 1;
        BestTime = PlayerPrefs.GetFloat("BestSurvivalTime", 0);
        BestScore = PlayerPrefs.GetInt("BestScore", 0);
        State = GameState.Menu;
        ui.Refresh();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var pad = Gamepad.current;
        // Enter e botão A são tratados pelo botão selecionado no EventSystem.
        bool confirm = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
        bool pause = (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            || (pad != null && pad.startButton.wasPressedThisFrame);

        if (confirm && State != GameState.Playing) StartGame();
        else if (pause && State == GameState.Playing) TogglePause();
        if (State == GameState.Playing && !Paused) SurvivalTime += Time.deltaTime;
        ui.UpdateHUD();
    }

    public void StartGame()
    {
        Time.timeScale = 1;
        Paused = false;
        spawner.Clear();
        Lives = 3;
        SurvivalTime = 0;
        Score = 0;
        State = GameState.Playing;
        player.ResetPosition();
        spawner.ResetTimer();
        audioPlayer.StartMusic();
        audioPlayer.PlayStart();
        ui.Refresh();
    }

    public void AddScore(int points)
    {
        if (State != GameState.Playing || Paused || points <= 0) return;
        Score += points;
    }

    public void TakeDamage()
    {
        if (State != GameState.Playing || Paused || player.IsInvulnerable) return;
        Lives--;
        audioPlayer.PlayHit();
        player.Protect();
        if (Lives <= 0)
        {
            State = GameState.GameOver;
            if (SurvivalTime > BestTime)
            {
                BestTime = SurvivalTime;
                PlayerPrefs.SetFloat("BestSurvivalTime", BestTime);
            }
            if (Score > BestScore)
            {
                BestScore = Score;
                PlayerPrefs.SetInt("BestScore", BestScore);
            }
            PlayerPrefs.Save();
            audioPlayer.StopMusic();
            audioPlayer.PlayGameOver();
        }
        ui.Refresh();
    }

    public void TogglePause()
    {
        if (State != GameState.Playing) return;
        Paused = !Paused;
        Time.timeScale = Paused ? 0 : 1;
        audioPlayer.SetPaused(Paused);
        ui.Refresh();
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1;
        Paused = false;
        State = GameState.Menu;
        spawner.Clear();
        player.ResetPosition();
        audioPlayer.StopMusic();
        ui.Refresh();
    }

    void OnApplicationFocus(bool focused)
    {
        if (!focused && State == GameState.Playing && !Paused) TogglePause();
    }

    void OnDestroy() { Time.timeScale = 1; }
}

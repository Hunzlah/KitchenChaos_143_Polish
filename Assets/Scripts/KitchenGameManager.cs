using System;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }

    private State state;
    private float countdownToStartTimer = 3f;
    private float gamePlayingTimer;
    private const float GAME_PLAYING_TIMER_MAX = 30f;
    private bool isGamePaused;

    private void Awake ()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple KitchenGameManager instances!");
        }
        Instance = this;

        state = State.WaitingToStart;
    }

    private void Start ()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    private void Update ()
    {
        switch (state)
        {
            case State.WaitingToStart:
                break;

            case State.CountdownToStart:
                UpdateCountdown();
                break;

            case State.GamePlaying:
                UpdateGamePlaying();
                break;

            case State.GameOver:
                break;
        }
    }

    // === Event Handlers ===
    private void GameInput_OnInteractAction (object sender, EventArgs e)
    {
        if (state == State.WaitingToStart)
        {
            StartCountdown();
        }
    }

    private void GameInput_OnPauseAction (object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    // === State Transitions ===
    private void StartCountdown ()
    {
        state = State.CountdownToStart;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void StartGame ()
    {
        state = State.GamePlaying;
        gamePlayingTimer = GAME_PLAYING_TIMER_MAX;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EndGame ()
    {
        state = State.GameOver;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    // === State Updates ===
    private void UpdateCountdown ()
    {
        countdownToStartTimer -= Time.deltaTime;
        if (countdownToStartTimer <= 0f)
        {
            StartGame();
        }
    }

    private void UpdateGamePlaying ()
    {
        gamePlayingTimer -= Time.deltaTime;
        if (gamePlayingTimer <= 0f)
        {
            EndGame();
        }
    }

    // === State Check Methods ===
    public bool IsGamePlaying () => state == State.GamePlaying;

    public bool IsCountdownToStartActive () => state == State.CountdownToStart;

    public bool IsGameOver () => state == State.GameOver;

    public float GetCountdownToStartTimer () => countdownToStartTimer;

    public float GetGamePlayingTimerNormalized ()
    {
        return 1 - (gamePlayingTimer / GAME_PLAYING_TIMER_MAX);
    }

    // === Pause Handling ===
    public void TogglePauseGame ()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;

        if (isGamePaused)
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        else
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
    }
}

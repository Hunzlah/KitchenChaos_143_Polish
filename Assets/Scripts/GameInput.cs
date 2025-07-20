using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private const string PLAYER_PREFS_BINDINGS = "InputBindings";

    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnBindingRebind;

    public enum Binding
    {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Interact,
        InteractAlternate,
        Pause,
        Gamepad_Interact,
        Gamepad_InteractAlternate,
        Gamepad_Pause
    }

    private PlayerInputActions playerInputActions;

    private void Awake ()
    {
        Instance = this;
        playerInputActions = new PlayerInputActions();

        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            string bindingsJson = PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS);
            playerInputActions.LoadBindingOverridesFromJson(bindingsJson);
        }

        playerInputActions.Player.Enable();
        SubscribeInputEvents();
    }

    private void OnDestroy ()
    {
        UnsubscribeInputEvents();
        playerInputActions.Dispose();
    }

    private void SubscribeInputEvents ()
    {
        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.InteractAlternate.performed += InteractAlternate_performed;
        playerInputActions.Player.Pause.performed += Pause_performed;
    }

    private void UnsubscribeInputEvents ()
    {
        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.InteractAlternate.performed -= InteractAlternate_performed;
        playerInputActions.Player.Pause.performed -= Pause_performed;
    }

    private void Interact_performed (InputAction.CallbackContext context) =>
        OnInteractAction?.Invoke(this, EventArgs.Empty);

    private void InteractAlternate_performed (InputAction.CallbackContext context) =>
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);

    private void Pause_performed (InputAction.CallbackContext context) =>
        OnPauseAction?.Invoke(this, EventArgs.Empty);

    public Vector2 GetMovementVectorNormalized ()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }

    public string GetBindingText (Binding binding)
    {
        var (inputAction, bindingIndex) = GetBindingData(binding);
        return inputAction.bindings[bindingIndex].ToDisplayString();
    }

    public void RebindBinding (Binding binding, Action onActionRebound)
    {
        playerInputActions.Player.Disable();

        var (inputAction, bindingIndex) = GetBindingData(binding);

        inputAction.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(callback =>
            {
                callback.Dispose();
                playerInputActions.Player.Enable();
                onActionRebound?.Invoke();

                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, playerInputActions.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();

                OnBindingRebind?.Invoke(this, EventArgs.Empty);
            })
            .Start();
    }

    private (InputAction inputAction, int bindingIndex) GetBindingData (Binding binding)
    {
        return binding switch
        {
            Binding.Move_Up => (playerInputActions.Player.Move, 1),
            Binding.Move_Down => (playerInputActions.Player.Move, 2),
            Binding.Move_Left => (playerInputActions.Player.Move, 3),
            Binding.Move_Right => (playerInputActions.Player.Move, 4),
            Binding.Interact => (playerInputActions.Player.Interact, 0),
            Binding.InteractAlternate => (playerInputActions.Player.InteractAlternate, 0),
            Binding.Pause => (playerInputActions.Player.Pause, 0),
            Binding.Gamepad_Interact => (playerInputActions.Player.Interact, 1),
            Binding.Gamepad_InteractAlternate => (playerInputActions.Player.InteractAlternate, 1),
            Binding.Gamepad_Pause => (playerInputActions.Player.Pause, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(binding), binding, null)
        };
    }
}

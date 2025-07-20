using System;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    // === Singleton ===
    public static Player Instance { get; private set; }

    // === Events ===
    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;

    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    // === Serialized Fields ===
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;

    // === Private Fields ===
    private bool isWalking;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    private const float INTERACT_DISTANCE = 2f;
    private const float PLAYER_RADIUS = 0.7f;
    private const float PLAYER_HEIGHT = 2f;
    private const float ROTATE_SPEED = 10f;

    // === Unity Methods ===
    private void Awake ()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one Player instance");
        }
        Instance = this;
    }

    private void Start ()
    {
        gameInput.OnInteractAction += OnInteractAction;
        gameInput.OnInteractAlternateAction += OnInteractAlternateAction;
    }

    private void Update ()
    {
        HandleMovement();
        HandleInteractions();
    }

    // === Public Methods ===
    public bool IsWalking () => isWalking;

    public Transform GetKitchenObjectFollowTransform () => kitchenObjectHoldPoint;

    public void SetKitchenObject (KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject () => kitchenObject;

    public void ClearKitchenObject () => kitchenObject = null;

    public bool HasKitchenObject () => kitchenObject != null;

    // === Input Callbacks ===
    private void OnInteractAction (object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        selectedCounter?.Interact(this);
    }

    private void OnInteractAlternateAction (object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;
        selectedCounter?.InteractAlternate(this);
    }

    // === Movement ===
    private void HandleMovement ()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        float moveDistance = moveSpeed * Time.deltaTime;

        bool canMove = TryMove(moveDir, moveDistance, out Vector3 adjustedMoveDir);
        if (canMove)
        {
            transform.position += adjustedMoveDir * moveDistance;
            transform.forward = Vector3.Slerp(transform.forward, adjustedMoveDir, Time.deltaTime * ROTATE_SPEED);
        }

        isWalking = adjustedMoveDir != Vector3.zero;
    }

    private bool TryMove (Vector3 moveDir, float moveDistance, out Vector3 adjustedMoveDir)
    {
        if (!IsObstructed(moveDir, moveDistance))
        {
            adjustedMoveDir = moveDir;
            return true;
        }

        Vector3 moveDirX = new Vector3(moveDir.x, 0f, 0f).normalized;
        if (Mathf.Abs(moveDir.x) > 0.5f && !IsObstructed(moveDirX, moveDistance))
        {
            adjustedMoveDir = moveDirX;
            return true;
        }

        Vector3 moveDirZ = new Vector3(0f, 0f, moveDir.z).normalized;
        if (Mathf.Abs(moveDir.z) > 0.5f && !IsObstructed(moveDirZ, moveDistance))
        {
            adjustedMoveDir = moveDirZ;
            return true;
        }

        adjustedMoveDir = Vector3.zero;
        return false;
    }

    private bool IsObstructed (Vector3 direction, float distance)
    {
        return Physics.CapsuleCast(
            transform.position,
            transform.position + Vector3.up * PLAYER_HEIGHT,
            PLAYER_RADIUS,
            direction,
            distance
        );
    }

    // === Interactions ===
    private void HandleInteractions ()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir;
        }

        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit hit, INTERACT_DISTANCE, countersLayerMask))
        {
            if (hit.transform.TryGetComponent(out BaseCounter counter))
            {
                if (counter != selectedCounter)
                {
                    SetSelectedCounter(counter);
                }
            }
            else
            {
                ClearSelectedCounter();
            }
        }
        else
        {
            ClearSelectedCounter();
        }
    }

    private void SetSelectedCounter (BaseCounter counter)
    {
        selectedCounter = counter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs { selectedCounter = counter });
    }

    private void ClearSelectedCounter ()
    {
        SetSelectedCounter(null);
    }
}

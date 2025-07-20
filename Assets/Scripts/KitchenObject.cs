using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    private IKitchenObjectParent kitchenObjectParent;

    // === Public Accessors ===
    public KitchenObjectSO GetKitchenObjectSO () => kitchenObjectSO;

    public IKitchenObjectParent GetKitchenObjectParent () => kitchenObjectParent;

    public bool TryGetPlate (out PlateKitchenObject plateKitchenObject)
    {
        plateKitchenObject = this as PlateKitchenObject;
        return plateKitchenObject != null;
    }

    // === Parenting Logic ===
    public void SetKitchenObjectParent (IKitchenObjectParent newParent)
    {
        if (kitchenObjectParent != null)
        {
            kitchenObjectParent.ClearKitchenObject();
        }

        kitchenObjectParent = newParent;

        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError($"IKitchenObjectParent '{kitchenObjectParent}' already has a KitchenObject!");
        }

        kitchenObjectParent.SetKitchenObject(this);

        AttachToParent();
    }

    private void AttachToParent ()
    {
        transform.SetParent(kitchenObjectParent.GetKitchenObjectFollowTransform());
        transform.localPosition = Vector3.zero;
    }

    // === Lifecycle ===
    public void DestroySelf ()
    {
        if (kitchenObjectParent != null)
        {
            kitchenObjectParent.ClearKitchenObject();
        }

        Destroy(gameObject);
    }

    // === Spawning ===
    public static KitchenObject SpawnKitchenObject (KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        if (kitchenObjectSO?.prefab == null)
        {
            Debug.LogError("Invalid KitchenObjectSO or missing prefab.");
            return null;
        }

        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();

        if (kitchenObject == null)
        {
            Debug.LogError("Spawned prefab does not have a KitchenObject component.");
            return null;
        }

        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
        return kitchenObject;
    }
}

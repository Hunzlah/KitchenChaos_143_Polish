using System;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    // === Singleton ===
    public static DeliveryManager Instance { get; private set; }

    // === Events ===
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    // === Serialized Fields ===
    [SerializeField] private RecipeListSO recipeListSO;

    // === Private Fields ===
    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer;
    private const float SPAWN_RECIPE_TIMER_MAX = 4f;
    private const int WAITING_RECIPES_MAX = 4;
    private int successfulRecipesAmount;

    // === Unity Methods ===
    private void Awake ()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple instances of DeliveryManager detected!");
        }
        Instance = this;

        waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Update ()
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return;

        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = SPAWN_RECIPE_TIMER_MAX;

            if (waitingRecipeSOList.Count < WAITING_RECIPES_MAX)
            {
                RecipeSO newRecipe = GetRandomRecipe();
                waitingRecipeSOList.Add(newRecipe);
                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    // === Public Methods ===
    public void DeliverRecipe (PlateKitchenObject plate)
    {
        foreach (RecipeSO recipe in waitingRecipeSOList)
        {
            if (MatchesRecipe(recipe, plate))
            {
                waitingRecipeSOList.Remove(recipe);
                successfulRecipesAmount++;

                OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
                return;
            }
        }

        // No matching recipe found
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeSO> GetWaitingRecipeSOList () => waitingRecipeSOList;

    public int GetSuccessfulRecipesAmount () => successfulRecipesAmount;

    // === Private Methods ===
    private RecipeSO GetRandomRecipe ()
    {
        var recipes = recipeListSO.recipeSOList;
        return recipes[UnityEngine.Random.Range(0, recipes.Count)];
    }

    private bool MatchesRecipe (RecipeSO recipe, PlateKitchenObject plate)
    {
        List<KitchenObjectSO> plateIngredients = plate.GetKitchenObjectSOList();

        if (recipe.kitchenObjectSOList.Count != plateIngredients.Count)
            return false;

        foreach (KitchenObjectSO requiredIngredient in recipe.kitchenObjectSOList)
        {
            if (!plateIngredients.Contains(requiredIngredient))
            {
                return false;
            }
        }

        return true;
    }
}

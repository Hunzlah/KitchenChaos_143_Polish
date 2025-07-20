Tutorial link - https://www.youtube.com/watch?v=AmGSEH7QcDg&ab_channel=CodeMonkey


Refactoring:

Player.cs
    Grouped related methods logically
    Reduced duplicate code
    Added comments for clarity
    Introduced constants where appropriate (e.g., interactDistance, rotateSpeed)
    Minor renaming for consistency
    
LookAtCamera.cs
    Extracted lookat logic in a method
    
DeliveryManager.cs
    Extracted MatchesRecipe() into a separate method to simplify the main logic.
    Grouped event triggers for clarity.
    Improved naming and spacing for better readability.
    Used early returns and reduced nesting in the Update() and DeliverRecipe() methods
    
GameInput.cs
    Centralized binding index logic into a GetBindingData() helper method to reduce redundancy.
    Grouped event subscriptions/unsubscriptions in their own methods.
    Minor formatting improvements.
    Made use of expression-bodied members where appropriate.
    
KitchenGameManager.cs
    Grouped event handlers and state-check methods logically.
    Made State enum and current state accessible in a cleaner way.
    Cleaned up redundant switch cases.
    Extracted methods for state transitions.
    Added a few code comments for clarity.
    
KitchenObject.cs
    Grouped public and private members logically.
    Improved error message readability.
    Reduced redundancy (this. where unnecessary).
    Added null checks to make behavior more defensive.
    Split up logic into small methods where useful.

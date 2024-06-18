public class CharacterInteraction
{
    private IInteractable interactableItem = null;

    public IInteractable InteractableItem { get => interactableItem; set => interactableItem = value; }

    public void TryInteractPlayer()
    {
        if (interactableItem == null) return;
        interactableItem?.Interact();
    }
}

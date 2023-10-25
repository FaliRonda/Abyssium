public interface I_Interactable
{
    public void Interact();

    public bool IsInteracting();
    public void SetInteracting(bool isInteracting);
    public bool CanInteract();
    public void SetCanInteract(bool canInteract);
}

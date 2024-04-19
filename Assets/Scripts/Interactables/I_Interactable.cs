public interface I_Interactable
{
    public void Interact(PJ player, bool cancel);

    public bool IsInteracting();
    public void SetInteracting(bool isInteracting);
    public bool CanInteract();
    public void SetCanInteract(bool canInteract);
}

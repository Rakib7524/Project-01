namespace Project01
{
    public enum InteractableType
    {
        Player
    }

    public interface IInteractable
    {
        public InteractableType GetInteractableType();
    }
}
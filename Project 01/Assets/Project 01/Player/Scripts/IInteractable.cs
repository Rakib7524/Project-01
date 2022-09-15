namespace Project01
{
    public enum InteractableType
    {
        Ground,
        Player
    }

    public interface IInteractable
    {
        public InteractableType GetInteractableType();
    }
}
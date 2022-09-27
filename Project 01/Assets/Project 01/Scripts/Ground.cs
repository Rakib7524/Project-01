using UnityEngine;

namespace Project01
{
    public class Ground : MonoBehaviour, IInteractable
    {
        public InteractableType GetInteractableType()
        {
            return InteractableType.Ground;
        }
    }
}
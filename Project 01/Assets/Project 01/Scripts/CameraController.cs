using UnityEngine;
using UnityEngine.Events;

namespace Project01
{
    public class CameraController : MonoBehaviour
    {
        [Header("Configurations")]
        [SerializeField] LayerMask _interactionMask;

        public static UnityAction<Vector3> leftClickOnGround;
        public static UnityAction<Player> leftClickOnPlayer;

        private Ray _ray;
        private RaycastHit _hit;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) // 0 = Mouse left button.
            {
                _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(_ray, out _hit, 50f, _interactionMask))
                {
                    if (_hit.transform.TryGetComponent(out IInteractable p_interactable))
                    {
                        switch (p_interactable.GetInteractableType())
                        {
                            case InteractableType.Ground:
                                Debug.Log("Left click on ground.");
                                leftClickOnGround?.Invoke(_hit.point);
                                break;

                            case InteractableType.Player:
                                Debug.Log("Left click on player.");
                                leftClickOnPlayer?.Invoke(_hit.transform.GetComponent<Player>());
                                break;
                        }
                    }
                    
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.DrawRay(_ray.origin, _ray.direction * 50f);
                Gizmos.DrawSphere(_hit.point, 0.5f);
            }
        }
    }
}
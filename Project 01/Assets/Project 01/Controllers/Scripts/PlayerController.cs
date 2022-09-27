using UnityEngine;

namespace Project01
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraController cameraController;

        [Header("Configurations")]
        [SerializeField] LayerMask interactionMask;

        private Player player;
        private Ray ray;
        private RaycastHit hit;

        #region Properties
        public Player Player { get => player; }
        #endregion

        public void SetPlayer(Player p_player)
        {
            Debug.Log("New player added: " + p_player.PlayerName);
            player = p_player;
        }

        private void Update()
        {
            if (player != null)
            {
                if (Input.GetMouseButtonDown(0)) // 0 = Mouse left button.
                {
                    ray = cameraController.Camera.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit, 50f, interactionMask))
                    {
                        if (hit.transform.TryGetComponent(out IInteractable p_interactable))
                        {
                            switch (p_interactable.GetInteractableType())
                            {
                                case InteractableType.Ground:
                                    //Debug.Log("Left click on ground.");
                                    // Check if target is walkable, if true move the player.
                                    if (player.IsSelected)
                                    {
                                        player.MoveToTarget(hit.point);
                                    }
                                    break;

                                case InteractableType.Player:
                                    //Debug.Log("Left click on player.");
                                    player.ChangeState();
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.DrawRay(ray.origin, ray.direction * 50f);
                Gizmos.DrawSphere(hit.point, 0.5f);
            }
        }
    }
}

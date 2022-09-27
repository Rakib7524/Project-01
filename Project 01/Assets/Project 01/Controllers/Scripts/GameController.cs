using UnityEngine;

namespace Project01
{
    public class GameController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private PlayerController playerController;

        private static GameController instance;

        #region Properties
        public static GameController Instance { get => instance; }
        #endregion

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Player _player = Instantiate(playerPrefab, playerController.transform).GetComponent<Player>();
            playerController.SetPlayer(_player);
        }
    }
}

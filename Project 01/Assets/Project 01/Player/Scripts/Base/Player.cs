using UnityEngine;
using UnityEngine.AI;

namespace Project01
{
    public class Player : MonoBehaviour
    {
        [Header("Player References")]
        [SerializeField] protected NavMeshAgent _characterController;

        [Header("Player configurations")]
        [SerializeField] protected string _name;

        private void Awake()
        {
            Project01Events.groundLeftClicked += OnGroundLeftClicked;
        }

        private void OnGroundLeftClicked(Vector3 p_groundPosition)
        {
            Debug.Log("SetDestination to: " + _name);
            _characterController.SetDestination(p_groundPosition);
        }

        public void OnMouseDown()
        {
            Debug.Log("MouseDown");
        }
    }
}
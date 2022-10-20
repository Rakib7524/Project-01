using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Project01
{
    public enum PlayerState
    {
        Idle,
        Moving
    }

    public class Player : MonoBehaviour, IInteractable
    {
        [Header("Player References")]
        [SerializeField] protected NavMeshAgent navMeshAgent;
        [SerializeField] private SpriteRenderer selectionCircle;

        [Header("Player configurations")]
        [SerializeField] protected string playerName;
        [SerializeField] private bool instantLookAtTargetDestination = false;

        private PlayerState playerState = PlayerState.Idle;
        private bool isSelected;

        #region Properties
        public string PlayerName
        {
            get => playerName;
        }

        public PlayerState PlayerState
        {
            get => playerState;

            private set
            {
                if (playerState == value)
                {
                    return;
                }

                playerState = value;
                Debug.Log("Player:" + name + " changed state to: " + playerState);
            }
        }

        public bool IsSelected 
        { 
            get => isSelected;

            private set
            {
                isSelected = value;
                selectionCircle.enabled = isSelected;
                Debug.Log("Player: " + name + (isSelected ? " is selected." : " was deselected."));
            }
        }
        #endregion

        private void Update()
        {
            PlayerState = navMeshAgent.velocity != Vector3.zero ? PlayerState.Moving : PlayerState.Idle;
        }

        public void MoveToTarget(Vector3 p_groundPosition)
        {
            Debug.Log("SetDestination to: " + name);

            if (instantLookAtTargetDestination)
            {
                switch (playerState)
                {
                    case PlayerState.Idle:
                        transform.LookAt(p_groundPosition + new Vector3(0f, navMeshAgent.height / 2f, 0f));
                        break;

                    case PlayerState.Moving:
                        navMeshAgent.destination = p_groundPosition;
                        navMeshAgent.velocity *= 0.3f;
                        break;
                }
            }

            navMeshAgent.SetDestination(p_groundPosition);
        }

        public InteractableType GetInteractableType()
        {
            return InteractableType.Player;
        }

        public void ChangeState()
        {
            if (isSelected)
            {
                DeselectPlayer();
            }
            else
            {
                SelectPlayer();
            }
        }

        private void SelectPlayer()
        {
            IsSelected = true;
        } 

        private void DeselectPlayer()
        {
            IsSelected = false;
        }
    }
}
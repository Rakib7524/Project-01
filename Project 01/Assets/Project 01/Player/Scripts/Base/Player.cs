using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Project01
{
    public enum PlayerState
    {
        Idle,
        Movement
    }

    public class Player : MonoBehaviour, IInteractable
    {
        [Header("Player References")]
        [SerializeField] protected NavMeshAgent _navMeshAgent;
        [SerializeField] private SpriteRenderer _selectionCircle;

        [Header("Player configurations")]
        [SerializeField] protected string _name;
        [SerializeField] private bool _instantLookAtTargetDestination = false;

        private PlayerState _playerState = PlayerState.Idle;
        private bool _isSelected;

        #region Properties
        public string Name
        {
            get => _name;
        }

        public PlayerState PlayerState
        {
            get => _playerState;

            private set
            {
                if (_playerState == value)
                {
                    return;
                }

                _playerState = value;
                Debug.Log("Player:" + _name + " changed state to: " + _playerState);
            }
        }

        public bool IsSelected 
        { 
            get => _isSelected; 

            private set
            {
                _isSelected = value;
                _selectionCircle.enabled = _isSelected;
                Debug.Log("Player: " + _name + (_isSelected ? " is selected." : " was deselected."));
            }
        }
        #endregion

        private void Awake()
        {
            Project01Events.groundLeftClicked += OnGroundLeftClicked;
            Project01Events.playerLeftClicked += OnPlayerLeftClicked;

            if (_instantLookAtTargetDestination)
            {
                _navMeshAgent.angularSpeed = 0f;
            }
        }

        private void Update()
        {
            PlayerState = _navMeshAgent.velocity != Vector3.zero ? PlayerState.Movement : PlayerState.Idle;
        }

        private void OnGroundLeftClicked(Vector3 p_groundPosition)
        {
            if (_isSelected)
            {
                Debug.Log("SetDestination to: " + _name);

                if (_instantLookAtTargetDestination)
                {
                    transform.LookAt(p_groundPosition + new Vector3(0f, _navMeshAgent.height / 2f, 0f));

                    if (_playerState == PlayerState.Movement)
                    {
                        _navMeshAgent.SetDestination(transform.position);
                    }
                }

                _navMeshAgent.SetDestination(p_groundPosition);
            }
        }

        private void OnPlayerLeftClicked(Player p_player)
        {
            if (p_player == this)
            {
                if (_isSelected)
                {
                    DeselectPlayer();
                }
                else
                {
                    SelectPlayer();
                }
            }
        }

        public InteractableType GetInteractableType()
        {
            return InteractableType.Player;
        }

        private void SelectPlayer()
        {
            IsSelected = true;
        } 

        private void DeselectPlayer()
        {
            IsSelected = false;
        }

        private void OnDestroy()
        {
            Project01Events.groundLeftClicked -= OnGroundLeftClicked;
            Project01Events.playerLeftClicked -= OnPlayerLeftClicked;
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Project01
{
    public class Project01Events : MonoBehaviour
    {
        public static UnityAction<Vector3> groundLeftClicked;
        public static UnityAction<Player> playerLeftClicked;

        private void Awake()
        {
            CameraController.leftClickOnGround += OnLeftClickOnGround;
            CameraController.leftClickOnPlayer += OnLeftClickOnPlayer;
        }

        private void OnLeftClickOnGround(Vector3 p_position)
        {
            Debug.Log("Event: OnGroundClicked on position: " + p_position);
            groundLeftClicked?.Invoke(p_position);
        }

        private void OnLeftClickOnPlayer(Player p_player)
        {
            Debug.Log("Event: OnPlayerClicked: " + p_player.Name);
            playerLeftClicked?.Invoke(p_player);
        }

        private void OnDestroy()
        {
            CameraController.leftClickOnGround -= OnLeftClickOnGround;
            CameraController.leftClickOnPlayer -= OnLeftClickOnPlayer;
        }
    }
}
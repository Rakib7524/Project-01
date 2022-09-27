using UnityEngine;

namespace Project01
{
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private new Camera camera;

        #region Properties
        public Camera Camera { get => camera; }
        #endregion
    }
}
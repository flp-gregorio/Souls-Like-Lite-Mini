using Characters;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cameras
{
    public class CameraFollowObject : MonoBehaviour
    {
        [FormerlySerializedAs("_playerTransform")]
        [Header("References")]
        [SerializeField]
        private Transform playerTransform;

        [FormerlySerializedAs("_flipYRotationTime")]
        [Header("Flip Rotation Stats")]
        [SerializeField]
        private float flipYRotationTime = 0.5f;

        [FormerlySerializedAs("_lookAheadOffset")]
        [Header("Camera Offset")]
        [SerializeField]
        private Vector3 lookAheadOffset = new Vector3(2f, 0f, 0f); // Add offset in the direction player is facing
        [FormerlySerializedAs("_smoothSpeed")]
        [SerializeField]
        private float smoothSpeed = 5f; // How quickly to move to the target position

        private PlayerController _player;
        private Vector3 _targetPosition;
        private bool _isFacingRight;

        void Awake()
        {
            _player = playerTransform.gameObject.GetComponent<PlayerController>();
            _isFacingRight = _player.IsFacingRight;
        }

        void LateUpdate()
        {
            // Calculate the base position (player's position)
            Vector3 basePosition = playerTransform.position;

            // Calculate the look-ahead offset based on player facing direction
            Vector3 offset = _isFacingRight ? lookAheadOffset : new Vector3(-lookAheadOffset.x, 0, lookAheadOffset.z);

            // Set the target position with offset
            _targetPosition = basePosition + offset;

            // Smoothly move towards the target position
            transform.position = Vector3.Lerp(transform.position, _targetPosition, smoothSpeed * Time.deltaTime);
        }

        public void CallTurn()
        {
            _isFacingRight = !_isFacingRight;

            // Start a new rotation tween in the correct direction
            float targetRotation = _isFacingRight ? 180f : 0f;
            transform.DORotate(new Vector3(0, targetRotation, 0), flipYRotationTime)
                .SetEase(Ease.InOutSine);
        }
    }
}

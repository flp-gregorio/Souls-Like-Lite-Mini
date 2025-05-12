using UnityEngine;

namespace Cameras
{
    /// <summary>
    /// Creates a camera that follows the player position but ignores rotations,
    /// specifically designed for parallax backgrounds.
    /// </summary>
    public class ParallaxCamera : MonoBehaviour
    {
        [Tooltip("The player transform to follow")]
        public Transform playerTransform;
        
        [Tooltip("How quickly the camera follows the player (lower = smoother)")]
        public float smoothTime = 0.2f;
        
        [Tooltip("Whether this camera should render anything (should be false)")]
        public bool shouldRender = false;
        
        private Vector3 _velocity = Vector3.zero;
        private Vector3 _targetPosition;
        private Camera _camera;
        
        private void Awake()
        {
            _camera = GetComponent<Camera>();
            
            // Make sure this camera doesn't render anything
            if (!shouldRender && _camera != null)
            {
                _camera.cullingMask = 0; // Set to render nothing
                _camera.clearFlags = CameraClearFlags.Nothing;
                _camera.depth = -100; // Make sure it renders before main camera
                
                // Disable audio listener if present
                AudioListener audioListener = GetComponent<AudioListener>();
                if (audioListener != null)
                {
                    audioListener.enabled = false;
                }
            }
        }
        
        private void Start()
        {
            if (playerTransform == null)
            {
                Debug.LogError("ParallaxCamera: Player transform reference is missing!");
                enabled = false;
                return;
            }
            
            // Initialize camera at player position
            transform.position = new Vector3(
                playerTransform.position.x, 
                playerTransform.position.y, 
                transform.position.z
            );
            
            // Make sure this is not tagged as MainCamera
            if (gameObject.CompareTag("MainCamera"))
            {
                Debug.LogWarning("ParallaxCamera should not have the MainCamera tag. Removing tag.");
                gameObject.tag = "Untagged";
            }
        }
        
        private void LateUpdate()
        {
            if (!playerTransform) return;
            
            // Only follow XY position, ignore any rotation
            _targetPosition = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y,
                transform.position.z
            );
            
            // Smoothly move the camera toward the target position
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                _targetPosition, 
                ref _velocity, 
                smoothTime
            );
        }
    }
}
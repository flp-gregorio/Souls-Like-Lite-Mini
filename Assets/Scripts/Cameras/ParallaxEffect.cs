using UnityEngine;

namespace Cameras
{
    public class ParallaxEffect : MonoBehaviour
    {
        [Tooltip("The dedicated camera used for parallax calculations")]
        public Camera parallaxCam;
        
        [Tooltip("The transform to follow (usually the player)")]
        public Transform followTarget;

        [Tooltip("Multiplier to reduce vertical parallax effect (0 to 1). 1 means full effect, less than 1 weakens the vertical movement.")]
        public float verticalParallaxMultiplier = 0.2f;

        // Starting position for the parallax game object
        Vector2 _startingPosition;

        // Start Z value of the parallax game object
        float _startingZ;

        // Distance the camera has moved from the starting position of the parallax game object
        Vector2 CamMoveSinceStart
        {
            get
            {
                return (Vector2)parallaxCam.transform.position - _startingPosition;
            }
        }

        float ZDistanceFromTarget
        {
            get
            {
                return transform.position.z - followTarget.position.z;
            }
        }

        float ClippingPlane
        {
            get
            {
                return parallaxCam.transform.position.z + (ZDistanceFromTarget > 0 ? parallaxCam.farClipPlane : parallaxCam.nearClipPlane);
            }
        }

        // Parallax factor to multiply the camera movement by
        float ParallaxFactor
        {
            get
            {
                return Mathf.Abs(ZDistanceFromTarget) / 25 * ClippingPlane;
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // Ensure we have a parallax camera
            if (parallaxCam == null)
            {
                Debug.LogError("Parallax Effect on " + gameObject.name + " is missing a reference to parallaxCam!");
                enabled = false;
                return;
            }
            
            // Ensure we have a follow target
            if (followTarget == null)
            {
                Debug.LogError("Parallax Effect on " + gameObject.name + " is missing a reference to followTarget!");
                enabled = false;
                return;
            }
            
            _startingPosition = transform.position;
            _startingZ = transform.position.z;
        }

        // Update is called once per frame
        void Update()
        {
            // Calculate new horizontal and vertical offsets separately
            float newX = _startingPosition.x + CamMoveSinceStart.x * ParallaxFactor;
            float newY = _startingPosition.y + CamMoveSinceStart.y * ParallaxFactor * verticalParallaxMultiplier;
        
            transform.position = new Vector3(newX, newY, _startingZ);
        }
    }
}
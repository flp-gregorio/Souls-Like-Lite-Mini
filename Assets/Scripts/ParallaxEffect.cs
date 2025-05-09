using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public Camera cam;
    public Transform followTarget;

    [Tooltip("Multiplier to reduce vertical parallax effect (0 to 1). 1 means full effect, less than 1 weakens the vertical movement.")]
    public float verticalParallaxMultiplier = 0.2f;

    // Starting position for the parallax game object
    Vector2 startingPosition;

    // Start Z value of the parallax game object
    float startingZ;

    // Distance the camera has moved from the starting position of the parallax game object
    Vector2 CamMoveSinceStart => (Vector2)cam.transform.position - startingPosition;

    float ZDistanceFromTarget => transform.position.z - followTarget.position.z;

    float ClippingPlane => cam.transform.position.z + (ZDistanceFromTarget > 0 ? cam.farClipPlane : cam.nearClipPlane);

    // Parallax factor to multiply the camera movement by
    float ParallaxFactor => Mathf.Abs(ZDistanceFromTarget) / 25 * ClippingPlane;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingPosition = transform.position;
        startingZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate new horizontal and vertical offsets separately
        float newX = startingPosition.x + CamMoveSinceStart.x * ParallaxFactor;
        float newY = startingPosition.y + CamMoveSinceStart.y * ParallaxFactor * verticalParallaxMultiplier;
        
        transform.position = new Vector3(newX, newY, startingZ);
    }
}

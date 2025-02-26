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
    Vector2 camMoveSinceStart => (Vector2)cam.transform.position - startingPosition;

    float zDistanceFromTarget => transform.position.z - followTarget.position.z;

    float clippingPlane => cam.transform.position.z + (zDistanceFromTarget > 0 ? cam.farClipPlane : cam.nearClipPlane);

    // Parallax factor to multiply the camera movement by
    float parallaxFactor => Mathf.Abs(zDistanceFromTarget) / 25 * clippingPlane;

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
        float newX = startingPosition.x + camMoveSinceStart.x * parallaxFactor;
        float newY = startingPosition.y + camMoveSinceStart.y * parallaxFactor * verticalParallaxMultiplier;
        
        transform.position = new Vector3(newX, newY, startingZ);
    }
}

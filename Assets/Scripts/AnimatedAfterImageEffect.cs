using System.Collections.Generic;
using UnityEngine;

public class AnimatedAfterImageEffect : MonoBehaviour
{
    [Header("After-Image Settings")]
    [SerializeField] private float distanceThreshold = 0.1f;
    [SerializeField] private float imageDuration = 0.5f;
    [SerializeField] private Color overlayColor = new Color(0.5f, 0f, 0.5f, 0.5f); // Purple with 50% alpha
    [SerializeField] private Color endColor = new Color(0.5f, 0f, 0.5f, 0f); // Purple with 0% alpha (fully transparent)
    [SerializeField] private int maxImages = 10;
    
    [Header("Material Settings")]
    [SerializeField] private Material blendMaterial; // Optional custom material for blending
    
    [Header("Debugging")]
    [SerializeField] private bool showDebugInfo = true;
    
    private SpriteRenderer _spriteRenderer;
    private Vector3 _lastPosition;
    private List<AfterImageInstance> _activeImages = new List<AfterImageInstance>();
    
    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            Debug.LogError("AnimatedAfterImageEffect: No SpriteRenderer found on this GameObject!");
            enabled = false;
            return;
        }
        
        _lastPosition = transform.position;
        
        if (showDebugInfo)
        {
            Debug.Log("AnimatedAfterImageEffect initialized successfully on " + gameObject.name);
        }
    }
    
    private void LateUpdate()
    {
        // Clean up expired images
        for (int i = _activeImages.Count - 1; i >= 0; i--)
        {
            if (Time.time > _activeImages[i].EndTime)
            {
                Destroy(_activeImages[i].GameObject);
                _activeImages.RemoveAt(i);
            }
        }
        
        // Create new after-image based on distance moved
        float distanceMoved = Vector3.Distance(transform.position, _lastPosition);
        if (distanceMoved >= distanceThreshold)
        {
            CreateAfterImage();
            _lastPosition = transform.position;
            
            if (showDebugInfo)
            {
                Debug.Log("Created after-image at " + transform.position + " (moved " + distanceMoved + " units)");
            }
        }
    }
    
    private void CreateAfterImage()
    {
        // Check if SpriteRenderer has a valid sprite
        if (_spriteRenderer.sprite == null)
        {
            Debug.LogWarning("AnimatedAfterImageEffect: No sprite to copy!");
            return;
        }
        
        // Limit the number of images
        if (_activeImages.Count >= maxImages)
        {
            Destroy(_activeImages[0].GameObject);
            _activeImages.RemoveAt(0);
        }
        
        // Create new object with sprite renderer
        GameObject imageObj = new GameObject("AfterImage_" + Time.frameCount);
        imageObj.transform.SetParent(null); // Ensure it's in the root of the hierarchy
        imageObj.transform.position = transform.position;
        imageObj.transform.rotation = transform.rotation;
        imageObj.transform.localScale = transform.localScale;
        
        SpriteRenderer imageSpriteRenderer = imageObj.AddComponent<SpriteRenderer>();
        
        // Copy current animation frame and properties
        imageSpriteRenderer.sprite = _spriteRenderer.sprite;
        imageSpriteRenderer.flipX = _spriteRenderer.flipX;
        imageSpriteRenderer.flipY = _spriteRenderer.flipY;
        imageSpriteRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
        imageSpriteRenderer.sortingOrder = _spriteRenderer.sortingOrder - 1;
        
        // Set up the material for blending
        if (blendMaterial)
        {
            // Use custom blend material if provided
            imageSpriteRenderer.material = new Material(blendMaterial);
        }
        else 
        {
            // Use standard sprite material with blend mode
            imageSpriteRenderer.material = new Material(Shader.Find("Sprites/Default"));
            
            // Set the material's blend mode to overlay/multiply
            imageSpriteRenderer.material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
            imageSpriteRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            imageSpriteRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        }
        
        // Use the original sprite color multiplied by our overlay color
        // This preserves the original colors but tints them purple
        Color originalColor = _spriteRenderer.color;
        Color combinedColor = new Color(
            originalColor.r * overlayColor.r,
            originalColor.g * overlayColor.g,
            originalColor.b * overlayColor.b,
            originalColor.a * overlayColor.a
        );
        
        imageSpriteRenderer.color = combinedColor;
        
        // Add fade component
        AfterImageFade fadeBehavior = imageObj.AddComponent<AfterImageFade>();
        fadeBehavior.startColor = combinedColor;
        fadeBehavior.endColor = endColor;
        fadeBehavior.duration = imageDuration;
        fadeBehavior.showDebug = showDebugInfo;
        fadeBehavior.preserveOriginalColor = true;
        fadeBehavior.originalColor = originalColor;
        
        // Track the instance
        _activeImages.Add(new AfterImageInstance
        {
            GameObject = imageObj,
            EndTime = Time.time + imageDuration
        });
    }
    
    private class AfterImageInstance
    {
        public GameObject GameObject;
        public float EndTime;
    }
}

public class AfterImageFade : MonoBehaviour
{
    public Color startColor;
    public Color endColor;
    public float duration;
    public bool showDebug;
    public bool preserveOriginalColor;
    public Color originalColor;
    
    private SpriteRenderer _spriteRenderer;
    private float _startTime;
    
    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            if (showDebug)
            {
                Debug.LogError("AfterImageFade: No SpriteRenderer found!");
            }
            Destroy(gameObject);
            return;
        }
        
        _startTime = Time.time;
        
        if (showDebug)
        {
            Debug.Log("AfterImageFade started on " + gameObject.name);
        }
    }
    
    private void Update()
    {
        float progress = (Time.time - _startTime) / duration;
        if (progress >= 1)
        {
            if (showDebug)
            {
                Debug.Log("AfterImage faded completely: " + gameObject.name);
            }
            Destroy(gameObject);
            return;
        }
        
        if (preserveOriginalColor)
        {
            // Blend current color with end color while preserving original hue
            Color currentOverlay = Color.Lerp(startColor, endColor, progress);
            
            // Apply the current overlay color
            Color blendedColor = new Color(
                originalColor.r * currentOverlay.r,
                originalColor.g * currentOverlay.g,
                originalColor.b * currentOverlay.b,
                originalColor.a * currentOverlay.a
            );
            
            _spriteRenderer.color = blendedColor;
        }
        else
        {
            // Simple color lerp
            _spriteRenderer.color = Color.Lerp(startColor, endColor, progress);
        }
    }
}
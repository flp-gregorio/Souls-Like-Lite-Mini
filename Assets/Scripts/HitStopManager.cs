// Updated HitStopManager.cs
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    [SerializeField] private float hitStopDuration = 0.1f;
    [SerializeField] private float hitStopTimeScale = 0.01f;
    private bool isHitStopping;

    public void TriggerHitStop()
    {
        if (!isHitStopping)
        {
            StartCoroutine(DoHitStop());
        }
    }

    private System.Collections.IEnumerator DoHitStop()
    {
        isHitStopping = true;
        Time.timeScale = hitStopTimeScale;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;
        isHitStopping = false;
    }
}

using UnityEngine;
using UnityEngine.Playables;

public class TimelineSpeedController : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private float speedMultiplier = 2f; 

    private void Start()
    {
        if (playableDirector != null)
        {
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(speedMultiplier);
            Debug.Log($"Velocidad del Timeline ajustada a {speedMultiplier}x.");
        }
        else
        {
            Debug.LogError("PlayableDirector no asignado.");
        }
    }
}

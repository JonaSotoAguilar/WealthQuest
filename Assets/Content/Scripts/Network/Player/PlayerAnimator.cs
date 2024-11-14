using System.Collections;
using FishNet.Component.Animating;
using FishNet.Object;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    [SerializeField] private NetworkObject networkObject;

    public void ActiveNetworkAnimator()
    {
        networkObject = GetComponent<NetworkObject>();
        networkObject.enabled = true;
        networkAnimator.enabled = true;
    }

    // FIXME: Cambiar a bool
    public void SetMoving(float x, float y)
    {
        animator.SetFloat("X", x);
        animator.SetFloat("Y", y);
    }

    public void Jump()
    {
        if (networkAnimator != null) networkAnimator.SetTrigger("Jump");
        else animator.SetTrigger("Jump");
    }
}

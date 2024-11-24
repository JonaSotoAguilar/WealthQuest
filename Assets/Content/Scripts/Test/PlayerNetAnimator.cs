using FishNet.Component.Animating;
using FishNet.Object;
using UnityEngine;

public class PlayerNetAnimator : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;

    // FIXME: Cambiar a bool
    public void SetMoving(float x, float y)
    {
        animator.SetFloat("X", x);
        animator.SetFloat("Y", y);
    }

    public void Jump()
    {
        networkAnimator.SetTrigger("Jump");
    }
}

using UnityEngine;

public class InterractibleObject : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool open = false;
    public void LeverTrigger()
    {
        open = !open;
        animator.SetBool("Open", open);
    }
}

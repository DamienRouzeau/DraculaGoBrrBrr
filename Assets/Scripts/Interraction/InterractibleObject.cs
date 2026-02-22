using UnityEngine;

public class InterractibleObject : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool open = false;
    public void LeverTrigger()
    {
        open = true;
        animator.SetBool("Open", open);
        AudioManager.instance.PlayAudio(transform, "Lever");
    }
}

using UnityEngine;

public class InterractibleObject : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool open = false;
    public bool isTimeMachine = false;
    public void LeverTrigger()
    {
        if (!isTimeMachine && !open)
        {
            open = true;
            animator.SetBool("Open", open);
            AudioManager.instance.PlayAudio(transform, "Lever");
        }
        else if (!open)
        { 
            GameManager.instance.RemoveTimeMachine();
            AudioManager.instance.PlayAudio(transform, "StopTimeMachine");
            open = true;
        }
    }
}

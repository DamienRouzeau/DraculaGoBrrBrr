using UnityEngine;

public class InterractibleObject : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool open = false;
    public bool isTimeMachine = false;
    [SerializeField] private GameObject trigger;
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
            trigger.SetActive(false);
            AudioManager.instance.PlayAudio(transform, "StopTimeMachine");
            open = true;
        }
    }
}

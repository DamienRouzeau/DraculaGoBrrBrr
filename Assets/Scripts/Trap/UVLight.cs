using UnityEngine;

public class UVLight : MonoBehaviour
{
    [SerializeField] private float onDuration = 2;
    [SerializeField] private Animator anim;
    private bool isOn;
    private float timer;


    private void Start()
    {
        timer = onDuration;
    }
    private void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;
        if(timer <= 0 )
        {
            SwitchLight();
        }
    }

    private void SwitchLight()
    {
        isOn = !isOn;
        anim.SetBool("TurnOn", isOn);
        timer = onDuration;
    }
}

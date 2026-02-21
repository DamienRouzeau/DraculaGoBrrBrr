using UnityEngine;

public class TriggerBox : MonoBehaviour
{
    [SerializeField] private DialogData _data;
    [SerializeField] private bool destroyAfterTrigger;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            DialogManager.instance.AddDialogue(_data, _data.time);
            Destroy(gameObject);
        }
    }
}

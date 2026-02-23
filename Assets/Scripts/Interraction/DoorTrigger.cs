using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private GameObject garlics;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Player"))
        {
            GameManager.instance.Rewind();
            garlics.SetActive(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.instance.Rewind();
            garlics.SetActive(false);

        }
    }
}

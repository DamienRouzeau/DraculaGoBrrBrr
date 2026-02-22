using System.Collections;
using UnityEngine;

public class TPManager : MonoBehaviour
{
    private static TPManager Instance { get; set; }
    public static TPManager instance => Instance;

    [SerializeField] private PlayerController player;
    [SerializeField] private Transform tpDest;
    [SerializeField] private Animator anim;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void TP()
    {
        anim.SetTrigger("TP");
        StartCoroutine(DelayedTP());
    }
    private IEnumerator DelayedTP()
    {
        yield return new WaitForSeconds(1f);
        player.transform.position = tpDest.position;
    }

}

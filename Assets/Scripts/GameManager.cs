using System;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private static GameManager Instance { get; set; }
    public static GameManager instance => Instance;

    private UnityEvent rewindEvent = new UnityEvent();

     
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void SubscribeRewind(UnityAction _action)
    {
        rewindEvent.AddListener(_action);
    }

    public void UnsubscribeRewind(UnityAction _action)
    {
        rewindEvent.RemoveListener(_action);
    }

    public void Rewind()
    {
        rewindEvent.Invoke();
    }

}

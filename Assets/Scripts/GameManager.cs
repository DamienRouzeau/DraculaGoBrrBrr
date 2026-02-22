using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private static GameManager Instance { get; set; }
    public static GameManager instance => Instance;

    private UnityEvent rewindEvent = new UnityEvent();

    private Audio currentMusic;


     
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        currentMusic = AudioManager.instance.PlayAudio(transform, "Music1");
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
        AudioManager.instance.PlayAudio(transform, "Rewind");
    }

    public void StartTP()
    {
        TPManager.instance.TP();
    }

    public void RemoveTimeMachine()
    {
        currentMusic.Stop();
        currentMusic = null;
        LoopManager.instance.RemoveTimeMachine();
        FindFirstObjectByType<PlayerController>()?.SetMachine(false);
        StartCoroutine(MusicDelayed())
;   }
    
    private IEnumerator MusicDelayed()
    {
        yield return new WaitForSeconds(1.5f);
        currentMusic = AudioManager.instance.PlayAudio(transform, "Music2");
    }

}

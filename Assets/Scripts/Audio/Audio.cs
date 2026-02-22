using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    public string audioName;
    public AudioSource audioSource;
    public float timeToPlay;
    private bool shouldFadeOut;
    private bool shouldFadeIn;
    private Coroutine coroutine;
    private float maxVolume;

    private void Start()
    {
        if (transform.parent != null) { if (transform.parent.name == "AudioManager") { return; } }
        if (!audioSource.loop) StartCoroutine(StopAudio());
    }
    private void Update()
    {
        if(shouldFadeOut)
        {
            audioSource.volume -= Time.deltaTime;
            if(coroutine == null)coroutine = StartCoroutine(StopAudio());
        }
        else if(shouldFadeIn && audioSource.volume < maxVolume)
        {
            audioSource.volume += Time.deltaTime;
        }
        else if(shouldFadeIn && audioSource.volume > maxVolume)
        {
            audioSource.volume = maxVolume;
        }
    }

    public IEnumerator StopAudio()
    {
        yield return new WaitForSeconds(timeToPlay);
        audioSource.Stop();
        Destroy(this.gameObject);
    }

    public void Stop()
    {
        if(audioSource.isPlaying)audioSource.Stop();
        Destroy(this.gameObject);
    }

    public void FadeOut()
    {
        shouldFadeOut = true;
    }

    public void FadeIn(float max)
    {
        audioSource.volume = 0;
        audioSource.Play();
        shouldFadeIn = true;
        maxVolume = max;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager Instance { get; set; }
    public static AudioManager instance => Instance;

    [Header("Audio")]
    [SerializeField]
    private List<Audio> audios = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    #region Play audio

    public Audio PlayAudio(Transform parent, string _name)
    {
        foreach (Audio _audio in audios)
        {
            if (_audio.audioName == _name)
            {
                var audioObject = Instantiate(_audio, parent);
                audioObject.audioSource.Play();
                return audioObject;
            }
        }
        print("Audio : " + _name + " not found");
        return null;
    }

    public Audio PlayAudio(Transform parent, string _name, float _volume)
    {
        foreach (Audio _audio in audios)
        {
            if (_audio.audioName == _name)
            {
                var audioObject = Instantiate(_audio, parent);
                audioObject.audioSource.volume = _volume;
                audioObject.audioSource.Play();
                return audioObject;
            }
        }
        return null;
    }

    public Audio PlayAudio(Transform parent, string _name, float _volume, float _pitch)
    {
        foreach (Audio _audio in audios)
        {
            if (_audio.audioName == _name)
            {
                var audioObject = Instantiate(_audio, parent);
                audioObject.audioSource.volume = _volume;
                audioObject.audioSource.pitch = _pitch;
                audioObject.audioSource.Play();
                return audioObject;
            }
        }
        return null;
    }

    public Audio PlayRandomAudio(Transform parent, string[] _name)
    {
        string choosedName = _name[Random.Range(0, _name.Length)];
        foreach (Audio _audio in audios)
        {
            if (_audio.audioName == choosedName)
            {
                var audioObject = Instantiate(_audio, parent);
                audioObject.audioSource.Play();
                return audioObject;
            }
            else
            {
                print("Audio : " + _name + " not found");
            }
        }
        return null;
    }

    public Audio PlayRandomAudio(Transform parent, string[] _name, float _volume)
    {
        string choosedName = _name[Random.Range(0, _name.Length)];
        foreach (Audio _audio in audios)
        {
            if (_audio.audioName == choosedName)
            {
                var audioObject = Instantiate(_audio, parent);
                audioObject.audioSource.volume = _volume;
                audioObject.audioSource.Play();
                return audioObject;
            }
        }
        return null;
    }

    public Audio PlayRandomAudio(Transform parent, string[] _name, float _volume, float _pitch)
    {
        string choosedName = _name[Random.Range(0, _name.Length)];
        foreach (Audio _audio in audios)
        {
            if (_audio.audioName == choosedName)
            {
                var audioObject = Instantiate(_audio, parent);
                audioObject.audioSource.volume = _volume;
                audioObject.audioSource.pitch = _pitch;
                audioObject.audioSource.Play();
                return audioObject;
            }
        }
        return null;
    }

    public Audio PlayAudioAtSecond(Transform parent, string _name, float _time)
    {
        foreach (Audio _audio in audios)
        {
            if (_audio.audioName == _name)
            {
                var audioObject = Instantiate(_audio, parent);
                audioObject.audioSource.time = _time;
                audioObject.audioSource.Play();
                return audioObject;
            }
        }
        return null;
    }
    public Audio FadeIn(Transform parent, string name, float maxVolume)
    {
        foreach (Audio _audio in audios)
        {
            if (_audio.audioName == name)
            {
                var audioObject = Instantiate(_audio, parent);
                audioObject.audioSource.volume = 0;
                audioObject.FadeIn(maxVolume);
                return audioObject;
            }
        }
        return null;
    }
    #endregion

    public void StopAudio(string _name)
    {
        foreach (Audio _audio in audios)
        {
            if (_audio.audioName == _name)
            {
                _audio.StopAudio();
            }
        }
    }
}

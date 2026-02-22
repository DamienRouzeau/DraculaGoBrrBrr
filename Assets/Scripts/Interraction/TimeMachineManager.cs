using UnityEngine;
using UnityEngine.Events;

public class TimeMachineManager : MonoBehaviour
{
    private static TimeMachineManager Instance { get; set; }
    public static TimeMachineManager instance => Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }
}

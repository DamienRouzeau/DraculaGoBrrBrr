using UnityEngine;

[CreateAssetMenu(fileName = "Dialog", menuName = "ScriptableObjects/Dialog", order = 1)]
public class DialogData : ScriptableObject
{
    public string[] text;
    public float time;
}

using UnityEngine;
using TMPro;
using System.Collections;

public class DialogManager : MonoBehaviour
{
    private static DialogManager Instance;
    public static DialogManager instance => Instance;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject ui;
    private int index = 0;
    private Coroutine coroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        ui.SetActive(false);
    }

    public void AddDialogue(DialogData _data, float _time)
    {
        if (coroutine != null) StopCoroutine(coroutine);
        index = 0;
        ui.SetActive(true);
        text.text = _data.text[0];
        coroutine = StartCoroutine(RemoveDialogue(_time, _data));
    }

    private IEnumerator RemoveDialogue(float _time, DialogData _data)
    {
        yield return new WaitForSeconds(_time);
        if (_data.text.Length > 1)
        {
            index++;
            if (index < _data.text.Length)
            {
                text.text = _data.text[index];
                coroutine = StartCoroutine(RemoveDialogue(_time, _data));
            }
            else
            {
                ui.SetActive(false);
            }
        }
        else
        {
            ui.SetActive(false);
        }
    }
}

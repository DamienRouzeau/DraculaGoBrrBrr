using UnityEngine;

public class Credit : MonoBehaviour
{
    public float speed = 20f;
    public RectTransform creditsText;

    void OnEnable()
    {
        creditsText.anchoredPosition = new Vector2(0, -500f);
    }

    void Update()
    {
        creditsText.anchoredPosition += Vector2.up * speed * Time.deltaTime;
    }
}

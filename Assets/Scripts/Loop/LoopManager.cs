using UnityEngine;
using UnityEngine.UI;

public class LoopManager : MonoBehaviour
{
    private static LoopManager Instance { get; set; }
    public static LoopManager instance => Instance;

    [Header("Gauge")]
    [SerializeField] private float maxTime;
    [SerializeField] private float minTime;
    [SerializeField] private float timeAvailable;
    [SerializeField] private float removedTime;

    [Header("Timer")]
    [SerializeField] private float inGameTimer;
    [SerializeField] private float removeTimeMult;

    [Header("UI")]
    [SerializeField] private RectTransform gauge;
    [SerializeField] private HorizontalLayoutGroup gaugeContainer;
    [SerializeField] private Image timeRemaining;
    [SerializeField] private Image timeRemovedUI;
    [SerializeField] private Image timeAvailableUI;


    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        timeAvailable = maxTime;
        UpdateTimerUI();
        GameManager.instance.SubscribeRewind(OnPlayerDeath);
        inGameTimer = timeAvailable;
        OnPlayerDeath();
    }

    #region Timer management
    private void FixedUpdate()
    {
        inGameTimer -= Time.fixedDeltaTime;
        if (inGameTimer <= 0)
        {
            GameManager.instance.Rewind();
        }
        UpdateTimerUI();
    }
    #endregion

    #region Modify timers
    public void OnPlayerDeath()
    {
        float elapsedTime = timeAvailable - inGameTimer;
        Debug.Log(timeAvailable + " - " + inGameTimer + " = " + elapsedTime);
        timeAvailable -= elapsedTime * removeTimeMult;
        timeAvailable = Mathf.Max(timeAvailable, minTime);
        removedTime = maxTime - timeAvailable;
        inGameTimer = timeAvailable;
        UpdateTimerUI();
    }

    public void AddTime(float _time)
    {
        timeAvailable += _time;
    }


    public void UpdateTimerUI()
    {
        float _size = gauge.sizeDelta.x;
        float _spacing = gaugeContainer.spacing;
        RectTransform _rectTransform = timeRemaining.GetComponent<RectTransform>();


        //  Gauge of time avaible
        float _sizeTimeAvailable = (timeAvailable * _size) / maxTime;
        _rectTransform = timeAvailableUI.GetComponent<RectTransform>();
        _rectTransform.sizeDelta = new Vector2(_sizeTimeAvailable, _rectTransform.sizeDelta.y);

        // Gauge of time remaining
        float _sizeRemainingTime = (inGameTimer * (_sizeTimeAvailable) / timeAvailable);
        _rectTransform = timeRemaining.GetComponent<RectTransform>();
        _rectTransform.sizeDelta = new Vector2(_sizeRemainingTime, _rectTransform.sizeDelta.y);

        // Gauge of time removed
        float _sizeRemovedTime = (removedTime * (_size - _spacing)) / maxTime;
        Debug.Log(removedTime + " x (" + _size + " - " + _spacing + ") / " + maxTime);
        _rectTransform = timeRemovedUI.GetComponent<RectTransform>();
        _rectTransform.sizeDelta = new Vector2(_sizeRemovedTime, _rectTransform.sizeDelta.y);


    }
    #endregion
}

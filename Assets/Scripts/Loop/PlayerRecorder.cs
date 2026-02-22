using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRecorder : MonoBehaviour
{

    [Header("Recording")]
    [Tooltip("Durée max enregistrée en secondes (20min = 1200)")]
    [SerializeField] private float maxRecordDuration = 1200f;

    [Tooltip("Intervalle entre deux snapshots en secondes. 0.2 = 5 snapshots/sec, largement suffisant pour un effet visuel")]
    [SerializeField] private float recordInterval = 0.2f;

    [Header("Rewind")]
    [Tooltip("Durée totale du rembobinage en secondes, peu importe la longueur du run")]
    [SerializeField] private float rewindTotalDuration = 3f;

    [Tooltip("Délai avant de commencer le rembobinage (freeze dramatique)")]
    [SerializeField] private float rewindDelay = 0.4f;

    [Header("References")]
    [SerializeField] private PlayerController playerController;


    public System.Action OnRewindComplete;


    private struct Snapshot
    {
        public Vector3 position;
        public Vector3 scale;
        public Vector2 colliderSize;
        public Vector2 colliderOffset;
        public float timestamp;
    }

    private List<Snapshot> history = new List<Snapshot>();
    private CapsuleCollider2D col;
    private Rigidbody2D rb;

    private bool isRewinding = false;
    private bool isRecording = true;
    private float recordTimer = 0f;

    private int maxSnapshots;


    private void Awake()
    {
        col = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        maxSnapshots = Mathf.CeilToInt(maxRecordDuration / recordInterval);
    }

    private void Start()
    {
        GameManager.instance.SubscribeRewind(TriggerRewind);
    }

    private void OnDestroy()
    {
        GameManager.instance.UnsubscribeRewind(TriggerRewind);
    }

    private void Update()
    {
        if (!isRecording || isRewinding) return;

        recordTimer += Time.deltaTime;
        if (recordTimer >= recordInterval)
        {
            recordTimer = 0f;
            Record();
        }
    }


    private void Record()
    {
        Snapshot snap = new Snapshot
        {
            position = transform.position,
            scale = transform.localScale,
            colliderSize = col.size,
            colliderOffset = col.offset,
            timestamp = Time.time
        };

        history.Add(snap);

        if (history.Count > maxSnapshots)
            history.RemoveAt(0);
    }


    public void TriggerRewind()
    {
        if (isRewinding) return;
        StartCoroutine(RewindRoutine());
    }

    private IEnumerator RewindRoutine()
    {
        isRewinding = true;
        isRecording = false;

        FindFirstObjectByType<VHSEffectController>()?.OnRewindStart();

        if (playerController != null)
            playerController.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        yield return new WaitForSeconds(rewindDelay);

        int totalSnapshots = history.Count;
        float timePerSnapshot = rewindTotalDuration / Mathf.Max(1, totalSnapshots);
        float snapshotTimer = 0f;
        int index = totalSnapshots - 1;

        while (index >= 0)
        {
            Snapshot snap = history[index];
            transform.position = snap.position;
            transform.localScale = snap.scale;
            col.size = snap.colliderSize;
            col.offset = snap.colliderOffset;

            snapshotTimer = 0f;
            while (snapshotTimer < timePerSnapshot)
            {
                snapshotTimer += Time.deltaTime;
                yield return null;
            }

            index--;
        }

        yield return new WaitForSeconds(0.2f);
        FinishRewind();
    }

    private void FinishRewind()
    {
        history.Clear();
        isRewinding = false;
        isRecording = true;

        rb.bodyType = RigidbodyType2D.Dynamic;

        if (playerController != null)
            playerController.enabled = true;

        OnRewindComplete?.Invoke();
    }


    public void ResetRecording()
    {
        history.Clear();
        isRecording = true;
        isRewinding = false;
    }

    public void SetRecording(bool active) => isRecording = active;

    public bool IsRewinding => isRewinding;
}
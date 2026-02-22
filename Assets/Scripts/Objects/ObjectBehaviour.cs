using System.Collections;
using UnityEngine;

public class ObjectBehaviour : MonoBehaviour
{
    [SerializeField] private ObjectType type;
    [SerializeField] private float strenght;
    [SerializeField] private float duration;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CircleCollider2D col;
    public void Collect()
    {

        switch (type)
        {
            case ObjectType.AddTime:
                LoopManager.instance.AddTime(strenght);
                Destroy(gameObject);
                break;

            case ObjectType.Slowmo:
                LoopManager.instance.ModifyTimeModifier(strenght);
                col.enabled = false;
                spriteRenderer.enabled = false;
                StartCoroutine(EffectDuration());
                break;

            default:
                break;
        }
    }

    private IEnumerator EffectDuration()
    {
        yield return new WaitForSeconds(duration);
        LoopManager.instance.ModifyTimeModifier(1);
        Destroy(gameObject) ;

    }
}

public enum ObjectType
{
    AddTime,
    Slowmo
}

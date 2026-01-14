using UnityEngine;
using DG.Tweening;

public class Block : MonoBehaviour
{
    public int y, x, colorIdx;
    public ParticleSystem effect;
    [SerializeField] private Sprite[] sprites;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        SetColor();
    }

    public void SetColor(int idx = -1)
    {
        colorIdx = idx == -1 ? Random.Range(0, sprites.Length) : idx;
        spriteRenderer.sprite = sprites[colorIdx];
    }

    public void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.Play();
    }
    
    public void MoveToTarget(Vector3 targetPos, float duration, Ease ease = Ease.Linear)
    {
        transform.DOKill();
        transform.DOMove(targetPos, duration).SetEase(ease);
    }
}

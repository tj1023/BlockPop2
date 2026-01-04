using UnityEngine;

public class Block : MonoBehaviour
{
    public int y, x, colorIdx;
    public ParticleSystem effect;
    
    private readonly Color[] colors = {Color.red, Color.orange, Color.yellow, Color.green, Color.blue, Color.navyBlue, Color.purple};
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
        colorIdx = idx == -1 ? Random.Range(0, colors.Length) : idx;
        spriteRenderer.color = colors[colorIdx];
    }

    public void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.Play();
    }
}

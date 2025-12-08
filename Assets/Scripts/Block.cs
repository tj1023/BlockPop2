using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Block : MonoBehaviour
{
    public int y, x, colorIdx;
    public ParticleSystem effect;
    
    private Color[] colors = {Color.red, Color.orange, Color.yellow, Color.green, Color.blue, Color.navyBlue, Color.purple};
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
        if(idx == -1) colorIdx = UnityEngine.Random.Range(0, colors.Length);
        else colorIdx = idx;
        spriteRenderer.color = colors[colorIdx];
    }

    public void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.Play();
    }
}

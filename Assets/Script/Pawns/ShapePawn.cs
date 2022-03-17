using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapePawn : MonoBehaviour
{
    protected DialogWaveform waveFormContainer = null;
    public Transform WaveAttachPoint;
    private Vector2 InitWaveAttachPoint;

    public Color InitColor = Color.white;
    protected Color PawnColor;

    public SpriteRenderer spriteRendererRef;
    protected Rigidbody2D rigidBodyRef;

    public float DistanceTravelled = 0;
    private Vector2 previousLocation;

    public void Start()
    {
        previousLocation = transform.position;
        SetPawnColor(InitColor);
        rigidBodyRef = GetComponent<Rigidbody2D>();
        InitWaveAttachPoint = WaveAttachPoint.localPosition;
    }

    public void SetPawnColor(Color _color)
    {
        InitColor = _color;
        PawnColor = _color;
        spriteRendererRef.color = _color;
    }

    public Color GetPawnColor()
    {
        return PawnColor;
    }

    protected void Update()
    {
        WaveAttachPoint.localPosition = InitWaveAttachPoint * spriteRendererRef.transform.localScale.x;
    }

    protected void FixedUpdate()
    {
        float distance = Vector2.Distance(transform.position, previousLocation);
        previousLocation = transform.position;
        if(distance > 0)
        {
            DistanceTravelled += distance;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoopParticle : MonoBehaviour
{
    public Rigidbody2D rigidBodyRef;
    public SpriteRenderer spriteRendererRef;
    public Color32 ParticleColor;
    private int SplatSize = 0;

    // Start is called before the first frame update
    void Start()
    {
        int tempChoice = Random.Range(0, 2);
        switch(tempChoice)
        {
            case 0:
                spriteRendererRef.transform.localScale = new Vector2(3, 3);
                SplatSize = 0;
                break;
            case 1:
                spriteRendererRef.transform.localScale = new Vector2(6, 6);
                SplatSize = 1;
                break;
            default:
                break;
        }
    }


    private void FixedUpdate()
    {
        rigidBodyRef.velocity = Vector2.MoveTowards(rigidBodyRef.velocity, Vector2.zero, 2f * Time.fixedDeltaTime);

        if (rigidBodyRef.velocity == Vector2.zero)
        {
            ParticleDeath();
        }

    }
    void ParticleDeath()
    {
        CharHandler.Instance.DoSplat(transform.position, ParticleColor, (ESPLATSIZE)SplatSize);
        DestroyImmediate(gameObject);
    }
}

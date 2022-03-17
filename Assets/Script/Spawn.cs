using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using static Utils;


public class Spawn : MonoBehaviour
{
    public GameObject Opening_1;
    public float Opening1_Velocity = 0;
    public GameObject Opening_2;
    public float Opening2_Velocity = 0;

    public NPCShape Parent;

    public Light2D Light2DRef;
    public AudioSource wombSoundRef;
    private float LightIntensity;
    private float WombSoundVolume;

    public float InitTimer = 5;
    public float WombTimer = 5;

    private Vector2 wombScale;

    private float LightDecrease = 0;
    // Start is called before the first frame update
    void Start()
    {
        wombScale = transform.localScale;
        LightIntensity = Light2DRef.intensity;
        Light2DRef.intensity = 0;
        WombSoundVolume = wombSoundRef.volume;
        wombSoundRef.volume = 0;

        SetTimer(InitTimer, StartGame);
    }

    // Update is called once per frame
    void Update()
    {
        if(Opening1_Velocity > 0)
        {
            Opening_1.transform.localPosition = Opening_1.transform.localPosition + Vector3.right *  Opening1_Velocity * Time.deltaTime;
        }
        if (Opening2_Velocity > 0)
        {
            Opening_2.transform.localPosition = Opening_2.transform.localPosition + Vector3.right * -Opening2_Velocity * Time.deltaTime;
        }
        Opening1_Velocity -= 0.2f * Time.deltaTime;
        Opening2_Velocity -= 0.2f * Time.deltaTime;
        if(Opening1_Velocity < 0)
        {
            Opening1_Velocity = 0;
        }
        if (Opening2_Velocity < 0)
        {
            Opening2_Velocity = 0;
        }

        Light2DRef.intensity = Mathf.Clamp01(Light2DRef.intensity - LightDecrease * Time.deltaTime);

        switch (CharHandler.Instance.currentGameStage)
        {
            case EGAMESTAGE.INIT:
                transform.localScale = wombScale + 0.03f * new Vector2(Mathf.PerlinNoise(Time.time / 4, 0), Mathf.PerlinNoise(0, Time.time / 2));
                wombSoundRef.volume = Mathf.Clamp(wombSoundRef.volume + (Time.deltaTime / InitTimer) * WombSoundVolume, 0, WombSoundVolume);
                break;
            case EGAMESTAGE.WOMB:
                transform.localScale = wombScale + 0.03f * new Vector2(Mathf.PerlinNoise(Time.time / 4, 0), Mathf.PerlinNoise(0, Time.time / 2));
                if (Light2DRef.intensity != LightIntensity)
                {
                    Light2DRef.intensity = Mathf.Clamp(Light2DRef.intensity + (Time.deltaTime / WombTimer) * LightIntensity, 0, LightIntensity);
                }
                break;
            case EGAMESTAGE.BIRTH:
                transform.localScale = wombScale + 0.03f * new Vector2(Mathf.PerlinNoise(Time.time / 4, 0), Mathf.PerlinNoise(0, Time.time / 2));
                wombSoundRef.volume = WombSoundVolume * (1 - Mathf.Clamp01(Vector2.Distance(CharHandler.Instance.PlayerRef.transform.position.normalized * 0.5f, CharHandler.Instance.PlayerRef.transform.position) / 2 ));
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(CharHandler.Instance.currentGameStage > EGAMESTAGE.WOMB && CharHandler.Instance.PlayerShapeRef.growthMultiplier == 1)
        {
            Debug.Log("Boing");
            Opening1_Velocity += 0.3f;
            Opening2_Velocity += 0.3f;
            LightDecrease = 0.2f;

            SetTimer(0.3f, TurnOffParent);
            Parent.Light2DRef.enabled = true;
        }
    }

    private void StartGame()
    {
        CharHandler.Instance.ChangeGameStage(EGAMESTAGE.WOMB);
        SetTimer(WombTimer, StartBirthStage);
    }

    private void StartBirthStage()
    {
        CharHandler.Instance.ChangeGameStage(EGAMESTAGE.BIRTH);
    }

    private void TurnOffParent()
    {
        Parent.Light2DRef.enabled = false;
        CharHandler.Instance.InstantiateSpeechBubble(CharHandler.EWaveForm.BoopResponse, Parent.WaveAttachPoint.position, Parent.WaveAttachPoint);
    }
}

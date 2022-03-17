using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using static Utils;
using EWave = CharHandler.EWaveForm;


public class PlayerShape : ShapePawn
{
    //  [Header("STAGING")]
    //  public float GrowTime = 20;
    //  public float CamGrowTime = 20;
    //  public float LearnToMoveTime = 100;
    public float CurrentAgeGoal;

    public SpriteRenderer FadeScreen;

    [Header("COMPONENT REFS")]
    public Camera CameraRef;
    public Light2D BackLight2DRef;
    public Light2D CenterLight2DRef;

    [Header("PLAYERSHAPE")]
    public EGAMESTATEDataDict PlayerAgeDataDictionary;

    public NPCShape Parent_1 = null;
    public NPCShape Parent_2 = null;

    public float distanceToMouse;
    public Vector2 PastPlayerPosition;
    // Start is called before the first frame update
    TimerContainer BoopTimer;

    [Header("LIGHT")]
    public bool IsInDarkness = false;

    public float BackLightIntensity = 0.3f;
    public float CenterLightIntensity = 0.8f;

    public int MaxBackLight = 2;
    private float currentBackLight;
    public int MaxCenterLight = 4;
    private float currentCenterLight;

    [Header("CLOSEST LIGHT")]
    public int closestLightID = -1;
    public float closestLightDistance = float.MaxValue;
    public float closestLightModifier = 1;

    public TimerContainer LightTimer;
    [Header("DEBUG")]
    public float currentVelocity;
    private float currentVeloFloor;
    public float growthMultiplier = 0;
    private float growFloor;
    public float cameraMultiplier = 0;
    private float cameraFloor;
    public float movespeedGrowthMultiplier = 0;
    private float movespeedGrowthFloor;
    public float lightMultiplier = 0;

    private int CurrentAgeGoalRangeIndex = 1;

    public bool GameOver;
    private TimerContainer DeathTimer;
    public int DeathCounter = 0;

    new private void Start()
    {
        CurrentAgeGoal = CharHandler.Instance.MapDividers[CurrentAgeGoalRangeIndex];

        BackLight2DRef.intensity = BackLightIntensity;
        CenterLight2DRef.intensity = CenterLightIntensity;

        spriteRendererRef.transform.localScale = new Vector2(0.1f, 0.1f);
        base.Start();
        PastPlayerPosition = transform.position;

        SetTimer(15, SavePlayerPosition, false);

        LightTimer = SetTimer(5, ReduceLight, false);
    }

    new private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if(GameOver)
        {
            FadeScreen.color = new Color(0, 0, 0, FadeScreen.color.a + 1f / 30f * Time.deltaTime);
            return;
        }
        if (DistanceTravelled >= 500 && DeathTimer == null)
        {
            TryDeath();
            DeathTimer = SetTimer(5, TryDeath, false);
        }

        base.Update();
        switch (CharHandler.Instance.currentGameStage)
        {
            case EGAMESTAGE.INIT:

                break;
            case EGAMESTAGE.WOMB:

                break;
            case EGAMESTAGE.BIRTH:
                //CameraRef.orthographicSize = 1 + 3 * Mathf.Clamp(cameraMultiplier * cameraMultiplier, 0.1f, 1);
                break;
        }

        if(CharHandler.Instance.currentGameStage > EGAMESTAGE.WOMB)
        {
            lightMultiplier = Mathf.Clamp01(lightMultiplier + Time.deltaTime * 1 / 30);
        }

        if(transform.position.magnitude > CurrentAgeGoal)
        {
            CurrentAgeGoalRangeIndex++;
            if(CurrentAgeGoalRangeIndex == CharHandler.Instance.MapDividers.Length)
            {
                CurrentAgeGoal = float.MaxValue;
            }
            else
            {
                CurrentAgeGoal = CharHandler.Instance.MapDividers[CurrentAgeGoalRangeIndex];
                CharHandler.Instance.IncreaseStage();
            }
        }

        ApplyPlayerData();
        DoLight();
    }

    private void TryDeath()
    {
        if (Random.Range(0, 5) == 0)
        {
            PlayerDeath();
        }
    }

    public void PlayerDeath()
    {
        CharHandler.Instance.CreateProperSplat(19, transform.position, Vector2.right, GetPawnColor(), 3, 180);
        rigidBodyRef.bodyType = RigidbodyType2D.Static;
        if(LightTimer != null)
        {
            LightTimer.CancelTimer();
        }
        if(BoopTimer != null)
        {
            BoopTimer.CancelTimer();
        }
        if(DeathTimer != null)
        {
            DeathTimer.CancelTimer();
        }
        BackLight2DRef.enabled = false;
        CenterLight2DRef.enabled = false;
        SetPawnColor(Color.gray);
        GameOver = true;

        SetTimer(60, EndGame);
    }

    private void EndGame ()
    {
        Application.Quit();
    }

    private void DoLight()
    {
        if(IsInDarkness == false && LightTimer.IsPaused() == false)
        {
            LightTimer.PauseTimer();
        }
        else if (IsInDarkness && LightTimer.IsPaused())
        {
            LightTimer.ResetTimer();
        }

        float tempRadiusOffset = (Mathf.Sin(Time.unscaledTime) + 1) * 0.3f;
        float tempIntensityOffset = (0.25f * Mathf.Clamp01( Mathf.PerlinNoise(Time.unscaledTime * 4f, 0) ) + 0.75f);


        BackLight2DRef.intensity = BackLightIntensity * closestLightModifier * tempIntensityOffset * (lightMultiplier* lightMultiplier);
        CenterLight2DRef.intensity = CenterLightIntensity * closestLightModifier * tempIntensityOffset * (lightMultiplier * lightMultiplier);

        float backlightRadiusTemp = currentBackLight;
        float centerlightRadiusTemp = currentCenterLight;

        BackLight2DRef.pointLightInnerRadius = backlightRadiusTemp * 0.5f + 0.2f * tempRadiusOffset;
        BackLight2DRef.pointLightOuterRadius = (backlightRadiusTemp + 2) * 0.5f + 0.2f * tempRadiusOffset;
        CenterLight2DRef.pointLightOuterRadius = centerlightRadiusTemp * 0.5f + 0.2f * tempRadiusOffset;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameOver)
        {
            return;
        }
        if (collision.gameObject.tag == "Spawn")
        {
            rigidBodyRef.AddForce(transform.position.normalized * -3);
            BoopTimer = SetTimer(0.45f, null);
            if(waveFormContainer == null)
            {
                waveFormContainer = CharHandler.Instance.InstantiateSpeechBubble(EWave.Boop, collision.GetContact(0).point, WaveAttachPoint);
            }
            CharHandler.Instance.SomeoneGotBooped(collision.GetContact(0).point, new Color32(), null, EMEM.SPAWN);
        }
        else
        {
            NPCShape otherShape = collision.gameObject.GetComponent<NPCShape>();
            rigidBodyRef.velocity = rigidBodyRef.velocity / 2;
            BoopTimer = SetTimer(0.3f, null);

            if (otherShape != null)
            {
                if (otherShape.IsDead)
                {
                    rigidBodyRef.AddForce((otherShape.transform.position - transform.position).normalized * -150);
                }
                else
                {
                    if (Parent_1 == null)
                    {
                        Parent_1 = otherShape;
                        otherShape.IsParent1 = true;
                    }
                    else if (Parent_2 == null && otherShape.IsParent1 == false)
                    {
                        Parent_2 = otherShape;
                        otherShape.IsParent2 = true;
                    }
                    if (waveFormContainer == null)
                    {
                        CharHandler.Instance.InstantiateSpeechBubble(EWave.Boop, collision.GetContact(0).point, WaveAttachPoint);
                    }

                    Color tempColor = MixColors(GetPawnColor(), otherShape.GetPawnColor(), PlayerAgeDataDictionary[CharHandler.Instance.currentGameStage].SelfMixValue, 1);
                    SetPawnColor(tempColor);

                    if (otherShape.IsParent1 && (otherShape.ChildBoopCounter % 5 == 0))
                    {
                        CharHandler.Instance.SomeoneGotBooped(collision.GetContact(0).point, tempColor, otherShape, EMEM.FUTURE);
                    }
                    else if (otherShape.IsParent2 && (otherShape.ChildBoopCounter % 3 == 0))
                    {
                        CharHandler.Instance.SomeoneGotBooped(collision.GetContact(0).point, tempColor, otherShape, EMEM.OUTWARD);
                    }
                    else
                    {
                        CharHandler.Instance.SomeoneGotBooped(collision.GetContact(0).point, tempColor, otherShape);
                    }
                    AddLight(+10);
                }
            }
        }

    }

    private void ReduceLight()
    {
        AddLight(-1);
    }

    private void AddLight(float _value)
    {
        currentBackLight = Mathf.Clamp(currentBackLight + 1 * _value, 0, MaxBackLight);
        currentCenterLight = Mathf.Clamp(currentCenterLight + 1 * _value, 1, MaxCenterLight);

        if(currentBackLight == 0 && CharHandler.Instance.currentGameStage >= EGAMESTAGE.BABY)
        {
            CharHandler.Instance.CreateProperSplat(19, transform.position, Vector2.right, Color.red, 1, 180);
            DeathCounter++;
            if(DeathCounter >= 3)
            {
                TryDeath();
            }
        }
        else
        {
            DeathCounter = 0;
        }
    }

    private void SavePlayerPosition()
    {
        Debug.Log("Last known player position was: " + PastPlayerPosition);

        if(Vector2.Distance(PastPlayerPosition, transform.position) > 15)
        {
            PastPlayerPosition = transform.position;
        }
    }

    new private void FixedUpdate()
    {
        if (GameOver)
        {
            return;
        }
        base.FixedUpdate();
        Vector2 mousePos = Input.mousePosition;
        Vector2 mouseScreenPos = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector2 movementDirection = (mousePos - (Vector2)transform.position).normalized;

        spriteRendererRef.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, movementDirection));

        Debug.DrawLine(transform.position, mousePos, Color.red);
        if (BoopTimer?.going == true)
        {
            //  rigidBodyRef.velocity = Vector2.zero;
        }
        else
        {
            distanceToMouse = Vector2.Distance(new Vector2(0.5f, 0.5f), mouseScreenPos);
            if (distanceToMouse > 0.1f)
            {
                if(CharHandler.Instance.currentGameStage > EGAMESTAGE.INIT)
                {
                    movespeedGrowthMultiplier = Mathf.Clamp01(movespeedGrowthMultiplier + Time.deltaTime / PlayerAgeDataDictionary[CharHandler.Instance.currentGameStage].LearnToMoveTime);
                    float veloDistance = PlayerAgeDataDictionary[CharHandler.Instance.currentGameStage].MovementSpeed - movespeedGrowthFloor;
                    currentVelocity = ( movespeedGrowthFloor +
                                        veloDistance *
                                        Mathf.Clamp(movespeedGrowthMultiplier * movespeedGrowthMultiplier, 0, 1));
                }

                rigidBodyRef.velocity = movementDirection * Mathf.Clamp((distanceToMouse - 0.1f) / 0.2f, 0, 1) * currentVelocity;
            }
            else
            {
                rigidBodyRef.velocity = Vector2.zero;
            }
        }
    }

    public void GameStageChanged(EGAMESTAGE _stage)
    {
        growthMultiplier = 0;
        cameraMultiplier = 0;
        movespeedGrowthMultiplier = 0;
        growFloor = spriteRendererRef.transform.localScale.x;
        cameraFloor = CameraRef.orthographicSize - 0.25f;
        movespeedGrowthFloor = currentVelocity;

        MaxBackLight = PlayerAgeDataDictionary[_stage].MaxBackLight;
        MaxCenterLight = PlayerAgeDataDictionary[_stage].MaxCenterLight;

        //  growthMultiplier = Mathf.Sqrt(spriteRendererRef.transform.localScale.x / PlayerAgeDataDictionary[_stage].GrowScale);
        //  cameraMultiplier = Mathf.Sqrt((CameraRef.orthographicSize - 1) / PlayerAgeDataDictionary[_stage].CamSize);
        //  movespeedGrowthMultiplier = Mathf.Sqrt(currentVelocity / PlayerAgeDataDictionary[_stage].LearnToMoveTime);
    }

    private void ApplyPlayerData()
    {
        growthMultiplier = Mathf.Clamp01(growthMultiplier + Time.deltaTime / PlayerAgeDataDictionary[CharHandler.Instance.currentGameStage].GrowTime);
        float growthDistance = PlayerAgeDataDictionary[CharHandler.Instance.currentGameStage].GrowScale - growFloor;
        spriteRendererRef.transform.localScale = Vector2.one * (growFloor + 
                (growthDistance *
                Mathf.Clamp(growthMultiplier * growthMultiplier, 0, 1)));

        cameraMultiplier = Mathf.Clamp01(cameraMultiplier + Time.deltaTime / PlayerAgeDataDictionary[CharHandler.Instance.currentGameStage].CamGrowTime);
        float camDistance = PlayerAgeDataDictionary[CharHandler.Instance.currentGameStage].CamSize - cameraFloor;
        CameraRef.orthographicSize = 0.25f + (cameraFloor +
                                     camDistance *
                                     Mathf.Clamp(cameraMultiplier * cameraMultiplier, 0, 1));
    }
}
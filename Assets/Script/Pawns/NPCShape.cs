using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using static Utils;

public class NPCShape : ShapePawn
{
    public bool applyLight = true;
    public EGAMESTAGE ShapeAge;

    private Vector2 currentTargetVelocity = Vector2.zero;
    private Vector2 lastImpulse = Vector2.zero;

    public bool hasTarget = false;
    public Vector2 targetVector;

    TimerContainer BoopTimer;

    static int NPCShapeCount = 1;
    public int ShapeID = -1;

    public Light2D Light2DRef;

    public int closestShapeID = -1;
    public float closestShapeDistance = float.MaxValue;

    public bool IsParent1 = false;
    public bool IsParent2 = false;
    public int ChildBoopCounter = 0;

    private TimerContainer ReaquireTimer;
    private TimerContainer DeathTimer;
    private TimerContainer FriendDecay;

    public static int CoreFriends = 0;
    public int Friendship = 0;
    public bool isCoreFriend = false;

    public bool IsDead = false;

    private void Awake()
    {
        ShapeID = NPCShapeCount;
        NPCShapeCount++;
    }

    new private void Start()
    {
        if(ShapeAge == EGAMESTAGE.RANDOM)
        {
            ShapeAge = (EGAMESTAGE)Random.Range((int)EGAMESTAGE.CHILD, (int)EGAMESTAGE.RANDOM);
        }
        ApplyAgeData();

        FriendDecay = SetTimer(5, DecayFriendship);

        base.Start();
    }

    private void DecayFriendship()
    {
        Friendship--;
        if(Friendship < 0)
        {
            Friendship = 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(IsDead)
        {
            return;
        }
        NPCShape otherShape = collision.gameObject.GetComponent<NPCShape>();
        if(otherShape != null)
        {
            if(otherShape.IsDead)
            {
                return;
            }
        }

        if (Light2DRef.enabled == false)
        {
            Light2DRef.enabled = true;
            if(CharHandler.Instance.Spawn != null)
            {
                Destroy(CharHandler.Instance.Spawn);
                CharHandler.Instance.Spawn = null;
            }

            CharHandler.Instance.ChangeGameStage(EGAMESTAGE.BABY);
        }

        if(IsParent1)
        {
            ChildBoopCounter++;
            if (ChildBoopCounter >= 3 && hasTarget == false)
            {
                hasTarget = true;
                targetVector = GetRandomVector(Vector2.right) * Random.Range(2, 4) + (Vector2)CharHandler.Instance.ListOfNPCShapes[closestShapeID - 1].transform.position;
            }
        }

        if(!IsParent1 && !IsParent2)
        {
            Friendship++;
            if (Friendship >= 10 && CoreFriends < 2)
            {
                CoreFriends++;
                isCoreFriend = true;
                FriendDecay.CancelTimer();
            }
        }

        //  Debug.Log("Boop");
        Vector2 tempVector = (transform.position - collision.rigidbody.transform.position).normalized;

        currentTargetVelocity = tempVector * 25 * CharHandler.Instance.PlayerShapeRef.PlayerAgeDataDictionary[ShapeAge].MovementSpeed;
        lastImpulse = currentTargetVelocity;

        rigidBodyRef.AddForce(currentTargetVelocity);

        BoopTimer = TimeManager.Instance.SetTimer(0.3f, () => { BoopResponse(collision.GetContact(0).point); });
    }

    new private void FixedUpdate()
    {
        if(IsDead)
        {
            return;
        }
        base.FixedUpdate();

        if(hasTarget && BoopTimer == null)
        {
            float velomulti = CharHandler.Instance.PlayerShapeRef.PlayerAgeDataDictionary[ShapeAge].MovementSpeed;
            if(IsParent1 && CharHandler.Instance.currentGameStage <= EGAMESTAGE.BABY)
            {
                velomulti = 0.2f;
            }
            else if((IsParent1 || IsParent2) && CharHandler.Instance.currentGameStage >= EGAMESTAGE.CHILD)
            {
                velomulti = 0.5f;
            }

            Vector2 tempTargetVector = targetVector - (Vector2)transform.position;

            Vector2 tempVelocity = velomulti * tempTargetVector.normalized;
            if(tempVelocity.magnitude * Time.deltaTime > tempTargetVector.magnitude)
            {
                rigidBodyRef.velocity = tempTargetVector.normalized * tempTargetVector.magnitude;
                hasTarget = false;
            }
            else
            {
                rigidBodyRef.velocity = tempVelocity;
            }

            RaycastHit2D[] tempHit = Physics2D.BoxCastAll(transform.position, transform.localScale, 0, rigidBodyRef.velocity.normalized, 0.3f);
            if(tempHit.Length > 1)
            {
                rigidBodyRef.velocity = Vector2.zero;
            }
        }

        //rigidBodyRef.velocity = (CharHandler.Instance.PlayerRef.transform.position - transform.position).normalized * 5;
        //if(rigidBodyRef.velocity.magnitude < 0.3f && lastImpulse != Vector2.zero)
        //{
        //    Debug.Log("return");
        //    rigidBodyRef.velocity = -lastImpulse + rigidBodyRef.velocity;
        //    lastImpulse = Vector2.zero;
        //}
    }

    private void TryTargetAgain()
    {
        hasTarget = true;
    }
    private void ReachedTarget()
    {
        hasTarget = false;
    }

    new private void Update()
    {
        if(BoopTimer != null)
        {
            if(BoopTimer.going == false)
            {
                BoopTimer = null;
            }
        }
        base.Update();
        
        if(IsDead)
        {
            return;
        }

        if(DistanceTravelled >= 200 && DeathTimer == null)
        {
            TryDeath();
            DeathTimer = SetTimer(10, TryDeath, false);
        }

        if ((IsParent1 || IsParent2) && CharHandler.Instance.currentGameStage >= EGAMESTAGE.CHILD)
        {
            if(hasTarget == false)
            {
                hasTarget = true;
                ReaquireTargetVectorParents(CharHandler.Instance.PlayerRef.transform, 3, 8);
                SetTimer(10, ReachedTarget);
            }
        }
        else if(isCoreFriend || Friendship >= 5)
        {
            if (hasTarget == false)
            {
                hasTarget = true;
                ReaquireTargetVector(CharHandler.Instance.PlayerRef.transform, 3, 8);
                SetTimer(10, ReachedTarget);
            }
        }
    }

    private void TryDeath()
    {
        if (Random.Range(0, 5) == 0)
        {
            ShapeDie();
        }
    }

    public void ReaquireTargetVectorParents(Transform _transform, int _lower = 1, int _upper = 3)
    {
        targetVector = (Vector2)_transform.position + Random.Range(_lower, _upper) * GetRandomVector(_transform.position.normalized, 45);
    }

    public void ReaquireTargetVector(Transform _transform, int _lower = 1, int _upper = 3)
    {
        targetVector = (Vector2)_transform.position + Random.Range(_lower, _upper) * GetRandomVector(Vector2.right);
    }

    private int GetDistanceToChild()
    {
        switch(CharHandler.Instance.currentGameStage)
        {
            case EGAMESTAGE.CHILD:
                return Random.Range(1, 3);
            case EGAMESTAGE.ADOLESCENT:
                return Random.Range(1, 5);
            case EGAMESTAGE.ADULT:
                return Random.Range(3, 7);
            default:
                return Random.Range(1, 3);
        }
    }

    public void SetClosestShape(int _shapeID, float _shapeDistance)
    {
        closestShapeID = _shapeID;
        closestShapeDistance = _shapeDistance;
    }

    public void ShapeDie()
    {
        CharHandler.Instance.CreateProperSplat(19, transform.position, Vector2.right, GetPawnColor(), 3, 180);
        rigidBodyRef.bodyType = RigidbodyType2D.Static;
        Light2DRef.enabled = false;
        if(DeathTimer != null)
        {
            DeathTimer.CancelTimer();
        }
        if(FriendDecay != null)
        {
            FriendDecay.CancelTimer();
        }        
        SetPawnColor(Color.gray);
        IsDead = true;
    }

    private void BoopResponse(Vector2 _boopPosition)
    {
        currentTargetVelocity = -lastImpulse * 0.8f;
        rigidBodyRef.velocity = Vector2.zero;
        rigidBodyRef.AddForce(-lastImpulse * 0.3f);

        //  BoopTimer = SetTimer(0.15f, () => { BoopEnd(spriteRendererRef); });
        BoopTimer = SetTimer(0.15f, BoopEnd);
    }

    private void SetLightRange(int _range)
    {
        Light2DRef.pointLightOuterRadius = Mathf.Clamp(_range, 2, 20) * 0.5f;
    }

    private void BoopEnd()
    {
        if(waveFormContainer == null)
        {
            waveFormContainer = CharHandler.Instance.InstantiateSpeechBubble(CharHandler.EWaveForm.BoopResponse, WaveAttachPoint.position, WaveAttachPoint);
        }

        currentTargetVelocity = Vector2.zero;
    }

    private void ApplyAgeData()
    {
        transform.localScale = Vector3.one * CharHandler.Instance.PlayerShapeRef.PlayerAgeDataDictionary[ShapeAge].GrowScale;
        if(applyLight)
        {
            Light2DRef.pointLightOuterRadius = CharHandler.Instance.PlayerShapeRef.PlayerAgeDataDictionary[ShapeAge].MaxCenterLight;
        }
    }
}

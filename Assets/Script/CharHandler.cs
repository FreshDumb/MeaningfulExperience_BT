using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;


public class CharHandler : MonoBehaviour
{
    public enum EWaveForm
    {
        Boop,
        BoopResponse
    }

    public EGAMESTAGE currentGameStage = EGAMESTAGE.INIT;
    public GameObject MapBase;
    public float[] MapDividers = { 2, 5, 20, 60, 120};
    public GameObject Spawn;

    [Header("PREFABS AND SO")]
    public WaveFormContainerContainer_SO WaveFormData;
    public static CharHandler Instance;
    public GameObject TilePrefab;
    public GameObject ParticlePrefab;
    public GameObject NPCPrefab;


    public GameObject SpeechPrefab;

    private Transform TileContainerTransform;
    private Transform NPCShapeContainerTransform;
    private Transform SpeechBubbleContainerTransform;

    [Header("GENERATION")]
    public float RandomShapeSpread = 1;
    public int NumOfNPCShapes = 0;
    public bool DebugTiles;

    public float TileDimension;
    public int TileSize = 64;
    public int PixelsPerUnit = 16;
    public Queue<SpriteRenderer> CanvasIndicesToRebuild = new Queue<SpriteRenderer>();

    public Dictionary<Vector2, SpriteRenderer> TileContainer;
    public List<NPCShape> ListOfNPCShapes;
    [Header("PLAYER")]
    public GameObject PlayerRef;
    public PlayerShape PlayerShapeRef;

    [Header("BRUSHES")]
    public Sprite Brush;
    public Sprite BigSplat;
    public Sprite SmallSplat;
    public bool[,] BrushBitMap;
    public bool[,] BigSplatBitMap;
    public bool[,] SmallSplatBitMap;

    Color32 black = new Color32(50,50,50,255);
    Color32 white = new Color32(200, 200, 200, 255);

    List<Vector2> AvailableShapeCoords;
    float[,] DistanceArray;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        SpriteRenderer MapRenderer = MapBase.GetComponent<SpriteRenderer>();
        for (int i = 0; i < MapDividers.Length; i++)
        {
            MapRenderer.material.SetFloat("Divider_" + i, MapDividers[i]);
        }

        GameObject TileContainerTransform_temp = new GameObject();
        TileContainerTransform_temp.transform.SetParent(transform);
        TileContainerTransform_temp.name = "TileContainer";
        TileContainerTransform = TileContainerTransform_temp.transform;

        GameObject NPCShapeContainerTransform_temp = new GameObject();
        NPCShapeContainerTransform_temp.transform.SetParent(transform);
        NPCShapeContainerTransform_temp.name = "NPCShapeContainer";
        NPCShapeContainerTransform = NPCShapeContainerTransform_temp.transform;

        GameObject SpeechBubbleContainerTransform_temp = new GameObject();
        SpeechBubbleContainerTransform_temp.transform.SetParent(transform);
        SpeechBubbleContainerTransform_temp.name = "SpeechBubbleContainer";
        SpeechBubbleContainerTransform = SpeechBubbleContainerTransform_temp.transform;

        
        TileDimension = TileSize / PixelsPerUnit;
        InitShapePositions();

        GameObject[] tempObjects = GameObject.FindGameObjectsWithTag("NPCShape");
        PlayerShapeRef = PlayerRef.GetComponent<PlayerShape>();
        ListOfNPCShapes = new List<NPCShape>();
        for (int i = 0; i < tempObjects.Length; i++)
        {
            ListOfNPCShapes.Add(null);
        }
        for (int i = 0; i < tempObjects.Length; i++)
        {
            ListOfNPCShapes[tempObjects[i].GetComponent<NPCShape>().ShapeID - 1] = tempObjects[i].GetComponent<NPCShape>();
        }

        BrushBitMap = new bool[1, 1];
        BrushBitMap = CreateBitMapFromSprite(Brush);
        BigSplatBitMap = new bool[1, 1];
        BigSplatBitMap = CreateBitMapFromSprite(BigSplat);
        SmallSplatBitMap = new bool[1, 1];
        SmallSplatBitMap = CreateBitMapFromSprite(SmallSplat);

        TileContainer = new Dictionary<Vector2, SpriteRenderer>();

        for (int i = 0; i < NumOfNPCShapes; i++)
        {
            SpawnRandomShape();
        }
    }

    private void FixedUpdate()
    {
        if(currentGameStage > EGAMESTAGE.BIRTH)
        {
            DoTrailWrapper(PlayerShapeRef);
        }
        int counter = 0;

        List<NPCShape> tempShapes = new List<NPCShape>();
        PlayerShapeRef.IsInDarkness = true;
        PlayerShapeRef.closestLightDistance = float.MaxValue;
        PlayerShapeRef.closestLightID = -1;
        PlayerShapeRef.closestLightModifier = 1;
        for (int i = 0; i < ListOfNPCShapes.Count; i++)
        {
            if(ListOfNPCShapes[i].IsDead)
            {

            }
            else
            {
                DistanceArray[0, ListOfNPCShapes[i].ShapeID] = (ListOfNPCShapes[i].transform.position - PlayerRef.transform.position).sqrMagnitude;
                if (DistanceArray[0, ListOfNPCShapes[i].ShapeID] < 400)
                {
                    counter++;
                    DoTrailWrapper(ListOfNPCShapes[i]);
                    tempShapes.Add(ListOfNPCShapes[i]);
                    float tempLightRadiusSqrd = ListOfNPCShapes[i].Light2DRef.pointLightOuterRadius * ListOfNPCShapes[i].Light2DRef.pointLightOuterRadius;
                    if (DistanceArray[0, ListOfNPCShapes[i].ShapeID] < tempLightRadiusSqrd && ListOfNPCShapes[i].Light2DRef.enabled)
                    {
                        PlayerShapeRef.IsInDarkness = false;
                        if (DistanceArray[0, ListOfNPCShapes[i].ShapeID] < PlayerShapeRef.closestLightDistance)
                        {
                            PlayerShapeRef.closestLightDistance = DistanceArray[0, ListOfNPCShapes[i].ShapeID];
                            PlayerShapeRef.closestLightID = ListOfNPCShapes[i].ShapeID;
                        }
                    }
                }
            }

        }
        if (PlayerShapeRef.IsInDarkness)
        {
            PlayerShapeRef.closestLightDistance = float.MaxValue;
            PlayerShapeRef.closestLightID = -1;
            PlayerShapeRef.closestLightModifier = 1;
        }
        else
        {
            float tempLightRadiusSqrd = Mathf.Pow(ListOfNPCShapes[PlayerShapeRef.closestLightID - 1].Light2DRef.pointLightOuterRadius, 2);
            PlayerShapeRef.closestLightModifier = DistanceArray[0, PlayerShapeRef.closestLightID] / tempLightRadiusSqrd;
        }

        for (int i = 0; i < tempShapes.Count; i++)
        {
            if(tempShapes[i].closestShapeID > 0)
            {
                if (ListOfNPCShapes[tempShapes[i].closestShapeID - 1].IsDead)
                {
                    tempShapes[i].closestShapeID = -1;
                    tempShapes[i].closestShapeDistance = float.MaxValue;
                }
            }
            for (int j = 0; j < ListOfNPCShapes.Count; j++)
            {
                if(tempShapes[i].ShapeID == ListOfNPCShapes[j].ShapeID || ListOfNPCShapes[j].IsDead)
                {

                }
                else
                {
                    DistanceArray[tempShapes[i].ShapeID, ListOfNPCShapes[j].ShapeID] = 
                        (ListOfNPCShapes[j].transform.position - PlayerRef.transform.position).sqrMagnitude;

                    if(DistanceArray[tempShapes[i].ShapeID, ListOfNPCShapes[j].ShapeID] < tempShapes[i].closestShapeDistance)
                    {
                        tempShapes[i].SetClosestShape(ListOfNPCShapes[j].ShapeID, 
                                                           DistanceArray[tempShapes[i].ShapeID, ListOfNPCShapes[j].ShapeID]);
                    }
                    if(DistanceArray[tempShapes[i].ShapeID, ListOfNPCShapes[j].ShapeID] < ListOfNPCShapes[j].closestShapeDistance)
                    {
                        ListOfNPCShapes[j].SetClosestShape(tempShapes[i].ShapeID, DistanceArray[tempShapes[i].ShapeID, ListOfNPCShapes[j].ShapeID]);
                    }
                }
            }
        }
        //  Debug.Log(counter);

        //  Debug.Log(CanvasIndicesToRebuild.Count);
        while (CanvasIndicesToRebuild.Count > 0)
        {
            CanvasIndicesToRebuild.Dequeue().sprite.texture.Apply();
        }
    }

    private void Update()
    {

    }

    private void InitShapePositions()
    {
        AvailableShapeCoords = new List<Vector2>();
        for (int x = -13; x < 13; x++)
        {
            for (int y = -13; y < 13; y++)
            {
                if(Vector2.Distance(Vector2.zero, new Vector2(x, y)) < 4)
                {

                }
                else
                {
                    AvailableShapeCoords.Add(new Vector2(x, y));
                }
            }
        }

        DistanceArray = new float[105 , 105];
        for (int x = 0; x < 105; x++)
        {
            for (int y = 0; y < 105; y++)
            {
                if(x == y)
                {
                    DistanceArray[x, y] =  0;
                }
                else
                {
                    DistanceArray[x, y] = -1;
                }
            }
        }
        Debug.Log("Initialized Positions");
    }

    private void SpawnRandomShape()
    {
        int tempRandomIndex = Random.Range(0, AvailableShapeCoords.Count);
        Vector2 ShapeLocation = AvailableShapeCoords[tempRandomIndex];
        AvailableShapeCoords.RemoveAt(tempRandomIndex);

        GameObject instance = Instantiate(NPCPrefab);
        instance.transform.position = (new Vector2( ShapeLocation.x * ( 1 + (Mathf.Abs(ShapeLocation.x) * RandomShapeSpread / 50.0f)), 
                                                    ShapeLocation.y * ( 1 + (Mathf.Abs(ShapeLocation.y) * RandomShapeSpread / 50.0f)))) * TileDimension;
        NPCShape tempShapeComp = instance.GetComponent<NPCShape>();

        //  tempShapeComp.SetPawnColor();
        tempShapeComp.SetPawnColor(new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255));
        tempShapeComp.DistanceTravelled = Random.Range(0, 150);

        ListOfNPCShapes.Add(instance.GetComponent<NPCShape>());
        instance.transform.SetParent(NPCShapeContainerTransform);
    }
       
    private void DoTrailWrapper(ShapePawn _pawn)
    {
        AdjustPixel(_pawn.transform.position.x, _pawn.transform.position.y, _pawn.GetPawnColor(), BrushBitMap);
    }

    private void AdjustPixel(float _xPos, float _yPos, Color32 _color, bool[,] _bitMap)
    {
        int dictIndexKeyX = (int)(_xPos / TileDimension);
        int dictIndexKeyY = (int)(_yPos / TileDimension);


        if (_xPos < 0)
        {
            dictIndexKeyX = dictIndexKeyX - 1;
        }

        if (_yPos < 0)
        {
            dictIndexKeyY = dictIndexKeyY - 1;
        }



        Vector2 tempVector = new Vector2(dictIndexKeyX, dictIndexKeyY);
        if (TileContainer.ContainsKey(tempVector) == false)
        {
            CreateTile(tempVector);
        }

        int newXInt = (int)((_xPos - TileDimension * dictIndexKeyX) * PixelsPerUnit);
        int newYInt = (int)((_yPos - TileDimension * dictIndexKeyY) * PixelsPerUnit);

        SpriteRenderer rendererRef;
        TileContainer.TryGetValue(tempVector, out rendererRef);

        CheckBrushCorners(newXInt, newYInt, dictIndexKeyX, dictIndexKeyY, _bitMap.GetLength(0));

        newXInt = newXInt - _bitMap.GetLength(0) / 2 ;
        newYInt = newYInt - _bitMap.GetLength(0) / 2 ;

        for (int x = 0; x < _bitMap.GetLength(0); x++)
        {
            for (int y = 0; y < _bitMap.GetLength(0); y++)
            {
                if(_bitMap[x,y] == true )
                {
                    PaintSinglePixel(newXInt + x, newYInt + y, dictIndexKeyX, dictIndexKeyY, _color);
                }
            }
        }
    }

    private void PaintSinglePixel(int _x, int _y, int _dictX, int _dictY, Color32 _color)
    {
        int dictModX;
        int x;
        int dictModY;
        int y;
        if (_x < 0)
        {
            dictModX = -1;
            x = TileSize + _x;
        }
        else if(_x >= TileSize)
        {
            dictModX = +1;
            x = _x - TileSize;
        }
        else
        {
            dictModX = 0;
            x = _x;
        }

        if (_y < 0)
        {
            dictModY = -1;
            y = TileSize + _y;
        }
        else if(_y >= TileSize)
        {
            dictModY = +1;
            y = _y - TileSize;
        }
        else
        {
            dictModY = 0;
            y = _y;
        }

        Vector2 dictIndexVector = new Vector2(_dictX + dictModX, _dictY + dictModY);

        SpriteRenderer tempRendererRef;
        TileContainer.TryGetValue(dictIndexVector, out tempRendererRef);

        int newIndex = (y * TileSize + x);

        var tempTextureData = tempRendererRef.sprite.texture.GetRawTextureData<Color32>();
        tempTextureData[newIndex] = _color;

        if (CanvasIndicesToRebuild.Contains(tempRendererRef) == false)
        {
            CanvasIndicesToRebuild.Enqueue(tempRendererRef);
        }
    }

    void CheckBrushCorners(int _indexX, int _indexY, int _dictIndexX, int _dictIndexY, int _bitMapSize)
    {

        Vector2 topLeft = new Vector2(_indexX - _bitMapSize / 2, _indexY - _bitMapSize / 2);

        Vector2 topRight = new Vector2(_indexX + _bitMapSize / 2, _indexY - _bitMapSize / 2);

        Vector2 bottomLeft = new Vector2(_indexX - _bitMapSize / 2, _indexY + _bitMapSize / 2);

        Vector2 bottomRight = new Vector2(_indexX + _bitMapSize / 2, _indexY + _bitMapSize / 2);

        if (IsPixelInBounds((int)topLeft.x, (int)topLeft.y) == false)
        {
            int modX;
            if (topLeft.x < 0)
            {
                modX = -1;
            }
            else
            {
                modX = 0;
            }
            
            int modY;
            if (topLeft.y < 0)
            {
                modY = -1;
            }
            else
            {
                modY = 0;
            }

            Vector2 tempKey = new Vector2(_dictIndexX + modX, _dictIndexY + modY);

            if (TileContainer.ContainsKey(tempKey) == false)
            {
                CreateTile(tempKey);
            }
        }

        if (IsPixelInBounds((int)topRight.x, (int)topRight.y) == false)
        {
            int modX;
            if (TileSize - topRight.x < 0)
            {
                modX = +1;
            }
            else
            {
                modX = 0;
            }

            int modY;
            if (topRight.y < 0)
            {
                modY = -1;
            }
            else
            {
                modY = 0;
            }

            Vector2 tempKey = new Vector2(_dictIndexX + modX, _dictIndexY + modY);

            if (TileContainer.ContainsKey(tempKey) == false)
            {
                CreateTile(tempKey);
            }
        }

        if (IsPixelInBounds((int)bottomLeft.x, (int)bottomLeft.y) == false)
        {
            int modX;
            if (bottomLeft.x < 0)
            {
                modX = -1;
            }
            else
            {
                modX = 0;
            }

            int modY;
            if (TileSize - bottomLeft.y < 0)
            {
                modY = +1;
            }
            else
            {
                modY = 0;
            }

            Vector2 tempKey = new Vector2(_dictIndexX + modX, _dictIndexY + modY);

            if (TileContainer.ContainsKey(tempKey) == false)
            {
                CreateTile(tempKey);
            }
        }

        if (IsPixelInBounds((int)bottomRight.x, (int)bottomRight.y) == false)
        {
            int modX;
            if (TileSize - bottomRight.x < 0)
            {
                modX = +1;
            }
            else
            {
                modX = 0;
            }

            int modY;
            if (TileSize - bottomRight.y < 0)
            {
                modY = +1;
            }
            else
            {
                modY = 0;
            }

            Vector2 tempKey = new Vector2(_dictIndexX + modX, _dictIndexY + modY);

            if (TileContainer.ContainsKey(tempKey) == false)
            {
                CreateTile(tempKey);
            }
        }
    }

    private bool IsPixelInBounds(int _indexX, int _indexY)
    {
        if (_indexX >= 0 && _indexX < TileSize && _indexY >= 0 && _indexY < TileSize)
        {
            return true;
        }
        return false;
    }

    private void CreateTile(Vector2 _key)
    {
        GameObject temp = Instantiate(TilePrefab);
        SpriteRenderer spriteRef = temp.GetComponent<SpriteRenderer>();
        spriteRef.transform.position = _key * TileDimension;
        spriteRef.sprite = Sprite.Create(new Texture2D(TileSize, TileSize, TextureFormat.RGBA32, false), new Rect(0, 0, TileSize, TileSize), Vector2.zero, PixelsPerUnit);
        spriteRef.sprite.texture.filterMode = FilterMode.Point;
        var tempTextureData = spriteRef.sprite.texture.GetRawTextureData<Color32>();

        if(DebugTiles)
        {
            Color32 tempColor = (((_key.x + _key.y) % 2) == 0 ? black : white);
            for (int i = 0; i < tempTextureData.Length; i++)
            {
                tempTextureData[i] = tempColor;
            }
            spriteRef.sprite.texture.Apply();
        }
        else
        {
            for (int i = 0; i < tempTextureData.Length; i++)
            {
                tempTextureData[i] = new Color32(0, 0, 0, 0);
            }
            spriteRef.sprite.texture.Apply();
        }


        temp.transform.SetParent(TileContainerTransform);
        TileContainer.Add(_key, spriteRef);
    }

    public void SomeoneGotBooped(Vector2 _boopPosition, Color32 _mixedBoopColor, NPCShape _otherShape = null, EMEM _convoType = 0)
    {
        EMEM convoType;
        if (_convoType == 0)
        {
            convoType = (EMEM)Random.Range(0, (int)EMEM.COUNT);
        }
        else
        {
            convoType = _convoType;
        }

        float veloMod = 1;
        int spread = 180;
        Vector2 boopOrigin = _boopPosition;
        Vector2 convoDirection = Vector2.right;
        Color32 splatColor = _mixedBoopColor;

        Debug.Log(convoType);

        switch (convoType)
        {
            case EMEM.PAST:
                splatColor = PlayerShapeRef.GetPawnColor();
                convoDirection = (PlayerShapeRef.PastPlayerPosition - (Vector2)PlayerRef.transform.position).normalized;
                boopOrigin = PlayerRef.transform.position;
                spread = 30;
                veloMod = 1.0f;
                break;
            case EMEM.PRESENT:
                veloMod = 1.0f;
                break;
            case EMEM.FUTURE:
                splatColor = ListOfNPCShapes[_otherShape.closestShapeID - 1].GetPawnColor();
                convoDirection = (ListOfNPCShapes[_otherShape.closestShapeID - 1].transform.position - _otherShape.transform.position).normalized;
                boopOrigin = _otherShape.transform.position;
                spread = 15;
                veloMod = 2f;
                break;
            case EMEM.SELF:
                splatColor = _otherShape.GetPawnColor();
                boopOrigin = _otherShape.transform.position;
                veloMod = 0.5f;
                break;
            case EMEM.OUTWARD:
                convoDirection = PlayerShapeRef.transform.position.normalized;
                boopOrigin = _otherShape.transform.position;
                splatColor = PlayerShapeRef.GetPawnColor();
                spread = 5;
                veloMod = 3f;
                break;
            case EMEM.SPAWN:
                splatColor = PlayerShapeRef.GetPawnColor();
                convoDirection = _boopPosition.normalized;
                veloMod = 1.5f;
                spread = 60;
                break;
            default:
                break;
        }



        for (int i = 0; i < 5; i++)
        {
            Vector2 tempRandomVector = GetRandomVector(convoDirection, spread);
            for (int j = 0; j < 6; j++)
            {
                CreateBoopParticle(boopOrigin, GetRandomVector(tempRandomVector, 5), splatColor, veloMod);
            }
        }
    }

    public void CreateProperSplat(int _count, Vector2 _origin, Vector2 _direction, Color32 _color, float _veloMod, float _spread = 5)
    {
        for (int j = 0; j < _count; j++)
        {
            CreateBoopParticle(_origin, GetRandomVector(_direction, _spread), _color, _veloMod);
        }
    }

    public void CreateBoopParticle(Vector2 _position, Vector2 _direction, Color32 _color, float _veloMod)
    {

        GameObject tempParticle = Instantiate(ParticlePrefab);
        BoopParticle tempBoopParticleRef = tempParticle.GetComponent<BoopParticle>();
        tempBoopParticleRef.rigidBodyRef.velocity = _direction * 3 * Random.Range(0.2f, 1.0f) * _veloMod;
        tempBoopParticleRef.spriteRendererRef.color = _color;
        tempBoopParticleRef.ParticleColor = _color;
        tempParticle.transform.position = _position;
        tempParticle.transform.parent = gameObject.transform;
    }

    public void DoSplat(Vector2 _position, Color32 _color, ESPLATSIZE _size)
    {
        if(_size == ESPLATSIZE.SMALL)
        {
            AdjustPixel(_position.x, _position.y, _color, SmallSplatBitMap);
        }
        else
        {
            AdjustPixel(_position.x, _position.y, _color, BigSplatBitMap);
        }
    }

    bool[,] CreateBitMapFromSprite(Sprite _sprite)
    {
        bool[,] result = new bool[(int)_sprite.rect.size.x, (int)_sprite.rect.size.x];
        for (int x = 0; x < _sprite.rect.size.x; x++)
        {
            for (int y = 0; y < _sprite.rect.size.x; y++)
            {
                Color tempColor = _sprite.texture.GetPixel(x, y);
                if (tempColor.a > 0)
                {
                    result[x, y] = true;
                }
                else
                {
                    result[x, y] = false;
                }
            }
        }
        //  Debug.Log(BrushBitMap.GetLength(0));
        //  Debug.Log(BrushBitMap.GetLength(1));
        return result;
    }

    public DialogWaveform InstantiateSpeechBubble(EWaveForm _type, Vector2 _position, Transform _parent = null)
    {
        GameObject instance = Instantiate(SpeechPrefab);
        if(_parent == null)
        {
            instance.transform.SetParent(SpeechBubbleContainerTransform);
            instance.transform.position = _position;
        }
        else
        {
            Vector2 tempOffset = _position - (Vector2)_parent.transform.position;
            instance.transform.SetParent(_parent);
            instance.transform.localPosition = Vector2.zero;
            instance.transform.localScale = (PlayerShapeRef.CameraRef.orthographicSize / 5.0f) * Vector2.one * 1.2f / _parent.lossyScale.x;
        }
        DialogWaveform waveformRef = instance.GetComponent<DialogWaveform>();

        switch (_type)
        {
            case EWaveForm.Boop:
                waveformRef.InitDialog(WaveFormData.BoopData);
                break;
            case EWaveForm.BoopResponse:
                waveformRef.InitDialog(WaveFormData.BoopResponse);
                break;
            default:
                waveformRef.InitDialog(WaveFormData.BoopData);
                break;
        }
        return waveformRef;
    }

    public void ChangeGameStage(EGAMESTAGE _newState)
    {
        //  EGAMESTAGE tempStage = currentGameStage;
        currentGameStage = _newState;
        PlayerShapeRef.GameStageChanged(currentGameStage);
    }

    public void IncreaseStage()
    {
        currentGameStage = currentGameStage + 1;
        PlayerShapeRef.GameStageChanged(currentGameStage);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testLocation : MonoBehaviour
{
    public AudioSource AudioRef;
    public float Volume;

    public float _angle;
    public float _dotVectors;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //  TestiTest.transform.position = new Vector3(-tempVector.y, tempVector.x);
        Vector2 tempVector = (transform.position - CharHandler.Instance.PlayerRef.transform.position).normalized;
        _dotVectors = Vector2.Dot(tempVector, CharHandler.Instance.PlayerShapeRef.spriteRendererRef.transform.right);
        _angle = Vector2.SignedAngle(tempVector, CharHandler.Instance.PlayerShapeRef.spriteRendererRef.transform.right);

        AudioRef.volume = Volume * (1 - Mathf.Clamp01(Vector2.Distance(CharHandler.Instance.PlayerRef.transform.position, transform.position)/10f))
                * (1 - Mathf.Clamp( ((Mathf.Abs(_angle) - 90f) / 90f), 0.2f, 1f));

        AudioRef.panStereo = Mathf.Clamp((1 - Mathf.Abs(_angle - 90)/90), -1, 1);
        //  Debug.Log(dotVectors);
    }
}

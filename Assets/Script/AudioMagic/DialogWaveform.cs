using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogWaveform : MonoBehaviour
{
    public AudioSource AudioSourceRef;
    public SpriteRenderer SpriteRendererRef;

    private void Update()
    {
        if(AudioSourceRef.isPlaying)
        {
            SpriteRendererRef.enabled = true;
        }
        else
        {
            SpriteRendererRef.enabled = false;
        }
    }

    public void InitDialog(WaveFormData_SO _data)
    {
        SpriteRendererRef.material.SetTexture("WaveformTextures", _data.textureArray);
        SpriteRendererRef.material.SetFloat("ArrayLength", _data.textureArray.depth);
        SpriteRendererRef.material.SetFloat("MaterialReso", _data.MaterialReso);
        AudioSourceRef.clip = _data.AudioClip;

        PlayDialog();
    }

    void PlayDialog()
    {
        AudioSourceRef.Play();
        SpriteRendererRef.material.SetFloat("TimePlayed", Time.time);
        Destroy(gameObject, AudioSourceRef.clip.length + 0.1f);
    }
}

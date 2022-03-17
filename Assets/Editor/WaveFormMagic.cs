using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveFormMagic", menuName = "WaveFormData/WaveFormMagic", order = 1)]
public class WaveFormMagic : ScriptableObject
{
    public AudioClip[] Clips;
    public int TextureHeight = 64;

    public void BuildTextures()
    {
        Debug.Log("It Working!");

        for (int i = 0; i < Clips.Length; i++)
        {
            CreateTextureFromAudioClip(Clips[i]);
        }
    }

    private void CreateTextureFromAudioClip(AudioClip _source)
    {
        float[] samples = new float[_source.channels * _source.samples];
        _source.GetData(samples, 0);

        //float[] test = new float[5000];
        //for (int i = 0; i < 5000; i++)
        //{
        //    test[i] = samples[i];
        //}

        //float maxValue = float.MinValue;
        //float minValue = float.MaxValue;
        //for (int i = 0; i < samples.Length; i++)
        //{
        //    if(samples[i] < minValue)
        //    {
        //        minValue = samples[i];
        //    }

        //    if (samples[i] > maxValue)
        //    {
        //        maxValue = samples[i];
        //    }
        //}

        Debug.Log(samples.Length);
        float tempValue = samples.Length / 2.0f;
        int multiplyer = 1;
        if (tempValue < 2048)
        {

        }
        else
        {
            multiplyer = (int)(tempValue / 2048.0f);
            multiplyer++;
        }

        Texture2DArray tempTextureArray = new Texture2DArray(2048, TextureHeight, multiplyer, TextureFormat.Alpha8, false);
        tempTextureArray.filterMode = FilterMode.Point;
        tempTextureArray.wrapMode = TextureWrapMode.Clamp;
        int currentTexturePart = 0;
        for (int i = 0; i < multiplyer; i++)
        {
            int width = 2048;
            int height = TextureHeight;

            Texture2D tex = new Texture2D(width, height, TextureFormat.Alpha8, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            for (var tx = 0; tx < width; tx++)
            {
                int sampleIndex = Mathf.Clamp(2048 * currentTexturePart * 2+ (tx * 2), 0, samples.Length - 1);
                float tempVolume = Mathf.Clamp(((samples[sampleIndex] + 1) * 0.5f) * TextureHeight, 0, TextureHeight);
                for (var ty = 0; ty < height; ty++)
                {
                    if (Mathf.Abs(tempVolume - ty) < 1)
                    {
                        tex.SetPixel(tx, ty, new Color(1, 1, 1, 1));
                    }
                    else
                    {
                        tex.SetPixel(tx, ty, new Color(0, 0, 0, 0));
                    }
                }
            }
            tex.Apply();
            Graphics.CopyTexture(tex, 0, 0, tempTextureArray, currentTexturePart, 0);
            byte[] bytes = tex.EncodeToPNG();
            if(Directory.Exists("Assets/Audio/BakedWaveForms/" + _source.name) == false)
            {
                Directory.CreateDirectory("Assets/Audio/BakedWaveForms/" + _source.name);
            }
            //  File.WriteAllBytes("Assets/Audio/BakedWaveForms/" + _source.name + "/" + _source.name + "_" + currentTexturePart + ".png", bytes);
            currentTexturePart++;
        }
        AssetDatabase.CreateAsset(tempTextureArray, "Assets/Audio/BakedWaveForms/" + _source.name + "/" + _source.name + "TextureArray.asset");


        //  AudioTextures.Add(tex);
        //  Sprite temp = Sprite.Create(tex, new Rect(0, 0, 512, 512), Vector2.zero, 64);
        //  AudioSprites.Add();

        Debug.Log("Done Baking Textures");

    }
}

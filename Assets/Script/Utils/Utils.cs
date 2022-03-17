using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EMEM
{
    PAST, PRESENT, FUTURE, SELF, OUTWARD, COUNT, SPAWN
}
public enum ESPLATSIZE
{
    SMALL, BIG
}
public enum EGAMESTAGE
{
    INIT,
    WOMB,
    BIRTH,
    BABY,
    CHILD,
    ADOLESCENT,
    ADULT,
    OLD,
    RANDOM
}

[Serializable]
public class EGAMESTATEDataDict : SerializableDictionary<EGAMESTAGE, PlayerAgeData> { }

public static class Utils
{
    static float[] rho_R = new float[] { 0.021592459f, 0.020293111f, 0.021807906f, 0.023803297f, 0.025208132f, 0.025414957f, 0.024621282f, 0.020973705f, 0.015752802f, 0.01116804f, 0.008578277f, 0.006581877f, 0.005171723f, 0.004545205f, 0.00414512f, 0.004343112f, 0.005238155f, 0.007251939f, 0.012543656f, 0.028067132f, 0.091342277f, 0.484081092f, 0.870378324f, 0.939513128f, 0.960926994f, 0.968623763f, 0.971263883f, 0.972285819f, 0.971898742f, 0.972691859f, 0.971734812f, 0.97234454f, 0.97150339f, 0.970857997f, 0.970553866f, 0.969671404f };
    static float[] rho_G = new float[] { 0.010542406f, 0.010878976f, 0.011063512f, 0.010736566f, 0.011681813f, 0.012434719f, 0.014986907f, 0.020100392f, 0.030356263f, 0.063388962f, 0.173423837f, 0.568321142f, 0.827791998f, 0.916560468f, 0.952002841f, 0.964096452f, 0.970590861f, 0.972502542f, 0.969148203f, 0.955344651f, 0.892637233f, 0.5003641f, 0.116236717f, 0.047951391f, 0.027873526f, 0.020057963f, 0.017382174f, 0.015429109f, 0.01543808f, 0.014546826f, 0.015197773f, 0.014285896f, 0.015069123f, 0.015506263f, 0.015545797f, 0.016302839f };
    static float[] rho_B = new float[] { 0.967865135f, 0.968827912f, 0.967128582f, 0.965460137f, 0.963110055f, 0.962150324f, 0.960391811f, 0.958925903f, 0.953890935f, 0.925442998f, 0.817997886f, 0.42509696f, 0.167036273f, 0.078894327f, 0.043852038f, 0.031560435f, 0.024170984f, 0.020245519f, 0.01830814f, 0.016588218f, 0.01602049f, 0.015554808f, 0.013384959f, 0.012535491f, 0.011199484f, 0.011318274f, 0.011353953f, 0.012285073f, 0.012663188f, 0.012761325f, 0.013067426f, 0.013369566f, 0.013427487f, 0.01363574f, 0.013893597f, 0.014025757f };

    static float[] TMATRIXR = new float[] { 0f, 0.000184722f, 0.000935514f, 0.003096265f, 0.009507714f, 0.017351596f, 0.022073595f, 0.016353161f, 0.002002407f, -0.016177731f, -0.033929391f, -0.046158952f, -0.06381706f, -0.083911194f, -0.091832385f, -0.08258148f, -0.052950086f, -0.012727224f, 0.037413037f, 0.091701812f, 0.147964686f, 0.181542886f, 0.210684154f, 0.210058081f, 0.181312094f, 0.132064724f, 0.093723787f, 0.057159281f, 0.033469657f, 0.018235464f, 0.009298756f, 0.004023687f, 0.002068643f, 0.00109484f, 0.000454231f, 0.000255925f };
    static float[] TMATRIXG = new float[] { 0f, -0.000157894f, -0.000806935f, -0.002707449f, -0.008477628f, -0.016058258f, -0.02200529f, -0.020027434f, -0.011137726f, 0.003784809f, 0.022138944f, 0.038965605f, 0.063361718f, 0.095981626f, 0.126280277f, 0.148575844f, 0.149044804f, 0.14239936f, 0.122084916f, 0.09544734f, 0.067421931f, 0.035691251f, 0.01313278f, -0.002384996f, -0.009409573f, -0.009888983f, -0.008379513f, -0.005606153f, -0.003444663f, -0.001921041f, -0.000995333f, -0.000435322f, -0.000224537f, -0.000118838f, 0f, 0f };
    static float[] TMATRIXB = new float[] { 0.00032594f, 0.001107914f, 0.005677477f, 0.01918448f, 0.060978641f, 0.121348231f, 0.184875618f, 0.208804428f, 0.197318551f, 0.147233899f, 0.091819086f, 0.046485543f, 0.022982618f, 0.00665036f, -0.005816014f, -0.012450334f, -0.015524259f, -0.016712927f, -0.01570093f, -0.013647887f, -0.011317812f, -0.008077223f, -0.005863171f, -0.003943485f, -0.002490472f, -0.001440876f, -0.000852895f, -0.000458929f, -0.000248389f, -0.000129773f, 0f, 0f, -0.00001f, 0f, 0f, 0f };

    static float[] resultSpectrum = new float[32];
    static float[] rhoColorA = new float[32];
    static float[] rhoColorB = new float[32];

    public static Vector2 GetRandomVector(Vector2 _vectorDirection, float _offset = 180)
    {
        Vector2 dirVector = _vectorDirection.normalized;
        float offsetInRadiants = (_offset * Mathf.PI) / 180.0f;
        float rotation = UnityEngine.Random.Range(-offsetInRadiants, offsetInRadiants);
        float newX = Mathf.Cos(rotation) * dirVector.x - Mathf.Sin(rotation) * dirVector.y;
        float newY = Mathf.Sin(rotation) * dirVector.x + Mathf.Cos(rotation) * dirVector.y;
        return new Vector2(newX, newY);
    }

    public static Vector2 ConvertToLocalPositionNoScale(Vector2 _coordinateLocation, float _coordinateRotation, Vector2 _sourcePos)
    {
        Vector2 result = _coordinateLocation - _sourcePos;
        //  result = result.rotated(-_coordinateRotation)
        result =  Quaternion.Euler(0,0, -_coordinateRotation) * result;

        return result;
    }


    public static Vector3 ConvertToLinearRGB(Color _color)
    {
        Vector3 result;

        if (_color.r < 0.04045f)
        {
            result.x = _color.r / 12.92f;
        }
	    else
        {
            result.x = Mathf.Pow(((_color.r + 0.055f) / 1.055f), 2.4f);
        }


        if (_color.g < 0.04045f)
        {
            result.y = _color.g / 12.92f;
        }
        else
        {
            result.y = Mathf.Pow(((_color.g + 0.055f) / 1.055f), 2.4f);
        }


        if (_color.b < 0.04045f)
        {
            result.z = _color.b / 12.92f;
        }
        else
        {
            result.z = Mathf.Pow(((_color.b + 0.055f) / 1.055f), 2.4f);
        }
        return result;
    }


    public static Color ConvertToColor(Vector3 _colorData)
    {
        float tempR;
        float tempG;
        float tempB;

        if (_colorData.x < 0.0031308f)
        {
            tempR = 12.92f * _colorData.x;
        }
	    else
        {
            tempR = 1.055f * Mathf.Pow(_colorData.x, 1f / 2.4f) - 0.055f;
        }


        if (_colorData.y < 0.0031308)
        {
            tempG = 12.92f * _colorData.y;
        }
	    else
        {
            tempG = 1.055f * Mathf.Pow(_colorData.y, 1f / 2.4f) - 0.055f;
        }

        if (_colorData.z < 0.0031308f)
        {
            tempB = 12.92f * _colorData.z;
        }
	    else
        {
            tempB = 1.055f * Mathf.Pow(_colorData.z, 1f / 2.4f) - 0.055f;
        }

        return new Color(Mathf.Clamp(tempR, 0, 1), Mathf.Clamp(tempG, 0, 1), Mathf.Clamp(tempB, 0, 1));
    }


    public static Color MixColors(Color _colorA, Color _colorB, int _mixA = 1, int _mixB = 1)
    {
        Vector3 linearColorA = ConvertToLinearRGB(_colorA);

        Vector3 linearColorB = ConvertToLinearRGB(_colorB);
        float sumOfMix = _mixA + _mixB;

        for (int i = 0; i < 32; i++)
        {
            rhoColorA[i] = (rho_R[i] * linearColorA.x + rho_G[i] * linearColorA.y + rho_B[i] * linearColorA.z);
            rhoColorB[i] = (rho_R[i] * linearColorB.x + rho_G[i] * linearColorB.y + rho_B[i] * linearColorB.z);
        }

        for (int i = 0; i < 32; i++)
        {
            resultSpectrum[i] = (Mathf.Pow(rhoColorA[i], (float)_mixA / sumOfMix) * Mathf.Pow(rhoColorB[i], (float)_mixB / sumOfMix));
        }

        float tempX = 0;
        for (int i = 0; i < 32; i++)
        {
            tempX = tempX + TMATRIXR[i] * resultSpectrum[i];
        }

        float tempY = 0;
        for (int i = 0; i < 32; i++)
        {
            tempY = tempY + TMATRIXG[i] * resultSpectrum[i];
        }

        float tempZ = 0;
        for (int i = 0; i < 32; i++)
        {
            tempZ = tempZ + TMATRIXB[i] * resultSpectrum[i];
        }

        return ConvertToColor(new Vector3(tempX, tempY, tempZ));
    }
    public static TimerContainer SetTimer(float _duration, TimerDelegate _endfunction, bool _oneshot = true)
    {
        return TimeManager.Instance.SetTimer(_duration, _endfunction, _oneshot);
    }
}

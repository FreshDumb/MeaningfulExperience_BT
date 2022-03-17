#ifndef GetColorByDistance_INCLUDED
#define GetColorByDistance_INCLUDED

void GetColorByDistance_float(float distance, float g0, float g1, float g2, float g3, float g4, out float3 color)
{
    color[0] = 1;
    color[1] = 1;
    color[2] = 1;
    [branch]
    if (distance < g0)
    {
        color[0] = 0.8;
        color[1] = 0.8;
        color[2] = 0.8;
    }
    else if (distance >= g0 && distance < g1)
    {
        color[0] = 0.6;
        color[1] = 0.6;
        color[2] = 0.6;
    }
    else if (distance >= g1 && distance < g2)
    {
        color[0] = 0.4;
        color[1] = 0.4;
        color[2] = 0.4;
    }
    else if (distance >= g2 && distance < g3)
    {
        color[0] = 0.2;
        color[1] = 0.2;
        color[2] = 0.2;
    }
    else if (distance >= g3 && distance < g4)
    {
        color[0] = 0.1;
        color[1] = 0.1;
        color[2] = 0.1;
    }
    else
    {
        color[0] = 0;
        color[1] = 0;
        color[2] = 0;
    }

}
#endif //GetColorByDistance_INCLUDED
#ifndef WAVE_FUNCTIONS_INCLUDED
#define WAVE_FUNCTIONS_INCLUDED


// Hash function pour générer du bruit cohérent
float2 hash2(float2 pValue)
{
    pValue = float2(dot(pValue, float2(127.1, 311.7)),
               dot(pValue, float2(269.5, 183.3)));
    return frac(sin(pValue) * 43758.5453);
}

// Fonction de bruit simple type "value noise"
float noise(float2 pValue)
{
    float2 vInt = floor(pValue);
    float2 vDec = frac(pValue);

    float2 u = vDec * vDec * (3.0 - 2.0 * vDec);

    float a = dot(hash2(vInt + float2(0.0, 0.0)), vDec - float2(0.0, 0.0));
    float b = dot(hash2(vInt + float2(1.0, 0.0)), vDec - float2(1.0, 0.0));
    float c = dot(hash2(vInt + float2(0.0, 1.0)), vDec - float2(0.0, 1.0));
    float d = dot(hash2(vInt + float2(1.0, 1.0)), vDec - float2(1.0, 1.0));

    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}


void WaveHeight_float(float2 pWorldPosXZ, float pTime, float pSpeed, out float vHeight)
{
     vHeight = sin(pWorldPosXZ.x + pTime*pSpeed) * 0.2 + cos(pWorldPosXZ.y + pTime*pSpeed) * 0.2;
    //vHeight = noise(pWorldPosXZ*10 + float2(pTime*pSpeed,pTime*pSpeed));
}

#endif
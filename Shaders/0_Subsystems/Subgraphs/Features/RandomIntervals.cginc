float fhash(float n)
{
    return frac(sin(n) * 43758.5453123);
}

float smoothInterp(float t)
{
    return t * t * (3.0 - 2.0 * t);
}

float hash(float x, float seed)
{
    float s0 = floor(seed);
    float f  = frac(seed);
    float a = fhash(x + s0);
    float b = fhash(x + s0 + 1.0);
    float t = smoothInterp(f);
    return lerp(a, b, t);
}

float fhash2(float x, float y)
{
    return frac(sin(dot(float2(x,y), float2(127.1, 311.7))) * 43758.5453123);
}

float ValueNoise2D(float x, float y)
{
    float ix = floor(x);
    float iy = floor(y);
    float fx = frac(x);
    float fy = frac(y);

    float a = fhash2(ix,     iy);
    float b = fhash2(ix + 1, iy);
    float c = fhash2(ix,     iy + 1);
    float d = fhash2(ix + 1, iy + 1);

    float ux = smoothInterp(fx);
    float uy = smoothInterp(fy);

    float ab = lerp(a, b, ux);
    float cd = lerp(c, d, ux);
    return lerp(ab, cd, uy);
}

void RandomIntervals_float(int Divisions, float Coordinate, float VariationAmount, float Seed, out float Out, out float Index)
{
    float count = min(Divisions, 30);

    float r[30];
    float sum = 0.0;
    float uni = 1.0 / count;

    for(int i = 0; i < count; i++)
    {
	    r[i] = ValueNoise2D((i + 1), Seed);
        sum += r[i];
    }

    for(int i = 0; i < count; i++)
    {
	    r[i] /= sum;
        r[i] = lerp(uni, r[i], VariationAmount);
    }

    for(int i = 1; i < count; i++)
        r[i] += r[i - 1];

    float lo = 0.0;
    float hi = r[0];
    float idx = 0;
    for(int i = (count - 1); i > 0; i--)
        if (Coordinate >= r[i - 1])
        {
            lo = r[i - 1];
            hi = r[i];
            idx = i;
            break;
        }

    float width = hi - lo;
    Index = idx;
    Out = saturate((Coordinate - lo) / width);
}
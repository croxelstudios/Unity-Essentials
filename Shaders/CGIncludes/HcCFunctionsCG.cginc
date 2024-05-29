fixed4 Pixelate(sampler2D tex, float4 texelSize, float2 uv, uint2 pixelate, int2 precission = int2(3, 3))
{
	fixed4 rescol = float4(0, 0, 0, 0);
	float2 pixelateAbsolute = texelSize.zw / pixelate;
	if ((pixelateAbsolute.x > 1) || (pixelateAbsolute.y > 1))
	{
        float2 blockSize = uint2(1, 1) / pixelate;
		uv = floor(pixelate * uv) / (float2)pixelate;

		float2 precissionSize = blockSize / precission;
		float2 precissionSizeHalf = precissionSize / 2;
		for (int x = 0; x < precission.x; x++)
			for (int y = 0; y < precission.y; y++)
			{
				float2 newuv = uv + ((float2(x, y) * precissionSize) - precissionSizeHalf);
				newuv = (round(newuv * texelSize.zw) + 0.5) * texelSize.xy;
				rescol += tex2D(tex, newuv);
			}
		rescol /= precission.x * precission.y;

		//if (!interpolateColors) //This code chooses the most dominant color between the possible choices
		//{
		//	//TO DO: Seems interpolated anyway, but the problem is not this code but how the UV works.
		//	//It always interpolates the pixels, probably due to float inaccuracy.
		//	fixed4 finalColor = rescol;
		//	float minDif = 1;
		//	for (int x = 0; x < precission.x; x++)
		//		for (int y = 0; y < precission.y; y++)
		//		{
		//			fixed4 pixel = tex2D(tex, cbCoords + (float2(x, y) * precissionSize));
		//			float dif = length(rescol - pixel);
		//			if (dif < minDif)
		//			{
		//				finalColor = pixel;
		//				minDif = dif;
		//			}
		//		}
		//	rescol = finalColor;
		//}
	}
	else
	{
		rescol = tex2D(tex, uv);
	}
	return rescol;
}

float2 RotateUV(float2 uv, float rotationDeg, float aspect)
{
	float2 cuv = fixed2(0.5, 0.5);

	float2 v = uv - cuv;
	v.x *= aspect;
	float r = radians(rotationDeg);
	float cs = cos(r);
	float sn = sin(r);
	uv = float2((v.x * cs - v.y * sn) / aspect, v.x * sn + v.y * cs) + cuv.xy;
	return uv;
}

float AlphaFromRangeToPosition(float3 position, float min, float max)
{
	float l = length(position);
	if (l < min) return 1;
	else if (l > max) return 0;
	else return 1 - ((l - min) / (max - min));
}

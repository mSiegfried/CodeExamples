texture2D normalMap : register(t0); // normal map for models
texture2D baseTexture : register(t1); // base texture for models

SamplerState filters : register(s0); // filter 0 using CLAMP, filter 1 using WRAP

									 // Per-pixel color data passed through the pixel shader.
struct PixelShaderInput
{
	float4 pos : SV_POSITION;
	float3 uv : UV;
	float4 normal : NORMAL;
	float4 worldPos : WORLD_POSITION;
};

cbuffer dirLight : register(b0)
{
	float3 direction_dir;
	float4 color_dir;
};

cbuffer pointLight : register(b1)
{
	float3 position_point;
	float4 color_point;
};

cbuffer spotLight : register(b2)
{
	float3 direction_spot;
	float ratio_spot;
	float3 position_spot;
	float4 color_spot;
};

// A pass-through function for the (interpolated) color data.
float4 main(PixelShaderInput input) : SV_TARGET
{
	float4 baseColor = baseTexture.Sample(filters, input.uv); // get base color

																  // Directional Light Code
	float3 surfaceNormals = input.normal.xyz;
	float lightRatio = saturate(dot(-normalize(direction_dir), normalize(surfaceNormals)));
	float4 dirLightValue = lightRatio * color_dir * baseColor;


	/*         DIRECTIONAL LIGHT
	LIGHTRATIO = CLAMP( DOT( -LIGHTDIR, SURFACENORMAL ) )
	RESULT = LIGHTRATIO * LIGHTCOLOR * SURFACECOLOR
	*/
	float3 pLightDir = normalize(position_point - input.worldPos);
	float attenuation = 1.0 - saturate(length(position_point - input.worldPos) / 2.5f);
	float pLightRatio = saturate(dot(normalize(pLightDir), normalize(surfaceNormals)));
	float4 pLightValue = pLightRatio * attenuation * color_point * baseColor;

	// return point light only
	//return pLightValue;

	// return directional & point lights


	/*             SPOT LIGHT
	LIGHTDIR = NORMALIZE( LIGHTPOS – SURFACEPOS ) )
	SURFACERATIO = CLAMP( DOT(   -LIGHTDIR, CONEDIR ) )
	SPOTFACTOR = ( SURFACERATIO > CONERATIO ) ? 1 : 0
	LIGHTRATIO = CLAMP( DOT( LIGHTDIR, SURFACENORMAL ) )
	RESULT = SPOTFACTOR * LIGHTRATIO * LIGHTCOLOR * SURFACECOLOR
	*/
	float3 sLightDir = normalize(direction_spot - input.worldPos);
	float surfaceRatio = saturate(dot(-sLightDir, direction_spot));
	float spotFactor = (surfaceRatio > ratio_spot) ? 1 : 0;
	float sLightRatio = saturate(dot(sLightDir, surfaceNormals));
	float sLightValue = spotFactor * sLightRatio * color_spot * baseColor;
	
	float4 totalLight = saturate(dirLightValue + pLightValue + sLightValue);
	return totalLight;
}




/*           POINT LIGHT
LIGHTDIR = NORMALIZE( LIGHTPOS – SURFACEPOS )
LIGHTRATIO = CLAMP( DOT( LIGHTDIR, SURFACENORMAL ) )
RESULT = LIGHTRATIO * LIGHTCOLOR * SURFACECOLOR
*/


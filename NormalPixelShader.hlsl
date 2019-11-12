texture2D normalMap : register(t0); // normal map for models
texture2D baseTexture : register(t1); // base texture for models

SamplerState filters : register(s0); // filter 0 using CLAMP, filter 1 using WRAP

									 // Per-pixel color data passed through the pixel shader.
struct PixelShaderInput
{
	float4 pos : SV_POSITION;
	float2 uv : UV;
	float3 normal : NORMAL;
	float3 worldPos : WORLD_POSITION;
	float3 tan : TANGENT;
	float3 biTangent : BI_TANGENT;
};

cbuffer dirLight : register(b0)
{
	float3 direction_dir;
	float4 color_dir;
	float4 specular_dir;
};

cbuffer pointLight : register(b1)
{
	float3 position_point;
	float4 color_point;
	float4 specular_point;
};

cbuffer spotLight : register(b2)
{
	float3 direction_spot;
	float ratio_spot;
	float3 position_spot;
	float4 color_spot;
	float4 specular_spot;
};

cbuffer cameraConstantBuffer : register(b3)
{
	float4 camera;
};

// A pass-through function for the (interpolated) color data.
float4 main(PixelShaderInput input) : SV_TARGET
{
	float4 baseColor = baseTexture.Sample(filters, input.uv); // get base color
	float4 baseNorms = normalMap.Sample(filters, input.uv); // get the normal mapping
	
	// adjust for negatives
	baseNorms.xyz = 2 * baseNorms.xyz - 1;

	float3x3 TangentBitangentNormal;
	TangentBitangentNormal[0] = normalize(input.tan);
	TangentBitangentNormal[1] = normalize(input.biTangent);
	TangentBitangentNormal[2] = normalize(input.normal);

	baseNorms.xyz = mul(normalize(baseNorms.xyz), TangentBitangentNormal);

	input.normal = normalize(input.normal);

	//// AMBIENT LIGHTING
	//float ambientRatio = saturate(dot(-direction_dir, input.normal));
	//float ambient = saturate(ambientRatio + 2.0f);

	/*         DIRECTIONAL LIGHT
	LIGHTRATIO = CLAMP( DOT( -LIGHTDIR, SURFACENORMAL ) )
	RESULT = LIGHTRATIO * LIGHTCOLOR * SURFACECOLOR
	*/
	float lightRatio = saturate(dot(-normalize(direction_dir), normalize(baseNorms.xyz)));
	float4 dirLightValue = lightRatio * color_dir * /*ambient **/ baseColor;

	//// SPECULAR LIGHTING
	//float3 spec_DIR = normalize(camera.xyz - input.worldPos);
	//float3 halfVec_DIR = normalize(-direction_dir + spec_DIR);
	//float3 intensity_DIR = max(pow(saturate(dot(input.normal, halfVec_DIR)), 100), 0);
	//float3 reflected_DIR = color_dir.xyz * intensity_DIR.x * intensity_DIR;

	/*           POINT LIGHT
	LIGHTDIR = NORMALIZE( LIGHTPOS – SURFACEPOS )
	LIGHTRATIO = CLAMP( DOT( LIGHTDIR, SURFACENORMAL ) )
	RESULT = LIGHTRATIO * LIGHTCOLOR * SURFACECOLOR
	*/
	float3 pLightDir = normalize(position_point - input.worldPos);
	float attenuation = 1.0 - saturate(length(position_point - input.worldPos) / 2.5f);
	float pLightRatio = saturate(dot(normalize(pLightDir), normalize(baseNorms.xyz)));
	float4 pLightValue = pLightRatio * attenuation * color_point * baseColor;

	/*             SPOT LIGHT
	LIGHTDIR = NORMALIZE( LIGHTPOS – SURFACEPOS ) )
	SURFACERATIO = CLAMP( DOT(   -LIGHTDIR, CONEDIR ) )
	SPOTFACTOR = ( SURFACERATIO > CONERATIO ) ? 1 : 0
	LIGHTRATIO = CLAMP( DOT( LIGHTDIR, SURFACENORMAL ) )
	RESULT = SPOTFACTOR * LIGHTRATIO * LIGHTCOLOR * SURFACECOLOR
	*/
	float3 sLightDir = normalize(position_spot - input.worldPos);
	float surfaceRatio = saturate(dot(-sLightDir, direction_spot));
	float spotFactor = (surfaceRatio > ratio_spot) ? 1 : 0;
	float sLightRatio = saturate(dot(sLightDir, baseNorms.xyz));
	float sLightValue = spotFactor * sLightRatio * color_spot * baseColor;

	float4 totalLight = saturate(dirLightValue + pLightValue + sLightValue);
	return totalLight;
}
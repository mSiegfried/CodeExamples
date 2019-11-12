// A constant buffer that stores the three basic column-major matrices for composing geometry.
cbuffer ModelViewProjectionConstantBuffer : register(b0)
{
	matrix model;
	matrix view;
	matrix projection;
};

// Per-vertex data used as input to the vertex shader.
struct VertexShaderInput
{
	float3 pos : POSITION;
	float2 uv : UV;
	float3 normal : NORMAL;
	float3 tan : TANGENT;
	float3 biTangent : BI_TANGENT;
};


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

// Simple shader to do vertex processing on the GPU.
PixelShaderInput main(VertexShaderInput input)
{
	PixelShaderInput output;
	float4 pos = float4(input.pos.xyz, 1.0f);

	// Transform the vertex position into projected space.
	pos = mul(pos, model);
	output.worldPos = pos;
	pos = mul(pos, view);
	pos = mul(pos, projection);
	output.pos = pos;

	output.uv = input.uv;
	output.normal = input.normal;
	output.normal = mul(output.normal, model);
	output.tan = mul(input.tan, model);
	output.biTangent = mul(cross(input.normal, input.tan), model);

	return output;
}

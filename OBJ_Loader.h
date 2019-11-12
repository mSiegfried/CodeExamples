#pragma once

class OBJ_Loader
{
private:
	std::vector<uint32> vertexIndices, UV_Indices, normalIndices, multiple_Indices;
	std::vector<DirectX::XMFLOAT3> tempVertices, tempNormals, tempMultiples;
	std::vector<DirectX::XMFLOAT2>  tempUVs;
	UINT count;
public:
	struct __declspec(align(16))Vertex
	{
		DirectX::XMFLOAT3 position;
		DirectX::XMFLOAT2 UV;
		DirectX::XMFLOAT3 normal;
	};
	OBJ_Loader();
	~OBJ_Loader();
	bool readOBJ(const char * path, std::vector<Vertex> & out_vertices, std::vector<UINT> & out_indices);
};


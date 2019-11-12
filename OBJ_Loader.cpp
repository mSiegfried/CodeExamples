#include "pch.h"
#include "OBJ_Loader.h"


OBJ_Loader::OBJ_Loader()
{
}


OBJ_Loader::~OBJ_Loader()
{
}

bool OBJ_Loader::readOBJ(const char * path, std::vector<Vertex> & out_vertices, std::vector<UINT> & out_indices)
{
	FILE * objFile;
	fopen_s(&objFile, path, "r");
	count = 0;

	// did the file fail to open?
	if (objFile == NULL)
	{
		printf("CANNOT OPEN FILE\n");
		return false;
	}

	// file exists. Open and read data
	for (;;)
	{
		char header[128];

		int res = fscanf_s(objFile, "%s", header, 128);
		if (res == EOF)
		{
			break; // Reached end of file. Exit loop.
		}
		// read vertices
		if (strcmp(header, "v") == 0)
		{
			DirectX::XMFLOAT3 vertex;
			fscanf_s(objFile, "%f %f %f\n", &vertex.x, &vertex.y, &vertex.z);
			tempVertices.push_back(vertex);
		}
		// read UVs
		else if (strcmp(header, "vt") == 0)
		{
			DirectX::XMFLOAT2 uv;
			fscanf_s(objFile, "%f %f\n", &uv.x, &uv.y);
			uv.y = 1 - uv.y;
			tempUVs.push_back(uv);
		}
		// read normals
		else if (strcmp(header, "vn") == 0)
		{
			DirectX::XMFLOAT3 normal;
			fscanf_s(objFile, "%f %f %f\n", &normal.x, &normal.y, &normal.z);
			tempNormals.push_back(normal);
		}
		// read 'f's
		else if (strcmp(header, "f") == 0)
		{
			std::string vertex1, vertex2, vertex3;
			UINT vertexIndex[3], UV_Index[3], normalIndex[3];
			int matches = fscanf_s(objFile, "%d/%d/%d %d/%d/%d %d/%d/%d\n", &vertexIndex[0], &UV_Index[0], &normalIndex[0],
				&vertexIndex[1], &UV_Index[1], &normalIndex[1],
				&vertexIndex[2], &UV_Index[2], &normalIndex[2]);
			if (matches != 9)
			{
				printf("File cannot be read.");
				return false;
			}
			for (UINT i = 0; i < 3; ++i)
			{
				vertexIndices.push_back(vertexIndex[i]);
				UV_Indices.push_back(UV_Index[i]);
				normalIndices.push_back(normalIndex[i]);
			}
		}
		
		// read multi mesh texturing information
		//else if (strcmp(header, "g") == 0)
		//{
		//	count += 1;
		//	vertexIndices.resize(count);
		//	UV_Indices.resize(count);
		//	normalIndices.resize(count);
		//	//multiple_Indices.resize(count);
		//}
	}
	for (UINT i = 0; i < vertexIndices.size(); ++i)
	{
		Vertex v;
		UINT vertexIndex = vertexIndices[i];
		UINT UV_Index = UV_Indices[i];
		UINT normalIndex = normalIndices[i];
		//UINT multi_Index = multiple_Indices[i];

		DirectX::XMFLOAT3 vertex = tempVertices[vertexIndex - 1];
		DirectX::XMFLOAT2 uv = tempUVs[UV_Index - 1];
		DirectX::XMFLOAT3 normal = tempNormals[normalIndex - 1];
		//DirectX::XMFLOAT3 multi = tempMultiples[multi_Index - 1];

		v.position = vertex;
		v.UV = uv;
		v.normal = normal;
		
		out_vertices.push_back(v);
		out_indices.push_back(i);
	}
	
	// clear out the vectors of previous data
	vertexIndices.clear();
	UV_Indices.clear(); 
	normalIndices.clear();
	tempVertices.clear(); 
	tempUVs.clear(); 
	tempNormals.clear();
	return true;
}


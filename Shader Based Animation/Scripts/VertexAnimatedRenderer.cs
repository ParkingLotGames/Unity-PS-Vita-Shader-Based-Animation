using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VertexAnimatedRenderer : MonoBehaviour
{
    Mesh mesh;
    Material material;
    int vertexCountPropertyID = Shader.PropertyToID("_VertexNum");
    List<Vector2> uvs = new List<Vector2>();

    private void InitializeMesh()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        material = GetComponent<MeshRenderer>().sharedMaterial;
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            uvs.Add(new Vector2((float)i / mesh.vertexCount, 0));
        }
        mesh.SetUVs(1, uvs);
        material.SetInt(vertexCountPropertyID, mesh.vertexCount);
    }
#if UNITY_EDITOR
    void Awake()
    {
        InitializeMesh();
    }
#else
	void OnEnable ()
    {
        InitializeMesh();
    }
#endif
}

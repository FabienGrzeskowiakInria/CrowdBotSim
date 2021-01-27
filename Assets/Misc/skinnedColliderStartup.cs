using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skinnedColliderStartup : MonoBehaviour
{
    // mesh collider type
    private SkinnedMeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Mesh colliderMesh;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<SkinnedMeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        colliderMesh = new Mesh();
        colliderMesh.Clear();
        meshRenderer.BakeMesh(colliderMesh);
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = colliderMesh;
        
    }

    void OnDestroy(){
        try
        {
            Destroy(colliderMesh);
        }
        catch{}
    }
}

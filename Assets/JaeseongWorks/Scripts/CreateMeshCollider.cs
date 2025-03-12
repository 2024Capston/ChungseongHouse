using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMeshCollider : MonoBehaviour
{
    void Awake()
    {
        // 부모 객체의 MeshCollider 가져오기
        MeshCollider parentMeshCollider = GetComponent<MeshCollider>();
        if (parentMeshCollider == null)
        {
            parentMeshCollider = gameObject.AddComponent<MeshCollider>();
        }

        // 자식 객체들의 Mesh를 모두 찾아서 결합
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = meshFilter.sharedMesh;
                combineInstance.transform = meshFilter.transform.localToWorldMatrix;
                combineInstances.Add(combineInstance);
            }
        }

        // 결합된 메쉬 생성 및 부모 객체에 추가
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
        parentMeshCollider.sharedMesh = combinedMesh;

        Debug.Log("Combined mesh colliders added to parent");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealVoxelController : MonoBehaviour {

    private MeshRenderer m_MeshRenderer;
    private MeshCollider m_MeshCollider;
    private Mesh m_Mesh;

    // Use this for initialization
    void Start() {
        m_Mesh = GetComponent<MeshFilter>().mesh;
        m_MeshRenderer = GetComponent<UnityEngine.MeshRenderer>();
        m_MeshCollider = GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update() {
        m_MeshCollider.sharedMesh = m_Mesh;     
    }

}

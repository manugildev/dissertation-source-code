using UnityEngine;
using GoogleARCore;

public class Reconstructor {

    static private string TAG = "RECONSTRUCTOR";

    private const int k_MaxPointCount = 61440;

    private Vector4[] m_Points;
    private SpatialHashing m_SpatialHashing;

    // Use this for initialization
    public void Start() {
        Debug.Log("Reconstructor START");
        m_Points = new Vector4[k_MaxPointCount];
        m_SpatialHashing = new SpatialHashing();
    }

    // Update is called once per frame
    public void Update() {
        if (Frame.PointCloud.IsUpdatedThisFrame) {
            for (int i = 0; i < Frame.PointCloud.PointCount; i++) {
                m_Points[i] = Frame.PointCloud.GetPoint(i);
            }


            foreach (Vector4 point in m_Points) {
                int hash = m_SpatialHashing.GetHash(point, m_SpatialHashing.GetScale(), m_SpatialHashing.GetHashTableSize());
                if (m_SpatialHashing.Contains(hash)) {
                    Debug.Log(TAG + ": " + "Point > " + point.ToString() + ", NEW Hash: " + hash);
                    // Does contain the hash, add point to hash
                    m_SpatialHashing.AddPointToEntry(point);
                } else {
                    // Does not contain the hash, make a new one
                    Debug.Log(TAG + ": " + "Point > " + point.ToString() + ", NEW Hash: " + hash);
                    // HashEntry hashEntry = spatialHashing.CreateHash(point);
                    //spatialHashing.AddToTable(hashEntry);

                }
            }
        }

    }

    public SpatialHashing GetSpatialHashing() { return m_SpatialHashing; }
}

using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;
using Reconstruction.Data;
using Reconstruction.Optimization;
using System.Collections.Generic;
using Configuration;
using GoogleARCore.Examples.Common;

public class SpatialHashing {

    private Hashtable m_HashTable = new Hashtable();
    private int m_HashTableSize = Constants.HASH_TABLE_SIZE;
    private int m_Scale = Constants.SCALE;
    private float m_MaxReconstructionDistance = Constants.MAX_RECONSTRUCTION_DISTANCE;

    private List<Division> m_Divisions = new List<Division>();

    private GameObject SubVoxelPrefab;

    public SpatialHashing() {
        //m_Divisions.Add(new Division(6, 0.0f, 0.25f));
        m_Divisions.Add(new Division(4, 0.00f, 0.80f));
        m_Divisions.Add(new Division(2, 0.80f, 0.90f));
    }

    public HashEntry CreateHash(Vector4 position, Voxel voxel, GameObject lodContainer) {
        HashEntry hashEntry = CreateDefaultHashEntry(position, voxel);
        AddPointToLevelOfDetails(hashEntry, position, hashEntry.voxel.gameObject);
        hashEntry.lodGameObjects[0] = lodContainer;
        return hashEntry;
    }

    private void AddPointToLevelOfDetails(HashEntry hashEntry, Vector3 pointPosition, GameObject voxel) {
        int i = 0;
        foreach (Division d in m_Divisions) {

            GameObject levelContainer = GetLODContainer(voxel, d.numberOfDivisions);
            Vector3 localPoint = levelContainer.transform.InverseTransformPoint(pointPosition);
            Vector3 newPoint = GetClosestCenterByDivision(localPoint, d.numberOfDivisions);

            // Make a hash of the subvoxel based on the level and the CenterPoint
            int subHash = GetHash(newPoint, 100, m_HashTableSize);

            if (!hashEntry.lodContains(d.numberOfDivisions, subHash)) {
                //Debug.Log("> NewPoint: " + " " + hashEntry.hash  + " " + newPoint + " SubHash: " + subHash);
                GameObject subVoxel = AddPointToVoxel(levelContainer, pointPosition);
                if (i == 0) {
                    subVoxel.GetComponent<MeshRenderer>().material = PointcloudVisualizer.MaterialBlue;
                }
                hashEntry.lodCreateNewHash(d.numberOfDivisions, subHash, new Voxel(subVoxel));
                subVoxel.name = levelContainer.name + "_voxel";
                float subVoxelScale = 1.0f / (float) d.numberOfDivisions;
                subVoxel.transform.localPosition = newPoint;
                subVoxel.transform.localScale = new Vector3(subVoxelScale, subVoxelScale, subVoxelScale) * 0.95f;
                levelContainer.transform.SetParent(GameObject.Find("_voxel_" + hashEntry.hash).transform);
                hashEntry.lodGameObjects.Add(d.numberOfDivisions, levelContainer);
                DebugText.Instance.increaseNumberOfDetails(i); 
            }
            i++;
        }
    }

    private static GameObject GetLODContainer(GameObject voxel, int level) {
        GameObject levelContainer;
        if (GameObject.Find("_levelOfDetail_" + level) == null) {
            // Create new LevelOfDetail container to store all members
            levelContainer = new GameObject("_levelOfDetail_" + level);
            levelContainer.transform.parent = voxel.transform;
            levelContainer.transform.localPosition = Vector3.zero;
            levelContainer.transform.localScale = Vector3.one;
            levelContainer.transform.rotation = voxel.transform.rotation;
        } else {
            levelContainer = GameObject.Find("_levelOfDetail_" + level);
        }

        return levelContainer;
    }

    private GameObject AddPointToVoxel(GameObject container, Vector4 position) {
        GameObject subVoxelGameObject = GameObject.Instantiate(SubVoxelPrefab, position, new Quaternion());
        subVoxelGameObject.transform.SetParent(container.transform);

        subVoxelGameObject.GetComponent<Renderer>().enabled = PointcloudVisualizer.Instance.GetActiveRenderer(); 
        return subVoxelGameObject;
    }

    private HashEntry CreateDefaultHashEntry(Vector4 position, Voxel voxel) {
        HashEntry hashEntry = new HashEntry();
        hashEntry.hash = GetHash(position, m_Scale, m_HashTableSize);
        hashEntry.position = position;
        hashEntry.confidence = position.w;
        hashEntry.weight = 1;
        hashEntry.remove = false;
        hashEntry.voxel = voxel;
        hashEntry.lodGameObjects = new Dictionary<int, GameObject>();

        // Init the HashTables
        hashEntry.levelOfDetails = new Dictionary<int, Hashtable>();
        foreach (Division d in m_Divisions) {
            hashEntry.levelOfDetails.Add(d.numberOfDivisions, new Hashtable());
        }
        return hashEntry;
    }

    private Vector3 GetClosestCenterByDivision(Vector3 position, int divisions) {

        //position.x = Mathf.Clamp(position.x, -0.4f, 0.4f);
        //position.y = Mathf.Clamp(position.y, -0.4f, 0.4f);
        //position.z = Mathf.Clamp(position.z, -0.4f, 0.4f);

        float gridSize = 1.0f / (float) divisions;
        float gridCenter = gridSize / 2.0f;
        float newx, newy, newz;

        if (divisions == 3) {
            newx = Mathf.Floor((position.x + gridCenter) / gridSize) * gridSize;
            newy = Mathf.Floor((position.y + gridCenter) / gridSize) * gridSize;
            newz = Mathf.Floor((position.z + gridCenter) / gridSize) * gridSize;
        } else {
            newx = Mathf.Floor(position.x / gridSize) * gridSize + gridCenter;
            newy = Mathf.Floor(position.y / gridSize) * gridSize + gridCenter;
            newz = Mathf.Floor(position.z / gridSize) * gridSize + gridCenter;
        }

        return new Vector3(newx, newy, newz);
    }



    public void Clear() {
        foreach (DictionaryEntry entry in m_HashTable) {
            HashEntry hashEntry = (HashEntry) entry.Value;
            UnityEngine.Object.Destroy(hashEntry.voxel.gameObject);

        }
        m_HashTable.Clear();
    }

    public int GetHash(Vector3 position, float scale, int hashTableSize) {
        position = position * scale;

        /* Round the actual float positions to integers  */
        int x_int = (int) Math.Floor(position.x);
        int y_int = (int) Math.Floor(position.y);
        int z_int = (int) Math.Floor(position.z);

        return (73856093 * x_int) ^ (19349669 * y_int) ^ (83492791 * z_int) % hashTableSize;
    }

    public void AddToTable(HashEntry hashEntry) {
        m_HashTable.Add(hashEntry.hash, hashEntry);
    }

    public void AddPointToEntry(Vector4 position) {
        HashEntry hashEntry = (HashEntry) m_HashTable[GetHash(position, m_Scale, m_HashTableSize)];
        hashEntry.weight++;
        // Weigted Average of the confidence (https://math.stackexchange.com/questions/22348/how-to-add-and-subtract-values-from-an-average)
        hashEntry.confidence = hashEntry.confidence + ((position.w - hashEntry.confidence) / hashEntry.weight);
        m_HashTable[GetHash(position, m_Scale, m_HashTableSize)] = hashEntry;

        AddPointToLevelOfDetails(hashEntry, position, hashEntry.voxel.gameObject);
    }


    public HashEntry GetFromTable(int hash) {
        /* Try what hyappens when the key does not exist */
        return (HashEntry) m_HashTable[hash];
    }

    public Boolean Contains(int hash) {
        /* Try what hyappens when the key does not exist */
        return m_HashTable.ContainsKey(hash);
    }

    public void DeleteFromTable(HashEntry hashEntry) {
        m_HashTable.Remove(hashEntry.hash);
    }

    // This should be probably associated to the hashtable itself
    public override String ToString() {
        String finalString = "HashTable -> " + Size() + "\n";
        foreach (HashEntry hashEntry in m_HashTable) {
            finalString += hashEntry.ToString() + "\n";
        }
        return finalString;
    }

    public int Size() {
        return m_HashTable.Count;
    }

    public void RemoveFromTable(int hash) {
        // Maybe dont need to check
        if (!m_HashTable.ContainsKey(hash)) return;
        UnityEngine.Object.Destroy(((HashEntry) m_HashTable[hash]).voxel.gameObject);
        foreach(Division d in m_Divisions) {
            UnityEngine.Object.Destroy(((HashEntry) m_HashTable[hash]).lodGameObjects[d.numberOfDivisions].gameObject);
        }

        m_HashTable.Remove(hash);
    }

    public void setRemoved(int hash) {
        HashEntry hashEntry = ((HashEntry) m_HashTable[hash]);
        hashEntry.remove = true;
        m_HashTable[hash] = hashEntry;
    }

    public HashEntry GetFromTable(Vector3 point) {
        return (HashEntry) m_HashTable[GetHash(point, m_Scale, m_HashTableSize)];
    }

    public HashEntry[] GetHashArray() {
        HashEntry[] foos = new HashEntry[m_HashTable.Count];
        m_HashTable.Values.CopyTo(foos, 0);
        return foos;
    }

    public void SetDistance(HashEntry hashEntry, float distance) {
        hashEntry.distanceToCamera = distance;
        m_HashTable[hashEntry.hash] = hashEntry;
    }

    public int GetHashTableSize() { return m_HashTableSize; }
    public void SetHashTableSize(int value) { this.m_HashTableSize = value; }

    public int GetScale() { return m_Scale; }
    public void SetScale(int value) { this.m_Scale = value; }

    public float GetMaxReconstructionDistance() { return m_MaxReconstructionDistance; }
    public void SetMaxReconstructionDistance(float value) { this.m_MaxReconstructionDistance = value; }

    public void SetSubVoxelPrefab(GameObject gameObject) { this.SubVoxelPrefab = gameObject; }

    public Hashtable GetHashtable() { return m_HashTable; }


    public List<Division> GetDivisions() {
        return m_Divisions;
    }

    public void UpdateDistanceResolution(HashEntry hashEntry) {
        float distance = hashEntry.distanceToCamera;
        GetLevelOfDetail(hashEntry, 0).SetActive(false);

        foreach (Division d in m_Divisions) {
            GetLevelOfDetail(hashEntry, d.numberOfDivisions).SetActive(false);
        }

        for (int i = 0; i < m_Divisions.Count; i++) {
            float minDistance = m_Divisions[i].minPercent * Constants.MAX_RECONSTRUCTION_DISTANCE;
            float maxDistance = m_Divisions[i].maxPercent * Constants.MAX_RECONSTRUCTION_DISTANCE;
            int level = m_Divisions[i].numberOfDivisions;

            if (distance >= minDistance && distance <= maxDistance) {
                GetLevelOfDetail(hashEntry, level).SetActive(true);
            }
        }

        if (distance >= m_Divisions[m_Divisions.Count - 1].maxPercent * Constants.MAX_RECONSTRUCTION_DISTANCE) {
            GetLevelOfDetail(hashEntry, 0).SetActive(true);
        }
    }

    public void UpdateDistanceResolutions() {
        foreach (DictionaryEntry entry in m_HashTable) {
            HashEntry hashEntry = (HashEntry) entry.Value;
            UpdateDistanceResolution(hashEntry);
        }
    }

    private GameObject GetLevelOfDetail(HashEntry hashEntry, int level) {
        return hashEntry.lodGameObjects[level];
    }

    private void SetAllResolutions(bool value) {
        foreach (DictionaryEntry entry in m_HashTable) {
            HashEntry hashEntry = (HashEntry) entry.Value;
            GetLevelOfDetail(hashEntry, 0).SetActive(value);

            foreach (Division d in m_Divisions) {
                GetLevelOfDetail(hashEntry, d.numberOfDivisions).SetActive(value);
            }
        }
    }

    public void RemoveCollider(HashEntry hashEntry) {
        ((HashEntry) m_HashTable[hashEntry.hash]).voxel.gameObject.GetComponent<MeshCollider>().enabled = false;
        foreach (Division d in m_Divisions) {
            ((HashEntry) m_HashTable[hashEntry.hash]).lodGameObjects[d.numberOfDivisions].GetComponent<MeshCollider>().enabled = false;
        }
    }
}


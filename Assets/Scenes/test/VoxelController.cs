using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelController : MonoBehaviour {

    public int[] divisions = new int[2] { 2, 3 };
    public GameObject subVoxelPrefab;
    public GameObject sphere;

    private void addPointToLevelOfDetails(Vector3 pointPosition, GameObject voxel) {
        foreach (int level in divisions) {
            // Create new LevelOfDetail container to store all members
            GameObject levelContainer = new GameObject("_levelOfDetail" + level);
            levelContainer.transform.SetParent(voxel.transform);
            levelContainer.transform.localPosition = Vector3.zero;
            levelContainer.transform.localScale = Vector3.one;

            GameObject point = addPointToVoxel(levelContainer, pointPosition);
            Vector3 newPoint = getClosestCenterByDivision(point.transform.localPosition, level);

            float subVoxelScale = 1.0f / (float) level;
            point.transform.localPosition = newPoint;
            point.transform.localScale = new Vector3(subVoxelScale, subVoxelScale, subVoxelScale);
        }
    }
    private GameObject addPointToVoxel(GameObject container, Vector4 position) {
        var pointGameObject = GameObject.Instantiate(subVoxelPrefab, position, new Quaternion());
        pointGameObject.transform.SetParent(container.transform);
        return pointGameObject;
    }


    private Vector3 getClosestCenterByDivision(Vector3 position, int divisions) {
        Debug.Log("-----------------------" + divisions);
        float gridSize = (1.0f / (float) divisions);
        float gridCenter =  gridSize / 2.0f;
        Debug.Log(position + "gridSize: " + gridSize + " gridCenter" + gridCenter  );
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

        Vector3 r = new Vector3(newx, newy, newz);
        Debug.Log(r.ToString("0.000"));
        return r;
    }


    // Use this for initialization
    void Start() {
        addPointToLevelOfDetails(sphere.transform.position, this.gameObject);

    }

    // Update is called once per frame
    void Update() {
        /*foreach (Transform child in transform) {
            Vector3 newPos = getClosestCenterByDivision(child.localPosition, divisions);
            Debug.Log("> " + child.transform.localPosition.ToString("0.000") + " " + newPos.ToString("0.000"));
            child.localPosition = newPos;
            float newScale = 1.0f / (float) divisions;
            child.localScale = new Vector3(newScale, newScale, newScale);

        }*/

    }
}

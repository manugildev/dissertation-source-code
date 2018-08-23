using Configuration;
using GoogleARCore.Examples.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public GameObject pointCloud;
    private PointcloudVisualizer pointCloudVisualizer;

    private Vector3 previousPosition;

    // Use this for initialization
    void Start() {
        pointCloudVisualizer = pointCloud.GetComponent<PointcloudVisualizer>();
        previousPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update() {
        if (DebugText.Instance.UpdateDistances)
            if (Vector3.Distance(previousPosition, transform.position) > Constants.UPDATE_DISTANCE) {
                previousPosition = transform.position;
                PointcloudVisualizer.Instance.UpdateResolution(transform.position);
            }

    }
}

using GoogleARCore.Examples.Common;
using Reconstruction.Data;
using System;
using UnityEngine;

namespace Reconstruction.Optimization {

    static class Optimization {

        public const float MAX_PLANE_DISTANCE = 0.05f;

        public enum VoxelPlaneCollision { UP, DOWN, INSIDE, NONE }

        public static VoxelPlaneCollision DoRayCastFromHashEntryToPlane(DetectedPlaneVisualizer newPlane, HashEntry hashEntry) {
            //RaycastHit hit;
            //int layer_mask = LayerMask.GetMask("Plane");

            if (newPlane.GetChildCollider().bounds.Contains(hashEntry.position)) {
                return VoxelPlaneCollision.INSIDE;
            }

            // Separate the point a bit from the object based on the scale of it
            // float distance = (hashEntry.voxel.gameObject.transform.localScale + 0.01f);
            // Vector3 newPoint_neg = distance * (-1 * newPlane.m_planeNormal) + hashEntry.position;
            // Vector3 newPoint_pos = distance * (newPlane.m_planeNormal) + hashEntry.position;


            /*if (Physics.Raycast(hashEntry.position, -1 * newPlane.m_planeNormal, out hit, MAX_PLANE_DISTANCE, layer_mask)) {
                return VoxelPlaneCollision.DOWN;
            }

            if (Physics.Raycast(hashEntry.position, newPlane.m_planeNormal, out hit, MAX_PLANE_DISTANCE, layer_mask)) {
                return VoxelPlaneCollision.UP;
            }*/
            return VoxelPlaneCollision.NONE;

        }

        public static Vector3 ScalePoint(Vector3 point, int scale) {
            point.x = (float) Math.Floor(point.x * scale);
            point.y = (float) Math.Floor(point.y * scale);
            point.z = (float) Math.Floor(point.z * scale);

            point = point / (float) scale;

            // Get the center of the grid, instead of the bottom corner
            float s = 1.0f / (float) scale;
            point = point + (new Vector3(s, s, s) / 2);

            return point;
        }
    }
}

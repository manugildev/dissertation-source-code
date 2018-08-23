using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace Reconstruction.Data {

    public struct HashEntry {

        public Vector3 position;
        public int weight;
        public int hash;
        public Voxel voxel;
        public float confidence;
        public bool remove;
        public Dictionary<int, Hashtable> levelOfDetails;
        public Dictionary<int, GameObject> lodGameObjects;
        public float distanceToCamera;

        public override string ToString() {
            return "HashEntry -> " + hash + " > Position: " + position.ToString("0.000") + ", Weight: " + weight + ", Confidence: " + confidence.ToString("0.0000");
        }

        public bool lodContains(int level, int hash) {
            return levelOfDetails[level].ContainsKey(hash);
        }

        public void lodCreateNewHash(int level, int subHash, Voxel subVoxel) {
            levelOfDetails[level].Add(subHash, subVoxel);
        }

        public void SetDistance(float distance) {
            distanceToCamera = distance;
        }
    }
}

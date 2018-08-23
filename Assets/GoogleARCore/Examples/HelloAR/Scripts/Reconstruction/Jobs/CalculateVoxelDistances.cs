using Reconstruction.Data;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Reconstruction.Jobs {
    struct CalculateVoxelDistances : IJobParallelFor {

        public NativeArray<float> Distances;
        [ReadOnly] public Vector3 cameraPosition;
        [ReadOnly] public NativeArray<HashEntry> HashEntries;

        public void Execute(int i) {
            Distances[i] = Vector3.Distance(HashEntries[i].position, cameraPosition);
        }
    }
}

using Reconstruction.Data;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Reconstruction.Jobs {
    struct PrepareRaycastCommands : IJobParallelFor {

        public NativeArray<RaycastCommand> Raycasts;
        [ReadOnly] public NativeArray<HashEntry> HashEntries;
        [ReadOnly] public float distance;
        [ReadOnly] public Vector3 normal;
        [ReadOnly] public int layerMask;

        public void Execute(int i) {
            Raycasts[i] = new RaycastCommand(HashEntries[i].position, normal, distance, layerMask);
        }
    }
}

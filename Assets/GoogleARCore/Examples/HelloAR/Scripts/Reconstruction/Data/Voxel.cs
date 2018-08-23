using UnityEngine;

namespace Reconstruction.Data
{
    public struct Voxel
    {
        public GameObject gameObject;

        public Voxel(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }
    }
}

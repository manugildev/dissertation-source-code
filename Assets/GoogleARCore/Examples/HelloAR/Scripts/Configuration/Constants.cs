using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Configuration {

    // Initial Values for the Reconstruction
    public static class Constants {
        public const float CONFIDENCE_THRESHOLD = 0.55f;
        public const float MAX_PLANE_DISTANCE = 0.05f;
        public const int HASH_TABLE_SIZE = 2099;
        public const int SCALE = 1;
        public const float UPDATE_DISTANCE = 0.1f;
        public const float MAX_RECONSTRUCTION_DISTANCE = 1.0f;
    }
}

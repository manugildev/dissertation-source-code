using UnityEngine;
using UnityEngine.Events;

namespace Lean.Touch {
    // This script calls the OnFingerTap event when a finger taps the screen
    public class LeanFingerTap : MonoBehaviour {
        // Event signature
        [System.Serializable] public class LeanFingerEvent : UnityEvent<LeanFinger> { }

        [Tooltip("Ignore fingers with StartedOverGui?")]
        public bool IgnoreStartedOverGui = true;

        [Tooltip("Ignore fingers with OverGui?")]
        public bool IgnoreIsOverGui;

        [Tooltip("How many times must this finger tap before OnTap gets called? (0 = every time) Keep in mind OnTap will only be called once if you use this.")]
        public int RequiredTapCount = 0;

        [Tooltip("How many times repeating must this finger tap before OnTap gets called? (0 = every time) (e.g. a setting of 2 means OnTap will get called when you tap 2 times, 4 times, 6, 8, 10, etc)")]
        public int RequiredTapInterval;

        [Tooltip("Do nothing if this LeanSelectable isn't selected?")]
        public LeanSelectable RequiredSelectable;

        public LeanFingerEvent OnTap;

        public GameObject PlaceGameObject;
        private GameObject ObjectsContainer;

#if UNITY_EDITOR
        protected virtual void Reset() {
            Start();
        }
#endif

        protected virtual void Start() {
            ObjectsContainer = new GameObject("ExternalObjectsContainer");
            if (RequiredSelectable == null) {
                RequiredSelectable = GetComponent<LeanSelectable>();
            }
        }

        protected virtual void OnEnable() {
            // Hook events
            LeanTouch.OnFingerTap += FingerTap;
        }

        protected virtual void OnDisable() {
            // Unhook events
            LeanTouch.OnFingerTap -= FingerTap;
        }

        private void FingerTap(LeanFinger finger) {
            // Ignore?
            if (IgnoreStartedOverGui == true && finger.StartedOverGui == true) {
                return;
            }

            if (IgnoreIsOverGui == true && finger.IsOverGui == true) {
                return;
            }

            if (RequiredTapCount > 0 && finger.TapCount != RequiredTapCount) {
                return;
            }

            if (RequiredTapInterval > 0 && (finger.TapCount % RequiredTapInterval) != 0) {
                return;
            }

            if (RequiredSelectable != null && RequiredSelectable.IsSelected == false) {
                return;
            }

            // Call event
            if (OnTap != null) {
                OnTap.Invoke(finger);
                addCube();
            }

        }

        public void addCube() {
            var gameObject = GameObject.Instantiate(PlaceGameObject, transform.position + transform.forward * 0.2f, UnityEngine.Random.rotation);
            gameObject.GetComponent<Rigidbody>().AddForce(-transform.position + transform.forward * 200.3f);
            gameObject.transform.parent = ObjectsContainer.transform;
        }

        private void Shoot(Vector2 position) {
            var ray = Camera.main.ScreenPointToRay(position);
            var hitInfo = new RaycastHit();
            if (Physics.Raycast(ray, out hitInfo)) {
                hitInfo.rigidbody.AddForceAtPosition(ray.direction, hitInfo.point);
            }
        }
    }
}
using UnityEngine;

namespace Eternity {
    public class HiddenWallSwitch : MonoBehaviour {
        public SlidingWall target;
        public Rigidbody vaultPanel;
        private Vector3 openPosition;
        private bool used;

        void Start() {
            if (vaultPanel) openPosition = vaultPanel.position + Vector3.up * 3.85f;
        }

        void OnTriggerStay(Collider other) {
            if (used || !other.GetComponentInParent<GauntletPlayer>() || !Input.GetKey(KeyCode.E)) return;
            used = true;
            if (target) {
                target.hold = 99999f;
                target.Arm();
            }
        }

        void FixedUpdate() {
            if (used && vaultPanel && vaultPanel.position != openPosition)
                vaultPanel.MovePosition(Vector3.MoveTowards(vaultPanel.position, openPosition, 2f * Time.fixedDeltaTime));
        }
    }
}

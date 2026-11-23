using UnityEngine;
namespace Eternity {
public class SwitchMotion:TrapBase {
 
 protected override void Sample(float t){Pose(Vector3.right*(.12f*t),Quaternion.identity);}
}
}

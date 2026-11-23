using UnityEngine;
namespace Eternity {
public class PlateMotion:TrapBase {
 
 protected override void Sample(float t){Pose(Vector3.down*(.03f*t),Quaternion.identity);}
}
}

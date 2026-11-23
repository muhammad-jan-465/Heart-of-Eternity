using UnityEngine;
namespace Eternity {
public class DartWallTrap:TrapBase {
 
 protected override void Sample(float t){Pose(Vector3.right*(5.4f*t),Quaternion.identity);}
}
}

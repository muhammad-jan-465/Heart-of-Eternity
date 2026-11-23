using UnityEngine;
namespace Eternity {
public class FallingBlockTrap:TrapBase {
 
 protected override void Sample(float t){Pose(Vector3.down*(4.9f*t*t),Quaternion.identity);}
}
}

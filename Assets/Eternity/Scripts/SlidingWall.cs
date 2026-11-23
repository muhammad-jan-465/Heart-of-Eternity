using UnityEngine;
namespace Eternity {
public class SlidingWall:TrapBase {
 
 protected override void Sample(float t){Pose(Vector3.left*(2.5f*t),Quaternion.identity);}
}
}

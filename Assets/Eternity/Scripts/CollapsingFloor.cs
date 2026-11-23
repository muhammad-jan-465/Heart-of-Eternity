using UnityEngine;
namespace Eternity {
public class CollapsingFloor:TrapBase {
 
 protected override void Sample(float t){Pose(Vector3.down*(3.6f*t*t),Quaternion.identity);}
}
}

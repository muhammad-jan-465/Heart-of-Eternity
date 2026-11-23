using UnityEngine;
namespace Eternity {
public class SpikeTrap:TrapBase {
 
 protected override void Sample(float t){Pose(Vector3.up*(1.36f*t),Quaternion.identity);}
}
}

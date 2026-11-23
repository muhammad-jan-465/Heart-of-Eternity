using UnityEngine;
namespace Eternity {
public class GearPlatform:TrapBase {
 
 protected override void Sample(float t){Pose(Vector3.zero,Quaternion.AngleAxis(360*t,Vector3.up));}
}
}

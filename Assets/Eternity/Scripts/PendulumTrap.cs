using UnityEngine;
namespace Eternity {
public class PendulumTrap:TrapBase {
 
 protected override void Sample(float t){Pose(Vector3.zero,Quaternion.AngleAxis(Mathf.Sin(t*2*Mathf.PI)*52,Vector3.forward));}
}
}

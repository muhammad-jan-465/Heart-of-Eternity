using UnityEngine;
namespace Eternity {
public class RollingBoulderTrap:TrapBase {
 public float travel=6;
 protected override void Sample(float t){Pose(Vector3.right*travel*t,Quaternion.AngleAxis(-travel*t*Mathf.Rad2Deg,Vector3.forward));}
}
}

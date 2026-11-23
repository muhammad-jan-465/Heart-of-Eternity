using UnityEngine;
namespace Eternity {
public class TempleDoorSlam : TrapBase {
 protected override void Start(){base.Start();if(mover)mover.rotation=mover.transform.parent.rotation*rest*Quaternion.Euler(0,90,0);}
 protected override void Sample(float t){Pose(Vector3.zero,Quaternion.Euler(0,90*(1-t),0));}
 public void SealPermanently(){hold=99999;Fire();}
}
}

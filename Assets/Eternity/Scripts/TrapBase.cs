using UnityEngine;
using UnityEngine.Events;

namespace Eternity {
public abstract class TrapBase : MonoBehaviour {
 public Rigidbody mover;
 public Collider movingCollider;
 public DamageZone damage;
 public float warning=.5f, duration=1f, hold=.25f, cooldown=1.2f;
 public float startDelay;public bool oneShot,repeat,solid=true;
 public UnityEvent onArm=new UnityEvent(), onFire=new UnityEvent(), onReset=new UnityEvent();
 protected Vector3 home; protected Quaternion rest;
 Transform basis; int state; float clock; bool spent;
 protected virtual void Start(){ if(!mover){enabled=false;return;} basis=mover.transform.parent;home=mover.transform.localPosition;rest=mover.transform.localRotation;mover.isKinematic=true;mover.interpolation=RigidbodyInterpolation.Interpolate;if(damage)damage.active=false; }
 public void Arm(){if(!enabled||spent||state!=0)return;state=1;clock=0;onArm.Invoke();}
 public void Fire(){if(spent||state==2)return;state=2;clock=0;if(damage)damage.active=true;onFire.Invoke();}
 public void ResetTrap(){state=4;clock=0;if(damage)damage.active=false;onReset.Invoke();}
 protected void Pose(Vector3 shift,Quaternion turn){mover.MovePosition(basis.TransformPoint(home+shift));mover.MoveRotation(basis.rotation*rest*turn);}
 protected abstract void Sample(float phase);
 protected virtual void FixedUpdate(){
  if(startDelay>0){startDelay-=Time.fixedDeltaTime;return;}clock+=Time.fixedDeltaTime;
  switch(state){
   case 0: if(repeat)Arm();break;
   case 1: if(clock>=warning)Fire();break;
   case 2: Sample(Mathf.Clamp01(clock/Mathf.Max(.02f,duration)));if(clock>=duration){clock=0;state=3;}break;
   case 3: if(clock>=hold)ResetTrap();break;
   case 4: Sample(1-Mathf.Clamp01(clock/Mathf.Max(.02f,duration)));if(clock>=duration){Sample(0);clock=0;state=5;spent=oneShot;}break;
   case 5: if(clock>=cooldown){state=0;clock=0;}break;
  }
 }
}
}

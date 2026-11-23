using UnityEngine;
namespace Eternity {
[RequireComponent(typeof(CharacterController))]
public class TemplePursuer : MonoBehaviour {
 public GauntletPlayer player;public Animator animator;public int health=3;public float delay;
 CharacterController motor;LineRenderer trail;float shot,hide,gravity;int pose;
 public int shotsFired{get;private set;}
 void Start(){
  motor=GetComponent<CharacterController>();shot=Time.time+delay+1.5f;
  trail=gameObject.AddComponent<LineRenderer>();trail.positionCount=2;trail.widthMultiplier=.024f;trail.sharedMaterial=Resources.Load<Material>("TempleShot");trail.enabled=false;
 }
 void State(int next){if(animator&&next!=pose){pose=next;animator.CrossFadeInFixedTime(next,.12f);}}
 void Update(){
  if(!player)return;
  if(Time.time>=hide)trail.enabled=false;
  var to=player.transform.position-transform.position;float dist=to.magnitude;to.y=0;
  Vector3 move=Vector3.zero;
  if(dist<27&&Mathf.Abs(player.transform.position.y-transform.position.y)<5&&to.sqrMagnitude>.01f){
   var forward=to.normalized;transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(forward),220*Time.deltaTime);
   Vector3 muzzle=transform.position+Vector3.up*1.45f+forward*.55f;
   var aim=player.transform.position+Vector3.up*1.05f-muzzle;
   bool clear=Physics.Raycast(muzzle,aim.normalized,out var hit,aim.magnitude+.45f,~0,QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<GauntletPlayer>()==player;
   if(dist>7||!clear){
    move=forward;
    if(Physics.SphereCast(transform.position+Vector3.up,.27f,forward,out _,1.1f,~0,QueryTriggerInteraction.Ignore))move=Vector3.Cross(Vector3.up,forward)*((GetInstanceID()&1)==0?1:-1);
    // Avoid following the player off a shaft or cliff.
    if(!Physics.Raycast(transform.position+move*.8f+Vector3.up*.6f,Vector3.down,2,~0,QueryTriggerInteraction.Ignore))move=Vector3.zero;
   }
   if(clear&&dist<=15&&Time.time>=shot){
    shot=Time.time+2.8f+delay;hide=Time.time+.13f;shotsFired++;player.Hit(1);
    trail.SetPosition(0,muzzle);trail.SetPosition(1,hit.point);trail.enabled=true;pose=0;State(Animator.StringToHash("Base Layer.Shoot"));
   }
  }
  if(Time.time>hide+.18f)State(Animator.StringToHash(move.sqrMagnitude>.1f?"Base Layer.Run":"Base Layer.Idle"));
  gravity=motor.isGrounded?-2:Mathf.Max(-22,gravity-18*Time.deltaTime);motor.Move((move*2.45f+Vector3.up*gravity)*Time.deltaTime);
 }
 public void Hit(){if(--health<=0)gameObject.SetActive(false);}
}
}

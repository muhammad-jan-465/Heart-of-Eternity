using UnityEngine;
namespace Eternity {
[RequireComponent(typeof(CharacterController))]
public class TempleBeast:MonoBehaviour {
 public GauntletPlayer player;public TempleAwakening altar;public int egg=-1,health=4;
 public Animator animator;public AudioClip voice;public float speed=2.4f;
 CharacterController motor;AudioSource audioSource;float nextAttack,call,fall;int state; float attackStarted;
 void Awake(){motor=GetComponent<CharacterController>();audioSource=gameObject.AddComponent<AudioSource>();audioSource.spatialBlend=1;audioSource.minDistance=2;audioSource.maxDistance=27;}
 public static AudioClip EncounterVoice(int id){
  string clip=id switch {0=>"Lizard_Sound",1=>"Drakonit",2=>"wolf",-1=>"Zombie_Sound",_=>null};
  return clip==null?null:Resources.Load<AudioClip>("EncounterAudio/"+clip);
 }
 void Start(){voice=EncounterVoice(egg)??voice;}
 void Call(float volume){
  if(!voice||audioSource.isPlaying)return;
  audioSource.PlayOneShot(voice,volume);
 }
 void Update(){
  if(health<=0||!player)return;var delta=player.transform.position-transform.position;float vertical=delta.y;delta.y=0;float distance=delta.magnitude;
  bool hunting=distance<24&&Mathf.Abs(vertical)<3.5f;int current=0;var move=Vector3.zero;
  if(hunting){
   if(Time.time>=call){Call(.7f);call=Time.time+4.5f;}
   if(distance>.05f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(delta),210*Time.deltaTime);
   if(distance>1.55f){move=delta.normalized;current=1;
    if(Physics.SphereCast(transform.position+Vector3.up,.28f,move,out var block,1,~0,QueryTriggerInteraction.Ignore)&&!block.collider.GetComponentInParent<GauntletPlayer>())move=Vector3.Cross(Vector3.up,move);
    if(!Physics.Raycast(transform.position+move*.8f+Vector3.up,Vector3.down,2.5f,~0,QueryTriggerInteraction.Ignore))move=Vector3.zero;
   }else if(Mathf.Abs(vertical)<1.8f){
    Vector3 origin=transform.position+Vector3.up;
    Vector3 aim=player.transform.position+Vector3.up*.9f-origin;
    if(Physics.Raycast(origin,aim.normalized,out var hit,aim.magnitude+.25f,~0,QueryTriggerInteraction.Ignore)
       && hit.collider.GetComponentInParent<GauntletPlayer>()==player){
     if(Time.time>=nextAttack){
      attackStarted=Time.time;
      nextAttack=Time.time+1.6f;
      if(animator)animator.CrossFadeInFixedTime("Base Layer.Attack",.09f,0,0f);
      state=2;
      player.Hit(1);
     }
     if(state==2 && (Time.time-attackStarted<.15f || animator && animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Attack") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime<1f))current=2;
    }
   }
  }
  Vector3 beforeMove=transform.position;fall=motor.isGrounded?-2:Mathf.Max(-18,fall-18*Time.deltaTime);motor.Move((move*speed+Vector3.up*fall)*Time.deltaTime);
  var travelled=transform.position-beforeMove;travelled.y=0;
  if(current==1 && (move==Vector3.zero || travelled.sqrMagnitude < .01f*Time.deltaTime*Time.deltaTime))current=0;
  if(animator&&current!=state){string n=current==1?"Move":current==2?"Attack":"Idle";int h=Animator.StringToHash("Base Layer."+n);if(animator.HasState(0,h))animator.CrossFadeInFixedTime(h,.14f);state=current;}

 }
 void Hide(){gameObject.SetActive(false);}
 public void Hit(){if(health<=0)return;if(--health>0){Call(.45f);return;}if(egg>=0)altar.Drop(egg,transform.position);motor.enabled=false;if(animator&&animator.HasState(0,Animator.StringToHash("Base Layer.Die"))){animator.CrossFadeInFixedTime("Base Layer.Die",.08f);Invoke(nameof(Hide),2.5f);}else Hide();}
}
}

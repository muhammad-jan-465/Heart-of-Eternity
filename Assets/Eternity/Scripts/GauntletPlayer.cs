using UnityEngine;
namespace Eternity {
public class GauntletPlayer : MonoBehaviour {
 public int health=10, maxHealth=10, keys;
 float hurtUntil;
 float airPose;
 public bool TryJump(){
  if(!controller||!controller.enabled||!controller.isGrounded||fall>0)return false;
  fall=7.2f;return true;
 }
 void LateUpdate(){
  if(!bodyAnimator||!bodyAnimator.isHuman||!controller.enabled)return;
  airPose=Mathf.MoveTowards(airPose,controller.isGrounded?0:1,8*Time.deltaTime);
  if(airPose<=0||!bodyAnimator.isActiveAndEnabled)return;
  PoseLeg(HumanBodyBones.LeftUpperLeg,-30);PoseLeg(HumanBodyBones.LeftLowerLeg,52);
  PoseLeg(HumanBodyBones.RightUpperLeg,-16);PoseLeg(HumanBodyBones.RightLowerLeg,32);
 }
 void PoseLeg(HumanBodyBones bone,float angle){
  var t=bodyAnimator.GetBoneTransform(bone);if(t)t.rotation=Quaternion.AngleAxis(angle*airPose,transform.right)*t.rotation;
 }
 public void EndClimb(){fall=0;}
 public float immunity=1.25f, walkSpeed=2.4f, runSpeed=5.5f;
 public Transform eye;
 public CheckpointManager checkpoints;
 public Animator bodyAnimator;
 public GameObject firstPersonWeapon;
 public bool thirdPerson=true;
 CharacterController controller;
 Renderer[] bodyRenderers;
 Camera view;
 Vector3 eyeOrigin;
 float nextHit,fall,pitch,nextShot,flashUntil;
 LineRenderer tracer;
 static readonly int Idle=Animator.StringToHash("Base Layer.Idle"), Walk=Animator.StringToHash("Base Layer.Walk"), Run=Animator.StringToHash("Base Layer.Run"), Shoot=Animator.StringToHash("Base Layer.Shoot");
 int pose; AudioSource gunAudio; AudioClip gunshot; float recoil;
 void Awake(){
  var physicsBody=GetComponent<Rigidbody>();if(physicsBody){physicsBody.useGravity=false;physicsBody.isKinematic=true;}
  controller=GetComponent<CharacterController>(); if(!eye)return;
  view=eye.GetComponent<Camera>();eyeOrigin=eye.localPosition;
  bodyRenderers=bodyAnimator?bodyAnimator.GetComponentsInChildren<Renderer>(true):new Renderer[0];
  tracer=gameObject.AddComponent<LineRenderer>();tracer.enabled=false;tracer.positionCount=2;tracer.widthMultiplier=.018f;
  tracer.sharedMaterial=Resources.Load<Material>("TempleShot");tracer.useWorldSpace=true;
  gunAudio=GetComponent<AudioSource>();if(!gunAudio)gunAudio=gameObject.AddComponent<AudioSource>();gunshot=Resources.Load<AudioClip>("EncounterAudio/RifleSingle");
  if(bodyAnimator)bodyAnimator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
  gameObject.AddComponent<TempleObjectiveGuide>();
  SetView(thirdPerson);
 }
 public void SetView(bool third){
  thirdPerson=third;
  if(bodyRenderers!=null)foreach(var r in bodyRenderers)r.shadowCastingMode=third?UnityEngine.Rendering.ShadowCastingMode.On:UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
  if(firstPersonWeapon)firstPersonWeapon.SetActive(!third);
 }
 void Update(){
  if(!controller||!eye)return;
  bool locked=Cursor.lockState==CursorLockMode.Locked;
  if(Input.GetKeyDown(KeyCode.Escape)){Cursor.lockState=CursorLockMode.None;locked=false;}
  else if(!locked && Input.GetMouseButtonDown(0))Cursor.lockState=CursorLockMode.Locked;
  if(Input.GetKeyDown(KeyCode.V))SetView(!thirdPerson);
  if(locked){
   transform.Rotate(0,Input.GetAxis("Mouse X")*2,0);
   pitch=Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*2,-72,72);
  }
  if(Input.GetKeyDown(KeyCode.Space))TryJump();
  if(controller.enabled)Move(new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")),Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift),Time.deltaTime);
  recoil=Mathf.MoveTowards(recoil,0,Time.deltaTime*5f);if(bodyAnimator&&bodyAnimator.layerCount>1)bodyAnimator.SetLayerWeight(1,recoil);
  PositionCamera();
  if(locked && Input.GetMouseButton(0))Fire();
  if(tracer.enabled && Time.time>=flashUntil)tracer.enabled=false;
  if(transform.position.y < -18 && checkpoints)checkpoints.Respawn(this);
 }
 public void Move(Vector2 input,bool running,float dt){
  var stick=Vector2.ClampMagnitude(input,1);var dir=transform.right*stick.x+transform.forward*stick.y;
  fall=controller.isGrounded && fall<=0 ? -2 : fall-18*dt;
  var contact=controller.Move((dir*(running?runSpeed:walkSpeed)+Vector3.up*fall)*dt);
  if((contact&CollisionFlags.Above)!=0 && fall>0)fall=0;
  Animate(stick.sqrMagnitude<.01f?Idle:running?Run:Walk);
 }
 void Animate(int state){
  if(!bodyAnimator||state==pose)return;
  bodyAnimator.CrossFadeInFixedTime(state,.09f);pose=state;
 }
 void PositionCamera(){
  eye.localRotation=Quaternion.Euler(pitch,0,0);
  var pivot=transform.TransformPoint(eyeOrigin);
  if(!thirdPerson){eye.position=pivot;return;}
  var offset=eye.rotation*new Vector3(.48f,.23f,-2.8f);float dist=offset.magnitude;
  if(Physics.SphereCast(pivot,.17f,offset/dist,out var hit,dist,~(1<<gameObject.layer),QueryTriggerInteraction.Ignore))dist=Mathf.Max(.02f,hit.distance-.045f);
  eye.position=pivot+offset.normalized*dist;
 }
 public bool Fire(){
  if(!view||Time.time<nextShot)return false;
  nextShot=Time.time+.34f;recoil=1;if(gunAudio&&gunshot)gunAudio.PlayOneShot(gunshot,.66f);
  if(bodyAnimator&&bodyAnimator.layerCount>1){bodyAnimator.SetLayerWeight(1,1);bodyAnimator.Play("Rifle recoil.Shoot",1,0); }
  var ray=view.ViewportPointToRay(new Vector3(.5f,.5f,0));
  int mask=~(1<<gameObject.layer);var end=ray.GetPoint(70);
  if(Physics.Raycast(ray,out var sight,70,mask,QueryTriggerInteraction.Ignore))end=sight.point;
  var origin=transform.position+transform.up*1.43f+transform.right*.23f+transform.forward*.36f;
  if(Physics.Linecast(origin,end+(end-origin).normalized*.03f,out var hit,mask,QueryTriggerInteraction.Ignore)){
   end=hit.point;var target=hit.collider.GetComponentInParent<TemplePracticeTarget>();if(target)target.Hit();
   var enemy=hit.collider.GetComponentInParent<TempleBeast>();if(enemy)enemy.Hit();
  }
  tracer.SetPosition(0,origin);tracer.SetPosition(1,end);tracer.enabled=true;flashUntil=Time.time+.055f;
  return true;
 }
 void OnGUI(){
  var previous=GUI.color;
  if(Time.time<hurtUntil){GUI.color=new Color(.75f,.025f,.02f,.16f*Mathf.Clamp01((hurtUntil-Time.time)/.3f));GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);}
  var rect=new Rect(21,Screen.height-46,180,12);
  GUI.color=new Color(.12f,.12f,.15f,.9f);GUI.DrawTexture(rect,Texture2D.whiteTexture);
  rect.width*=Mathf.Clamp01((float)health/maxHealth);GUI.color=Time.time<hurtUntil?Color.white:new Color(.86f,.24f,.22f);GUI.DrawTexture(rect,Texture2D.whiteTexture);
  GUI.color=previous;GUI.Label(new Rect(20,Screen.height-70,190,24),"Health  "+health+" / "+maxHealth);
  if(Cursor.lockState==CursorLockMode.Locked){var r=new Rect(Screen.width/2f-2,Screen.height/2f-2,4,4);GUI.DrawTexture(r,Texture2D.whiteTexture);}}
 public void Hit(int amount){if(Time.time<nextHit)return;nextHit=Time.time+immunity;hurtUntil=Time.time+.3f;health=Mathf.Max(0,health-amount);if(health<=0&&checkpoints)checkpoints.Respawn(this);}
 public void Warp(Vector3 p){var body=GetComponent<Rigidbody>();if(body){body.position=p;if(!body.isKinematic)body.linearVelocity=Vector3.zero;}if(controller)controller.enabled=false;transform.position=p;fall=0;health=maxHealth;if(controller)controller.enabled=true;}
}
}


using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Eternity;
public static class TempleCharacterSetup {
 const string Art="Assets/LowPolySoldiers_demo/", Custom="Assets/Eternity/CharacterArt/", Request="InstallCharacter.request";
 const string Scene="Assets/Eternity/HeartOfEternity_Temple.unity";
 [InitializeOnLoadMethod] static void Schedule(){EditorApplication.delayCall+=InstallRequested;EditorApplication.playModeStateChanged+=TestState;}
 static void InstallRequested(){
  if(!File.Exists(Request)||EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().path!=Scene)return;
  File.Delete(Request);
  try{Install();SessionState.SetBool("Eternity.CharacterTest",true);EditorApplication.EnterPlaymode();}
  catch(Exception e){File.WriteAllText("CharacterCheck.txt",e.ToString());Debug.LogException(e);}
 }
 static Material MaterialAt(string file,string shader,Texture tex,Color color){
  var mat=AssetDatabase.LoadAssetAtPath<Material>(file);
  if(!mat){mat=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(mat,file);}
  mat.shader=Shader.Find(shader);mat.mainTexture=tex;mat.color=color;EditorUtility.SetDirty(mat);return mat;
 }
 static AnimatorController Controller(){
  var path=Custom+"TempleSoldier.controller";
  var ac=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
  if(!ac)ac=AnimatorController.CreateAnimatorControllerAtPath(path);
  var sm=ac.layers[0].stateMachine;
  foreach(var name in new[]{"Idle","Walk","Run","Shoot"}){
   var source=name=="Walk"?"run":name.ToLowerInvariant();
   string f=Art+"animation/demo_combat_"+source+".FBX";
   var clip=AssetDatabase.LoadAllAssetsAtPath(f).OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview"));
   var state=sm.states.Select(s=>s.state).FirstOrDefault(s=>s.name==name)??sm.AddState(name);
   state.motion=clip;state.speed=name=="Walk"?.54f:1;
   if(name=="Idle")sm.defaultState=state;
  }return ac;
 }
 static Bounds BoundsOf(GameObject go){var rs=go.GetComponentsInChildren<Renderer>(true);var bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);return bounds;}
 static Animator Body(Transform parent,AnimatorController ac){
  var visual=new GameObject("Soldier visual");visual.transform.SetParent(parent,false);
  var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"models/Soldier_demo.FBX"),visual.transform,false);
  var bounds=BoundsOf(model);visual.transform.localScale*=1.80f/bounds.size.y;bounds=BoundsOf(model);
  visual.transform.position+=parent.position-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
  var animator=model.GetComponent<Animator>()??model.AddComponent<Animator>();animator.runtimeAnimatorController=ac;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
  foreach(var r in model.GetComponentsInChildren<Renderer>()){
   var mats=r.sharedMaterials;
   for(int i=0;i<mats.Length;i++){string n=mats[i]&&mats[i].name.ToLowerInvariant().Contains("weapon")?"demo_weapon":"demo_soldier_512";mats[i]=AssetDatabase.LoadAssetAtPath<Material>(Art+"models/Materials/"+n+".mat");}r.sharedMaterials=mats;
  }return animator;
 }
 [MenuItem("Eternity/Install playable soldier")]
 public static void Install(){
  if(SceneManager.GetActiveScene().path!=Scene)return;
  var player=UnityEngine.Object.FindFirstObjectByType<GauntletPlayer>();if(!player)throw new Exception("Player missing");
  foreach(var n in new[]{"demo_soldier_512","demo_weapon"})MaterialAt(Art+"models/Materials/"+n+".mat","Standard",AssetDatabase.LoadAssetAtPath<Texture>(Art+"models/Materials/"+n+".tga"),Color.white);
  MaterialAt("Assets/Eternity/Resources/TempleShot.mat","Unlit/Color",null,new Color(1,.72f,.23f));
  var ac=Controller();if(!player.bodyAnimator)player.bodyAnimator=Body(player.transform,ac);
  if(!player.firstPersonWeapon){
   var holder=new GameObject("First person sidearm");holder.transform.SetParent(player.eye,false);
   var gun=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Custom+"Pistol_F.fbx"),holder.transform,false);
   var b=BoundsOf(gun);if(b.size.x>b.size.z)gun.transform.Rotate(0,90,0,Space.World);
   b=BoundsOf(gun);gun.transform.localScale*=.32f/Mathf.Max(b.size.x,b.size.y,b.size.z);b=BoundsOf(gun);
   gun.transform.position+=holder.transform.position-b.center;
   holder.transform.localPosition=new Vector3(.20f,-.18f,.46f);
   var mat=MaterialAt(Custom+"Sidearm.mat","Standard",AssetDatabase.LoadAssetAtPath<Texture>(Custom+"Low Poly Weapon.png"),Color.white);
   foreach(var r in gun.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>mat).ToArray();
   holder.SetActive(false);player.firstPersonWeapon=holder;
  }
  if(!UnityEngine.Object.FindFirstObjectByType<TemplePracticeTarget>()){
   var obj=new GameObject("Practice sentry");var target=obj.AddComponent<TemplePracticeTarget>();var cap=obj.AddComponent<CapsuleCollider>();cap.height=1.8f;cap.radius=.35f;cap.center=Vector3.up*.9f;
   var pos=player.transform.position+new Vector3(3,0,5);
   if(Physics.Raycast(pos+Vector3.up*2,Vector3.down,out var floor,5,~(1<<8),QueryTriggerInteraction.Ignore))pos.y=floor.point.y;
   obj.transform.position=pos;obj.transform.rotation=Quaternion.Euler(0,210,0);
   var a=Body(obj.transform,ac);target.model=a.transform.parent.gameObject;
  }
  EditorUtility.SetDirty(player);EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
  AssetDatabase.SaveAssets();if(!EditorSceneManager.SaveOpenScenes())throw new IOException("Could not save scene");
 }
 static double began;
 static void TestState(PlayModeStateChange state){
  if(!SessionState.GetBool("Eternity.CharacterTest",false))return;
  if(state==PlayModeStateChange.EnteredPlayMode){began=EditorApplication.timeSinceStartup;EditorApplication.update+=Check;}
  if(state==PlayModeStateChange.EnteredEditMode)SessionState.SetBool("Eternity.CharacterTest",false);
 }
 static void Check(){
  if(EditorApplication.timeSinceStartup-began<2)return;EditorApplication.update-=Check;
  try{
   var p=UnityEngine.Object.FindFirstObjectByType<GauntletPlayer>();var t=UnityEngine.Object.FindFirstObjectByType<TemplePracticeTarget>();
   var a=p.bodyAnimator;var checks=new System.Collections.Generic.List<string>();
   foreach(var s in new[]{"Idle","Walk","Run","Shoot"}){int h=Animator.StringToHash("Base Layer."+s);if(!a.HasState(0,h))throw new Exception("Missing "+s);a.Play(h,0,0);a.Update(.12f);if(!a.GetCurrentAnimatorStateInfo(0).IsName(s))throw new Exception("State "+s);checks.Add(s);}
   var loc=p.transform.position;p.Move(Vector2.up,false,.08f);float walk=Vector3.Distance(loc,p.transform.position);loc=p.transform.position;p.Move(Vector2.up,true,.08f);float run=Vector3.Distance(loc,p.transform.position);
   p.SetView(false);if(!p.firstPersonWeapon.activeSelf)throw new Exception("Sidearm hidden");
   p.eye.position=p.transform.position+Vector3.up*1.55f;p.eye.LookAt(t.transform.position+Vector3.up*1.1f);
   if(!p.Fire()||t.health!=2)throw new Exception("Camera shot missed target; health="+t.health);
   p.SetView(true);if(p.firstPersonWeapon.activeSelf||run<=walk||walk<.05f)throw new Exception("Movement/view test failed");
   File.WriteAllText("CharacterCheck.txt","PASS Unity Play Mode: "+string.Join(", ",checks)+"; first and third person; walk="+walk+" m; run="+run+" m; target health="+t.health);
  }catch(Exception e){File.WriteAllText("CharacterCheck.txt",e.ToString());}
  EditorApplication.ExitPlaymode();
 }
}

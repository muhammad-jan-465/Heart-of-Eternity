using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Eternity;
using O=UnityEngine.Object;
[InitializeOnLoad]
public static class TempleEncounterSetup {
 const string Flag="TempleEncounter.request",Test="Eternity.EncounterTest", Log="EncounterCheck.txt";
 const string Scene="Assets/Eternity/HeartOfEternity_Temple.unity", Pack="Assets/SciFi_Space_Soldier/";
 static TempleEncounterSetup(){EditorApplication.delayCall+=Requested;EditorApplication.playModeStateChanged+=State;}
 static void Requested(){if(!File.Exists(Flag)||SceneManager.GetActiveScene().path!=Scene||EditorApplication.isPlayingOrWillChangePlaymode)return;
 File.Delete(Flag);try{Install();SessionState.SetBool(Test,true);EditorApplication.delayCall+=EditorApplication.EnterPlaymode;}catch(Exception e){File.WriteAllText(Log,e.ToString());Debug.LogException(e);}}
 static AnimatorController Controller(){
 string path="Assets/Eternity/CharacterArt/SpaceSoldier.controller";var c=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);if(!c)c=AnimatorController.CreateAnimatorControllerAtPath(path);var sm=c.layers[0].stateMachine;
 foreach(var pair in new[]{("Idle","Anims_Inplace/Soldier@Rifle_Idle"),("Walk","Anims_Inplace/Soldier@Rifle_Walk_F"),("Run","Anims_Inplace/Soldier@Rifle_Run_F"),("Shoot","Anims_Partial/Soldier@Rifle_Aiming_Shoot")}){
 var file=Pack+"Resources/Space_Soldier/Meshes/"+pair.Item2+".FBX";
 var clip=AssetDatabase.LoadAllAssetsAtPath(file).OfType<AnimationClip>().First(a=>!a.name.StartsWith("__preview"));
 var state=sm.states.Select(e=>e.state).FirstOrDefault(e=>e.name==pair.Item1)??sm.AddState(pair.Item1);state.motion=clip;if(pair.Item1=="Idle")sm.defaultState=state;
 }return c;}
 static void Slab(string name,Transform parent,Vector3 offset,Vector3 size,Material m,bool solid){
 var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;var t=g.transform;t.SetParent(parent,true);t.position=parent.position+offset;t.rotation=Quaternion.identity;t.localScale=size;
 g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)O.DestroyImmediate(g.GetComponent<Collider>());else g.layer=11;}
 static void Install(){
 var p=O.FindFirstObjectByType<GauntletPlayer>();var ac=Controller();var guards=O.FindObjectsByType<TemplePursuer>(FindObjectsSortMode.None);
 if(guards.Length==0){
 var practice=O.FindFirstObjectByType<TemplePracticeTarget>();var old=practice.model;
 var positions=new[]{new Vector3(-14,0,-13),new Vector3(12,0,-9),new Vector3(-9,0,6)};
 for(int i=0;i<3;i++){var obj=new GameObject("Temple sentry "+(i+1));var pos=positions[i];if(Physics.Raycast(pos+Vector3.up*2,Vector3.down,out var ground,6,~0,QueryTriggerInteraction.Ignore))pos.y=ground.point.y+.04f;
 obj.transform.position=pos;var cc=obj.AddComponent<CharacterController>();cc.center=Vector3.up*.9f;cc.height=1.8f;cc.radius=.32f;cc.stepOffset=.25f;
 var ai=obj.AddComponent<TemplePursuer>();ai.player=p;ai.delay=.4f*i;var visual=O.Instantiate(old,obj.transform,false);ai.animator=visual.GetComponentInChildren<Animator>();}
 O.DestroyImmediate(practice.gameObject);
 }
 if(!p.bodyAnimator.name.Contains("Space Soldier Player")){
 var imported=AssetDatabase.LoadAssetAtPath<GameObject>(Pack+"Prefabs/Space_Soldier_A.prefab");if(!imported)throw new Exception("Space soldier prefab missing");
 var body=O.Instantiate(imported,p.transform,false);body.name="Space Soldier Player";body.transform.localPosition=Vector3.zero;body.transform.localRotation=Quaternion.identity;
 var anim=body.GetComponentInChildren<Animator>();anim.runtimeAnimatorController=ac;anim.applyRootMotion=false;
 foreach(var col in body.GetComponentsInChildren<Collider>())O.DestroyImmediate(col);
 foreach(var rb in body.GetComponentsInChildren<Rigidbody>())O.DestroyImmediate(rb);
 var rs=body.GetComponentsInChildren<Renderer>();var b=rs.First().bounds;foreach(var r in rs)b.Encapsulate(r.bounds);if(b.size.y>.01f)body.transform.localScale*=1.8f/b.size.y;
 var old=p.transform.Find("Soldier visual");if(old)O.DestroyImmediate(old.gameObject);p.bodyAnimator=anim;
 }
 var playerBody=p.GetComponent<Rigidbody>();if(playerBody){playerBody.useGravity=false;playerBody.isKinematic=true;EditorUtility.SetDirty(playerBody);}p.health=p.maxHealth=10;EditorUtility.SetDirty(p);
 if(!O.FindFirstObjectByType<TempleLadder>()){
 var stone=Resources.Load<Material>("MAT_Sandstone_Block");var bronze=Resources.Load<Material>("MAT_Bronze_Gear");
 foreach(var n in new[]{"Threshold","Sanctuary","Mechanism"}){
 var shaft=GameObject.Find("Shaft_"+n).transform;
 foreach(Transform child in shaft)child.gameObject.SetActive(false);
 if(n!="Threshold"){Slab("Sealed shaft floor",shaft,new Vector3(0,14.52f,0),new Vector3(3.95f,.36f,3.95f),stone,true);continue;}
 var l=shaft.gameObject.AddComponent<TempleLadder>();l.player=p;
 foreach(float z in new[]{-.42f,.42f})Slab("Bronze ladder rail",shaft,new Vector3(.15f,7.5f,z),new Vector3(.09f,15.2f,.09f),bronze,false);
 for(int j=0;j<48;j++)Slab("Ladder rung "+(j+1),shaft,new Vector3(.15f,.22f+j*.31f,0),new Vector3(.075f,.065f,.9f),bronze,false);
 foreach(float y in new[]{0f,14.7f})Slab("Ladder landing",shaft,new Vector3(2.05f,y-.12f,0),new Vector3(2.1f,.24f,1.65f),stone,true);
 }}
 Physics.IgnoreLayerCollision(8,10,false);Physics.IgnoreLayerCollision(8,9,false);
 EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();if(!EditorSceneManager.SaveScene(SceneManager.GetActiveScene()))throw new IOException("Scene save failed");
 }
 static int phase,hp;static float mark;static Vector3 start;static TemplePursuer guard;static GauntletPlayer player;static TempleLadder ladder;static SpikeTrap spike;
 static void State(PlayModeStateChange s){if(!SessionState.GetBool(Test,false))return;if(s==PlayModeStateChange.EnteredPlayMode){File.WriteAllText(Log,"");phase=0;mark=Time.time;EditorApplication.update+=Tick;}
 if(s==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Test,false);File.AppendAllText(Log,"Returned to Edit Mode\n");}}
 static void Note(string text){File.AppendAllText(Log,text+"\n");}
 static void Tick(){if(!EditorApplication.isPlaying||Time.time-mark<.35f)return;try{
 switch(phase){
 case 0: player=O.FindFirstObjectByType<GauntletPlayer>();ladder=O.FindFirstObjectByType<TempleLadder>();var guards=O.FindObjectsByType<TemplePursuer>(FindObjectsSortMode.None);guard=guards[0];
 Note("Guard count="+guards.Length+"; player="+player.bodyAnimator.name+"; avatar="+player.bodyAnimator.avatar+"; health="+player.health);
 foreach(var g in guards.Skip(1))g.gameObject.SetActive(false);
 player.Warp(new Vector3(-2,.08f,-24));guard.GetComponent<CharacterController>().enabled=false;guard.transform.position=player.transform.position+Vector3.forward*9;guard.GetComponent<CharacterController>().enabled=true;start=guard.transform.position;break;
 case 1: if(Time.time-mark<5)return;Note("Guard displacement="+Vector3.Distance(start,guard.transform.position)+"; shots="+guard.shotsFired+"; player health="+player.health);guard.gameObject.SetActive(false);
 var w=O.FindFirstObjectByType<TempleAwakening>();w.awakened=true;w.riseSeconds=.2f;break;
 case 2:player.Warp(ladder.Top);if(!ladder.Use())throw new Exception("Top mount failed");break;
 case 3:if(ladder.occupied)return;Note("Bottom dismount error="+Vector3.Distance(ladder.Bottom,player.transform.position));if(!ladder.Use())throw new Exception("Bottom mount failed");break;
 case 4:if(ladder.occupied)return;Note("Top dismount error="+Vector3.Distance(ladder.Top,player.transform.position));spike=O.FindFirstObjectByType<SpikeTrap>();spike.hold=4;spike.Fire();break;
 case 5:var zone=spike.damage.GetComponent<Collider>();player.Warp(zone.bounds.center-Vector3.up*.85f);hp=player.health;Physics.SyncTransforms();break;
 case 6:if(Time.time-mark<.65f)return;Note("Active spike contact health "+hp+" -> "+player.health);EditorApplication.update-=Tick;EditorApplication.delayCall+=EditorApplication.ExitPlaymode;return;
 }
 phase++;mark=Time.time;
 }catch(Exception e){Note(e.ToString());EditorApplication.update-=Tick;EditorApplication.delayCall+=EditorApplication.ExitPlaymode;}}
}


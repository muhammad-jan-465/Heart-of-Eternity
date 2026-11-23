using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Eternity;

public static class TempleLayoutRepair {
 [Serializable] class Layout {public Item[] items;}
 [Serializable] class Item {public string asset;public float x,y,z;public int segment;}
 const string Folder="Assets/Eternity/";
 const string Request="AlignTemple.request";
 [InitializeOnLoadMethod] static void Start(){EditorApplication.delayCall+=RunRequested;}
 static void RunRequested(){
  if(!File.Exists(Request)||EditorApplication.isPlayingOrWillChangePlaymode)return;
  if(SceneManager.GetActiveScene().path!=Folder+"HeartOfEternity_Temple.unity")return;
  File.Delete(Request);
  try{Repair();}catch(Exception e){File.WriteAllText("AlignmentResult.txt",e.ToString());Debug.LogException(e);}
 }
 [MenuItem("Eternity/Align temple with Blender")]
 public static void Repair(){
  var scene=SceneManager.GetActiveScene();
  if(scene.path!=Folder+"HeartOfEternity_Temple.unity")return;
  var used=new HashSet<Transform>();
  var plan=JsonUtility.FromJson<Layout>(File.ReadAllText(Folder+"Layout.json"));
  foreach(var i in plan.items){
   var parent=GameObject.Find("Seg_"+i.segment.ToString("00")).transform;
   var pos=new Vector3(i.x,i.y,i.z);
   var t=parent.Cast<Transform>().Where(c=>c.name==i.asset&&!used.Contains(c)).OrderBy(c=>(c.position-pos).sqrMagnitude).First();
   used.Add(t);
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"Prefabs/"+i.asset+".prefab");
   t.SetPositionAndRotation(pos,Quaternion.Euler(0,180,0)*prefab.transform.rotation);
   PrefabUtility.RecordPrefabInstancePropertyModifications(t);
  }
  var jewel=used.First(t=>t.name=="Upper_Reliquary").GetComponentsInChildren<Renderer>().First(r=>r.name.StartsWith("Heart of Eternity upper"));
  GameObject.Find("Heart interaction").transform.position=jewel.bounds.center+Vector3.down*.65f;
  var world=UnityEngine.Object.FindFirstObjectByType<TempleAwakening>();
  if(!world){FullTempleScene.Populate();world=UnityEngine.Object.FindFirstObjectByType<TempleAwakening>();}
  foreach(var part in world.parts.Where(p=>used.Contains(p.target))){
    part.raised=part.target.position;part.buried=part.raised+Vector3.down*14;part.first=part.last=part.target.rotation;
  }
  var lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
  string before=string.Join(", ",lights.GroupBy(l=>l.name+" "+l.type).Select(g=>g.Key+" x"+g.Count()));
  foreach(var l in lights){
   if(l.name.StartsWith("Sublevel oil sconce")){
    l.type=LightType.Point;l.range=5.5f;l.intensity=1.2f;l.shadows=LightShadows.None;
    PrefabUtility.RecordPrefabInstancePropertyModifications(l);continue;
   }
   switch(l.name){
    case "Torchlight": l.enabled=!lights.Any(other=>other.name.StartsWith("Sublevel oil sconce") && (other.transform.position-l.transform.position).sqrMagnitude<49);l.type=LightType.Point;l.range=5.5f;l.intensity=1.2f;break;
    case "Heart ruby light":l.type=LightType.Point;l.transform.position=jewel.bounds.center+Vector3.up*.25f;l.range=3.5f;l.intensity=1.4f;break;
    case "Entrance amber light":l.type=LightType.Point;l.range=4.5f;l.intensity=1.1f;l.shadows=LightShadows.Soft;break;
    case "Moonlight":l.intensity=.38f;break;
   }
  }
  RenderSettings.skybox=null;RenderSettings.reflectionIntensity=.22f;
  RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
  RenderSettings.ambientLight=new Color(.13f,.16f,.21f);
  if(SceneView.lastActiveSceneView){var sv=SceneView.lastActiveSceneView;sv.sceneLighting=true;sv.LookAt(new Vector3(0,3,-7),Quaternion.Euler(10,8,0),17);}
  EditorUtility.SetDirty(world);EditorSceneManager.MarkSceneDirty(scene);
  if(!EditorSceneManager.SaveScene(scene))throw new IOException("Save failed");
  TempleInspection.Inspect();
  File.WriteAllText("AlignmentResult.txt","Saved modules="+used.Count+"; initial light types="+before+"; Heart="+jewel.bounds.center);
 }
}

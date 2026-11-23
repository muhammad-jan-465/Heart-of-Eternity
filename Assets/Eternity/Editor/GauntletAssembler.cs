using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Eternity;

public static class GauntletAssembler {
 [Serializable] class Item { public string asset,kind;public float x,y,z;public int segment; }
 [Serializable] class Plan {public Item[] items;}
 const string Folder="Assets/Eternity";
 const string SceneFile=Folder+"/HeartOfEternity_Temple.unity";
 static Transform Find(Transform parent,string prefix){return parent.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name.StartsWith(prefix,StringComparison.Ordinal));}
 static void Layer(string name,int index){var so=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var layers=so.FindProperty("layers");layers.GetArrayElementAtIndex(index).stringValue=name;so.ApplyModifiedProperties();}
 static BoxCollider Box(GameObject g,Bounds b,bool trigger){var c=g.AddComponent<BoxCollider>();c.center=b.center;c.size=b.size;c.isTrigger=trigger;return c;}
 static GameObject Marker(string name,Vector3 p,Vector3 size){var o=new GameObject(name);o.transform.position=p;var box=o.AddComponent<BoxCollider>();box.size=size;box.isTrigger=true;o.layer=10;return o;}
 static TrapBase Behaviour(GameObject o,string n){
  if(n.Contains("SpikeFloor"))return o.AddComponent<SpikeTrap>();
  if(n.Contains("Pendulum"))return o.AddComponent<PendulumTrap>();
  if(n.Contains("FallingBlock"))return o.AddComponent<FallingBlockTrap>();
  if(n.Contains("DartWall"))return o.AddComponent<DartWallTrap>();
  if(n.Contains("RollingBoulder"))return o.AddComponent<RollingBoulderTrap>();
  if(n.Contains("CollapsingFloor"))return o.AddComponent<CollapsingFloor>();
  if(n.Contains("SlidingWall"))return o.AddComponent<SlidingWall>();
  if(n.Contains("GearPlatform"))return o.AddComponent<GearPlatform>();
  if(n.Contains("DoorSlam"))return o.AddComponent<TempleDoorSlam>();
  if(n.Contains("HiddenSwitch"))return o.AddComponent<SwitchMotion>();
  return o.AddComponent<PlateMotion>();
 }
 static GameObject Assemble(string name){
  var source=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/Resources/FBX/"+name+".fbx");if(!source)throw new Exception("Missing "+name);
  var root=(GameObject)PrefabUtility.InstantiatePrefab(source);PrefabUtility.UnpackPrefabInstance(root,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
  root.name=name;
  // Gallery modules carry an FBX light at the modeled flame.
  foreach(var lamp in root.GetComponentsInChildren<Light>(true)){
   if(!lamp.name.StartsWith("Sublevel oil sconce"))continue;
   lamp.type=LightType.Point;lamp.range=5.5f;lamp.intensity=1.2f;lamp.shadows=LightShadows.None;
  }
  foreach(var anim in root.GetComponentsInChildren<Animator>())UnityEngine.Object.DestroyImmediate(anim);
  Transform moving=Find(root.transform,"Moving_");TrapBase trap=null;
  if(moving){trap=Behaviour(root,name);trap.mover=moving.gameObject.AddComponent<Rigidbody>();trap.mover.isKinematic=true;trap.mover.interpolation=RigidbodyInterpolation.Interpolate;moving.gameObject.layer=9;}
  foreach(var mf in root.GetComponentsInChildren<MeshFilter>(true)){
   var obj=mf.gameObject;var id=obj.name;var proxy=id.StartsWith("COL_")||id.StartsWith("TRG_")||id.StartsWith("OCC_");
   if(!proxy){
    var renderer=obj.GetComponent<Renderer>();if(renderer){var materials=renderer.sharedMaterials;for(int i=0;i<materials.Length;i++){if(!materials[i])continue;var nameHint=materials[i].name.ToLowerInvariant();string mapped=nameHint.Contains("qollapse seal")?"MAT_Qollapse_Seal":nameHint.Contains("sandstone")?"MAT_Sandstone_Block":nameHint.Contains("bronze")?"MAT_Bronze_Gear":nameHint.Contains("gold")?"MAT_Gold":null;if(mapped!=null){var shared=Resources.Load<Material>(mapped);if(shared)materials[i]=shared;}}renderer.sharedMaterials=materials;}
    continue;}
   var bounds=mf.sharedMesh.bounds;obj.GetComponent<Renderer>().enabled=false;
   if(id.StartsWith("OCC_")){GameObjectUtility.SetStaticEditorFlags(obj,StaticEditorFlags.OccluderStatic);continue;}
   bool trigger=id.StartsWith("TRG_"),dynamic=id.StartsWith("COL_Moving")||id.StartsWith("TRG_Damage");
   if(dynamic&&moving)obj.transform.SetParent(moving,true);
   obj.layer=trigger?10:dynamic?9:11;
   if(id.StartsWith("COL_Surface_")||id.StartsWith("COL_Step")||id.StartsWith("COL_Ramp")){var surface=obj.AddComponent<MeshCollider>();surface.sharedMesh=mf.sharedMesh;surface.convex=false;continue;}
   Collider collider;
   if(name.Contains("RollingBoulder")&&dynamic){var sphere=obj.AddComponent<SphereCollider>();sphere.center=bounds.center;sphere.radius=bounds.extents.x;sphere.isTrigger=trigger;collider=sphere;}else collider=Box(obj,bounds,trigger);
   if(trap&&id.StartsWith("COL_Moving"))trap.movingCollider=collider;
   if(trap&&id.StartsWith("TRG_Damage")){trap.damage=obj.AddComponent<DamageZone>();if(name.Contains("GearPlatform")||name.Contains("HiddenSwitch")||name.Contains("PressurePlate"))collider.enabled=false;}
   if(trap&&id.StartsWith("TRG_Detect")){var plate=obj.AddComponent<PressurePlateTrigger>();UnityEditor.Events.UnityEventTools.AddPersistentListener(plate.onPressed,trap.Arm);}
  }
  if(trap){
   if(trap is PendulumTrap){trap.repeat=true;trap.duration=2.5f;trap.warning=.1f;trap.hold=0;trap.cooldown=0;}
   if(trap is FallingBlockTrap){trap.warning=.5f;trap.duration=.6f;}
   if(trap is DartWallTrap){trap.warning=.3f;trap.duration=.22f;}
   if(trap is RollingBoulderTrap){trap.warning=.8f;trap.duration=6/7f;}
   if(trap is CollapsingFloor){trap.warning=.7f;trap.duration=.55f;}
   if(trap is GearPlatform){trap.repeat=true;trap.duration=3;var pad=new GameObject("Rider volume");pad.transform.SetParent(moving,false);pad.transform.localPosition=Vector3.up*.65f;var detector=pad.AddComponent<BoxCollider>();detector.size=new Vector3(2.2f,1.1f,2.2f);detector.isTrigger=true;var rider=pad.AddComponent<PlatformRider>();rider.platform=trap.mover;}
   if(trap is RollingBoulderTrap){var levels=moving.GetComponentsInChildren<Renderer>(true).Where(r=>r.name.Contains("_LOD")).OrderBy(r=>r.name).ToArray();if(levels.Length==3){var lod=moving.gameObject.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.55f,new[]{levels[0]}),new LOD(.22f,new[]{levels[1]}),new LOD(.02f,new[]{levels[2]})});lod.RecalculateBounds();}}
   if(trap is PendulumTrap){var blade=Find(moving,"Ceremonial blade");if(blade){var bladeBox=blade.gameObject.AddComponent<BoxCollider>();bladeBox.center=blade.GetComponent<MeshFilter>().sharedMesh.bounds.center;bladeBox.size=blade.GetComponent<MeshFilter>().sharedMesh.bounds.size;bladeBox.isTrigger=true;var bladeDamage=blade.gameObject.AddComponent<DamageZone>();bladeDamage.active=true;}}

  }
  var prefab=PrefabUtility.SaveAsPrefabAsset(root,Folder+"/Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(root);return prefab;
 }
 public static void Build(){
  PrepareMaterials();
  Layer("Player",8);Layer("Trap",9);Layer("TrapTrigger",10);Layer("Static",11);
  Physics.IgnoreLayerCollision(10,10,true);Physics.IgnoreLayerCollision(10,11,true);
  Directory.CreateDirectory(Folder+"/Prefabs");AssetDatabase.Refresh();
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var data=JsonUtility.FromJson<Plan>(File.ReadAllText(Folder+"/Layout.json"));var cache=new Dictionary<string,GameObject>();
  var traps=new List<TrapBase>();var segments=new Dictionary<int,GameObject>();
  foreach(var item in data.items){
   if(!cache.ContainsKey(item.asset))cache[item.asset]=Assemble(item.asset);
   if(!segments.ContainsKey(item.segment))segments[item.segment]=new GameObject("Seg_"+item.segment.ToString("00"));
   var obj=(GameObject)PrefabUtility.InstantiatePrefab(cache[item.asset]);obj.transform.SetParent(segments[item.segment].transform);obj.transform.position=new Vector3(item.x,item.y,item.z);obj.transform.rotation=Quaternion.Euler(0,180,0)*cache[item.asset].transform.rotation;
   var trap=obj.GetComponent<TrapBase>();if(trap){traps.Add(trap);if(trap is FallingBlockTrap&&traps.OfType<FallingBlockTrap>().Count()==2)trap.repeat=true;if(trap is PendulumTrap)trap.startDelay=.42f*(traps.OfType<PendulumTrap>().Count()-1);if(trap is RollingBoulderTrap roller){roller.travel=Mathf.Min(20,28-item.x);roller.duration=roller.travel/7;}}
  }
  var manager=new GameObject("CheckpointManager").AddComponent<CheckpointManager>();manager.last=new Vector3(0,2.85f,-8.65f);
  var p=new GameObject("Player");p.layer=8;p.tag="Player";p.transform.position=manager.last;var cc=p.AddComponent<CharacterController>();cc.center=new Vector3(0,.9f,0);cc.height=1.8f;cc.radius=.5f;cc.stepOffset=.24f;
  var script=p.AddComponent<GauntletPlayer>();script.checkpoints=manager;var eye=new GameObject("Player camera");eye.transform.SetParent(p.transform,false);eye.transform.localPosition=Vector3.up*1.65f;eye.AddComponent<Camera>().nearClipPlane=.07f;eye.AddComponent<AudioListener>();script.eye=eye.transform;
  var start=new GameObject("PlayerStart");start.transform.position=manager.last;
  foreach(var seg in segments){var last=data.items.Last(q=>q.segment==seg.Key&&q.kind=="module");Marker("Checkpoint_"+seg.Key.ToString("00"),new Vector3(last.x,-11.85f,last.z+2),new Vector3(2,.4f,1)).AddComponent<Checkpoint>();}
  var seq=new GameObject("Temple collapse").AddComponent<TempleCollapseSequence>();seq.traps=traps.Where(t=>!(t is TempleDoorSlam)).ToArray();seq.seals=traps.OfType<SlidingWall>().ToArray();
  var switches=traps.OfType<SwitchMotion>().ToArray();for(int i=0;i<switches.Length;i++){var sw=Find(switches[i].transform,"TRG_Detect").gameObject.AddComponent<HiddenWallSwitch>();sw.target=seq.seals[i];}
  var doors=traps.OfType<TempleDoorSlam>().ToArray();
  for(int i=0;i<doors.Length;i++){
   var key=GameObject.CreatePrimitive(PrimitiveType.Cylinder);key.name="Temple key "+(i+1);key.transform.position=new Vector3(i==0?1:8*i,-11,i==0?-10:i*8);key.transform.localScale=new Vector3(.34f,.13f,.34f);key.GetComponent<Collider>().isTrigger=true;key.AddComponent<TempleKey>().index=i;
   var gate=GameObject.CreatePrimitive(PrimitiveType.Cube);gate.name="Key gate "+(i+1);gate.transform.position=new Vector3(16,-10.4f,-11+8*i);gate.transform.localScale=new Vector3(3,.0f+3.2f,.35f);
   var sensor=Marker("Key gate sensor",gate.transform.position,new Vector3(4,3,3));var lockScript=sensor.AddComponent<KeyDoor>();lockScript.keyIndex=i;lockScript.gate=gate;
  }
  var jewel = segments[10].GetComponentsInChildren<MeshRenderer>().First(v => v.name.StartsWith("Heart of Eternity upper"));
  var centre = jewel.bounds.center;
  var heart = Marker("Heart interaction", centre + Vector3.down * .65f, new Vector3(2.5f, 2.4f, 2.5f));
  var pickup = heart.AddComponent<HeartOfEternityPickup>();
  pickup.sequence = seq;
  pickup.visual = jewel.gameObject;
  var glow = new GameObject("Heart ruby light").AddComponent<Light>();
  glow.transform.position = centre + Vector3.up * .25f;
  glow.transform.SetParent(jewel.transform, true);
  glow.color = new Color(1f, .035f, .02f);
  glow.type = LightType.Point;
  glow.range = 3.5f;
  glow.intensity = 2f;
  var panel = segments[10].transform.Find("SecretReliquaryPanel");
  var doorBody = panel.gameObject.AddComponent<Rigidbody>();
  doorBody.isKinematic = true;
  doorBody.interpolation = RigidbodyInterpolation.Interpolate;
  var nearestSwitch = switches.OrderBy(v => (v.transform.position - panel.position).sqrMagnitude).First();
  nearestSwitch.GetComponentInChildren<HiddenWallSwitch>().vaultPanel = doorBody;
  Marker("ExitTrigger",new Vector3(23,6.98f,4.7f),new Vector3(2,2,2)).AddComponent<ExitZone>().sequence=seq;
  var feedback=p.AddComponent<TempleFeedback>();feedback.eye=eye.transform;feedback.source=p.AddComponent<AudioSource>();
  foreach(var d in doors) UnityEditor.Events.UnityEventTools.AddPersistentListener(d.onFire,feedback.Impact);
  var plates=traps.OfType<PlateMotion>().ToArray();var spikes=traps.OfType<SpikeTrap>().ToArray();
  for(int i=0;i<plates.Length;i++){var remote=Find(plates[i].transform,"TRG_Detect").GetComponent<PressurePlateTrigger>();UnityEditor.Events.UnityEventTools.AddPersistentListener(remote.onPressed,spikes[i%spikes.Length].Arm);}
  var glyphs=new GameObject("Three glyph puzzle").AddComponent<GlyphPuzzle>();glyphs.wall=seq.seals[0];
  for(int i=0;i<3;i++){var button=Marker("Glyph "+(i+1),new Vector3(18.3f,-10.9f,-9+i*1.8f),new Vector3(1,1.8f,1));var glyph=button.AddComponent<GlyphButton>();glyph.puzzle=glyphs;glyph.symbol=i;var badge=GameObject.CreatePrimitive(PrimitiveType.Cube);badge.transform.SetParent(button.transform,false);badge.transform.localScale=new Vector3(.12f,.28f*(i+1),.48f);badge.GetComponent<Collider>().enabled=false;}
  UnityEditor.Events.UnityEventTools.AddPersistentListener(seq.onFinalSeal,doors[0].SealPermanently);
  var lamps=data.items.Where(q=>q.kind=="module");foreach(var q in lamps){var lamp=new GameObject("Torchlight").AddComponent<Light>();lamp.transform.position=new Vector3(q.x+2,-8.8f,q.z);lamp.color=new Color(1,.62f,.32f);lamp.type=LightType.Point;lamp.range=5.5f;lamp.intensity=1.2f;lamp.shadows=LightShadows.None;lamp.enabled=!segments[q.segment].GetComponentsInChildren<Light>().Any(l=>l.name.StartsWith("Sublevel oil sconce") && (l.transform.position-lamp.transform.position).sqrMagnitude<49);}
  RenderSettings.ambientLight=new Color(.26f,.22f,.18f);RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
  EditorSceneManager.SaveScene(scene,SceneFile);AssetDatabase.SaveAssets();Debug.Log("GAUNTLET BUILT "+traps.Count+" traps, "+cache.Count+" prefabs");
 }
 [InitializeOnLoadMethod] static void InstallMaterials(){EditorApplication.delayCall+=()=>{if(!EditorApplication.isPlayingOrWillChangePlaymode)PrepareMaterials();};}
 static void PrepareMaterials(){
  Directory.CreateDirectory(Folder+"/Resources");
  foreach(var name in new[]{"MAT_Sandstone_Block","MAT_Bronze_Gear","MAT_Gold","MAT_Qollapse_Seal"}){
   string path=Folder+"/Resources/"+name+".mat";if(AssetDatabase.LoadAssetAtPath<Material>(path))continue;
   var m=new Material(Shader.Find("Standard"));m.name=name;
   foreach(var suffix in new[]{"BaseMap","Normal","MaskMap"}){
    var file=Directory.GetFiles(Folder+"/Textures",name+"_"+suffix+".*").FirstOrDefault(v=>!v.EndsWith(".meta"));if(file==null)continue;file=file.Replace('\\','/');
    var importer=AssetImporter.GetAtPath(file) as TextureImporter;if(importer!=null&&suffix=="Normal"){importer.textureType=TextureImporterType.NormalMap;importer.SaveAndReimport();}
    var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(file);m.SetTexture(suffix=="BaseMap"?"_MainTex":suffix=="Normal"?"_BumpMap":"_MetallicGlossMap",tex);if(suffix=="Normal")m.EnableKeyword("_NORMALMAP");if(suffix=="MaskMap")m.EnableKeyword("_METALLICGLOSSMAP");
   }AssetDatabase.CreateAsset(m,path);
  }
 }
 [MenuItem("Eternity/Rebuild demo scene")] public static void MenuBuild(){Build();}
}


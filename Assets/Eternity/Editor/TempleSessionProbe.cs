using System;
using System.IO;
using System.Linq;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using Eternity;
using O=UnityEngine.Object;
[InitializeOnLoad]
static class TempleSessionProbe{
 static int phase,cursor;static float since;static GauntletPlayer p;static TempleCollapseSequence seq;static Vector2[] path;
 const string Log="TemplePlaytest.txt";
 static TempleSessionProbe(){EditorApplication.delayCall+=Launch;EditorApplication.playModeStateChanged+=State;}
 static void Launch(){if(File.Exists("RouteProbe.request")&&!EditorApplication.isPlayingOrWillChangePlaymode){File.Delete("RouteProbe.request");SessionState.SetBool("Eternity.RouteProbe",true);EditorApplication.EnterPlaymode();}}
 static void State(PlayModeStateChange state){if(!SessionState.GetBool("Eternity.RouteProbe",false))return;if(state==PlayModeStateChange.EnteredPlayMode){phase=0;since=Time.time;EditorApplication.update+=Tick;}if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool("Eternity.RouteProbe",false);File.AppendAllText(Log,"Returned to Edit Mode\n");}}
 static void Note(string s)=>File.AppendAllText(Log,s+"\n");
 static void Go(Vector3 at){p.Warp(at);Physics.SyncTransforms();since=Time.time;}
 static void Tick(){if(!EditorApplication.isPlaying)return;try{
 if(Time.time-since<.35f)return;
 switch(phase){
 case 0:
 p=O.FindFirstObjectByType<GauntletPlayer>();seq=O.FindFirstObjectByType<TempleCollapseSequence>();File.WriteAllText(Log,"");Note("Spawn "+p.transform.position+" grounded="+p.GetComponent<CharacterController>().isGrounded);
 var w=O.FindFirstObjectByType<TempleAwakening>();Note("Altar eggs "+w.sockets.Length+"; buried on start="+!w.awakened);w.awakened=true;w.riseSeconds=.5f;since=Time.time;phase++;break;
 case 1:if(Time.time-since<1)return;
 Note("Raised pose max error "+O.FindFirstObjectByType<TempleAwakening>().parts.Where(x=>x.target&&!x.cover).Max(x=>Vector3.Distance(x.target.position,x.raised)));
 path=File.ReadAllLines("RouteProbe.csv").Select(l=>l.Split(',')).Select(a=>new Vector2(float.Parse(a[0],CultureInfo.InvariantCulture),float.Parse(a[1],CultureInfo.InvariantCulture))).ToArray();
 Go(new Vector3(path[0].x,-11.92f,path[0].y));cursor=0;phase++;break;
 case 2:
 var cc=p.GetComponent<CharacterController>();for(int step=0;step<12&&cursor<path.Length;step++){
 var at=p.transform.position;var goal=path[cursor];var offset=new Vector3(goal.x-at.x,0,goal.y-at.z);
 if(offset.magnitude<.07f){cursor++;continue;}cc.Move(Vector3.ClampMagnitude(offset,.16f)+Vector3.down*.035f);
 if((p.transform.position-at).sqrMagnitude<.000004f||Mathf.Abs(p.transform.position.y+12)>2.5f){Note("Route stopped at waypoint "+cursor+"/"+path.Length+" position="+p.transform.position+" goal="+goal);phase++;break;}}
 if(cursor>=path.Length){Note("Traversed "+cursor+" path points");phase++;}break;
 case 3:var spike=O.FindFirstObjectByType<SpikeTrap>();Note("Bound traps="+O.FindObjectsByType<TrapBase>(FindObjectsSortMode.None).Count(t=>t.mover));spike.Arm();
 var pick=O.FindFirstObjectByType<HeartOfEternityPickup>();Go(pick.GetComponent<Collider>().bounds.center-Vector3.up*.6f);phase++;break;
 case 4:Note("Heart pickup began collapse="+seq.running);var exit=O.FindFirstObjectByType<ExitZone>();Go(exit.GetComponent<Collider>().bounds.center-Vector3.up*.6f);phase++;break;
 case 5:Note("Exit stopped countdown="+!seq.running);Note("Completed targeted Play Mode checks");EditorApplication.update-=Tick;EditorApplication.delayCall+=EditorApplication.ExitPlaymode;break;
 }
 }catch(Exception e){Note(e.ToString());EditorApplication.update-=Tick;EditorApplication.delayCall+=EditorApplication.ExitPlaymode;}}
}

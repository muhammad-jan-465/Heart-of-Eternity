using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Eternity;
[InitializeOnLoad]
public static class GauntletPlayCheck {
 const string Flag="Eternity.Validation";
 static double start;static int step;static Vector3 home;
 static GauntletPlayCheck(){EditorApplication.playModeStateChanged+=State;}
 public static void Run(){
  EditorSceneManager.OpenScene("Assets/Eternity/HeartOfEternity_Temple.unity");
  SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();
 }
 static void State(PlayModeStateChange state){
  if(!SessionState.GetBool(Flag,false))return;
  if(state==PlayModeStateChange.EnteredPlayMode){step=0;start=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;}
  if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Flag,false);EditorApplication.Exit(SessionState.GetInt("Eternity.Errors",1));}
 }
 static void Tick(){
  if(!EditorApplication.isPlaying)return;
  try{
   var passed=EditorApplication.timeSinceStartup-start;
   if(passed<.5)return;
   var trap=UnityEngine.Object.FindFirstObjectByType<SpikeTrap>();var player=UnityEngine.Object.FindFirstObjectByType<GauntletPlayer>();
   if(!trap||!player||!trap.mover||!trap.damage)throw new Exception("Incomplete spike/player bindings");
   if(step==0){
    var all=UnityEngine.Object.FindObjectsByType<TrapBase>(FindObjectsSortMode.None);
    if(all.Length<33||all.Any(x=>!x.mover||!x.mover.isKinematic))throw new Exception("Missing trap/kinematic bodies");
    home=trap.mover.position;player.Warp(trap.transform.TransformPoint(new Vector3(0,.1f,-1.45f)));trap.Arm();step++;start=EditorApplication.timeSinceStartup;
   }else if(step==1&&passed>1){
    if((trap.mover.position-home).sqrMagnitude<.002f)throw new Exception("Spike did not move");
    player.Warp(new Vector3(0,-11.9f,-10));step++;start=EditorApplication.timeSinceStartup;
   }else if(step==2&&passed>4){
    if((trap.mover.position-home).sqrMagnitude>.0025f)throw new Exception("Spike failed to reset");
    if(player.transform.position.y < -12.4f)throw new Exception("Unsupported player spawn");
    Finish(0,"PLAY CHECK PASSED: 33 bound movers, spike travel and reset, player spawn.");
   }
  }catch(Exception e){Finish(1,e.ToString());}
 }
 static void Finish(int code,string report){Debug.Log(report);System.IO.File.WriteAllText("PlayCheck.txt",report);SessionState.SetInt("Eternity.Errors",code);EditorApplication.update-=Tick;EditorApplication.ExitPlaymode();}
}

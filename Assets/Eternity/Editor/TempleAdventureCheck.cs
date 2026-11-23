using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Eternity;
using O=UnityEngine.Object;
[InitializeOnLoad]
public static class TempleAdventureCheck {
 const string Flag="AdventureTest.request", Report="AdventurePlaytest.txt", Key="Eternity.AdventureTest";
 static TempleAdventureCheck(){EditorApplication.update+=Tick;}
 static GauntletPlayer player;static TempleLadder ladder;static TempleAwakening altar;static TempleBeast[] guardians;
 static int phase,swings;static float previousAttackFrame;static float since,y,peak;
 static void Log(string text){File.AppendAllText(Report,text+"\n");}
 static void Require(bool condition,string label){if(!condition)throw new Exception(label);Log(label+" passed");}
 static void Tick(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
  if(!SessionState.GetBool(Key,false)){
   if(File.Exists("EndingTest.request")&&!EditorApplication.isPlayingOrWillChangePlaymode && SceneManager.GetActiveScene().path=="Assets/Eternity/HeartOfEternity_Temple.unity"){
    string requestedEnding=File.ReadAllText("EndingTest.request").Trim();
    File.Delete("EndingTest.request");SessionState.SetString("Eternity.EndingCheck",requestedEnding);
    SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();return;
   }
   if(File.Exists("AudioTest.request")&&!EditorApplication.isPlayingOrWillChangePlaymode && SceneManager.GetActiveScene().path=="Assets/Eternity/HeartOfEternity_Temple.unity"){File.Delete("AudioTest.request");SessionState.SetBool("Eternity.AudioCheck",true);SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();return;}
   if(!File.Exists(Flag)||File.Exists("Guardians.request")||EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().path!="Assets/Eternity/HeartOfEternity_Temple.unity")return;
   File.Delete(Flag);File.WriteAllText(Report,"Play Mode encounter check\n");SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();return;
  }
  if(!EditorApplication.isPlaying)return;Application.runInBackground=true;EditorApplication.QueuePlayerLoopUpdate();if(EditorApplication.isPaused||Time.time<.5f)return;
  string ending=SessionState.GetString("Eternity.EndingCheck","");
  if(ending!=""){
   try{
    var temple=O.FindFirstObjectByType<TempleAwakening>();
    var sequence=O.FindFirstObjectByType<TempleCollapseSequence>();
    var sound=temple.transform.Find("Temple stone movement").GetComponent<AudioSource>();
    bool win=ending=="Win";
    if(phase==0){
     temple.awakened=true;temple.riseSeconds=.2f;temple.sinkSeconds=2;
     temple.player.Warp(O.FindFirstObjectByType<TempleLadder>().Bottom);
     phase++;since=Time.time;return;
    }
    if(phase==1){
     if(Time.time-since<.5f)return;
     if(!temple.parts.Any(p=>p.target && Vector3.Distance(p.target.position,p.buried)>1))throw new Exception("Temple did not rise");
     temple.player.Warp(O.FindFirstObjectByType<HeartOfEternityPickup>().transform.position + Vector3.right*2f);
     temple.player.enabled=false;
     sequence.seconds=.25f;sequence.Begin();if(win)sequence.Escape();
     phase++;since=Time.time;return;
    }
    if(phase==2){
     if(Time.time-since<.65f)return;
     if(!sound.isPlaying||!sound.clip||!temple.parts.Any(p=>p.target && Vector3.Distance(p.target.position,p.buried)>.1f && Vector3.Distance(p.target.position,p.raised)>.1f))throw new Exception("Missing burial motion or sound");
     phase++;return;
    }
    if(Time.time-since<3f)return;
    if(sound.isPlaying||sequence.running||sequence.escaped!=win||sequence.sealedTemple==win||temple.parts.Any(p=>p.target&&Vector3.Distance(p.target.position,p.buried)>.015f))throw new Exception($"Incorrect terminal pose, audio or outcome: audio={sound.isPlaying}, running={sequence.running}, escaped={sequence.escaped}, sealed={sequence.sealedTemple}, feet={temple.player.transform.position}, largest offset={temple.parts.Where(p=>p.target).Max(p=>Vector3.Distance(p.target.position,p.buried))}");
    File.WriteAllText("Ending"+ending+"Check.txt","PASS: burial motion, sound, underground pose and final result.");
   }catch(Exception e){File.WriteAllText("Ending"+ending+"Check.txt","FAILED "+e.Message);}
   SessionState.SetString("Eternity.EndingCheck","");SessionState.SetBool(Key,false);EditorApplication.ExitPlaymode();return;
  }
  if(SessionState.GetBool("Eternity.AudioCheck",false)){
   try{
    var actor=O.FindFirstObjectByType<GauntletPlayer>();
    var world=O.FindFirstObjectByType<TempleAwakening>();
    var movement=world.transform.Find("Temple stone movement").GetComponent<AudioSource>();
    if(phase==0){
     if(!actor.Fire()||!actor.GetComponents<AudioSource>().Any(a=>a.isPlaying))throw new Exception("Rifle playback");
     var beasts=O.FindObjectsByType<TempleBeast>(FindObjectsInactive.Include,FindObjectsSortMode.None);
     foreach(var b in beasts){if(!TempleBeast.EncounterVoice(b.egg))throw new Exception(b.name+" missing clip");}
     foreach(var b in beasts.Where(b=>b.egg>=0)){b.Hit();if(!b.voice||!b.GetComponent<AudioSource>().isPlaying)throw new Exception(b.name+" playback");}
     var ladder=O.FindFirstObjectByType<TempleLadder>();ladder.undergroundZombies.SetActive(true);
     world.awakened=true;world.riseSeconds=3f;phase=1;return;
    }
    var zombie=O.FindObjectsByType<TempleBeast>(FindObjectsSortMode.None).First(b=>b.egg==-1);
    if(!zombie.voice)return;
    zombie.Hit();
    if(!zombie.GetComponent<AudioSource>().isPlaying||!movement.clip||!movement.isPlaying)throw new Exception("Zombie or temple playback");
    world.BeginBurial();
    if(!movement.isPlaying)throw new Exception("Burial playback");
    File.WriteAllText("EncounterAudioCheck.txt","PASS: rifle, three guardian voices, underground zombie, temple rise and burial.");
   }catch(Exception e){File.WriteAllText("EncounterAudioCheck.txt","FAILED "+e.Message);}
   SessionState.SetBool("Eternity.AudioCheck",false);SessionState.SetBool(Key,false);EditorApplication.ExitPlaymode();return;
  }
  try{
   if(phase==0){
    player=O.FindFirstObjectByType<GauntletPlayer>();ladder=O.FindFirstObjectByType<TempleLadder>();altar=O.FindFirstObjectByType<TempleAwakening>();
    guardians=O.FindObjectsByType<TempleBeast>(FindObjectsSortMode.None).Where(x=>x.egg>=0).OrderBy(x=>x.egg).ToArray();
    Require(guardians.Length==3&&guardians.All(x=>x.animator&&new[]{"Idle","Move","Attack","Die"}.All(n=>x.animator.HasState(0,Animator.StringToHash("Base Layer."+n)))), "Three animated guardian rigs");
    Require(player.GetComponent<TempleObjectiveGuide>()!=null,"Objective guide");
    player.Warp(new Vector3(-2,.15f,-24));since=Time.time;phase++;return;
   }
   if(phase==1){if(Time.time-since<.75f)return;y=player.transform.position.y;Require(player.TryJump(),"Grounded jump");peak=y;phase++;since=Time.time;return;}
   if(phase==2){peak=Mathf.Max(peak,player.transform.position.y);if(Time.time-since<1.4f)return;Require(peak-y>.6f&&player.GetComponent<CharacterController>().isGrounded,"Jump and landing");
    var hunter=guardians[0];foreach(var other in guardians.Skip(1))other.enabled=false;
    var motor=hunter.GetComponent<CharacterController>();motor.enabled=false;hunter.transform.position=player.transform.position+Vector3.forward*1.2f;motor.enabled=true;
    previousAttackFrame=-1;swings=0;phase=20;since=Time.time;return;}
   if(phase==20){
    var frame=guardians[0].animator.GetCurrentAnimatorStateInfo(0);
    if(frame.IsName("Base Layer.Attack")){
     if(previousAttackFrame<0||frame.normalizedTime+.03f<previousAttackFrame)swings++;
     previousAttackFrame=frame.normalizedTime;
    }else previousAttackFrame=-1;
    if(Time.time-since<4.4f)return;
    Require(swings>=2 && player.health<player.maxHealth,"Repeated guardian attack animation and damage");
    foreach(var g in guardians){var pos=g.transform.position;int hp=g.health;for(int i=0;i<hp;i++)g.Hit();player.Warp(pos);altar.Interact();player.Warp(altar.sockets[g.egg].position);altar.Interact();}
    Require(altar.awakened,"Three egg drops, collection and altar placements");altar.riseSeconds=.15f;phase=3;since=Time.time;return;}
   if(phase==3){if(Time.time-since<.6f)return;player.Warp(ladder.Top);Require(ladder.Use(),"Ladder descent");phase++;since=Time.time;return;}
   if(phase==4){if(ladder.occupied&&Time.time-since<12)return;Require(!ladder.occupied&&ladder.undergroundZombies.activeInHierarchy&&ladder.undergroundZombies.GetComponentsInChildren<TempleBeast>().Count(b=>b.animator&&b.animator.HasState(0,Animator.StringToHash("Base Layer.Attack")))==3,"Three animated underground zombies");
    var heart=O.FindFirstObjectByType<HeartOfEternityPickup>();heart.sequence.seconds=8;altar.sinkSeconds=.6f;Log("Heart at "+heart.transform.position+"; visual "+heart.visual.name);player.Warp(heart.transform.position);phase++;since=Time.time;return;}
   if(phase==5){if(Time.time-since<.4f)return;Require(O.FindFirstObjectByType<TempleCollapseSequence>().running,"Heart pickup starts escape");phase++;since=Time.time;return;}
   if(phase==6){
    player.Warp(ladder.Bottom);Require(ladder.Use(),"Return ladder mount");phase++;since=Time.time;return;}
   if(phase==7){
    if(ladder.occupied&&Time.time-since<12)return;
    var collapse=O.FindFirstObjectByType<TempleCollapseSequence>();Require(collapse.escaped&&!collapse.running,"Return ladder awards victory");
    phase++;since=Time.time;return;}
   if(phase==8){if(Time.time-since<8.2f)return;
    var collapse=O.FindFirstObjectByType<TempleCollapseSequence>();Require(collapse.escaped&&!collapse.sealedTemple,"Victory survives original deadline");
    collapse.escaped=false;collapse.seconds=2;player.Warp(new Vector3(-2,.08f,-24));collapse.Begin();phase++;since=Time.time;return;}
   if(phase==9){if(Time.time-since<.6f)return;
    var collapse=O.FindFirstObjectByType<TempleCollapseSequence>();Require(collapse.escaped,"Jungle clearing awards victory");
    var exit=O.FindFirstObjectByType<ExitZone>();collapse.escaped=false;player.Warp(exit.transform.position);collapse.Begin();phase++;since=Time.time;return;}
   if(phase==10){if(Time.time-since<.5f)return;
    var collapse=O.FindFirstObjectByType<TempleCollapseSequence>();Require(collapse.escaped,"Chasm exit awards victory");collapse.escaped=false;player.Warp(ladder.Bottom);collapse.Begin();phase++;since=Time.time;return;}
   if(phase==11){if(Time.time-since<3.5f)return;
    var collapse=O.FindFirstObjectByType<TempleCollapseSequence>();collapse.Escape();
    Require(collapse.sealedTemple&&!collapse.escaped&&!player.enabled,"Timeout produces terminal loss");
    Require(altar.parts.Where(p=>p.target).All(p=>Vector3.Distance(p.target.position,p.buried)<.015f),"Temple returns to buried pose");
    Log("COMPLETE");}
  }catch(Exception e){Log("FAILED "+e.Message);}
  SessionState.SetBool(Key,false);EditorApplication.ExitPlaymode();
 }
}

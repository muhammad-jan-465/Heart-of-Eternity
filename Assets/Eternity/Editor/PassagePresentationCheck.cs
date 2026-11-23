using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Eternity;

[InitializeOnLoad]
static class PassagePresentationCheck {
    const string Key="Eternity.PassagePresentationCheck";
    const string Log="Logs/PassagePresentationCheck.txt";
    static int step;
    static double mark;
    static float pausedTime;
    static PassagePresentationCheck(){EditorApplication.update+=Tick;}
    static void Check(bool condition,string message){
        if(!condition)throw new Exception(message);
        File.AppendAllText(Log,"PASS "+message+"\n");
    }
    static void Tick(){
        if(!SessionState.GetBool(Key,false)){
            if(!File.Exists("PassageCheck.request")||EditorApplication.isPlayingOrWillChangePlaymode)return;
            File.Delete("PassageCheck.request");
            File.WriteAllText(Log,"");
            SessionState.SetBool(Key,true);
            EditorApplication.EnterPlaymode();return;
        }
        if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup-mark<.6)return;
        var player=UnityEngine.Object.FindFirstObjectByType<GauntletPlayer>();
        if(!player)return;
        var menu=player.GetComponent<ExpeditionMenu>();
        var collapse=UnityEngine.Object.FindFirstObjectByType<TempleCollapseSequence>();
        try{
            switch(step++){
            case 0:
                Check(menu && ExpeditionMenu.Visible && Time.timeScale==0,"Title menu pauses the scene");
                Check(Resources.Load<Texture2D>("Menu/TempleAtNight"),"Menu artwork loads");
                menu.SendMessage("Choose",(object)0);
                Check(!ExpeditionMenu.Visible && Time.timeScale==1,"Start enters gameplay");
                for(int i=1;i<=3;i++) Check(Resources.Load<AudioClip>("EncounterAudio/Footfall"+i),"Footfall "+i+" loads");
                Check(Resources.Load<AudioClip>("EncounterAudio/PebbleScatter"),"Stone sound loads");
                var altar=UnityEngine.Object.FindFirstObjectByType<TempleAwakening>();
                altar.riseSeconds=.1f;altar.awakened=true;
                break;
            case 1:
                var heart=UnityEngine.Object.FindFirstObjectByType<HeartOfEternityPickup>();
                player.Warp(heart.GetComponent<Collider>().bounds.center-Vector3.up*.6f);
                Physics.SyncTransforms();break;
            case 2:
                Check(collapse.running,"Collecting the Heart starts the seal timer");
                Check(player.transform.Find("Tunnel pebble shower").GetComponent<ParticleSystem>().isPlaying,"Underground stone shower active");
                menu.SendMessage("SetMenu",true);pausedTime=collapse.Remaining;break;
            case 3:
                Check(collapse.Remaining==pausedTime && AudioListener.pause,"Pause freezes countdown and audio");
                menu.SendMessage("Choose",1);
                Check(!AudioListener.pause && !ExpeditionMenu.Visible,"Resume restores gameplay");
                break;
            case 4:
                Check(collapse.Remaining<pausedTime,"Countdown resumes");
                menu.SendMessage("SetMenu",true);menu.SendMessage("Choose",2);break;
            case 5:
                Check(!collapse.running && !ExpeditionMenu.Visible && Time.timeScale==1,"Restart clears collapse and starts a fresh run");
                Stop();break;
            }
            mark=EditorApplication.timeSinceStartup;
        }catch(Exception error){File.AppendAllText(Log,error.ToString());Stop();}
    }
    static void Stop(){SessionState.SetBool(Key,false);EditorApplication.ExitPlaymode();}
}


using System;
using System.IO;
using System.Linq;
using Eternity;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using O=UnityEngine.Object;

[InitializeOnLoad]
static class GuardianRunCheck {
    static GuardianRunCheck(){EditorApplication.update+=CheckRequest;}
    static void CheckRequest(){
        const string flag="GuardianRun.request";
        if(!File.Exists(flag)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
        File.Delete(flag);
        using(var log=new StreamWriter("GuardianRunCheck.txt")){
            try{
                var guards=O.FindObjectsByType<TempleBeast>(FindObjectsSortMode.None).Where(b=>b.egg>=0).ToArray();
                if(guards.Length!=3)throw new Exception("Open the temple scene to check the three guardians.");
                foreach(var beast in guards)Check(beast,log);
            }catch(Exception e){log.WriteLine(e);}
        }
    }
    static void Check(TempleBeast beast,TextWriter log){
        var controller=(AnimatorController)beast.animator.runtimeAnimatorController;
        var move=controller.layers[0].stateMachine.states.First(s=>s.state.name=="Move").state;
        string path=AssetDatabase.GetAssetPath(move.motion);
        var importer=(ModelImporter)AssetImporter.GetAtPath(path);
        var clips=importer.clipAnimations;
        if(clips.Length==0)clips=importer.defaultClipAnimations;
        string clipName=move.motion.name;
        var take=clips.Single(c=>c.name==clipName);
        log.WriteLine(beast.name+" loop before repair = "+take.loopTime);
        if(!take.loopTime){take.loopTime=true;importer.clipAnimations=clips;importer.SaveAndReimport();}
        var scene=EditorSceneManager.NewPreviewScene();
        try{
            var clone=(GameObject)O.Instantiate(beast.animator.gameObject,scene);
            var anim=clone.GetComponent<Animator>();
            anim.cullingMode=AnimatorCullingMode.AlwaysAnimate;anim.Rebind();
            anim.Play("Base Layer.Move",0,0);anim.Update(0f);
            var clip=anim.GetCurrentAnimatorClipInfo(0).First().clip;
            var bones=clone.GetComponentsInChildren<SkinnedMeshRenderer>().SelectMany(r=>r.bones).Where(t=>t).Distinct().ToArray();
            anim.Update(clip.length*1.1f);
            var first=bones.Select(b=>b.localRotation).ToArray();
            anim.Update(clip.length*.27f);
            float angle=bones.Select((b,i)=>Quaternion.Angle(first[i],b.localRotation)).Max();
            if(!clip.isLooping||angle<1f)throw new Exception("Run pose failed to advance after first cycle: "+beast.name);
            anim.Update(clip.length);var state=anim.GetCurrentAnimatorStateInfo(0);
            log.WriteLine("PASS "+clip.name+" cycles="+state.normalizedTime+" bone motion after first loop="+angle+" degrees");
        }finally{EditorSceneManager.ClosePreviewScene(scene);}
    }
}

using System;
using System.IO;
using System.Linq;
using Eternity;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using O = UnityEngine.Object;

[InitializeOnLoad]
public static class GuardianEncounterSetup {
    const string Folder="Assets/Eternity/CharacterArt/";
    static readonly string[] Species={"Lizard","Drakonit","Wolf"};
    static GuardianEncounterSetup(){EditorApplication.update+=Check;}
    static void Check(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists("Guardians.request")||SceneManager.GetActiveScene().path!="Assets/Eternity/HeartOfEternity_Temple.unity")return;
        File.Delete("Guardians.request");
        try{Install();}catch(Exception e){File.AppendAllText("GuardianCheck.txt",e.ToString());Debug.LogException(e);}
    }
    static void Recoil(GauntletPlayer player){
        var controller=(AnimatorController)player.bodyAnimator.runtimeAnimatorController;
        if(controller.layers.Length==1)controller.AddLayer("Rifle recoil");
        var layers=controller.layers;var layer=layers.First(l=>l.name=="Rifle recoil");
        layer.avatarMask=AssetDatabase.LoadAssetAtPath<AvatarMask>("Assets/SciFi_Space_Soldier/Resources/Space_Soldier/Controller&Masks/Arms.mask");
        if(!layer.avatarMask)throw new Exception("Missing recoil mask");
        var sm=layer.stateMachine;var state=sm.states.Select(x=>x.state).FirstOrDefault(x=>x.name=="Shoot")??sm.AddState("Shoot");
        state.motion=layers[0].stateMachine.states.First(x=>x.state.name=="Shoot").state.motion;
        sm.defaultState=state;controller.layers=layers;player.bodyAnimator.applyRootMotion=false;EditorUtility.SetDirty(controller);
    }
    static AnimatorController ZombieController(){
        string path=Folder+"UndergroundZombie.controller";
        var c=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if(c)return c;
        c=AnimatorController.CreateAnimatorControllerAtPath(path);var sm=c.layers[0].stateMachine;
        foreach(var pair in new[]{("Idle","Z_Idle"),("Move","Z_Run_InPlace"),("Attack","Z_Attack")}){
            var state=sm.AddState(pair.Item1);state.motion=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Zombie/Animations/"+pair.Item2+".anim");
            if(pair.Item1=="Idle")sm.defaultState=state;
        }return c;
    }
    static void Body(TempleBeast b,GameObject asset,float height,bool zombie){
        var go=O.Instantiate(asset,b.transform,false);go.name=asset.name;go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.identity;
        foreach(var col in go.GetComponentsInChildren<Collider>())O.DestroyImmediate(col);
        foreach(var rb in go.GetComponentsInChildren<Rigidbody>())O.DestroyImmediate(rb);
        var renderers=go.GetComponentsInChildren<Renderer>().Where(r=>r is MeshRenderer||r is SkinnedMeshRenderer).ToArray();
        if(renderers.Length==0)throw new Exception("Missing creature mesh "+asset.name);
        Bounds bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        go.transform.localScale*=height/Mathf.Max(.01f,bounds.size.y);
        bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);go.transform.position+=Vector3.up*(b.transform.position.y-bounds.min.y);
        b.animator=go.GetComponentInChildren<Animator>();
        if(b.animator){b.animator.applyRootMotion=false;if(zombie)b.animator.runtimeAnimatorController=ZombieController();}
    }
    static TempleBeast Actor(GameObject root,GauntletPlayer p,TempleAwakening altar,int id){
        var cc=root.GetComponent<CharacterController>();if(!cc)cc=root.AddComponent<CharacterController>();
        cc.height=1.8f;cc.center=Vector3.up*.9f;cc.radius=.43f;cc.stepOffset=.25f;
        var ai=root.GetComponent<TempleBeast>();if(!ai)ai=root.AddComponent<TempleBeast>();ai.player=p;ai.altar=altar;ai.egg=id;ai.voice=TempleBeast.EncounterVoice(id);return ai;
    }
    static readonly string[] Prefabs={
        "Assets/Hatogame_new/Lizard/Perfabs/Lizard_A.prefab",
        "Assets/Drakonit_monster/prefab/Drakonit_monster_lod_0 (texture-1K).prefab",
        "Assets/Hatogame_new/BossMonsterPack1/Wolfboss/Perfabs/Wolfboss_A.prefab"};
    static AnimatorController CreatureController(int id){
        string[] prefixes={"Assets/Hatogame_new/Lizard/Animation/Lizard@","Assets/Drakonit_monster/animation/Drakonit_monster@","Assets/Hatogame_new/BossMonsterPack1/Wolfboss/Animation/Wolfboss@"};
        string path=Folder+Species[id]+"Encounter.controller";
        var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if(!controller)controller=AnimatorController.CreateAnimatorControllerAtPath(path);
        var sm=controller.layers[0].stateMachine;
        foreach(var pair in new[]{("Idle","idle"),("Move","run"),("Attack",id==1?"Atak_1":"attack1"),("Die",id==1?"dead":"die")}){
            string file=prefixes[id]+pair.Item2+(id==1?".fbx":".FBX");
            var motion=AssetDatabase.LoadAllAssetsAtPath(file).OfType<AnimationClip>().First(c=>!c.name.StartsWith("__preview"));
            var state=sm.states.Select(x=>x.state).FirstOrDefault(x=>x.name==pair.Item1);
            if(!state)state=sm.AddState(pair.Item1);
            state.motion=motion;if(pair.Item1=="Idle")sm.defaultState=state;
        }
        return controller;
    }
    [MenuItem("Eternity/Apply guardian encounters")]
    public static void Install(){
        var p=O.FindFirstObjectByType<GauntletPlayer>();var altar=O.FindFirstObjectByType<TempleAwakening>();var ladder=O.FindFirstObjectByType<TempleLadder>();
        Recoil(p);var stone=Resources.Load<Material>("MAT_Sandstone_Block");int stones=0;
        foreach(var r in O.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(r.name.StartsWith("Facade ashlar",StringComparison.OrdinalIgnoreCase)){r.sharedMaterial=stone;stones++;}
        File.WriteAllText("GuardianCheck.txt","Facade sandstone renderers "+stones+"; recoil mask installed\n");
        var guards=O.FindObjectsByType<TemplePursuer>(FindObjectsSortMode.None).OrderBy(x=>x.name).ToArray();
        for(int id=0;id<3;id++){
            var existing=O.FindObjectsByType<TempleBeast>(FindObjectsSortMode.None).FirstOrDefault(x=>x.egg==id);
            var old=guards.FirstOrDefault(x=>x && x.name=="Temple sentry "+(id+1));
            var root=existing?existing.gameObject:old?old.gameObject:new GameObject();
            if(!existing&&!old)root.transform.position=new[]{new Vector3(-14,.1f,-13),new Vector3(12,.1f,-9),new Vector3(-9,.1f,6)}[id];
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs[id]);if(!prefab)throw new Exception(Prefabs[id]);
            var controller=CreatureController(id);
            if(old)O.DestroyImmediate(old);
            while(root.transform.childCount>0)O.DestroyImmediate(root.transform.GetChild(0).gameObject);
            root.name=Species[id]+" guardian";var ai=Actor(root,p,altar,id);Body(ai,prefab,new[]{1.65f,2.25f,2f}[id],false);
            ai.animator.runtimeAnimatorController=controller;
            File.AppendAllText("GuardianCheck.txt",root.name+" = "+prefab.name+"; animation states "+controller.layers[0].stateMachine.states.Length+"\n");
        }
        if(ladder.undergroundZombies && ladder.undergroundZombies.GetComponentsInChildren<TempleBeast>(true).Length!=3)O.DestroyImmediate(ladder.undergroundZombies); if(!ladder.undergroundZombies){
            var group=new GameObject("Underground zombies");ladder.undergroundZombies=group;
            for(int i=0;i<3;i++){var seg=GameObject.Find("Seg_0"+(i+2));var root=new GameObject("Crypt zombie "+(i+1));root.transform.SetParent(group.transform);root.transform.position=new[]{new Vector3(0,-11.9f,5),new Vector3(0,-11.9f,21),new Vector3(12,-11.9f,32)}[i];
            var ai=Actor(root,p,altar,-1);Body(ai,AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Zombie/Prefabs/Zombie"+(i+1)+".prefab"),1.75f,true);}
            group.SetActive(false);
        }
        var cryptPositions=new[]{new Vector3(0,-11.9f,5),new Vector3(0,-11.9f,21),new Vector3(12,-11.9f,32)};
        for(int i=0;i<cryptPositions.Length;i++)ladder.undergroundZombies.transform.Find("Crypt zombie "+(i+1)).position=cryptPositions[i];
        var pickup=O.FindFirstObjectByType<HeartOfEternityPickup>();File.AppendAllText("GuardianCheck.txt","Heart "+pickup.visual.name+" at "+pickup.transform.position+"\n");
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());if(!EditorSceneManager.SaveScene(SceneManager.GetActiveScene()))throw new IOException("Could not save scene");
        File.AppendAllText("GuardianCheck.txt","Saved scene; zombie group contains "+ladder.undergroundZombies.transform.childCount+"; sockets "+altar.sockets.Length+"\n");
    }
}

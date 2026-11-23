using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using Eternity;

[InitializeOnLoad]
public static class TempleProjectSetup {
    const string PathToScene="Assets/Eternity/HeartOfEternity_Temple.unity";
    public static void AssembleFullScene() {
        if(!Object.FindFirstObjectByType<GauntletPlayer>()) GauntletAssembler.Build();
        if(!Object.FindFirstObjectByType<TempleAwakening>()) FullTempleScene.Populate();
    }
    static TempleProjectSetup(){EditorApplication.delayCall+=Start;EditorApplication.playModeStateChanged+=State;}
    static void Start(){
        if(!File.Exists("PrepareFullTemple.request") || EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating){EditorApplication.delayCall+=Start;return;}
        File.Delete("PrepareFullTemple.request");
        try {
            var scene=SceneManager.GetActiveScene();
            if(scene.path!=PathToScene && !scene.isDirty)EditorSceneManager.OpenScene(PathToScene);
            AssembleFullScene();
            SessionState.SetBool("Eternity.FullTest",true);
            EditorApplication.EnterPlaymode();
        } catch(System.Exception ex){Debug.LogException(ex);File.WriteAllText("FullTempleCheck.txt",ex.ToString());}
    }
    static double began;
    static void State(PlayModeStateChange change){
        if(!SessionState.GetBool("Eternity.FullTest",false))return;
        if(change==PlayModeStateChange.EnteredPlayMode){began=EditorApplication.timeSinceStartup;EditorApplication.update+=Check;}
        if(change==PlayModeStateChange.EnteredEditMode){SessionState.SetBool("Eternity.FullTest",false);}
    }
    static void Check(){
        if(EditorApplication.timeSinceStartup-began<3)return;
        EditorApplication.update-=Check;
        try{
            var w=Object.FindFirstObjectByType<TempleAwakening>();var player=w.player;
            var traps=Object.FindObjectsByType<TrapBase>(FindObjectsSortMode.None);
            var count=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length;
            var ok=traps.Length==33 && w.sockets.Length==3 && count>1000 && player.GetComponent<CharacterController>().isGrounded && Object.FindFirstObjectByType<HeartOfEternityPickup>();
            File.WriteAllText("FullTempleCheck.txt",(ok?"PASS":"CHECK")+"; renderers="+count+"; traps="+traps.Length+"; eggs="+w.sockets.Length+"; height="+player.transform.position.y+"; grounded="+player.GetComponent<CharacterController>().isGrounded);
        }catch(System.Exception ex){File.WriteAllText("FullTempleCheck.txt",ex.ToString());}
        EditorApplication.ExitPlaymode();
    }
}

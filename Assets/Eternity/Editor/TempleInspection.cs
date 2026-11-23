using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Linq;
public static class TempleInspection {
 [MenuItem("Eternity/Inspect temple layout")] public static void Inspect(){
 if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Eternity/HeartOfEternity_Temple.unity")return;
 using(var w=new StreamWriter("TempleInspection.tsv")){
 foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
 var t=r.transform;var path=t.name;for(var a=t.parent;a;a=a.parent)path=a.name+"/"+path;
 w.WriteLine(string.Join("\t",path,t.position.ToString("F4"),r.bounds.center.ToString("F4"),r.bounds.size.ToString("F4"),string.Join(",",r.sharedMaterials.Select(m=>m?m.name:"null")),r.enabled));}}
 }
}



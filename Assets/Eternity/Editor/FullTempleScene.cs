using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Eternity;

public static class FullTempleScene {
    const string World="Assets/Eternity/World/";
    const string ScenePath="Assets/Eternity/HeartOfEternity_Temple.unity";
    [Serializable] class Source { public string[] groups; public Pose[] objects; public Surface[] materials; }
    [Serializable] class Pose { public string id,label; public float[] pos,start,rotation,initial; public bool cover; }
    [Serializable] class Labels { public Node[] nodes; }
    [Serializable] class Node { public string id,label; public string[] mats; }
    [Serializable] class Surface { public string label,albedo,normal; public float[] color,emission; public float strength,metal,rough; }
    static Vector3 V(float[] a) => new Vector3(a[0],a[1],a[2]);
    static Quaternion Q(float[] a) => new Quaternion(a[0],a[1],a[2],a[3]);
    static Color C(float[] a) => new Color(a[0],a[1],a[2],a[3]);
    static Dictionary<string,Material> Materials(Surface[] list) {
        string folder=World+"Materials/"; Directory.CreateDirectory(folder);
        var result=new Dictionary<string,Material>();
        for(int i=0;i<list.Length;i++) {
            var s=list[i];string path=folder+i.ToString("D3")+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}
            m.name=s.label;var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(World+"Textures/"+s.albedo);
            m.mainTexture=tex;m.color=tex?Color.white:C(s.color).gamma;
            m.SetFloat("_Metallic",s.metal);m.SetFloat("_Glossiness",1-s.rough);
            if(!string.IsNullOrEmpty(s.normal)) {
                string normalPath=World+"Textures/"+s.normal;
                var ti=AssetImporter.GetAtPath(normalPath) as TextureImporter;
                if(ti && ti.textureType!=TextureImporterType.NormalMap){ti.textureType=TextureImporterType.NormalMap;ti.SaveAndReimport();}
                m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));m.EnableKeyword("_NORMALMAP");
            }
            if(s.strength>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",C(s.emission)*s.strength);}
            if(tex && (s.label.ToLowerInvariant().Contains("leaf") || s.label.Contains("Clusters"))){m.SetOverrideTag("RenderType","TransparentCutout");m.EnableKeyword("_ALPHATEST_ON");m.SetFloat("_Cutoff",.32f);m.renderQueue=2450;}
            EditorUtility.SetDirty(m);result[s.label]=m;
        }
        result["mossy_sandstone"] = Resources.Load<Material>("MAT_Sandstone_Block");
        return result;
    }

    public static void Populate() {
        var source=JsonUtility.FromJson<Source>(File.ReadAllText(World+"WorldLayout.json"));
        var labels=JsonUtility.FromJson<Labels>(File.ReadAllText(World+"Names.json")).nodes.ToDictionary(n=>n.id);
        var surfaces=Materials(source.materials);
        var poses=source.objects.ToDictionary(p=>p.id);
        var world=new GameObject("Midnight Jungle and Temple");
        var awakening=world.AddComponent<TempleAwakening>();
        var parts=new List<TempleAwakening.Part>();var eggs=new SortedDictionary<string,Transform>();
        foreach(var name in source.groups) {
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(World+name+".fbx");
            if(!model)throw new IOException("World import pending: "+name);
            var root=(GameObject)PrefabUtility.InstantiatePrefab(model);
            PrefabUtility.UnpackPrefabInstance(root,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            root.transform.SetParent(world.transform,true);root.name=name;
            var children=root.GetComponentsInChildren<Transform>(true);
            var x=children.First(c=>c.name=="WorldAxisX").position;
            var y=children.First(c=>c.name=="WorldAxisY").position;
            var z=children.First(c=>c.name=="WorldAxisZ").position;
            root.transform.localScale=Vector3.Scale(root.transform.localScale,new Vector3(1/x.x,1/z.y,1/y.z));
            foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true)) {
                var t=r.transform;string id=t.name;if(!labels.TryGetValue(id,out var node))continue;
                var slots=r.sharedMaterials;
                for(int i=0;i<slots.Length;i++)if(node.mats.Length>i && surfaces.TryGetValue(node.mats[i],out var mat))slots[i]=mat;
                r.sharedMaterials=slots;t.name=node.label;
                if(name.StartsWith("01")||node.label.StartsWith("Canopy trunk")||node.label.StartsWith("Triangle altar")) {
                    var col=t.gameObject.AddComponent<MeshCollider>();col.sharedMesh=t.GetComponent<MeshFilter>().sharedMesh;t.gameObject.layer=11;
                }
                if(!poses.TryGetValue(id,out var p))continue;
                if(node.label.StartsWith("Relic Egg")){eggs[node.label]=t;continue;}
                if(p.cover || (V(p.pos)-V(p.start)).sqrMagnitude>.0001f || Quaternion.Angle(Q(p.rotation),Q(p.initial))>.1f)
                    parts.Add(new TempleAwakening.Part{target=t,raised=t.position,buried=t.position+V(p.start)-V(p.pos),first=Q(p.initial)*Quaternion.Inverse(Q(p.rotation))*t.rotation,last=t.rotation,cover=p.cover});
            }
        }
        foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if(t.parent && t.parent.name.StartsWith("Seg_") && (t.name.StartsWith("Shaft_") || (t.name.StartsWith("Upper_") && !t.name.Contains("Reliquary"))))
                parts.Add(new TempleAwakening.Part{target=t,raised=t.position,buried=t.position+Vector3.down*14,first=t.rotation,last=t.rotation});
        awakening.parts=parts.ToArray();awakening.sockets=eggs.Values.ToArray();
        awakening.player=UnityEngine.Object.FindFirstObjectByType<GauntletPlayer>();
        var spawn=new Vector3(-2, .25f,-24);awakening.player.Warp(spawn);
        awakening.player.checkpoints.last=spawn;GameObject.Find("PlayerStart").transform.position=spawn;
        var moon=new GameObject("Moonlight").AddComponent<Light>();moon.type=LightType.Directional;moon.color=new Color(.57f,.73f,1);moon.intensity=.38f;moon.transform.rotation=Quaternion.Euler(46,-28,0);moon.shadows=LightShadows.Soft;
        var glow=new GameObject("Entrance amber light").AddComponent<Light>();glow.transform.position=new Vector3(0,5,-8);glow.color=new Color(1,.49f,.13f);glow.type=LightType.Point;glow.range=4.5f;glow.intensity=1.1f;glow.shadows=LightShadows.Soft;
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.018f;RenderSettings.fogColor=new Color(.075f,.12f,.16f);
        RenderSettings.skybox=null;RenderSettings.reflectionIntensity=.22f;RenderSettings.ambientLight=new Color(.13f,.16f,.21f);
        var eye=awakening.player.eye.GetComponent<Camera>();eye.clearFlags=CameraClearFlags.SolidColor;eye.backgroundColor=RenderSettings.fogColor;eye.tag="MainCamera";
        var scene=SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.LookAt(new Vector3(0,4,-5),Quaternion.Euler(14,18,0),32);
        Debug.Log("FULL WORLD SAVED: "+scene.rootCount+" roots, "+parts.Count+" animated parts, "+eggs.Count+" altar eggs.");
    }
}

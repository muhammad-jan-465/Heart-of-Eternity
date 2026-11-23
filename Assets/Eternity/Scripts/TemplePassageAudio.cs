using UnityEngine;

namespace Eternity {
public sealed class TemplePassageAudio : MonoBehaviour {
    AudioSource feet, gravel;
    readonly AudioClip[] steps = new AudioClip[3];
    AudioClip impact;
    TempleCollapseSequence collapse;
    TempleLadder ladder;
    ParticleSystem stones;
    Mesh pebble;
    float distance, emissionTime, impactTime;
    int step;
    void Awake() {
        feet = gameObject.AddComponent<AudioSource>();
        feet.playOnAwake = false;
        for(int i=0;i<steps.Length;i++) steps[i]=Resources.Load<AudioClip>("EncounterAudio/Footfall"+(i+1));
        impact=Resources.Load<AudioClip>("EncounterAudio/PebbleScatter");
        collapse=FindFirstObjectByType<TempleCollapseSequence>();
        ladder=FindFirstObjectByType<TempleLadder>();
        var emitter=new GameObject("Tunnel pebble shower");
        emitter.transform.SetParent(transform,false);
        gravel=emitter.AddComponent<AudioSource>();
        gravel.playOnAwake=false;
        gravel.spatialBlend=.65f;
        gravel.minDistance=3;
        stones=emitter.AddComponent<ParticleSystem>();
        stones.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=stones.main;
        main.playOnAwake=false; main.simulationSpace=ParticleSystemSimulationSpace.World;
        main.gravityModifier=1; main.maxParticles=56; main.startSpeed=0;
        var emission=stones.emission; emission.enabled=false;
        var shape=stones.shape; shape.enabled=false;
        var spin=stones.rotationOverLifetime; spin.enabled=true; spin.z=new ParticleSystem.MinMaxCurve(-4,4);
        pebble=new Mesh {name="Sandstone chips"};
        pebble.vertices=new[]{new Vector3(0,.49f,.06f),new Vector3(.6f,0,0),new Vector3(.06f,0,.4f),new Vector3(-.5f,-.04f,0),new Vector3(0,0,-.37f),new Vector3(.09f,-.36f,0)};
        pebble.triangles=new[]{0,2,1,0,3,2,0,4,3,0,1,4,5,1,2,5,2,3,5,3,4,5,4,1};
        pebble.RecalculateNormals(); pebble.RecalculateBounds();
        var renderer=emitter.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode=ParticleSystemRenderMode.Mesh; renderer.mesh=pebble;
        renderer.sharedMaterial=Resources.Load<Material>("MAT_Sandstone_Block");
    }
    public void Travel(float metres,bool grounded,bool running) {
        if(!grounded || metres<.0001f) {distance=0;return;}
        distance+=metres;
        float stride=running?1.68f:1.14f;
        if(distance<stride)return;
        distance%=stride;
        var clip=steps[step++%steps.Length];
        if(!clip)return;
        feet.pitch=Random.Range(.94f,1.05f)*(running?1.08f:1);
        feet.PlayOneShot(clip,running?.58f:.39f);
    }
    void Update() {
        if(ExpeditionMenu.Visible)return;
        bool active=collapse && collapse.running && ladder && transform.position.y < ladder.Bottom.y+7f;
        if(!active) {
            if(stones.isPlaying)stones.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            if(gravel.isPlaying)gravel.Stop();
            emissionTime=impactTime=0;return;
        }
        if(!stones.isPlaying)stones.Play();
        emissionTime-=Time.deltaTime;
        if(emissionTime>0)return;
        emissionTime=.085f;
        var offset=Random.insideUnitCircle*2.6f;
        var position=transform.position+transform.forward*2.1f+new Vector3(offset.x,.9f,offset.y);
        int mask=~(1<<gameObject.layer);
        if(!Physics.Raycast(position,Vector3.down,out var floor,3,mask,QueryTriggerInteraction.Ignore))return;
        float height=Physics.Raycast(position,Vector3.up,out var roof,6,mask,QueryTriggerInteraction.Ignore)?roof.distance-.12f:3.5f;
        if(height<.55f)return;
        position.y+=height;
        var particle=new ParticleSystem.EmitParams {
            position=position,velocity=new Vector3(Random.Range(-.25f,.25f),-1,Random.Range(-.25f,.25f)),
            startSize=Random.Range(.045f,.12f),rotation=Random.Range(0,360),
            startLifetime=(Mathf.Sqrt(1+2*9.81f*(position.y-floor.point.y))-1)/9.81f
        };
        stones.Emit(particle,1);
        if(Time.time>=impactTime && impact) {
            gravel.transform.position=floor.point;
            gravel.pitch=Random.Range(.83f,1.18f);
            gravel.PlayOneShot(impact,.14f);
            impactTime=Time.time+Random.Range(.55f,1.25f);
        }
    }
    void OnDestroy(){if(pebble)Destroy(pebble);}
}
}

using System;
using UnityEngine;
namespace Eternity {
    public class TempleAwakening : MonoBehaviour {
        [Serializable] public class Part {
            public Transform target;
            public Vector3 buried, raised;
            public Quaternion first, last;
            public bool cover;
        }
        public Part[] parts;
        public Transform[] sockets;
        public GauntletPlayer player;
        public float riseSeconds = 20;
        public bool awakened;
        [Min(.1f)] public float sinkSeconds = 6f;
        bool sinking;
        AudioSource movementAudio;
        public void BeginBurial(){
            if(sinking)return;
            sinking=true;
            if(time>0f)movementAudio.Play();
        }
        private float time;
        void Start() {
            var emitter=new GameObject("Temple stone movement");
            emitter.transform.SetParent(transform,false);
            if(sockets.Length>0 && sockets[0])emitter.transform.position=sockets[0].position;
            movementAudio=emitter.AddComponent<AudioSource>();
            movementAudio.playOnAwake=false;
            movementAudio.loop=true;
            movementAudio.spatialBlend=.8f;
            movementAudio.minDistance=6f;
            movementAudio.maxDistance=65f;
            movementAudio.volume=.58f;
            movementAudio.clip=Resources.Load<AudioClip>("EncounterAudio/Temple_Animation");
            Apply(0);
            foreach (var socket in sockets) socket.gameObject.SetActive(false);
        }
        void Update() {if(ExpeditionMenu.BlocksInput)return;
            if(sinking){
                if(time>0f){
                    time=Mathf.MoveTowards(time,0f,Time.deltaTime/Mathf.Max(.1f,sinkSeconds));
                    Apply(Mathf.SmoothStep(0,1,time));
                    if(time<=0f)movementAudio.Stop();
                }
                return;
            }
            if(Input.GetKeyDown(KeyCode.E))Interact();
            if (awakened && time < 1f) {
                if(time==0f)movementAudio.Play();
                time = Mathf.Min(1f,time + Time.deltaTime / riseSeconds);
                Apply(Mathf.SmoothStep(0,1,time));
                if(time>=1f)movementAudio.Stop();
            }
        }
        readonly GameObject[] drops=new GameObject[3];
        int collected, installed;
        public void Drop(int id,Vector3 at){
            int bit=1<<id;
            if(drops[id]||((collected|installed)&bit)!=0)return;
            var egg=Instantiate(sockets[id].gameObject,at+Vector3.up*.32f,sockets[id].rotation);
            egg.transform.localScale=sockets[id].lossyScale;
            egg.name="Dropped "+sockets[id].name;egg.SetActive(true);drops[id]=egg;
        }
        public void Interact(){
            int best=-1;float range=2.5f;
            for(int i=0;i<drops.Length;i++)if(drops[i]){
                float d=Vector3.Distance(player.transform.position,drops[i].transform.position);
                if(d<range){range=d;best=i;}
            }
            if(best>=0){collected|=1<<best;Destroy(drops[best]);drops[best]=null;return;}
            best=-1;range=3f;
            for(int i=0;i<sockets.Length;i++){
                if((collected&(1<<i))==0)continue;float d=Vector3.Distance(player.transform.position,sockets[i].position);
                if(d<range){best=i;range=d;}
            }
            if(best<0||(collected&(1<<best))==0)return;
            collected&=~(1<<best);installed|=1<<best;sockets[best].gameObject.SetActive(true);
            awakened=installed==7;
        }
        void OnGUI(){
            if(awakened||!player)return;
            string prompt="";
            for(int i=0;i<3;i++){
                if(drops[i]&&Vector3.Distance(player.transform.position,drops[i].transform.position)<2.5f){prompt="[E] Collect "+sockets[i].name;break;}
                if((collected&(1<<i))!=0&&Vector3.Distance(player.transform.position,sockets[i].position)<3)prompt="[E] Place "+sockets[i].name;
            }
            if(prompt.Length>0)GUI.Label(new Rect(Screen.width*.5f-115,Screen.height*.62f,300,32),prompt);
        }
        void Apply(float t) {
            foreach (var p in parts) {
                if (!p.target) continue;
                p.target.position = Vector3.Lerp(p.buried,p.raised,t);
                if (p.first != p.last) p.target.rotation = Quaternion.Slerp(p.first,p.last,t);
                if (p.cover) p.target.gameObject.SetActive(t < .58f);
            }
        }
    }
}

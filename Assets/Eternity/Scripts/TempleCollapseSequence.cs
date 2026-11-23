using UnityEngine;
using UnityEngine.Events;
namespace Eternity {
public class TempleCollapseSequence:MonoBehaviour {public TrapBase[] traps;public SlidingWall[] seals;public float seconds=75;public UnityEvent onFinalSeal=new UnityEvent();public bool running,escaped,sealedTemple;public float Remaining=>left;float left;int index;
 TempleAwakening world;
 Transform terrain;GauntletPlayer runner;
 void Awake(){
  world=FindFirstObjectByType<TempleAwakening>();
  if(world){terrain=world.transform.Find("01_Terrain");runner=world.player;}
 }
 bool OnJungleGround(){
  if(!terrain||!runner)return false;
  Vector3 feet=runner.transform.position;
  return feet.y>=-1f && Physics.Raycast(feet+Vector3.up*.25f,Vector3.down,out var hit,.65f,~(1<<runner.gameObject.layer),QueryTriggerInteraction.Ignore)
   && hit.collider.transform.IsChildOf(terrain);
 }
 public void Begin(){if(running||escaped||sealedTemple)return;running=true;left=seconds;index=0;foreach(var t in traps)if(t)t.repeat=true;}
 void FixedUpdate(){if(!running)return;if(OnJungleGround()){Escape();return;}left-=Time.fixedDeltaTime;if(index<seals.Length&&left<seconds-(index+1)*4){seals[index++].Arm();}if(left<=0)Finish(false);}
 public void Escape(){Finish(true);}
 void Finish(bool won){
  if(!running)return;
  running=false;
  escaped=won;
  sealedTemple=!won;
  if(world)world.BeginBurial();
  if(won)return;
  onFinalSeal.Invoke();
  if(runner)runner.enabled=false;
  foreach(var beast in FindObjectsByType<TempleBeast>(FindObjectsSortMode.None))beast.enabled=false;
 }
}
}

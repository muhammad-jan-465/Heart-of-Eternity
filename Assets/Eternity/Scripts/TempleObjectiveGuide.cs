using UnityEngine;
namespace Eternity {
public sealed class TempleObjectiveGuide : MonoBehaviour {
 TempleAwakening altar;TempleLadder ladder;TempleCollapseSequence collapse;bool controls=true;
 void Start(){altar=FindFirstObjectByType<TempleAwakening>();ladder=FindFirstObjectByType<TempleLadder>();collapse=FindFirstObjectByType<TempleCollapseSequence>();}
 void Update(){if(Input.GetKeyDown(KeyCode.H))controls=!controls;}
 void OnGUI(){
  if(!altar||!ladder||!collapse)return;
  if(collapse.escaped||collapse.sealedTemple){
   float w=Mathf.Min(420f,Screen.width-20f);
   var panel=new Rect((Screen.width-w)*.5f,Screen.height*.38f,w,104f);
   GUI.Box(panel,GUIContent.none);
   var title=new GUIStyle(GUI.skin.label){fontSize=34,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
   GUI.Label(new Rect(panel.x,panel.y+6,w,55),collapse.escaped?"You Won":"You Lost",title);
   GUI.Label(new Rect(panel.x,panel.y+59,w,32),collapse.escaped?"You escaped with the Heart of Eternity.":"The temple sealed before you could escape.",new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter});
   return;
  }
  int eggs=0;foreach(var socket in altar.sockets)if(socket&&socket.gameObject.activeSelf)eggs++;
  string objective;
  if(collapse.running)objective="The temple is collapsing! Reach the surface by the escape stairs or the ladder. "+Mathf.CeilToInt(collapse.Remaining)+" s";
  else if(!altar.awakened)objective="Hunt the Lizard, Drakonit and Wolf Boss. Press E at a dropped egg, then E at its matching pedestal. Eggs placed: "+eggs+"/3";
  else if(ladder.occupied)objective="Follow the ladder into the crypt. Zombies guard the passage.";
  else if(transform.position.y>ladder.Bottom.y+3)objective="Enter the temple and press E beside the ladder to descend.";
  else objective="Survive the traps. Find the hidden wall switch and press E to open the reliquary. Approach the glowing red Heart inside.";
  float width=Mathf.Min(385,Screen.width-24);var style=new GUIStyle(GUI.skin.label){wordWrap=true};
  string message=objective+(controls?"\nWASD Move   Shift Run   Space Jump   V Camera\nLeft click Fire   E Interact   H Hide controls":"\nH Show controls");
  float height=style.CalcHeight(new GUIContent(message),width-20)+18;
  GUI.Box(new Rect(12,12,width,height),"");GUI.Label(new Rect(22,18,width-20,height),message,style);
 }
}}

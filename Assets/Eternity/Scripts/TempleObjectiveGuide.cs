using UnityEngine;
namespace Eternity {
public sealed class TempleObjectiveGuide : MonoBehaviour {
 TempleAwakening altar;TempleLadder ladder;TempleCollapseSequence collapse;bool controls=true;
 void Start(){altar=FindFirstObjectByType<TempleAwakening>();ladder=FindFirstObjectByType<TempleLadder>();collapse=FindFirstObjectByType<TempleCollapseSequence>();}
 void Update(){if(ExpeditionMenu.BlocksInput)return;if(Input.GetKeyDown(KeyCode.H))controls=!controls;}
 void OnGUI(){
  if(ExpeditionMenu.Visible||!altar||!ladder||!collapse)return;
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
  if(collapse.running){
   float w=Mathf.Clamp(Screen.width*.21f,142,270);
   float h=Mathf.Clamp(Screen.height*.115f,65,108);
   var box=new Rect((Screen.width-w)*.5f,8,w,h);
   GUI.Box(box,GUIContent.none);
   var label=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold,fontSize=(int)(h*.16f)};
   GUI.Label(new Rect(box.x,box.y+3,w,h*.28f),"TEMPLE SEALING",label);
   var digits=new GUIStyle(label){fontSize=(int)(h*.57f)};
   int seconds=Mathf.Max(0,Mathf.CeilToInt(collapse.Remaining));
   digits.normal.textColor=seconds<=10?new Color(1,.42f,.27f):new Color(1,.84f,.52f);
   GUI.Label(new Rect(box.x,box.y+h*.24f,w,h*.74f),string.Format("{0:00}:{1:00}",seconds/60,seconds%60),digits);
   objective="Reach the surface by the escape stairs or the ladder.";
  }
  else if(!altar.awakened)objective="Hunt the Lizard, Drakonit and Wolf Boss. Press E at a dropped egg, then E at its matching pedestal. Eggs placed: "+eggs+"/3";
  else if(ladder.occupied)objective="Follow the ladder into the crypt. Zombies guard the passage.";
  else if(transform.position.y>ladder.Bottom.y+3)objective="Enter the temple and press E beside the ladder to descend.";
  else objective="Survive the traps. Find the hidden wall switch and press E to open the reliquary. Approach the glowing red Heart inside.";
  float width=Mathf.Min(385,Screen.width-24);var style=new GUIStyle(GUI.skin.label){wordWrap=true};
  string message=objective+(controls?"\nWASD Move   Shift Run   Space Jump   V Camera\nLeft click Fire   E Interact   H Hide controls":"\nH Show controls");
  float height=style.CalcHeight(new GUIContent(message),width-20)+18;
  float top=collapse.running && Screen.width<1050?Mathf.Clamp(Screen.height*.115f,65,108)+15:12;
  GUI.Box(new Rect(12,top,width,height),"");GUI.Label(new Rect(22,top+6,width-20,height),message,style);
 }
}}

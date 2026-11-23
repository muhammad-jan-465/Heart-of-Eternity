using System.Collections;
using UnityEngine;
namespace Eternity {
public class TempleLadder : MonoBehaviour {
 public GameObject undergroundZombies;
 public GauntletPlayer player;
 public bool occupied {get;private set;}
 public Vector3 Bottom => transform.position+Vector3.right*2.15f+Vector3.up*.08f;
 public Vector3 Top => Bottom+Vector3.up*14.7f;
 bool InReach(){
  if(!player||Mathf.Abs(transform.position.y+12)>.15f)return false;
  var d=player.transform.position-transform.position;
  return d.y>=-.7f&&d.y<16&&new Vector2(d.x,d.z).magnitude<3.7f;
 }
 void Update(){if(ExpeditionMenu.BlocksInput)return;if(Input.GetKeyDown(KeyCode.E)&&InReach())Use();}
 public bool Use(){
  if(occupied||!InReach())return false;
  StartCoroutine(Climb(player.transform.position.y<(Top.y+Bottom.y)*.5f));return true;
 }
 IEnumerator Climb(bool up){
  occupied=true;var cc=player.GetComponent<CharacterController>();cc.enabled=false;
  var end=up?Top:Bottom;var rail=transform.position+Vector3.right*.65f;
  var align=new Vector3(rail.x,player.transform.position.y,rail.z);
  foreach(var destination in new[]{align,new Vector3(rail.x,end.y,rail.z),end}){
   while((player.transform.position-destination).sqrMagnitude>.0001f){
    player.transform.position=Vector3.MoveTowards(player.transform.position,destination,4.8f*Time.deltaTime);yield return null;
   }
  }
  cc.enabled=true;player.EndClimb();occupied=false;
  if(up){var collapse=FindFirstObjectByType<TempleCollapseSequence>();if(collapse)collapse.Escape();}
  if(!up&&undergroundZombies)undergroundZombies.SetActive(true);
 }
 void OnGUI(){if(InReach()&&!occupied)GUI.Label(new Rect(Screen.width/2-75,Screen.height*.65f,175,28),"[E]  Climb ladder");}
}
}

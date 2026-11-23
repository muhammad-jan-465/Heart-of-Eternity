using UnityEngine;
namespace Eternity {
public class TemplePracticeTarget : MonoBehaviour {
 public int health=3;
 public GameObject model;
 public void Hit(){
  if(health<=0)return;
  health--;
  if(health==0){model.SetActive(false);GetComponent<Collider>().enabled=false;Invoke(nameof(Restore),3);}
 }
 void Restore(){health=3;model.SetActive(true);GetComponent<Collider>().enabled=true;}
}
}

using UnityEngine;
namespace Eternity {
public class KeyDoor:MonoBehaviour {public GameObject gate;public int keyIndex;public TempleDoorSlam door;bool open;void OnTriggerStay(Collider c){var p=c.GetComponentInParent<GauntletPlayer>();if(!p||open||(p.keys&(1<<keyIndex))==0)return;open=true;if(gate)gate.SetActive(false);if(door){door.hold=99999;door.Arm();}} }
}


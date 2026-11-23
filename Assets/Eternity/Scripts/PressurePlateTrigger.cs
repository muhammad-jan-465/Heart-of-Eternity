using UnityEngine;
using UnityEngine.Events;
namespace Eternity {
public class PressurePlateTrigger:MonoBehaviour {
 public UnityEvent onPressed=new UnityEvent();
 void OnTriggerEnter(Collider other){if(other.GetComponentInParent<GauntletPlayer>())onPressed.Invoke();}
}
}

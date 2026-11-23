using UnityEngine;
namespace Eternity {
// Apply the platform's actual fixed-step motion only to a grounded CharacterController.
public class PlatformRider:MonoBehaviour {
 public Rigidbody platform;Vector3 previous;Quaternion rotation;GauntletPlayer rider;
 void Start(){previous=platform.position;rotation=platform.rotation;}
 void OnTriggerStay(Collider c){var player=c.GetComponentInParent<GauntletPlayer>();if(player)rider=player;}
 void OnTriggerExit(Collider c){if(rider&&c.GetComponentInParent<GauntletPlayer>()==rider)rider=null;}
 void FixedUpdate(){if(rider){var cc=rider.GetComponent<CharacterController>();if(cc&&cc.enabled&&cc.isGrounded){var desired=platform.position+(platform.rotation*Quaternion.Inverse(rotation))*(cc.transform.position-previous);cc.Move(desired-cc.transform.position);}}previous=platform.position;rotation=platform.rotation;}
}
}

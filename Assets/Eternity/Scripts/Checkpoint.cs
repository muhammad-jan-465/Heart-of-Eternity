using UnityEngine;
namespace Eternity {
public class Checkpoint:MonoBehaviour {void OnTriggerEnter(Collider c){var p=c.GetComponentInParent<GauntletPlayer>();if(p&&p.checkpoints)p.checkpoints.last=transform.position+Vector3.up*.1f;} }
}

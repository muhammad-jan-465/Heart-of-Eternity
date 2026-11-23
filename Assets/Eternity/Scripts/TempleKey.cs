using UnityEngine;
namespace Eternity {
public class TempleKey:MonoBehaviour {public int index;void OnTriggerEnter(Collider c){var p=c.GetComponentInParent<GauntletPlayer>();if(p){p.keys|=1<<index;gameObject.SetActive(false);}} }
}

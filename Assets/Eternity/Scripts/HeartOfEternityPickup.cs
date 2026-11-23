using UnityEngine;
namespace Eternity {
public class HeartOfEternityPickup:MonoBehaviour {public GameObject visual;public TempleCollapseSequence sequence;void OnTriggerEnter(Collider c){if(sequence&&c.GetComponentInParent<GauntletPlayer>()){sequence.Begin();if(visual)visual.SetActive(false);gameObject.SetActive(false);}} }
}

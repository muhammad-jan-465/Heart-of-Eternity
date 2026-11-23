using UnityEngine;
namespace Eternity {
 public class ExitZone : MonoBehaviour {
  public TempleCollapseSequence sequence;
  void OnTriggerEnter(Collider other) => CheckEscape(other);
  void OnTriggerStay(Collider other) => CheckEscape(other);
  void CheckEscape(Collider other){
   if(sequence && sequence.running && other.GetComponentInParent<GauntletPlayer>())sequence.Escape();
  }
 }
}

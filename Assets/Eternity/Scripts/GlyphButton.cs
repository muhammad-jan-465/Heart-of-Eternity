using UnityEngine;
namespace Eternity {
public class GlyphButton:MonoBehaviour {
 public GlyphPuzzle puzzle;public int symbol;bool inside;
 void OnTriggerEnter(Collider c){if(c.GetComponentInParent<GauntletPlayer>())inside=true;}
 void OnTriggerExit(Collider c){if(c.GetComponentInParent<GauntletPlayer>())inside=false;}
 void Update(){if(inside&&Input.GetKeyDown(KeyCode.E)&&puzzle)puzzle.Press(symbol);}
}
}

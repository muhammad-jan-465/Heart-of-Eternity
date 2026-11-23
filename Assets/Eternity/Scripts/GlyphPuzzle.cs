using UnityEngine;
namespace Eternity {
public class GlyphPuzzle:MonoBehaviour {
 public SlidingWall wall;int progress;bool solved;
 public void Press(int symbol){if(solved)return;if(symbol==progress)progress++;else progress=symbol==0?1:0;if(progress==3&&wall){wall.hold=99999;wall.Arm();solved=true;}}
}
}

using UnityEngine;
namespace Eternity {
public class CheckpointManager:MonoBehaviour {public Vector3 last;public void Respawn(GauntletPlayer p){p.Warp(last);} }
}

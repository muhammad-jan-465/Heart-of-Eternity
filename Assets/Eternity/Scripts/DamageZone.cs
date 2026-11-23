using UnityEngine;
namespace Eternity {
public class DamageZone:MonoBehaviour {
 public bool active;public int amount=1;
 void OnTriggerEnter(Collider other){Damage(other);}
 void OnTriggerStay(Collider other){Damage(other);}
 void OnCollisionStay(Collision collision){Damage(collision.collider);}
 public void Damage(Collider other){if(!active)return;var player=other.GetComponentInParent<GauntletPlayer>();if(player)player.Hit(amount);}
}
}

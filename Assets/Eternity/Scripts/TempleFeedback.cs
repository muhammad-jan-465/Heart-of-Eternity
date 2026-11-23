using UnityEngine;
namespace Eternity {
// A small impulse is applied only to the camera. The controller and its collision capsule stay level.
public class TempleFeedback : MonoBehaviour {
 public AudioClip slam;public AudioSource source;public Transform eye;
 float remaining;Vector3 offset;
 public void Impact(){remaining=.2f;if(source&&slam)source.PlayOneShot(slam);}
 void LateUpdate(){if(!eye)return;eye.localPosition-=offset;offset=Vector3.zero;if(remaining>0){remaining-=Time.deltaTime;float a=.038f*Mathf.Clamp01(remaining/.2f);offset=Vector3.right*Mathf.Sin(Time.time*75)*a+Vector3.up*Mathf.Cos(Time.time*91)*a;eye.localPosition+=offset;}}
}

}

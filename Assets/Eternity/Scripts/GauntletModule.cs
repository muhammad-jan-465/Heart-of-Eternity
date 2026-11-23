using UnityEngine;
namespace Eternity {
// Standalone module prefabs resolve their shared FBX once when instantiated.
public sealed class GauntletModule:MonoBehaviour {
 public string asset;
 void Awake(){var model=GauntletBootstrap.InstantiateModule(asset);model.transform.SetParent(transform,false);}
}
}

using UnityEngine;

public class BotSkinChooser : MonoBehaviour {
    [SerializeField] private SkinWearer _skinWearer;

    private void OnEnable() {
        _skinWearer.WearRandomSkinForBot();
    }


}
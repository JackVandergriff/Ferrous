using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public PostProcessVolume ppv;
    public static Vignette vn;
    public static ChromaticAberration ca;

    void Start() {
        ppv.profile.TryGetSettings(out vn);
        ppv.profile.TryGetSettings(out ca);
    }

    public static void setDanger(float d) {
        d = Mathf.Clamp01(d);
        vn.intensity.value = d;
        ca.intensity.value = d;
    }
}
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public PostProcessVolume ppv;
    public List<GameObject> prefabs;

    public static Vignette vn;
    public static ChromaticAberration ca;
    public static Dictionary<string, GameObject> pf = new Dictionary<string, GameObject>();

    void Start() {
        ppv.profile.TryGetSettings(out vn);
        ppv.profile.TryGetSettings(out ca);
        foreach (GameObject g in prefabs) {
            pf.Add(g.name, g);
        }
    }

    public static void setDanger(float d) {
        d = Mathf.Clamp01(d);
        vn.intensity.value = d;
        ca.intensity.value = d;
    }
}
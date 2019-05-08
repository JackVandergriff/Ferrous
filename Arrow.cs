using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour {
    
    public Transform target, player;
    Spaceship s;
    Camera c;

    RectTransform rt;
    Image i;

    float heading;
    Vector2 vSpace, nvSpace;
    bool fadeOut = false;

    float FoV = 0; // Range at which planets are detectable

    void Start() {
        player = GameObject.FindObjectOfType<Spaceship>().transform;
        c = Camera.main;

        rt = GetComponent<RectTransform>();
        i = GetComponent<Image>();
    }

    void Update() {
        heading = Vector3.Dot(target.position - player.position, player.forward);
        if (heading > FoV) {
            vSpace = c.WorldToViewportPoint(target.position);
            if (vSpace.x > 1 || vSpace.x < 0 || vSpace.y > 1 || vSpace.y < 0) {
                if (fadeOut) { StopAllCoroutines(); StartCoroutine(FadeTo(0.4f, 0.7f)); fadeOut = false;}
                nvSpace.x = Mathf.Clamp(vSpace.x, 0.05f, 0.95f);
                nvSpace.y = Mathf.Clamp(vSpace.y, 0.05f, 0.95f);
                rt.position = c.ViewportToScreenPoint(nvSpace);
                Vector2 delta = vSpace - nvSpace;
                rt.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(delta.y, delta.x));
            } else {
                if (!fadeOut) { StopAllCoroutines(); StartCoroutine(FadeTo(0.4f, 0f)); fadeOut = true;}
            }
        } else {
            if (!fadeOut) { StopAllCoroutines(); StartCoroutine(FadeTo(0.4f, 0f)); fadeOut = true;}
        }
    }

    IEnumerator FadeTo(float time, float value) { // Makes transitions smoother
        float s = i.color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / time) {
            i.color = new Color(1, 1, 1, Mathf.Lerp(s, value, t));
            yield return null;
        }
        i.color = new Color(1, 1, 1, value);
    }

    public void instantiate(Gravity g, Spaceship s) {
        target = g.transform;
        this.s = s;
        s.arrows.Add(g, this);
    }

    public void destroy() {
        StopAllCoroutines();
        FoV = 1000;
        StartCoroutine(FadeTo(0.4f, 0f));
        StartCoroutine(remove());

        IEnumerator remove() { // Wait until faded out before removing
            yield return new WaitUntil(() => i.color.a == 0);
            s.arrows.Remove(target.GetComponent<Gravity>());
            Destroy(gameObject);
        }
    }
}
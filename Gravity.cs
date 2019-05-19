using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour {
    // This just handles all proportional to distance squared stuff, so:
    // Gravity, electrostatic force
    // Fun fact: this is so bloated it can probably handle any force

    protected Rigidbody rb; // used to apply forces
    Vector3 Fg;
    Vector3Int block;
    List<Gravity> localBodies;
    Dictionary<ForceTypes, Gravity> self = new Dictionary<ForceTypes, Gravity>();

    public bool Static;
    public ForceTypes type = ForceTypes.gravity;
    public float charge;

    public const double G = .5; //  Not accurate, makes mass  values easier
    public const double K = 9;  //  Not accurate, makes charge values easier
    public const int blockSize = 100;
    public static List<Gravity> bodiesNonStatic = new List<Gravity>();
    public static Dictionary<int, Dictionary<int, Dictionary<int, List<Gravity>>>> bodies =
        new Dictionary<int, Dictionary<int, Dictionary<int, List<Gravity>>>>();

    protected virtual void OnLocalBodiesUpdated(List<Gravity> localBodies) {}
    protected virtual void StartExtra() {}

    public enum ForceTypes { gravity, electrostatic}
    public static Dictionary<ForceTypes,Dictionary<int, Color>> colormap = new Dictionary<ForceTypes, Dictionary<int, Color>>(){
        {ForceTypes.gravity,        new Dictionary<int, Color>(){{0, new Color(0.5f, 0.4f, 0.8f)}}},
        {ForceTypes.electrostatic,  new Dictionary<int, Color>(){{0, new Color(0.8f, 0.3f, 0.4f)},
                                                                 {1, new Color(0.6f, 0.8f, 0.7f)}}}
    };

    void Start() {
        StartExtra();
        rb = GetComponent<Rigidbody>();

        if (Static) GameManager.addEffect(colormap[type][0], transform);

        foreach (Gravity g in GetComponents<Gravity>()) {
            if (g != this && g.self.Count > 0) {
                g.self.Add(type, this);
                return;
            }
        }
        self.Add(type, this);

        block = getBlock();
        SetBlock(block);
        
        if (!Static) {
            bodiesNonStatic.Add(this);
            StartCoroutine(StartLate());
        }

        IEnumerator StartLate() {
            yield return new WaitForEndOfFrame();
            UpdateLocalBodies();
            StartCoroutine(gravitate());
        }
    }

    IEnumerator gravitate() { // Main loop for gravity
        while (true) {
            Vector3Int nb = getBlock();
            if (nb != block) {
                bodies[block.x][block.y][block.z].Remove(this);
                block = nb;
                SetBlock(block);
                foreach (Gravity g in bodiesNonStatic)
                    g.UpdateLocalBodies();
            }

            foreach (Gravity g in localBodies) {
                foreach (KeyValuePair<ForceTypes, Gravity> kv in g.self) {
                    if (self.ContainsKey(kv.Key)) {
                        rb.AddForce(gravity(kv.Value, self[kv.Key], kv.Key));
                    }
                }
            }

            yield return null;
        }
    }

    Vector3Int getBlock() {
        return Vector3Int.RoundToInt(transform.position / blockSize);
    }

    Vector3 gravity(Gravity a, Gravity b, ForceTypes t) {
        Vector3 delta = (a.transform.position - b.transform.position);
        switch (t) {
            case ForceTypes.gravity:
                return delta * (float) G * a.rb.mass * b.rb.mass / (delta.magnitude * delta.magnitude * delta.magnitude);
            case ForceTypes.electrostatic:
                return delta * (float) K * a.charge * b.charge / (delta.magnitude * delta.magnitude * delta.magnitude);
        }
        return Vector3.zero;
    }
    
    void SetBlock(Vector3Int b) {
        if (!bodies.ContainsKey(b.x))
            bodies[b.x] = new Dictionary<int, Dictionary<int, List<Gravity>>>();
        if (!bodies[b.x].ContainsKey(b.y))
            bodies[b.x][b.y] = new Dictionary<int, List<Gravity>>();
        if (!bodies[b.x][b.y].ContainsKey(b.z))
            bodies[b.x][b.y][b.z] = new List<Gravity>();
        bodies[b.x][b.y][b.z].Add(this);
    }

    public void UpdateLocalBodies() { // Used to find planets close enough to calculate gravity
        localBodies = new List<Gravity>();
        for (int x = -1; x < 2; x++) { for (int y = -1; y < 2; y++) { for (int z = -1; z < 2; z++) {
            try {
                localBodies.AddRange(bodies[block.x+x][block.y+y][block.z+z]);
            } catch (KeyNotFoundException) {}
        }}}
        localBodies.Remove(this);
        OnLocalBodiesUpdated(localBodies);
    }

    public List<Gravity> getLocalBodies() {
        return localBodies;
    }
}
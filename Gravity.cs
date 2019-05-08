using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour {

    protected Rigidbody rb;
    Vector3 Fg;
    Vector3Int block;

    public List<Gravity> localBodies {get; protected set;}
    public bool Static;

    public const double G = .5;
    public const int blockSize = 100;
    public static List<Gravity> bodiesNonStatic = new List<Gravity>();
    public static Dictionary<int, Dictionary<int, Dictionary<int, List<Gravity>>>> bodies =
        new Dictionary<int, Dictionary<int, Dictionary<int, List<Gravity>>>>();

    protected virtual void OnLocalBodiesUpdated(List<Gravity> localBodies) {}
    protected virtual void StartExtra() {}

    void Start() {
        rb = GetComponent<Rigidbody>();

        block = getBlock();
        SetBlock(block);
        
        if (!Static) {
            bodiesNonStatic.Add(this);
            StartCoroutine(StartLate());
        }

        StartExtra();

        IEnumerator StartLate() {
            yield return new WaitForEndOfFrame();
            UpdateLocalBodies();
            StartCoroutine(gravitate());
        }
    }

    IEnumerator gravitate() {
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
                rb.AddForce(gravity(g, this));
            }

            yield return null;
        }
    }

    Vector3Int getBlock() {
        return Vector3Int.RoundToInt(transform.position / blockSize);
    }

    Vector3 gravity(Gravity a, Gravity b) {
        Vector3 delta = (a.transform.position - b.transform.position);
        return delta * (float) G * a.rb.mass * b.rb.mass / (delta.magnitude * delta.magnitude * delta.magnitude);
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

    public void UpdateLocalBodies() {
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
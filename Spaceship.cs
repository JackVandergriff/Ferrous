using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spaceship : Gravity { // Attached to the Camera for now

    public float moveSpeed, turnSpeed, scrollSpeed;
    public GameObject Arrow;
    public Transform canvas;
    public Dictionary<Gravity, Arrow> arrows = new Dictionary<Gravity, Arrow>();

    Camera c;
    float h, v;

    protected override void StartExtra() {
        c = GetComponent<Camera>();
    }

    void Update() {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        // Deals with adding forces to move spaceship
        rb.AddRelativeForce(Vector3.forward * moveSpeed * Input.GetAxis("Fire1"));
        rb.AddRelativeTorque(Vector3.left * turnSpeed * v);
        rb.AddRelativeTorque(Vector3.up * turnSpeed * h);
        
        if (rb.velocity.magnitude > 35) {
            GameManager.setDanger(1 - (35 / rb.velocity.magnitude));
        }
    }

    protected override void OnLocalBodiesUpdated(List<Gravity> localBodies) {
        // Adds arrows to help track planets outside field of view
        foreach (Gravity g in localBodies) {
            if (!arrows.ContainsKey(g))
                Instantiate(Arrow, canvas).GetComponent<Arrow>().instantiate(g, this);
        }
        foreach (KeyValuePair<Gravity, Arrow> kv in arrows) {
            if (!localBodies.Contains(kv.Key)) {
                kv.Value.destroy();
            }
        }
    }

    void OnCollisionEnter(Collision c) {
        GameManager.setDanger(0);
        Destroy(rb);
        Instantiate(GameManager.pf["Explosion"], transform.position, transform.rotation);
        transform.position = Vector3.LerpUnclamped(c.transform.position, transform.position, 2.8f);
        transform.LookAt(c.transform);
        Destroy(this);
    }
}
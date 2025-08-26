using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EventOnParticleSpawn : MonoBehaviour
{
	public UnityEngine.Events.UnityEvent spawnEvent;
	
	private ParticleSystem ps;
	private int lastParticleCount = 0;
	
	void Awake() {
		ps = GetComponent<ParticleSystem>();
	}
	
    void Update() {
		int curParticles = ps.particleCount;
		if(curParticles > lastParticleCount) {
			spawnEvent.Invoke();
		}
		lastParticleCount = curParticles;
	}
}

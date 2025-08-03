using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour {

    private PlayerData _playerData;
    
    private GameObject _sword;
    private Collider _swordCollider;
    private ParticleSystem[] _swordParticles;

    private int _comboCount;

    private EventArchive _eventArchive;
    
    void Start() {
        
        _playerData = GetComponent<PlayerController>().playerData;
        
        Subscribe();
        
        _sword = GameObject.FindGameObjectWithTag("Sword");
        _swordCollider = _sword.GetComponent<Collider>();
        _swordParticles = _sword.GetComponentsInChildren<ParticleSystem>();
        foreach(var swordParticle in _swordParticles) { swordParticle.Stop(); }
        
        DisableSwords();
    }

    private void Subscribe() {
        
        _eventArchive = FindFirstObjectByType<EventArchive>();
        
        _eventArchive.gameplay.OnAttackComboCount += count => _comboCount = count;
        _eventArchive.gameplay.OnDummyHit += ReturnDamage;
    }

    private void ReturnDamage() {

        var dmg = 0f;

        if(_comboCount < 3) { dmg = _comboCount * _playerData.baseDamage; }
        else { dmg = _playerData.finisherDamage; }
        _eventArchive.dummyEvents.InvokeOnGetDamage(dmg);
    }

    internal void EnableSwords() {
        _swordCollider.enabled = true;
        foreach(var swordParticle in _swordParticles) { swordParticle.Play(); }

    }

    internal void DisableSwords() {

        _swordCollider.enabled = false;
        foreach(var swordParticle in _swordParticles) { swordParticle.Stop(); }
    }
}
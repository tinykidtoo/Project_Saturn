﻿using System;
using UnityEngine;

namespace Player {
    public class Health : MonoBehaviour {
        
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int hp = 100;
        [SerializeField] private float stumbleDuration = 2f;
        [SerializeField] private int healthOnExitStumble = 40;
        [SerializeField] private float bloodOnKill = 25;
       
        private Color defaultColor;
        public bool canTakeDamage = true;
        
        [SerializeField] private bool canStumble = true;
        [SerializeField] private bool canDieToBulletInStuble = true;
        private float stumbleTimeTracker;
        public bool Stumbled => stumbleTimeTracker >= 1e-4;

        public int Hp => hp;
        public float PercentHp => (float)hp / maxHp;

        int lowHpDmgThresh = 80;
        
        private BloodTracker bloodTracker;
        private Abilities abilities;

        private void Start() {
            bloodTracker = FindObjectOfType<BloodTracker>();
            abilities = FindObjectOfType<Abilities>();
            canTakeDamage = true;
        }

        public void ApplyDamage(int damage) {
            if (!canTakeDamage) return;
            damage = LowerDamageBasedOnHp(damage);

            hp -= damage;
            
            CheckOnHitEffects(false, true);
            gameObject.BroadcastMessage("OnDamage", SendMessageOptions.DontRequireReceiver);
        }

        public void ApplyMeleeDamage(int damage) {
            if (!canTakeDamage) return;
            damage = LowerDamageBasedOnHp(damage);

            hp -= damage;
            
            CheckOnHitEffects(true, false);
            gameObject.BroadcastMessage("OnMeleeDamage", SendMessageOptions.DontRequireReceiver);
        }

        private int LowerDamageBasedOnHp(int damage) {
            if (CompareTag("Player") && hp < lowHpDmgThresh) {
                damage = Mathf.RoundToInt(damage * (1f - 0.8f * (lowHpDmgThresh - hp) / lowHpDmgThresh));
            }

            return damage;
        }

        public void ApplyDamageForceKill(int damage) {
            if (!canTakeDamage) return;
            hp -= damage;
            
            CheckDeath(true, abilities.BloodMultiplierForBloodRageKills);
            gameObject.BroadcastMessage("OnDamage", SendMessageOptions.DontRequireReceiver);
        }

        public void ApplyHeal(int heal) {
            hp += heal;
            if (hp > maxHp) {
                hp = maxHp;
            }
        }

        private void CheckOnHitEffects(bool killGivesBlood, bool isBullet) {
            if (canStumble && !Stumbled && hp <= 1) {
                hp = 1;
                stumbleTimeTracker = stumbleDuration;
                gameObject.BroadcastMessage("EnteredStumble", SendMessageOptions.DontRequireReceiver);
            }else if ((!canStumble || Stumbled) && (canDieToBulletInStuble || !isBullet)) {
                CheckDeath(killGivesBlood);
            }
        }
        
        private void CheckDeath(bool killGivesBlood, float bloodMult = 1f) {
            if (hp <= 0)
                Die(killGivesBlood, bloodMult);
        }

        void Die(bool killGivesBlood, float bloodMult = 1f) {
            if (killGivesBlood) {
                bloodTracker.AddBlood(bloodOnKill * bloodMult);
                gameObject.BroadcastMessage("OnBloodKill", SendMessageOptions.DontRequireReceiver);
            }

            gameObject.BroadcastMessage("OnDeath", SendMessageOptions.DontRequireReceiver); //Send to every object and call this function
        }

        void Update() {
            if (Stumbled) {
                stumbleTimeTracker -= Time.deltaTime;
                if (!Stumbled) { // Exiting stumble
                    ApplyHeal(healthOnExitStumble);
                    gameObject.BroadcastMessage("ExitedStumble", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}

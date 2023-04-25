using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAttackState : State{

    [Serializable]
    public class AIAttackSettings {
        [field:SerializeField] public float RotationSpeed { get; private set; } = 8f;
        public Animator Animator { get; set; }
        public CharacterMovement CharacterMovement { get; set; }
        public GameObject Target { get; set; }
        public Transform Transform { get; set; }
        public Sword Sword { get; set; }
    }
    public Action Parried;

    private AIAttackSettings settings;
    private HashSet<IHittable> alreadyHitObjects;
    public AIAttackState(AIAttackSettings settings) {
        this.settings = settings;
        alreadyHitObjects = new HashSet<IHittable>();
    }

    protected override void OnEnter() {
        settings.Animator.applyRootMotion = true;
        settings.Animator.SetBool(AnimatorUtils.attackHash, true);
    }

    protected override void OnUpdate() {
        UpdateRotation();
        UpdateHitDetection();
    }

    private void UpdateRotation() {
        Vector3 targetDirection = settings.Target.transform.position - settings.Transform.position;
        targetDirection.y = 0;
        targetDirection.Normalize();
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion rotation = Quaternion.Slerp(settings.Transform.rotation, targetRotation, settings.RotationSpeed * Time.deltaTime);
        settings.CharacterMovement.SetRotation(rotation);
    }
    private void UpdateHitDetection() {
        HitData[] hitsData = settings.Sword.CheckHit();
        if (hitsData != null) {
            foreach (HitData hitData in hitsData) {
                IHittable hittableObject = hitData.hittableObject;
                if (!alreadyHitObjects.Contains(hittableObject)) {
                    hittableObject.Hit();
                    alreadyHitObjects.Add(hittableObject);

                    if (hittableObject is IDamageable) {
                        IDamageable damageableObject = (IDamageable)hittableObject;
                        if (damageableObject.CanBeDamaged()) {
                            if (damageableObject is IShieldable && ((IShieldable)damageableObject).IsShielded()) {
                                Parried?.Invoke();
                                
                            } else {
                                damageableObject.ReceiveDamage(settings.Sword.Damage);
                            }
                        }

                    }
                }
            }
        }
    }

    protected override void OnExit() {
        settings.Animator.applyRootMotion = false;
        settings.Animator.SetBool(AnimatorUtils.attackHash, false);
        settings.Sword.StartCooldown();
        alreadyHitObjects.Clear();
    }

    public override void RestoreFieldsAndProperties(object stateObjectRecord) {
        AIAttackStateRecord record = (AIAttackStateRecord)stateObjectRecord;
        alreadyHitObjects = new HashSet<IHittable>(record.alreadyHitObjects);
    }

    public override object RecordFieldsAndProperties() {
        IHittable[] alreadyHitObjects = new IHittable[this.alreadyHitObjects.Count];
        this.alreadyHitObjects.CopyTo(alreadyHitObjects);
        return new AIAttackStateRecord(alreadyHitObjects);
    }
}
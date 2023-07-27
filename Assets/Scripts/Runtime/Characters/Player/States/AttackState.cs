using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State {
    

    [Serializable] public class AttackSettings {
        public Sword Sword { get; set; }
        public Animator Animator { get; set; }
        public InputController InputController { get; set; }
        public CharacterMovement CharacterMovement { get; set; }
        public Transform Transform { get; set; }
        public PlayerPerceptionSystem PerceptionSystem { get; set; }
        [field: SerializeField] public float AttackPressedBufferTime { get; private set; }  = 0.3f;
        [field: SerializeField] public float RotationSpeed { get; private set; }  = 5f;
        [field: SerializeField] public float BlockableAttackAngleThreshold { get; private set; } = 100;

    }

    public Action AttackEnded;
    public Action Parried;

    private const int ATTACK_PRESSED_BUFFER_SIZE = 500;
    private const int MAX_ATTACK_COMBO = 3;
    private AttackSettings settings;
    private int attackHash;
    private int nextComboAttackHash;
    private int [] comboAttackNameHashes;
    private AttackInputBuffer attackInputBuffer;
    private Camera mainCamera;

    private RewindableHashSet<IHittable> alreadyHitObjects;
    private RewindableVariable<bool> comboEnabled;
    private RewindableVariable<bool> rotationEnabled;
    private RewindableVariable<bool> followedCombo;
    private RewindableVariable<int> attackIndex;
    private RewindableTransform closestAttackTarget;
    private IRewindable[] rewindableVariables;

    public AttackState(AttackSettings settings) : base() {
        this.settings = settings;
        mainCamera = Camera.main;
        settings.Sword.OnSetComboEnabled = SetComboEnabled;
        settings.Sword.OnSetRotationEnabled = SetRotationEnabled;
        attackInputBuffer = new AttackInputBuffer(ATTACK_PRESSED_BUFFER_SIZE);

        InitRewindableVariables();

        attackHash = AnimatorUtils.attackHash;
        nextComboAttackHash = AnimatorUtils.nextComboAttackHash;
        comboAttackNameHashes = new int[MAX_ATTACK_COMBO];
        comboAttackNameHashes[0] = AnimatorUtils.attack1Hash;
        comboAttackNameHashes[1] = AnimatorUtils.attack2Hash;
        comboAttackNameHashes[2] = AnimatorUtils.attack3Hash;
        AnimatorUtils.AnimationEnded += OnAnimationEnded;
    }

    private void InitRewindableVariables() {
        comboEnabled = new RewindableVariable<bool>(false, onlyExecuteOnRewindStop: true);
        rotationEnabled = new RewindableVariable<bool>(false, onlyExecuteOnRewindStop: true);
        followedCombo = new RewindableVariable<bool>(false, onlyExecuteOnRewindStop: true);
        attackIndex = new RewindableVariable<int>(onlyExecuteOnRewindStop: true);
        alreadyHitObjects = new RewindableHashSet<IHittable>(onlyExecuteOnRewindStop: true);
        closestAttackTarget = new RewindableTransform(null, onlyExecuteOnRewindStop: true);
        rewindableVariables = new IRewindable[] { comboEnabled, rotationEnabled, followedCombo, attackIndex, alreadyHitObjects, closestAttackTarget };

#if UNITY_EDITOR
        comboEnabled.Name = "AttackStateComboEnabled";
        rotationEnabled.Name = "AttackStateRotationEnabled";
        followedCombo.Name = "AttackStateFollowedCombo";
        attackIndex.Name = "AttackStateAttackIndex";
        alreadyHitObjects.Name = "AttackStateAlreadyHitObjects";
        closestAttackTarget.Name = "AttackStateClosestAttackTarget";
#endif

        foreach (IRewindable rewindableVariable in rewindableVariables) {
            rewindableVariable.OnlyExecuteOnRewindStop = true;
        }
    }

    protected override void OnEnter() {
        settings.Sword.SheathingEnabled = false;
        settings.Sword.SetSwordAnimatorLayerEnabled(false);
        
        attackInputBuffer.Clear();
        alreadyHitObjects.Clear();

        attackIndex.Value = 1;
        followedCombo.Value = false;

        rotationEnabled.Value = true;

        settings.Animator.applyRootMotion = true;
        settings.Animator.SetBool(attackHash, true);

        closestAttackTarget.Value = null;
        if(settings.PerceptionSystem.IsEnemyInsideStrafeDetectionRadius()) {
            closestAttackTarget.Value = settings.PerceptionSystem.CurrentDetectedEnemies[0].transform;
        }
    }

    protected override void OnUpdate() {
        UpdateAttackCombo();
        UpdateRotation();
        UpdateHitDetection();
    }

    protected override void OnExit() {
        settings.Sword.SheathingEnabled = true;
        settings.Sword.SetSwordAnimatorLayerEnabled(true);
        settings.Sword.SetHitboxEnabled(Bool.False);

        settings.Animator.SetBool(attackHash, false);
        settings.Animator.applyRootMotion = false;
    }

    private void UpdateAttackCombo() {
        bool wasAttackPressed = settings.InputController.WasAttackPressedThisFrame();
        float now = Time.time;
        attackInputBuffer.Push(new AttackInput(wasAttackPressed, now));

        if (comboEnabled.Value && attackIndex.Value < MAX_ATTACK_COMBO && attackInputBuffer.WasAttackPressedInLastSeconds(settings.AttackPressedBufferTime)) {
            comboEnabled.Value = false;
            attackInputBuffer.Clear();
            alreadyHitObjects.Clear();
            settings.Animator.SetTrigger(nextComboAttackHash);
            rotationEnabled.Value = true;
            attackIndex.Value++;
            followedCombo.Value = true;
        }
    }

    private void UpdateRotation() {
        if (rotationEnabled.Value) {
            Vector3  moveDirection = Vector3.zero;
            if (closestAttackTarget.Value == null) { // No target near, use input direction
                Vector2 inputDirection = settings.InputController.GetMoveDirection();
                moveDirection = mainCamera.transform.TransformDirection(inputDirection.x, 0, inputDirection.y);
                
            } else { // target near, lock attack direction towards target
                moveDirection = (closestAttackTarget.position - settings.Transform.position);
            }
            moveDirection.y = 0;
            moveDirection.Normalize();

            Quaternion currentRotation = settings.CharacterMovement.Transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            if(moveDirection.magnitude > 0) {
                Quaternion newRotation = Quaternion.Slerp(currentRotation, targetRotation, settings.RotationSpeed*Time.deltaTime);
                settings.CharacterMovement.SetRotation(newRotation);
            }
        }
    }

    private void UpdateHitDetection() {
        HitData[] hitsData = settings.Sword.CheckHit(); 
        if (hitsData != null) {
            foreach(HitData hitData in hitsData) {
                IHittable hittableObject = hitData.hittableObject;
                if (!alreadyHitObjects.Contains(hittableObject)) {
                    hittableObject.Hit();
                    alreadyHitObjects.Add(hittableObject);

                    if(hittableObject is IDamageable) {
                        IDamageable damageableObject = (IDamageable)hittableObject;
                        if (damageableObject.CanBeDamaged()) {
                            if (damageableObject is IShieldable && ((IShieldable)damageableObject).IsShielded() && HittableObjectIsFacingAttacker(hitData.hittableObject)) {
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

    public void SetComboEnabled(bool enabled) {
        comboEnabled.Value = enabled;
        if (enabled) {
            followedCombo.Value = false;
        } else {
            if (!followedCombo.Value) {
                AttackEnded.Invoke();
            }
        }
    }

    public void SetRotationEnabled(bool enabled) {
        rotationEnabled.Value = enabled; 
    }

    private void OnAnimationEnded(int shortNameHash) {
        if (shortNameHash == comboAttackNameHashes[MAX_ATTACK_COMBO - 1]) { // Last combo attack finished
            AttackEnded.Invoke();
        } else if (!followedCombo.Value) { // Other combo attacks finished but player didn't keep pressing attack
            foreach (int comboAttackNameHash in comboAttackNameHashes) {
                if (comboAttackNameHash == shortNameHash) {
                    AttackEnded.Invoke();
                    break;
                }
            }
        }

        /* Cant disable hitbox at OnExit() function because the animation event that enables the hitbox is fired after 
         * exiting this state, that is, while transitioning to a different animation state. Since the event at the end of 
         * the animation won't be fired if the transition ends before, here's where the hitbox is disabled safely */
        if (shortNameHash == AnimatorUtils.attack1Hash || shortNameHash == AnimatorUtils.attack2Hash || shortNameHash == AnimatorUtils.attack3Hash) {
            settings.Sword.SetHitboxEnabled(Bool.False);
        }
        
    }

    private bool HittableObjectIsFacingAttacker(IHittable hittableObject) {
        Vector2 attackedHorizontalForward = hittableObject.GetTransform().forward.XZ().normalized;
        Vector2 attackerHorizontalForward = settings.Transform.forward.XZ().normalized;

        return Vector2.Angle(attackerHorizontalForward, -attackedHorizontalForward) < settings.BlockableAttackAngleThreshold;
    }

    public override void RestoreFieldsAndProperties(object stateObjectRecord) {
        /*attackInputBuffer.Clear();

        AttackStateRecord record = (AttackStateRecord)stateObjectRecord;
        attackIndex.Value = record.attackIndex;
        comboEnabled.Value = record.comboEnabled;
        rotationEnabled.Value = record.rotationEnabled;
        followedCombo.Value = record.followedCombo;
        closestAttackTarget.Value = record.closestAttackTarget;*/
        //alreadyHitObjects = new HashSet<IHittable>(record.alreadyHitObjects);
    }

    public override object RecordFieldsAndProperties() {
        //IHittable[] alreadyHitObjects = new IHittable[this.alreadyHitObjects.Count];
        //this.alreadyHitObjects.CopyTo(alreadyHitObjects);
        return null;// new AttackStateRecord(attackIndex, comboEnabled, rotationEnabled, followedCombo, alreadyHitObjects, closestAttackTarget);
    }

    
}
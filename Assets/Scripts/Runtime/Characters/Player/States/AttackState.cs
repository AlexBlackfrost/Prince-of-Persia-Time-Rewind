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
        [field: SerializeField] public float AttackPressedBufferTime { get; private set; }  = 0.3f;
        [field: SerializeField] public float RotationSpeed { get; private set; }  = 5f;
        
    }

    public Action AttackEnded;
    private const int ATTACK_PRESSED_BUFFER_SIZE = 500;
    private const int MAX_ATTACK_COMBO = 3;
    private AttackSettings settings;
    private int attackHash;
    private int nextComboAttackHash;
    private int [] comboAttackNameHashes;
    private AttackInputBuffer attackInputBuffer;
    private bool comboEnabled;
    private int attackIndex;
    private bool followedCombo;
    private bool rotationEnabled;
    private Camera mainCamera;
    private HashSet<IHittable> alreadyHitObjects;

    public AttackState(AttackSettings settings) : base() {
        this.settings = settings;
        mainCamera = Camera.main;
        settings.Sword.OnSetComboEnabled = SetComboEnabled;
        settings.Sword.OnSetRotationEnabled = SetRotationEnabled;
        comboEnabled = false;
        attackInputBuffer = new AttackInputBuffer(ATTACK_PRESSED_BUFFER_SIZE);
        alreadyHitObjects = new HashSet<IHittable>();

        attackHash = Animator.StringToHash("Attack");
        nextComboAttackHash = Animator.StringToHash("NextComboAttack");
        comboAttackNameHashes = new int[MAX_ATTACK_COMBO];
        comboAttackNameHashes[0] = Animator.StringToHash("Attack1");
        comboAttackNameHashes[1] = Animator.StringToHash("Attack2");
        comboAttackNameHashes[2] = Animator.StringToHash("Attack3");
        AnimatorUtils.AnimationEnded += OnAnimationEnded;
    }

    protected override void OnEnter() {
        settings.Sword.SheathingEnabled = false;
        settings.Sword.SetSwordAnimatorLayerEnabled(false);
        
        attackInputBuffer.Clear();
        alreadyHitObjects.Clear();
        
        attackIndex = 1;
        followedCombo = false;

        rotationEnabled = true;

        settings.Animator.applyRootMotion = true;
        settings.Animator.SetBool(attackHash, true);
    }

    protected override void OnUpdate() {
        UpdateAttackCombo();
        UpdateRotation();
        UpdateHitDetection();
    }

    protected override void OnExit() {
        settings.Sword.SheathingEnabled = true;
        settings.Sword.SetSwordAnimatorLayerEnabled(true);

        settings.Animator.SetBool(attackHash, false);
        settings.Animator.applyRootMotion = false;
    }

    private void UpdateAttackCombo() {
        bool wasAttackPressed = settings.InputController.WasAttackPressedThisFrame();
        float now = Time.time;
        attackInputBuffer.Push(new AttackInput(wasAttackPressed, now));

        if (comboEnabled && attackIndex < MAX_ATTACK_COMBO && attackInputBuffer.WasAttackPressedInLastSeconds(settings.AttackPressedBufferTime)) {
            comboEnabled = false;
            attackInputBuffer.Clear();
            alreadyHitObjects.Clear();
            settings.Animator.SetTrigger(nextComboAttackHash);
            rotationEnabled = true;
            attackIndex++;
            followedCombo = true;
        }
    }

    private void UpdateRotation() {
        if (rotationEnabled) {
            Vector2 inputDirection = settings.InputController.GetMoveDirection();
            Vector3 moveDirection = mainCamera.transform.TransformDirection(inputDirection.x, 0, inputDirection.y);
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
                        ((IDamageable)hittableObject).ReceiveDamage(settings.Sword.Damage);
                    }
                }
            }
        }
    }

    public void SetComboEnabled(bool enabled) {
        comboEnabled = enabled;
        if (enabled) {
            followedCombo = false;
        } else {
            if (!followedCombo) {
                AttackEnded.Invoke();
            }
        }
    }

    public void SetRotationEnabled(bool enabled) {
        rotationEnabled = enabled; 
    }

    private void OnAnimationEnded(int shortNameHash) {
        if(shortNameHash == comboAttackNameHashes[MAX_ATTACK_COMBO - 1]) { // Last combo attack finished
            AttackEnded.Invoke();
        } else if (!followedCombo){ // Other combo attacks finished but player didn't keep pressing attack
            foreach(int comboAttackNameHash in comboAttackNameHashes) {
                if (comboAttackNameHash == shortNameHash) {
                    AttackEnded.Invoke();
                    break;
                }
            }
        }
    }

    public override void RestoreFieldsAndProperties(object stateObjectRecord) {
        attackInputBuffer.Clear();

        AttackStateRecord record = (AttackStateRecord)stateObjectRecord;
        attackIndex = record.attackIndex;
        comboEnabled = record.comboEnabled;
        rotationEnabled = record.rotationEnabled;
        followedCombo = record.followedCombo;

        alreadyHitObjects = new HashSet<IHittable>(record.alreadyHitObjects);
    }

    public override object RecordFieldsAndProperties() {
        IHittable[] alreadyHitObjects = new IHittable[this.alreadyHitObjects.Count];
        this.alreadyHitObjects.CopyTo(alreadyHitObjects);
        return new AttackStateRecord(attackIndex, comboEnabled, rotationEnabled, followedCombo, alreadyHitObjects);
    }
}
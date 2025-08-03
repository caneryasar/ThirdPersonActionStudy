using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour {
    
    private static readonly int MoveLockOn = Animator.StringToHash("MoveLockOn");
    private static readonly int MoveFree = Animator.StringToHash("MoveFree");
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int DirectionX = Animator.StringToHash("DirectionX");
    private static readonly int DirectionY = Animator.StringToHash("DirectionY");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Combo = Animator.StringToHash("Combo");
    private static readonly int IsPlayable = Animator.StringToHash("isPlayable");

    private string _stateTransition;
    private bool _isPlayable = false;
    
    private Animator _animator;

    private EventArchive _eventArchive;

    private bool _isLockedOn;
    private bool _isDummyDead = false;
    
    void Start() {
        
        _animator = GetComponent<Animator>();
        
        _animator.SetLayerWeight(2, 1);
        
        Subscribe();

        this.UpdateAsObservable().Subscribe(_ => {

            if(_animator.GetAnimatorTransitionInfo(1).IsName("Attack_1 -> Exit") ||
               _animator.GetAnimatorTransitionInfo(1).IsName("Attack_2 -> Exit") ||
               _animator.GetAnimatorTransitionInfo(1).IsName("Attack_3 -> Exit")) {
                
               _animator.SetLayerWeight(1, 0); 
            }
            
            if(_animator.GetCurrentAnimatorStateInfo(1).IsName("Empty")) { _eventArchive.gameplay.InvokeOnAttackComboCount(0); }
            if(_animator.GetAnimatorTransitionInfo(1).IsName("Empty -> Attack_1")) { _eventArchive.gameplay.InvokeOnAttackComboCount(1); }
            if(_animator.GetAnimatorTransitionInfo(1).IsName("Attack_1 -> Attack_2")) { _eventArchive.gameplay.InvokeOnAttackComboCount(2); }
            if(_animator.GetAnimatorTransitionInfo(1).IsName("Attack_2 -> Attack_3")) { _eventArchive.gameplay.InvokeOnAttackComboCount(3); }
            
            if(_animator.GetLayerWeight(1) == 0) { _animator.SetBool(Attack, false);}
        });
    }

    private void Subscribe() {

        _eventArchive = FindFirstObjectByType<EventArchive>();


        _eventArchive.gameplay.OnPlayable += SetLayer; 
        _eventArchive.playerInputs.OnMove += AnimateMove;
        _eventArchive.playerInputs.OnLockOn += LockedOn;
        _eventArchive.gameplay.OnAttacking += Attacking;
        _eventArchive.gameplay.OnCombo += CanCombo;
        _eventArchive.dummyEvents.OnDeath += () => _isDummyDead = true;
        _eventArchive.dummyEvents.OnRespawn += () => _isDummyDead = false;
    }

    private void SetLayer(bool status) {

        _isPlayable = status;
        
        _animator.SetLayerWeight(2, 0);
        _animator.SetBool(IsPlayable, status);
        
    }

    private void LockedOn() {
        
        if(_isDummyDead) { return;}
        
        _isLockedOn = !_isLockedOn;
    }


    private void Attacking() {
        
        _animator.SetLayerWeight(1, 1);
        
        _animator.SetBool(Attack, true);
    }
    
    private void CanCombo(bool isComboing) {
        
        _animator.SetBool(Combo, isComboing);

        if(isComboing) { return; }
        
        _animator.SetBool(Attack, false);
        
    }

    private void AnimateMove(Vector2 direction) {
        
        if(!_isPlayable) { return; }

        if(direction != Vector2.zero) {

            if(_isDummyDead) { _isLockedOn = false; }

            if(_isLockedOn) {
                
                _animator.SetBool(MoveFree, false);
                _animator.SetBool(MoveLockOn, true);
            }
            else {
                
                _animator.SetBool(MoveFree, true);
                _animator.SetBool(MoveLockOn, false);
            }
            
            _animator.SetBool(Idle, false);
            
            _animator.SetFloat(DirectionX, direction.x);
            _animator.SetFloat(DirectionY, direction.y);
        }
        else {
            
            _animator.SetBool(Idle, true);
            _animator.SetBool(MoveFree, false);
            _animator.SetBool(MoveLockOn, false);
        }
    }
}
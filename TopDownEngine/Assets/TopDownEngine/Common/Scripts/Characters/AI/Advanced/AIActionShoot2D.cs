﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// An Action that shoots using the currently equipped weapon. If your weapon is in auto mode, will shoot until you exit the state, and will only shoot once in SemiAuto mode. You can optionnally have the character face (left/right) the target, and aim at it (if the weapon has a WeaponAim component).
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionShoot2D")]
    //[RequireComponent(typeof(CharacterOrientation2D))]
    //[RequireComponent(typeof(CharacterHandleWeapon))]
    public class AIActionShoot2D : AIAction
    {
        /// if true, the Character will face the target (left/right) when shooting
        [Tooltip("if true, the Character will face the target (left/right) when shooting")]
        public bool FaceTarget = true;
        /// if true the Character will aim at the target when shooting
        [Tooltip("if true the Character will aim at the target when shooting")]
        public bool AimAtTarget = false;

        protected CharacterOrientation2D _orientation2D;
        protected Character _character;
        protected CharacterHandleWeapon _characterHandleWeapon;
        protected WeaponAim _weaponAim;
        protected ProjectileWeapon _projectileWeapon;
        protected Vector3 _weaponAimDirection;
        protected int _numberOfShoots = 0;
        protected bool _shooting = false;

        /// <summary>
        /// On init we grab our CharacterHandleWeapon ability
        /// </summary>
        protected override void Initialization()
        {
            _character = GetComponentInParent<Character>();
            _orientation2D = _character?.FindAbility<CharacterOrientation2D>();
            _characterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon>();
        }

        /// <summary>
        /// On PerformAction we face and aim if needed, and we shoot
        /// </summary>
        public override void PerformAction()
        {
            MakeChangesToTheWeapon();
            TestFaceTarget();
            TestAimAtTarget();
            Shoot();
        }

        /// <summary>
        /// Sets the current aim if needed
        /// </summary>
        protected virtual void Update()
        {
            if (_characterHandleWeapon.CurrentWeapon != null)
            {
                if (_weaponAim != null)
                {
                    if (_shooting)
                    {
                        _weaponAim.SetCurrentAim(_weaponAimDirection);
                    }
                    else
                    {
                        if (_orientation2D != null)
                        {
                            if (_orientation2D.IsFacingRight)
                            {
                                _weaponAim.SetCurrentAim(Vector3.right);
                            }
                            else
                            {
                                _weaponAim.SetCurrentAim(Vector3.left);
                            }
                        }                        
                    }
                }
            }
        }

        /// <summary>
        /// Makes changes to the weapon to ensure it works ok with AI scripts
        /// </summary>
        protected virtual void MakeChangesToTheWeapon()
        {
            if (_characterHandleWeapon.CurrentWeapon != null)
            {
                _characterHandleWeapon.CurrentWeapon.TimeBetweenUsesReleaseInterruption = true;
            }
        }

        /// <summary>
        /// Faces the target if required
        /// </summary>
        protected virtual void TestFaceTarget()
        {
            if (!FaceTarget)
            {
                return;
            }

            if (this.transform.position.x > _brain.Target.position.x)
            {
                _orientation2D.FaceDirection(-1);
            }
            else
            {
                _orientation2D.FaceDirection(1);
            }            
        }

        /// <summary>
        /// Aims at the target if required
        /// </summary>
        protected virtual void TestAimAtTarget()
        {
            if (!AimAtTarget)
            {
                return;
            }

            if (_characterHandleWeapon.CurrentWeapon != null)
            {
                if (_weaponAim == null)
                {
                    _weaponAim = _characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
                }

                if (_weaponAim != null)
                {
                    if (_projectileWeapon != null)
                    {
                        _projectileWeapon.DetermineSpawnPosition();
                        _weaponAimDirection = _brain.Target.position - _projectileWeapon.SpawnPosition;
                    }
                    else
                    {
                        _weaponAimDirection = _brain.Target.position - _character.transform.position;
                    }                    
                }                
            }
        }

        /// <summary>
        /// Activates the weapon
        /// </summary>
        protected virtual void Shoot()
        {
            if (_numberOfShoots < 1)
            {
                _characterHandleWeapon.ShootStart();
                _numberOfShoots++;
            }
        }

        /// <summary>
        /// When entering the state we reset our shoot counter and grab our weapon
        /// </summary>
        public override void OnEnterState()
        {
            base.OnEnterState();
            _numberOfShoots = 0;
            _shooting = true;
            _weaponAim = _characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
            _projectileWeapon = _characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<ProjectileWeapon>();
        }

        /// <summary>
        /// When exiting the state we make sure we're not shooting anymore
        /// </summary>
        public override void OnExitState()
        {
            base.OnExitState();
            _characterHandleWeapon.ForceStop();
            _shooting = false;
        }
    }
}

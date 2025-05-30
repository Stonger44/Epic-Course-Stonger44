﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameDevHQ.FileBase.Gatling_Gun
{
    /// <summary>
    /// This script will allow you to view the presentation of the Turret and use it within your project.
    /// Please feel free to extend this script however you'd like. To access this script from another script
    /// (Script Communication using GetComponent) -- You must include the namespace (using statements) at the top. 
    /// "using GameDevHQ.FileBase.Gatling_Gun" without the quotes. 
    /// 
    /// For more, visit GameDevHQ.com
    /// 
    /// @authors
    /// Al Heck
    /// Jonathan Weinberger
    /// </summary>

    [RequireComponent(typeof(AudioSource))] //Require Audio Source component
    public class Gatling_Gun : MonoBehaviour
    {
        private Transform _gunBarrel; //Reference to hold the gun barrel
        public GameObject Muzzle_Flash; //reference to the muzzle flash effect to play when firing
        public ParticleSystem bulletCasings; //reference to the bullet casing effect to play when firing
        public AudioClip fireSound; //Reference to the audio clip

        private AudioSource _audioSource; //reference to the audio source component
        private bool _startWeaponNoise = true;

        //Extended Code
        public int WarFundCost { get; set; } = 500;
        public int WarFundSellValue { get; set; } = 250;

        private bool _isAttacking;
        [SerializeField] private int _damageAmount;

        public static event Action<GameObject, int> onShoot;

        private void OnEnable()
        {
            Aim.onTargetInRange += Shoot;
            Aim.onNoTargetInRange += StopShooting;
        }

        private void OnDisable()
        {
            Aim.onTargetInRange -= Shoot;
            Aim.onNoTargetInRange -= StopShooting;
        }

        // Use this for initialization
        void Start()
        {
            _gunBarrel = GameObject.Find("Barrel_to_Spin").GetComponent<Transform>(); //assigning the transform of the gun barrel to the variable
            Muzzle_Flash.SetActive(false); //setting the initial state of the muzzle flash effect to off
            _audioSource = GetComponent<AudioSource>(); //Assign the Audio Source to the reference variable
            _audioSource.playOnAwake = false; //disabling play on awake
            _audioSource.loop = true; //making sure our sound effect loops
            _audioSource.clip = fireSound; //assign the clip to play
        }

        // Update is called once per frame
        void Update()
        {

        }

        // Method to rotate gun barrel 
        void RotateBarrel() 
        {
            _gunBarrel.transform.Rotate(Vector3.forward * Time.deltaTime * -500.0f); //rotate the gun barrel along the "forward" (z) axis at 500 meters per second
        }

        private void Shoot(GameObject attackingTower, GameObject currentTarget)
        {
            if (attackingTower == this.gameObject && currentTarget.tag.Contains("Mech"))
            {
                if (!_isAttacking)
                {
                    _isAttacking = true;
                    StartCoroutine(AttackRoutine(currentTarget));
                }

                RotateBarrel(); //Call the rotation function responsible for rotating our gun barrel
                Muzzle_Flash.SetActive(true); //enable muzzle effect particle effect
                bulletCasings.Emit(1); //Emit the bullet casing particle effect  

                if (_startWeaponNoise == true) //checking if we need to start the gun sound
                {
                    _audioSource.Play(); //play audio clip attached to audio source
                    _startWeaponNoise = false; //set the start weapon noise value to false to prevent calling it again
                }
            }
        }

        private void StopShooting(GameObject attackingTower)
        {
            if (attackingTower == this.gameObject)
            {
                Muzzle_Flash.SetActive(false); //turn off muzzle flash particle effect
                _audioSource.Stop(); //stop the sound effect from playing
                _startWeaponNoise = true; //set the start weapon noise value to true 
            }
        }

        private IEnumerator AttackRoutine(GameObject currentTarget)
        {
            onShoot?.Invoke(currentTarget, _damageAmount);
            yield return new WaitForSeconds(1);

            _isAttacking = false;
        }
    }

}

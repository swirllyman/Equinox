using UnityEngine;
using UnityEngine.XR;
using Cinemachine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

namespace Asymmetry
{
    /// <summary>
    /// Primary Class for handling Asymmetrical Gameplay
    /// </summary>
    public class AsymmetricCrossPlatformController : MonoBehaviour
    {
        public static AsymmetricCrossPlatformController singleton;
        public AudioMixerGroup mixerGroup;
        public TMP_Text lobbyText;
        public TMP_Text pickupText;
        public TMP_Text movementText;
        [Header("Place VR Player Platform Here")]
        public GameObject vrPlayerObject;
        public AudioListener vrListener;

        [Header("Place PC Player Platform Here")]
        public GameObject pcPlayerObject;
        public AudioListener pcListener;

        [Header("Desktop View Camera")]
        public GameObject desktopCamera;
        public CinemachineFreeLook cinemachineData;
        public CinemachineBrain cinemachineBrain;

        [Header("UI")]
        public GameObject[] gameButtons;
        public Slider mySlider;
        public GameObject menuObject;

        /// <summary>
        /// Used to determine setup for online state
        /// {0 = VR, 1 = PC, 2 = Both (Asymmetric)}
        /// </summary>
        internal int playerType = 0;

        AsymmetricPlayerPlatform platform;

        private void Awake()
        {
            singleton = this;
            platform = GetComponent<AsymmetricPlayerPlatform>();

            //Check if we have a VR device
            if (XRSettings.isDeviceActive)
            {
                SetupVR();
                
                //If VR and android, make sure to disable asymmetric rendering stuff.
                if (Application.platform == RuntimePlatform.Android)
                {
                    desktopCamera.SetActive(false);
                }
            }
            else
            {
                SetupPC();
            }
        }

        private void Start()
        {
            if (!PlayerPrefs.HasKey("_Volume"))
                PlayerPrefs.SetFloat("_Volume", .5f);

            mySlider.value = PlayerPrefs.GetFloat("_Volume");

            menuObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                menuObject.SetActive(!menuObject.activeInHierarchy);

                if (menuObject.activeInHierarchy)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    cinemachineData.m_XAxis.m_MaxSpeed = 0;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    cinemachineData.m_XAxis.m_MaxSpeed = 3.0f;
                }
            }
        }

        //NOTE: Most of the setup calls are tied into Unity Canvas Buttons
        #region Platform Setup

        /// <summary>
        /// Sets up the VR Controller. If a PC player exists at this point they are removed.
        /// </summary>
        public void SetupVR()
        {
            lobbyText.text = "Press (A) To Play!";
            pickupText.text = "- Item Interaction: Grip";
            movementText.text = "- Movement: Joysticks";
            cinemachineData.Follow = platform.platform_VR.head;
            cinemachineData.LookAt = platform.platform_VR.head;
            cinemachineData.m_XAxis.m_MaxSpeed = 0.0f;
            playerType = 0;

            vrListener.enabled = true;
            pcListener.enabled = false;
           
            vrPlayerObject.SetActive(true);
            pcPlayerObject.SetActive(false);

            cinemachineBrain.m_WorldUpOverride = platform.platform_VR.playerMovement.transform;
            if (Application.platform != RuntimePlatform.Android)
            {
                gameButtons[0].SetActive(true);
                gameButtons[1].SetActive(false);
            }
        }

        /// <summary>
        /// Sets up the PC Controller. If a VR player exists at this point they are removed.
        /// </summary>
        public void SetupPC()
        {
            if (XRSettings.isDeviceActive)
            {
                gameButtons[0].SetActive(false);
                gameButtons[1].SetActive(true);
            }
            else
            {
                gameButtons[0].SetActive(false);
                gameButtons[1].SetActive(false);
            }


            lobbyText.text = "Press (Enter) To Play!";
            pickupText.text = "- Item Interaction: Q/E";
            movementText.text = "- Movement: WASD";

            playerType = 1;

            vrListener.enabled = false;
            pcListener.enabled = true;
            vrPlayerObject.SetActive(false);
            pcPlayerObject.SetActive(true);
            
            cinemachineData.m_XAxis.m_MaxSpeed = 3.0f;
            cinemachineData.Follow = platform.platform_PC.playerMovement.transform;
            cinemachineData.LookAt = platform.platform_PC.playerMovement.transform;

            cinemachineBrain.m_WorldUpOverride = platform.platform_PC.playerMovement.transform;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// Adds in a PC player for Asymmetric gameplay.
        /// </summary>
        public void AddPCPlayer()
        {
            pcPlayerObject.SetActive(true);
            playerType = 2;

            cinemachineData.m_XAxis.m_MaxSpeed = 3.0f;
            cinemachineData.Follow = platform.platform_PC.head;
            cinemachineData.LookAt = platform.platform_PC.head;
        }
        #endregion
    
        public void UpdateMasterVolume(float perc)
        {
            PlayerPrefs.SetFloat("_Volume", perc);
            float amount = Mathf.Log10(perc) * 20;
            mixerGroup.audioMixer.SetFloat("MasterVolume", amount);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
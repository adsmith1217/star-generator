using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager instance;

    [SerializeField] bool debugMode = false;
    [SerializeField] bool demoMode = false;

    [Header("Main Star:")]
    [SerializeField] GameObject mainStarParent;
    [SerializeField] GameObject mainStarObject;
    [SerializeField] Material mainStarMaterial;
    [SerializeField] ParticleSystem mainStarParticles;
    [SerializeField] UnityEngine.Rendering.Universal.Light2D mainStarLight;
    [SerializeField] UnityEngine.Rendering.Universal.Light2D mainStarLight2;
    [SerializeField] float minScale = 3f;
    [SerializeField] float maxScale = 8f;
    [SerializeField] float minRotationSpeed = 2f;
    [SerializeField] float maxRotationSpeed = 8f;
    [SerializeField] float minCellDensity = 10f;
    [SerializeField] float maxCellDensity = 100f;
    private float rotationSpeed;
    private bool reverseRotation;

    [Header("Background Stars:")]
    [SerializeField] Material nearStars;
    [SerializeField] Material farStars;
    [SerializeField] float twinkleSpeed = 0.0005f;

    [Header("Star Zoom Effect:")]
    [SerializeField] float startingStarsScale = 10f;
    [SerializeField] float minStarsScale = 0.0001f;
    [SerializeField] float zoomSpeed = 4f;
    private float currentStarsScale;
    private Vector3 currentMainStarScale;
    private bool zooming = false;
    private bool flipZoom = false;

    [Header("Parallax Effect:")]
    [SerializeField] GameObject shootingStarObject;
    [SerializeField] float mainStarParallaxFactor = 0.001f;
    [SerializeField] float shootingStarParallaxFactor = 0.01f;
    [SerializeField] float nearStarsParallaxFactor = 0.01f;
    [SerializeField] float farStarsParallaxFactor = 0.005f;
    [SerializeField] GameObject cameraObject;
    [SerializeField] CinemachineVirtualCamera virtualCam;
    [SerializeField] float mainStarXOffset = 7f;
    [SerializeField] float mainStarYOffset = 3.5f;
    private Vector3 cameraStartPos;
    private Vector3 mainStarStartScale;
    private Vector3 shootingStarStartScale;
    private float cameraStartOrtho;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        currentStarsScale = startingStarsScale;
        cameraStartPos = cameraObject.transform.position;
        currentMainStarScale = mainStarStartScale = mainStarParent.transform.localScale;
        shootingStarStartScale = shootingStarObject.transform.localScale;
        cameraStartOrtho = virtualCam.m_Lens.OrthographicSize;
        if(demoMode) {
            InvokeRepeating("StarDemo", 3f, 3f);
        }
    }

    private void StarDemo() {
        GameManager.instance.currentStar = Star.CreateDummyStar();
        ResetBackground(true, false);
    }

    private void Update() {
        ParallaxEffect();
        StarTwinkleEffect();
        RotateMainStar();
        if(zooming) {
            StarZoomEffect();
        }
    }

    public void HandleSceneChange() {
        if(GameSceneManager.instance == null || GameSceneManager.instance.currentScene == GlobalStrings.MAINMENU) {
            if(debugMode) Debug.Log("Resetting background for main menu");
            ResetBackground(false, true);
        } else if (GameSceneManager.instance.currentScene == GlobalStrings.HOMESTATION) {
            if(debugMode) Debug.Log("Resetting background for home station");
            GameManager.instance.currentStar = Star.CreateDummyStar();
            ResetBackground(true, false);
        } else if(GameSceneManager.instance.currentScene == GlobalStrings.SPACE) {
            if(debugMode) Debug.Log("Resetting background for space");
            if(GameManager.instance.currentStar.starSeed == 0) GameManager.instance.currentStar = Star.CreateDummyStar();
            ResetBackground(true, true);
        }
    }

    private void ResetBackground(bool showMainStar, bool resetBackgroundStars) {
        // Main Star
        if(showMainStar) {
            mainStarParent.SetActive(false);

            float newScale = Random.Range(minScale, maxScale);
            rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
            mainStarObject.transform.localScale = new Vector3(newScale, newScale, newScale);
            reverseRotation = Utils.RandomBoolean();
            Color c = GameManager.instance.currentStar.starColor;

            // Material
            mainStarMaterial.SetColor(GlobalStrings.BASECOLOR, c);
            mainStarMaterial.SetVector(GlobalStrings.RANDOMSEED, new Vector2(Random.Range(1f, 360f), Random.Range(1f, 360f)));
            mainStarMaterial.SetFloat(GlobalStrings.CELLDENSITY, Random.Range(minCellDensity, maxCellDensity));

            // Particles
            ParticleSystem.MainModule mm = mainStarParticles.main;
            mm.startColor = c;
            var trails = mainStarParticles.trails;
            trails.colorOverLifetime = c;
            var vOL = mainStarParticles.velocityOverLifetime;
            vOL.orbitalX = Random.Range(0f, 1f);
            vOL.orbitalY = Random.Range(0f, 1f);
            vOL.orbitalZ = Random.Range(0f, 1f);

            // Light
            c.a = 0.3f;
            mainStarLight.color = c;

            mainStarParent.SetActive(true);
        } else {
            mainStarParent.SetActive(false);
        }

        // Background Stars
        if(resetBackgroundStars) {
            nearStars.SetFloat(GlobalStrings.RANDOMNESS, Random.Range(1f, 5f));
            farStars.SetFloat(GlobalStrings.RANDOMNESS, Random.Range(1f, 5f));
            nearStars.SetFloat(GlobalStrings.STARSSCALE, startingStarsScale);
            farStars.SetFloat(GlobalStrings.STARSSCALE, startingStarsScale);
            nearStars.SetFloat(GlobalStrings.BRIGHTNESSVARIATIONSCALE, Random.Range(0.01f, 0.2f));
            farStars.SetFloat(GlobalStrings.BRIGHTNESSVARIATIONSCALE, Random.Range(0.01f, 0.2f));
        }
    }

    private void ParallaxEffect() {
        // Main Star
        float mainStarXDistance = (cameraStartPos.x - cameraObject.transform.position.x) * mainStarParallaxFactor;
        float mainStarYDistance = (cameraStartPos.y - cameraObject.transform.position.y) * mainStarParallaxFactor;
        mainStarParent.transform.position = new Vector3(
                cameraObject.transform.position.x + mainStarXDistance + mainStarXOffset,
                cameraObject.transform.position.y + mainStarYDistance + mainStarYOffset,
                mainStarParent.transform.position.z);
        float orthoScale = virtualCam.m_Lens.OrthographicSize / cameraStartOrtho;
        mainStarParent.transform.localScale = mainStarStartScale * orthoScale;

        // Shooting Stars
        float shootingStarXDistance = (cameraStartPos.x - cameraObject.transform.position.x) * shootingStarParallaxFactor;
        float shootingStarYDistance = (cameraStartPos.y - cameraObject.transform.position.y) * shootingStarParallaxFactor;
        shootingStarObject.transform.position = new Vector3(
                cameraObject.transform.position.x + shootingStarXDistance,
                cameraObject.transform.position.y + shootingStarYDistance,
                shootingStarObject.transform.position.z);
        shootingStarObject.transform.localScale = shootingStarStartScale * orthoScale;

        // Background Stars
        float nearStarXDistance = (cameraStartPos.x - cameraObject.transform.position.x) * nearStarsParallaxFactor;
        float nearStarYDistance = (cameraStartPos.y - cameraObject.transform.position.y) * nearStarsParallaxFactor;
        nearStars.SetVector(GlobalStrings.OFFSET, new Vector2(nearStarXDistance, nearStarYDistance));
        float farStarXDistance = (cameraStartPos.x - cameraObject.transform.position.x) * farStarsParallaxFactor;
        float farStarYDistance = (cameraStartPos.y - cameraObject.transform.position.y) * farStarsParallaxFactor;
        farStars.SetVector(GlobalStrings.OFFSET, new Vector2(farStarXDistance, farStarYDistance));
    }

    private void StarTwinkleEffect() {
        float twinkleDirection = Random.Range(-twinkleSpeed, twinkleSpeed);
        float nearBrightness = Mathf.Clamp(
                nearStars.GetFloat(GlobalStrings.BRIGHTNESSVARIATIONSCALE) + twinkleDirection, 0.01f, 0.2f);
        float farBrightness = Mathf.Clamp(
                farStars.GetFloat(GlobalStrings.BRIGHTNESSVARIATIONSCALE) + twinkleDirection, 0.01f, 0.2f);
        nearStars.SetFloat(GlobalStrings.BRIGHTNESSVARIATIONSCALE, nearBrightness);
        farStars.SetFloat(GlobalStrings.BRIGHTNESSVARIATIONSCALE, farBrightness);
    }

    private void RotateMainStar() {
        if(reverseRotation) {
            mainStarObject.transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
        } else {
            mainStarObject.transform.Rotate(Vector3.down * Time.deltaTime * rotationSpeed);
        }
    }

    public void TriggerStarZoomEffect() {
        zooming = true;
    }

    private void StarZoomEffect() {
        if(!flipZoom) {
            mainStarParticles.gameObject.SetActive(false);
            mainStarLight.enabled = false;
            mainStarLight2.enabled = false;
            currentStarsScale -= Time.unscaledDeltaTime * zoomSpeed;
            currentMainStarScale = new Vector3(
                    currentMainStarScale.x -= currentMainStarScale.x * Time.unscaledDeltaTime * zoomSpeed,
                    currentMainStarScale.y -= currentMainStarScale.y * Time.unscaledDeltaTime * zoomSpeed,
                    currentMainStarScale.z -= currentMainStarScale.z * Time.unscaledDeltaTime * zoomSpeed);
            if(currentStarsScale <= minStarsScale) {
                flipZoom = true;
            }
        } else {
            mainStarParticles.gameObject.SetActive(true);
            mainStarLight.enabled = true;
            mainStarLight2.enabled = true;
            currentStarsScale = startingStarsScale;
            currentMainStarScale = mainStarStartScale;
            zooming = false;
            flipZoom = false;
        }
        nearStars.SetFloat(GlobalStrings.STARSSCALE, currentStarsScale);
        farStars.SetFloat(GlobalStrings.STARSSCALE, currentStarsScale);
        mainStarParent.transform.localScale = currentMainStarScale;
        shootingStarObject.transform.localScale = currentMainStarScale;
    }
}

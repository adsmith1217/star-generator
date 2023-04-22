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

    private void Awake() {
        instance = this;
    }

    private void Start() {
        if(demoMode) {
            InvokeRepeating("StarDemo", 3f, 3f);
        }
    }

    private void StarDemo() {
        GameManager.instance.currentStar = Star.CreateDummyStar();
        ResetBackground(true, false);
    }

    private void Update() {
        RotateMainStar();
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
    }

    private void RotateMainStar() {
        if(reverseRotation) {
            mainStarObject.transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
        } else {
            mainStarObject.transform.Rotate(Vector3.down * Time.deltaTime * rotationSpeed);
        }
    }
}

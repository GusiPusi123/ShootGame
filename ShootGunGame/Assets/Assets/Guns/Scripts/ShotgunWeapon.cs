using UnityEngine;

public class ShotgunWeapon : MonoBehaviour
{
    public Camera shooterCamera;
    public float range = 50f; // меньшая дальность для дробовика
    public float fireRate = 1f; // интервал между выстрелами
    public int maxAmmo = 8;
    public float reloadTime = 2f;

    public GameObject muzzleFlashEffect;
    public Transform muzzleEffectPoint;
    public GameObject hitEffectPrefab;
    public Transform hitEffectPoint;
    public AudioClip gunshotSound;
    public Animator weaponAnimator;

    public int pelletCount = 8; // количество пуль за выстрел
    public float spreadAngle = 5f; // разброс для дроби
    public float crouchedSpreadAngle = 2f;

    public Crouch crouchScript;

    private int currentAmmo;
    private float nextFireTime = 0f;
    private bool isReloading = false;
    private AudioSource audioSource;

    void Start()
    {
        currentAmmo = maxAmmo;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime && !isReloading)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
            else
            {
                Debug.Log("Нет патронов! Нажмите R для перезарядки.");
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        if (weaponAnimator != null)
            weaponAnimator.SetTrigger("Shoot");
        if (gunshotSound != null)
            audioSource.PlayOneShot(gunshotSound);
        if (muzzleFlashEffect != null && muzzleEffectPoint != null)
        {
            GameObject muzzleEffect = Instantiate(muzzleFlashEffect, muzzleEffectPoint.position, muzzleEffectPoint.rotation);
            Destroy(muzzleEffect, 1f);
        }

        float currentSpread = (crouchScript != null && crouchScript.IsCrouched) ? crouchedSpreadAngle : spreadAngle;

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 shootDirection = shooterCamera.transform.forward;
            shootDirection = ApplySpread(shootDirection, currentSpread);

            RaycastHit hit;
            Ray ray = new Ray(shooterCamera.transform.position, shootDirection);
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit, range))
            {
                targetPoint = hit.point;
                Debug.Log("Попадание в: " + hit.collider.name);
                if (hitEffectPrefab != null)
                {
                    GameObject hitEffect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(hitEffect, 2f);
                }
            }
            else
            {
                targetPoint = ray.origin + ray.direction * range;
                Debug.Log("Промах");
            }

            // Можно добавить эффекты для каждой пульки, например, трассеры или вспышки попадания
        }

        currentAmmo--;
        Debug.Log("Патроны: " + currentAmmo + "/" + maxAmmo);
    }

    Vector3 ApplySpread(Vector3 direction, float angle)
    {
        float spreadRadius = Mathf.Tan(angle * Mathf.Deg2Rad);
        Vector2 randomPoint = Random.insideUnitCircle * spreadRadius;
        Vector3 right = Vector3.Cross(direction, Vector3.up);
        if (right == Vector3.zero)
            right = Vector3.Cross(direction, Vector3.forward);
        Vector3 up = Vector3.Cross(right, direction);
        Vector3 spreadDirection = direction + right * randomPoint.x + up * randomPoint.y;
        return spreadDirection.normalized;
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Перезарядка...");
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Reload");
        }
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("Перезарядка завершена");
    }
}
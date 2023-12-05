using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Animations.Rigging;

namespace HeroicArcade.CC
{
    public class AutoAiming : MonoBehaviour
    {
        //[SerializeField] Rig rig1;
        [SerializeField] Weapon weapon;
        [SerializeField] Image crosshairRedX;
        [SerializeField] Image crosshairRedDot;
        [SerializeField] Image crosshairWhiteCircleBlank;
        [SerializeField] Image crosshairWhiteCircleDash;
        [SerializeField] Image crosshairRedHit;
        [SerializeField] Image crosshairWhiteHit;

        const float debugDrawLineDuration = 0.1f;
        //FIXME: ideally, there would be a programmatic way to know
        //  what the diameter of a crosshair is, and set this value to it.
        //FIXME: right now, the distance is a 3D distance in the game world,
        //  whereas it should be the distance of the two icons on the 2D screen space.
        const float minCrosshairDistance = 0.01f;

        Ray ray1;
        Ray ray2;
        RaycastHit hitInfo1;
        RaycastHit hitInfo2;
        Camera cam;
        Target target1;
        Target target2;

        private void Awake()
        {
            weapon.weaponCurrentEnergy.Value = weapon.weaponMaxEnergy.Value;
            target1 = null;
            target2 = null;
            crosshairWhiteCircleBlank.transform.gameObject.SetActive(false);
            crosshairWhiteCircleDash.transform.gameObject.SetActive(false);
            crosshairRedHit.transform.gameObject.SetActive(false);
            crosshairWhiteHit.transform.gameObject.SetActive(false);
            crosshairRedX.transform.gameObject.SetActive(false);
            crosshairRedDot.transform.gameObject.SetActive(false);
        }

        void Start()
        {
            cam = GetComponent<Camera>();
        }

        public Target StartAiming()
        {
            target1 = null;
            target2 = null;
            crosshairWhiteCircleBlank.transform.gameObject.SetActive(true);
            crosshairWhiteCircleDash.transform.gameObject.SetActive(false);
            crosshairRedHit.transform.gameObject.SetActive(false);
            crosshairWhiteHit.transform.gameObject.SetActive(false);
            crosshairRedX.transform.gameObject.SetActive(false);
            crosshairRedDot.transform.gameObject.SetActive(false);

            ray1.origin = transform.position;
            ray1.direction = transform.forward;
            if (Physics.Raycast(ray1, out hitInfo1))//, Mathf.Infinity, layerMask))
            {
                //Debug.DrawLine(ray1.origin, hitInfo1.point, Color.white, debugDrawLineDuration);

                target1 = hitInfo1.transform.gameObject.GetComponent<Target>();
                if (target1 == null)
                {
                    //Perform another raycast parallel to the first raycast starting from the muzzle of the gun
                    ray2.origin = weapon.muzzle.position;
                    ray2.direction = hitInfo1.point - weapon.muzzle.position;
                    if (Physics.Raycast(ray2, out hitInfo2))
                    {
                        //Debug.DrawLine(ray2.origin, hitInfo2.point, Color.red, debugDrawLineDuration);

                        crosshairWhiteCircleBlank.transform.gameObject.SetActive(false);
                        crosshairWhiteCircleDash.transform.gameObject.SetActive(true);
                        target2 = hitInfo2.transform.gameObject.GetComponent<Target>();
                        if (target2 != null)
                        {
                            crosshairRedDot.transform.gameObject.SetActive(true);
                            Vector3 screenPos = cam.WorldToScreenPoint(hitInfo2.point);
                            crosshairRedDot.transform.position = screenPos;
                        }
                    }
                }
                else // target1 != null
                {
                    //Perform another raycast from the muzzle of the gun to the hitInfo.point
                    ray2.origin = weapon.muzzle.position;
                    ray2.direction = hitInfo1.point - weapon.muzzle.position;
                    if (Physics.Raycast(ray2, out hitInfo2))
                    {
                        if (Vector3.Distance(hitInfo1.point, hitInfo2.point) < minCrosshairDistance)
                        {
                            //Debug.DrawLine(ray2.origin, hitInfo2.point, Color.green, debugDrawLineDuration);
                        }
                        else
                        {
                            //Debug.DrawLine(ray2.origin, hitInfo2.point, Color.red, debugDrawLineDuration);
                        }

                        crosshairWhiteCircleBlank.transform.gameObject.SetActive(false);
                        target2 = hitInfo2.transform.gameObject.GetComponent<Target>();
                        if (target2 != null) // and target1 != null
                        {
                            //Since both target1 and target2 are not null, the camera crosshair is a hit.
                            if (Vector3.Distance(hitInfo1.point, hitInfo2.point) < minCrosshairDistance)
                            {
                                crosshairRedHit.transform.gameObject.SetActive(true);
                            }
                            else
                            {
                                crosshairWhiteHit.transform.gameObject.SetActive(true);
                                crosshairRedDot.transform.gameObject.SetActive(true);
                                Vector3 screenPos = cam.WorldToScreenPoint(hitInfo2.point);
                                crosshairRedDot.transform.position = screenPos;
                            }
                        }
                        else // target2 == null (and target1 != null)
                        {
                            if (Vector3.Distance(hitInfo1.point, hitInfo2.point) < minCrosshairDistance)
                            {
                                //The idea here, is to have a white circle with a red cross inside (and under) it.
                                crosshairWhiteCircleBlank.transform.gameObject.SetActive(true);
                            }
                            else
                            {
                                crosshairWhiteHit.transform.gameObject.SetActive(true);
                            }
                            crosshairRedX.transform.gameObject.SetActive(true);
                            Vector3 screenPos = cam.WorldToScreenPoint(hitInfo2.point);
                            crosshairRedX.transform.position = screenPos;
                        }
                    }
                }
            }
            else
            {
                target1 = null;
                //Perform another raycast parallel to the first raycast starting from the muzzle of the gun
                ray2.origin = weapon.muzzle.position;
                ray2.direction = ray1.direction;
                if (Physics.Raycast(ray2, out hitInfo2))
                {
                    target2 = hitInfo2.transform.gameObject.GetComponent<Target>();
                    if (target2 != null)
                    {
                        //Debug.DrawLine(ray2.origin, hitInfo2.point, Color.green, debugDrawLineDuration);

                        crosshairRedDot.transform.gameObject.SetActive(true);
                        Vector3 screenPos = cam.WorldToScreenPoint(hitInfo2.point);
                        crosshairRedDot.transform.position = screenPos;
                    }
                    else
                    {
                        //Debug.DrawLine(ray2.origin, hitInfo2.point, Color.red, debugDrawLineDuration);

                        crosshairRedX.transform.gameObject.SetActive(true);
                        Vector3 screenPos = cam.WorldToScreenPoint(hitInfo2.point);
                        crosshairRedX.transform.position = screenPos;
                    }
                }
            }

            // Always use ray1, so the weapon doesn't suddely skip
            // when either target1 or target 2 are null.
            weapon.hitPoint.position = ray1.GetPoint(9999);
            //if (target1)
            //    Debug.DrawLine(weapon.muzzle.position, hitInfo1.point, Color.magenta, debugDrawLineDuration);

            return target2;
        }

        public void StopAiming()
        {
            //rig1.weight = 0;

            target1 = null;
            target2 = null;
            crosshairWhiteCircleBlank.transform.gameObject.SetActive(false);
            crosshairWhiteCircleDash.transform.gameObject.SetActive(false);
            crosshairRedHit.transform.gameObject.SetActive(false);
            crosshairWhiteHit.transform.gameObject.SetActive(false);
            crosshairRedX.transform.gameObject.SetActive(false);
            crosshairRedDot.transform.gameObject.SetActive(false);
        }

        const int N = 4;
        float distanceN;
        float initialOffset = 0;
        float offsetSpeedChange = 20f;
        TrailRenderer tracer;
        public void StartFiring(Target target)
        {
            weapon.weaponCurrentEnergy.Value -= weapon.weaponEnergyConsumption.Value;
            if (weapon.weaponCurrentEnergy.Value < 0)
            {
                weapon.weaponCurrentEnergy.Value = 0;
                return;
            }

            //rig1.weight = 1;

            weapon.muzzleFlash.Emit(1);

            weapon.hitEffect.transform.position = hitInfo2.point;
            weapon.hitEffect.transform.forward = hitInfo2.normal;
            weapon.hitEffect.Emit(1);

            distanceN = Vector3.Distance(weapon.muzzle.position, hitInfo2.point) / N;
            initialOffset += offsetSpeedChange * Time.deltaTime;
            initialOffset %= distanceN;

            tracer = Instantiate(weapon.bulletTracer, weapon.muzzle.position, weapon.muzzle.rotation);
            tracer.AddPosition(weapon.muzzle.position);
            for (var index = 0; index < N; index++)
            {
                tracer.AddPosition(ray2.GetPoint(initialOffset + index * distanceN));
            }
            tracer.AddPosition(hitInfo2.point);
            tracer.transform.position = hitInfo2.point;

            target.Hit(weapon.hitImpactStrength.Value);
        }
    }
}
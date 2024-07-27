using System;
using System.Collections;
using RaycastPro;
using RaycastPro.Casters;
using RaycastPro.RaySensors;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class GunSwap : MonoBehaviour
    {
        public static GunSwap singleton;
    
        public GameObject[] guns;

        private int index;

        [SerializeField] private TextMeshProUGUI gunText;
        [SerializeField] private TextMeshProUGUI mGunText;
        [SerializeField] private Image eGunFill;
        [SerializeField] private TextMeshProUGUI sGunText;
        [SerializeField] private TextMeshProUGUI targetText;

        public BasicCaster gun;
        public AdvanceCaster miniGun;
        public RaySensor energyGun;
        public AdvanceCaster shotgun;
        public BasicCaster trackGun;
        private void Awake()
        {
            singleton = this;
        }

        private void Start()
        {
            gun.onRate.AddListener(() =>
            {
                gunText.text = $"{gun.ammo.Amount} / {gun.ammo.MagazineAmount}";
            });
            miniGun.onRate.AddListener(() =>
            {
                mGunText.text = $"E / {miniGun.ammo.MagazineAmount}";
            });
            energyGun.onCast.AddListener(() =>
            {
                eGunFill.fillAmount = energyGun.Influence;
            });
            shotgun.onCast.AddListener(() =>
            {
                sGunText.text = $"{(shotgun.ammo.reloadTime - shotgun.ammo.currentReloadTime):F1} sec";
            });
            trackGun.onRate.AddListener(() =>
            {
                targetText.text = $"{trackGun.trackTarget.GetInstanceID()}";
            });
        }

        public void Update()
        {
            var sign = Input.mouseScrollDelta.y;
            if (sign > 0)
            {
                index = (index + 1) % guns.Length;
                OnChange(index);
            }
            else if (sign < 0)
            {
                index--;
                if (index <= 0)
                {
                    index = guns.Length - 1;
                }
                OnChange(index);
            }

        }

        public void Revive(HoverEnemy enemy, float delay)
        {
            StartCoroutine(Reviver(enemy, delay));
        }
        private IEnumerator Reviver(HoverEnemy enemy, float delay)
        {
            yield return new WaitForSeconds(delay);
            enemy.Revive();
        }
    
        public void SetTimeScale(float value)
        {
            Time.timeScale = value;
        }
        public void OnChange(Int32 value)
        {
            index = value;
            for (var i = 0; i < guns.Length; i++)
            {
                guns[i].gameObject.SetActive(false);
            }
            guns[value].gameObject.SetActive(true);
        }
    }
}

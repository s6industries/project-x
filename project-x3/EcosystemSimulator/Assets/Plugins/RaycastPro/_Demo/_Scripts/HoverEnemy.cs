using System.Collections;
using RaycastPro.Bullets;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class HoverEnemy : MonoBehaviour
    {
        public float hp = 100f;
        public float maxHp = 100f;


        public float Hp
        {
            get => hp;
            set
            {
                hp = value;
                _material.color = Color.Lerp(red, green, hp/maxHp);
            }
        }
        public ParticleSystem explosionEffect;
        public ParticleSystem reviveEffect;
    
        private MeshRenderer _meshRenderer;
        private static Color green = new Color(0.24f, 1f, 0.17f);
        private static Color red = new Color(1f, 0.12f, 0.13f);
        private Material _material;
        public  Rigidbody body;
        private Vector3 startPosition, newPosition;
        private Quaternion startRotation;

        private void Start()
        {
            body = GetComponent<Rigidbody>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _material = _meshRenderer.materials[3];

            startPosition = transform.position;
            newPosition = startPosition;
            startRotation = transform.rotation;

            StartCoroutine(RandomMove());
        }

        private IEnumerator RandomMove()
        {
            newPosition = startPosition + Random.insideUnitSphere * 2;
            yield return new WaitForSeconds(Random.Range(2, 4));
            StartCoroutine(RandomMove());
        }

        private void Update()
        {
            body.MovePosition(Vector3.Lerp(body.position, newPosition, Time.deltaTime * 2));
        }


        public void Revive()
        {
            Hp = maxHp;
            if (reviveEffect)
            {
                Instantiate(reviveEffect, transform.position, transform.rotation);
            }
        
            gameObject.SetActive(true);
            transform.position = startPosition;
            transform.rotation = startRotation;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        public void Die()
        {
            if (gameObject.activeInHierarchy)
            {
                if (explosionEffect)
                {
                    Instantiate(explosionEffect, transform.position, transform.rotation);
                }
        
                gameObject.SetActive(false);
        
                GunSwap.singleton.Revive(this, 10);
            }
        }
    
        public void TakeDamage(float amount)
        {
            Hp -= amount;
            if (hp <= 0)
            {
                Die();
            }
        }
        void OnBullet(Bullet bullet) => TakeDamage(bullet.damage);
    }
}

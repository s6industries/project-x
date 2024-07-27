using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class FreeTurn : MonoBehaviour
    {
        [SerializeField] private Vector3 moveTurn;
        [SerializeField] private  Vector3 rotateTurn;
        [SerializeField] private Vector3 randomRotate;
        [SerializeField] private Vector3 randomMove;

        private void Start()
        {
            randomMove += new Vector3(Random.value * randomMove.x, Random.value * randomMove.y,
                Random.value * randomMove.z);
            rotateTurn += new Vector3(Random.value * randomRotate.x, Random.value * randomRotate.y,
                Random.value * randomRotate.z);
        }

        private void Update()
        {
            transform.Rotate(rotateTurn*Time.deltaTime);
            transform.Translate(moveTurn * Time.deltaTime);
        }
    }
}

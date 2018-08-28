using UnityEngine;
using System.Collections;

namespace Vastan.Game
{
    public class VastanCharacter : MonoBehaviour
    {
        public CharacterController controller { get; set; }

        public GameObject head;
        public GameObject[] body_pieces;

        private Material myMaterial;
        private MaterialPropertyBlock myPropertyBlock;

        public WalkerPhysics state;

        public DampenedSpring crouchSpring;

        public Leg leftLeg;
        public Leg rightLeg;

        public Vector2 targetDirection;
        public float pitchAngle;
        public Vector2 headRot;

        public float headRestY;
        public float headHeight;

        public void Start()
        {
            controller = GetComponent<CharacterController>();
            if (GetComponent<Rigidbody>()) {
                GetComponent<Rigidbody>().freezeRotation = true;
            }

            // TODO: When the rigging is fixed, this will change
            targetDirection = new Vector2(270f, 270f);

            headRestY = head.transform.localPosition.z;
            state = new WalkerPhysics(transform, transform.localEulerAngles.y);

            SetupMaterial();
        }

        private void SetupMaterial()
        {
            myMaterial = body_pieces[0].GetComponent<Renderer>().material;
            myPropertyBlock = new MaterialPropertyBlock();
            foreach(GameObject g in body_pieces) {
                g.GetComponent<Renderer>().material = myMaterial;
            }
        }

        public void SetColor(Color c)
        {
            myMaterial.color = c;
        }

        public void WasHit(float power, float max_power) {
            state.shield -= power;
            var glow = power / max_power;
            StartCoroutine(DoGlow(glow));
            //TODO: Damage sound
        }

        private IEnumerator DoGlow(float intensity) {
            for (float f = 1f; f >=0; f-= .1f) {
                var color = Color.Lerp(Color.black, Color.white * intensity, f);
                myMaterial.SetColor(Shader.PropertyToID("_EmissionColor"), color);
                yield return new WaitForSeconds(.001f);
            }
        }
    }


}
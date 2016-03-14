using UnityEngine;
using System.Collections;


namespace LlamaCave
{

    public class Player2D : MonoBehaviour
    {

        public Rigidbody2D rigidBody;
        public float moveSpeed = 15.0f;

        Vector2 velocity;
        bool isMoving;

        // Use this for initialization
        void Start()
        {
            rigidBody = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            float horizontalAxisValue = Input.GetAxisRaw("Horizontal");
            float verticalAxisValue = Input.GetAxisRaw("Vertical");

            if (horizontalAxisValue == 0 && verticalAxisValue == 0)
            {
                isMoving = false;
            }
            else
            {
                isMoving = true;
            }
            velocity = new Vector2(horizontalAxisValue, verticalAxisValue).normalized * moveSpeed;
        }

        void FixedUpdate()
        {
            if (velocity.sqrMagnitude >= 1.0f)
            {
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg + 90.0f;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            gameObject.GetComponent<Animator>().SetBool("IsMoving", isMoving);
            rigidBody.MovePosition(rigidBody.position + (velocity * Time.fixedDeltaTime));
        }
    }
}

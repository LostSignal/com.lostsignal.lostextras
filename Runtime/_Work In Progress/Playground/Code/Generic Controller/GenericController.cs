
#if UNITY

namespace Lost
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public interface IController
    {
        void Initialize(Transform characterTransform, Camera camera, CharacterController charaterController, GenericControllerInputs input);

        void Move(float speed, float verticalVelocity, float deltaTime);

        void UpdateCameraRotation(float sensitivity, float deltaTime);

        void Enable();

        void Disable();

        void Update(float deltaTime);

        void LateUpdate(float deltaTime);
    }

    // TODO [bgish]: Need Settings System
    //   Keyboard
    //     bool InvertLook
    //     float LookSensitivity
    //     float AimSensitivity
    //   Controller
    //     bool InvertLook
    //     float LookSensitivity
    //     float AimSensitivity

    // TODO [bgish]: Create a managing class that decides if the crosshair UI should be shown
    // TODO [bgish]: Create a managing class that decides whether to show mobile controls
    // TODO [bgish]: Update GenericController to take an enum GenericControllerType to change what control type to use

    public enum GenericControllerType
    {
        ThirdPerson,
        FirstPerson,
        FixedCamera,
    }

    public class GenericController : MonoBehaviour, IOnManagersReady
    {
        private const float threshold = 0.01f;

        #pragma warning disable 0649
        [SerializeField] private GenericControllerSettings settings;
        
        [Header("References")]
        [SerializeField] private Camera characterCamera;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private GenericControllerInputs input;

        [Header("Controller Types")]
        [SerializeField] private ThirdPersonController thirdPersonController;
        [SerializeField] private FirstPersonController firstPersonController;
        [SerializeField] private FixedCameraController fixedCameraController;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        [SerializeField] private bool isGrounded = true;
        #pragma warning restore 0649

        private IController[] controllers;
        private IController currentController;
        public int currentControllerIndex;

        // Player
        private float speed;
        private float animationBlend;
        private float verticalVelocity;
        private float terminalVelocity = 53.0f;

        // Timeout deltatime
        private float jumpTimeoutDelta;
        private float fallTimeoutDelta;

        // Animation IDs
        private int animIDSpeed;
        private int animIDGrounded;
        private int animIDJump;
        private int animIDFreeFall;
        private int animIDMotionSpeed;

        private Transform characterTransform;
        private Transform characterCameraTransform;

        private bool initialized;

        public bool IsGrounded
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.isGrounded;
        }

        public float Speed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.speed;
        }

        public float MaxSpeed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.settings.SprintSpeed;
        }

        public Camera Camera
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.characterCamera;
        }

        public void ToggleControllerType()
        {
            this.UpdateSelectedController((this.currentControllerIndex + 1) % this.controllers.Length);
        }

        public void OnManagersReady()
        {
            // Assigning Animation IDs
            this.animIDSpeed = Animator.StringToHash("Speed");
            this.animIDGrounded = Animator.StringToHash("Grounded");
            this.animIDJump = Animator.StringToHash("Jump");
            this.animIDFreeFall = Animator.StringToHash("FreeFall");
            this.animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

            // Reset our timeouts on start
            this.jumpTimeoutDelta = this.settings.JumpTimeout;
            this.fallTimeoutDelta = this.settings.FallTimeout;

            // Cahcing the characters transform
            this.characterTransform = this.characterController.transform;
            this.characterCameraTransform = this.characterCamera.transform;

            // Creating the character controller types list
            this.controllers = new IController[] { this.thirdPersonController, this.firstPersonController, this.fixedCameraController };

            // Initializing all the character controller types
            for (int i = 0; i < this.controllers.Length; i++)
            {
                this.controllers[i].Initialize(this.characterTransform, this.characterCamera, this.characterController, this.input);
            }

            this.UpdateSelectedController(this.currentControllerIndex);

            this.initialized = true;

            // UpdateManager.Instance.GetChannel("GenericController.Update").RegisterCallback(this, this);
            // UpdateManager.Instance.GetChannel("GenericController.LateUpdate").RegisterCallback(this, this);
        }

        private void Awake() => ManagersReady.Register(this);

        private void Update()
        {
            if (initialized == false)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            this.JumpAndGravity(deltaTime);
            this.GroundedCheck();
            this.Move(deltaTime);

            this.currentController.Update(deltaTime);
        }

        private void LateUpdate()
        {
            if (initialized == false)
            {
                return;
            }

            float sensitivity = this.input.aim ? this.settings.AimSensitivity : this.settings.LookSensitivity;

            //// TOOD [bgish]: Also take into account users mouse/gamepad settings

            float deltaTime = Time.deltaTime;
            this.currentController.UpdateCameraRotation(sensitivity, deltaTime);
            this.currentController.LateUpdate(deltaTime);
        }

        private void UpdateSelectedController(int newIndex)
        {
            for (int i = 0; i < this.controllers.Length; i++)
            {
                this.controllers[i].Disable();
            }

            this.currentControllerIndex = newIndex;
            this.currentController = this.controllers[newIndex];
            this.currentController.Enable();
        }

        private void GroundedCheck()
        {
            // Set sphere position, with offset
            Vector3 position = this.characterTransform.position;
            Vector3 spherePosition = new Vector3(position.x, position.y - this.settings.GroundedOffset, position.z);
            this.isGrounded = Physics.CheckSphere(spherePosition, this.settings.GroundedRadius, this.settings.GroundLayers, QueryTriggerInteraction.Ignore);

            // Update animator if using character
            this.characterAnimator.SetBool(this.animIDGrounded, this.isGrounded);
        }

        private void Move(float deltaTime)
        {
            // Set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = input.aim ? this.settings.AimSpeed :
                                input.sprint ? this.settings.SprintSpeed :
                                this.settings.MoveSpeed;

            // A simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // NOTE: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (this.input.move == Vector2.zero) 
            {
                targetSpeed = 0.0f;
            }

            // A reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z).magnitude;
            float inputMagnitude = input.analogMovement ? input.move.magnitude : 1f;
            float speedOffset = 0.1f;

            // Accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // Creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                this.speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, deltaTime * this.settings.SpeedChangeRate);

                // Round speed to 3 decimal places
                this.speed = Mathf.Round(this.speed * 1000f) / 1000f;
            }
            else
            {
                this.speed = targetSpeed;
            }

            this.animationBlend = Mathf.Lerp(this.animationBlend, targetSpeed, deltaTime * this.settings.SpeedChangeRate);

            this.currentController.Move(this.speed, this.verticalVelocity, deltaTime);

            // Update animator if using character
            this.characterAnimator.SetFloat(this.animIDSpeed, this.animationBlend);
            this.characterAnimator.SetFloat(this.animIDMotionSpeed, inputMagnitude);
        }

        private void JumpAndGravity(float deltaTime)
        {
            if (this.isGrounded)
            {
                // Reset the fall timeout timer
                this.fallTimeoutDelta = this.settings.FallTimeout;

                // Update animator if using character
                this.characterAnimator.SetBool(this.animIDJump, false);
                this.characterAnimator.SetBool(this.animIDFreeFall, false);
            
                // Stop our velocity dropping infinitely when grounded
                if (this.verticalVelocity < 0.0f)
                {
                    this.verticalVelocity = -2f;
                }

                // Jump
                if (this.input.jump && this.jumpTimeoutDelta <= 0.0f)
                {
                    // The square root of H * -2 * G = how much velocity needed to reach desired height
                    this.verticalVelocity = Mathf.Sqrt(this.settings.JumpHeight * -2f * this.settings.Gravity);

                    // Update animator if using character
                    this.characterAnimator.SetBool(this.animIDJump, true);
                }

                // Jump timeout
                if (this.jumpTimeoutDelta >= 0.0f)
                {
                    this.jumpTimeoutDelta -= deltaTime;
                }
            }
            else
            {
                // Reset the jump timeout timer
                this.jumpTimeoutDelta = this.settings.JumpTimeout;

                // Fall timeout
                if (this.fallTimeoutDelta >= 0.0f)
                {
                    this.fallTimeoutDelta -= deltaTime;
                }
                else
                {
                    // Update animator if using character
                    this.characterAnimator.SetBool(this.animIDFreeFall, true);
                }

                // If we are not grounded, do not jump
                this.input.jump = false;
            }

            // Apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (this.verticalVelocity < this.terminalVelocity)
            {
                this.verticalVelocity += this.settings.Gravity * deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) 
            {
                lfAngle += 360f;
            }

            if (lfAngle > 360f) 
            {
                lfAngle -= 360f;
            }

            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = this.isGrounded ? transparentGreen : transparentRed;

            // When selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            if  (this.characterTransform != null)
            {
                Vector3 position = this.characterTransform.position;
                Gizmos.DrawSphere(new Vector3(position.x, position.y - this.settings.GroundedOffset, position.z), this.settings.GroundedRadius);
            }
        }

        [Serializable]
        private class ThirdPersonController : IController
        {
            [Header("Rotation")]
            [Tooltip("How fast the character turns to face movement direction")]
            [Range(0.0f, 0.3f)]
            [SerializeField] private float rotationSmoothTime = 0.12f;
                        
            [Header("Cinemachine")]
            [Tooltip("The Cinemachine Virtual Camera this controller uses.")]
            [SerializeField] private GameObject cinemachineCamera;
            
            [Tooltip("The Cinemachine Virtual Camera this controller uses for aiming.")]
            [SerializeField] private GameObject cinemachineAimCamera;

            [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
            [SerializeField] private GameObject cinemachineCameraTarget;

            [Tooltip("How far in degrees can you move the camera up")]
            [SerializeField] private float topClamp = 70.0f;

            [Tooltip("How far in degrees can you move the camera down")]
            [SerializeField] private float bottomClamp = -30.0f;

            [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
            [SerializeField] private float cameraAngleOverride = 0.0f;

            [Tooltip("For locking the camera position on all axis")]
            [SerializeField] private bool lockCameraPosition = false;

            [Header("Temp Projectile Code")]
            [SerializeField] private Transform aimTargetTraransform;
            [SerializeField] private Transform shootTraransform;
            [SerializeField] private BulletProjectile bulletProjectilePrefab;
            [SerializeField] private HybridBulletProjectile hybridBulletProjectilePrefab;

            private Transform characterTransform;
            private CharacterController characterController;
            private GenericControllerInputs input;

            private Camera camera;
            private Transform cameraTransform;

            private float cinemachineTargetYaw;
            private float cinemachineTargetPitch;
            private float targetRotation = 0.0f;
            private float rotationVelocity = 0.0f;
            private bool currentAimValue;

            public void Initialize(Transform characterTransform, Camera camera, CharacterController characterController, GenericControllerInputs input)
            {
                this.camera = camera;
                this.cameraTransform = camera.transform;
                this.characterTransform = characterTransform;
                this.characterController = characterController;
                this.input = input;
            }

            public void Enable()
            {
                this.SetActive(true);
            }

            public void Disable()
            {
                this.SetActive(false);
            }

            public void Update(float deltaTime)
            {
                this.UpdateAim(deltaTime);
            }

            public void LateUpdate(float deltaTime)
            {
            }

            private void UpdateAim(float deltaTime)
            {
                if (this.currentAimValue != this.input.aim)
                {
                    this.currentAimValue = this.input.aim;
                    this.cinemachineAimCamera.SetActive(this.currentAimValue);
                }

                Transform hitTransform = null;
                Vector3 hitPosition;

                // Updating the Aim Target
                int layerMask = 1 << LayerMask.NameToLayer("Default");
                Ray ray = this.camera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0.0f));
                if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f, layerMask))
                {
                    hitTransform = hit.transform;
                    hitPosition = hit.point;
                }
                else
                {
                    hitPosition = this.cameraTransform.position + (this.cameraTransform.forward * 50.0f);
                }

                // Update the Aim Target
                this.aimTargetTraransform.position = hitPosition;

                // Rotating the character to where they're looking if in Aim mode
                if (this.input.aim)
                {
                    Vector3 worldAimTarget = this.aimTargetTraransform.position.SetY(0.0f);
                    Vector3 aimDirection = (worldAimTarget - this.characterTransform.position).normalized;
                    this.characterTransform.forward = Vector3.Lerp(this.characterTransform.forward, aimDirection, deltaTime * 20.0f);
                }

                this.UpdateShoot(hitPosition, hitTransform, deltaTime);
            }

            private void UpdateShoot(Vector3 hitPosition, Transform hitTransform, float deltaTime)
            {
                if (this.input.shoot)
                {
                    // this.ShootBulletProjectile();
                    this.ShootHybridBulletProjectile();
                    // this.ShootHitscan(hitPosition, hitTransform);
                    this.input.shoot = false;
                }
            }

            private void ShootBulletProjectile()
            {
                Vector3 shootPosition = this.shootTraransform.position;
                Vector3 aimTargetPosition = this.aimTargetTraransform.position;
                Quaternion rotation = Quaternion.LookRotation((aimTargetPosition - shootPosition).normalized,  Vector3.up);

                var bulletProjectile = GameObject.Instantiate(this.bulletProjectilePrefab, shootPosition, rotation);
                bulletProjectile.Shoot();
            }

            private void ShootHybridBulletProjectile()
            {
                Vector3 shootPosition = this.shootTraransform.position;
                Vector3 aimTargetPosition = this.aimTargetTraransform.position;

                var bullet = PoolManager.InstantiatePrefab(this.hybridBulletProjectilePrefab);
                bullet.Shoot(shootPosition, aimTargetPosition);
            }

            private void ShootHitscan(Vector3 hitPosition, Transform hitTransform)
            {
                // if hitTransform is not null and it has a health component, then fire off effects and do instant damage
            }

            private void SetActive(bool active)
            {
                this.cinemachineCamera.SetActive(active);
                this.cinemachineAimCamera.SetActive(active && this.input.aim);
                this.cinemachineCameraTarget.SetActive(active);
                this.currentAimValue = this.input.aim;
            }

            public void Move(float speed, float verticalVelocity, float deltaTime)
            {
                // Normalise input direction
                Vector3 inputDirection = new Vector3(input.move.x, 0.0f, input.move.y).normalized;

                // NOTE: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                // if there is a move input rotate player when the player is moving
                if (input.move != Vector2.zero)
                {
                    this.targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + this.cameraTransform.eulerAngles.y;
                    float rotation = Mathf.SmoothDampAngle(this.characterTransform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);

                    if (this.input.aim == false)
                    {
                        // Rotate to face input direction relative to camera position
                        this.characterTransform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                    }
                }

                Vector3 targetDirection = Quaternion.Euler(0.0f, this.targetRotation, 0.0f) * Vector3.forward;

                // Move the player
                this.characterController.Move(targetDirection.normalized * (speed * deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * deltaTime);
            }

            public void UpdateCameraRotation(float sensitivity, float deltaTime)
            {
                // If there is an input and camera position is not fixed
                if (input.look.sqrMagnitude >= threshold && !this.lockCameraPosition)
                {
                    this.cinemachineTargetYaw += (input.look.x * deltaTime * sensitivity);
                    this.cinemachineTargetPitch += (input.look.y * deltaTime * sensitivity);
                }

                // Clamp our rotations so our values are limited 360 degrees
                this.cinemachineTargetYaw = ClampAngle(this.cinemachineTargetYaw, float.MinValue, float.MaxValue);
                this.cinemachineTargetPitch = ClampAngle(this.cinemachineTargetPitch, bottomClamp, topClamp);

                // Cinemachine will follow this target
                this.cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOverride, cinemachineTargetYaw, 0.0f);
            }
        }

        [Serializable]
        private class FixedCameraController : IController
        {
            [Header("Rotation")]
            [Tooltip("How fast the character turns to face movement direction")]
            [Range(0.0f, 0.3f)]
            [SerializeField] private float rotationSmoothTime = 0.12f;

            [Header("Cinemachine")]
            [Tooltip("The Cinemachine Virtual Camera this controller uses.")]
            [SerializeField] private GameObject cinemachineCamera;

            [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
            [SerializeField] private GameObject cinemachineCameraTarget;

            [Tooltip("The desired rotation of the fixed camera")]
            [SerializeField] private Quaternion cameraRotation = Quaternion.Euler(30.0f, 40.0f, 0.0f);

            private Transform characterTransform;
            private CharacterController characterController;
            private GenericControllerInputs input;

            private Camera camera;
            private Transform cameraTransform;

            private float targetRotation;
            private float rotationVelocity;

            public void Initialize(Transform characterTransform, Camera camera, CharacterController characterController, GenericControllerInputs input)
            {
                this.camera = camera;
                this.cameraTransform = camera.transform;
                this.characterTransform = characterTransform;
                this.characterController = characterController;
                this.input = input;
            }

            public void Enable()
            {
                this.cinemachineCamera.SetActive(true);
                this.cinemachineCameraTarget.SetActive(true);
                this.cinemachineCamera.transform.localRotation = this.cameraRotation;
            }

            public void Disable()
            {
                this.cinemachineCamera.SetActive(false);
                this.cinemachineCameraTarget.SetActive(false);
            }

            public void Update(float deltaTime)
            {
            }

            public void LateUpdate(float deltaTime)
            {
            }

            public void Move(float speed, float verticalVelocity, float deltaTime)
            {
                // Normalise input direction
                Vector3 inputDirection = new Vector3(input.move.x, 0.0f, input.move.y).normalized;

                // NOTE: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                // If there is a move input rotate player when the player is moving
                if (input.move != Vector2.zero)
                {
                    this.targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + this.cameraTransform.eulerAngles.y;
                    float rotation = Mathf.SmoothDampAngle(this.characterTransform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);

                    // Rotate to face input direction relative to camera position
                    this.characterTransform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }

                Vector3 targetDirection = Quaternion.Euler(0.0f, this.targetRotation, 0.0f) * Vector3.forward;

                // Move the player
                this.characterController.Move(targetDirection.normalized * (speed * deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * deltaTime);
            }

            public void UpdateCameraRotation(float sensitivity, float deltaTime)
            {
            }
        }

        [Serializable]
        private class FirstPersonController : IController
        {
            [Header("Player Neck")]
            public GameObject neck;

            [Header("Rotation")]
            [Tooltip("Rotation speed of the character")]
            public float rotationSpeed = 1.0f;

            [Header("Cinemachine")]
            [Tooltip("The Cinemachine Virtual Camera this controller uses.")]
            [SerializeField] private GameObject cinemachineCamera;

            [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
            [SerializeField] private GameObject cinemachineCameraTarget;
            
            [Tooltip("How far in degrees can you move the camera up")]
            [SerializeField] private float topClamp = 70.0f;
            
            [Tooltip("How far in degrees can you move the camera down")]
            [SerializeField] private float bottomClamp = -30.0f;

            private Transform characterTransform;
            private CharacterController characterController;
            private GenericControllerInputs input;

            private Camera camera;
            private Transform cameraTransform;

            private float cinemachineTargetPitch;
            private float rotationVelocity;

            public void Initialize(Transform characterTransform, Camera camera, CharacterController characterController, GenericControllerInputs input)
            {
                this.camera = camera;
                this.cameraTransform = camera.transform;
                this.characterTransform = characterTransform;
                this.characterController = characterController;
                this.input = input;
            }

            public void Enable()
            {
                this.cinemachineCamera.SetActive(true);
                this.cinemachineCameraTarget.SetActive(true);
                this.neck.transform.localScale = Vector3.zero;
            }

            public void Disable()
            {
                this.cinemachineCamera.SetActive(false);
                this.cinemachineCameraTarget.SetActive(false);
                this.neck.transform.localScale = Vector3.one;
            }

            public void Update(float deltaTime)
            {
            }

            public void LateUpdate(float deltaTime)
            {
            }

            public void Move(float speed, float verticalVelocity, float deltaTime)
            {
                // Normalise input direction
                Vector3 inputDirection = new Vector3(input.move.x, 0.0f, input.move.y).normalized;

                // NOTE: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                // if there is a move input rotate player when the player is moving
                if (input.move != Vector2.zero)
                {
                    // Move
                    inputDirection = this.characterTransform.right * input.move.x + this.characterTransform.forward * input.move.y;
                }

                // Move the player
                this.characterController.Move(inputDirection.normalized * (speed * deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * deltaTime);
            }

            public void UpdateCameraRotation(float sensitivity, float deltaTime)
            {
                // If there is an input
                if (input.look.sqrMagnitude >= threshold)
                {
                    this.cinemachineTargetPitch += input.look.y * this.rotationSpeed * deltaTime * sensitivity;
                    this.rotationVelocity = input.look.x * this.rotationSpeed * deltaTime * sensitivity;

                    // clamp our pitch rotation
                    this.cinemachineTargetPitch = ClampAngle(this.cinemachineTargetPitch, this.bottomClamp, this.topClamp);

                    // Update Cinemachine camera target pitch
                    this.cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0.0f, 0.0f);

                    // Rotate the player left and right
                    this.characterTransform.Rotate(Vector3.up * this.rotationVelocity);
                }
            }
        }

        [Serializable]
        public class GenericControllerSettings
        {
            [Header("Player")]
            [Tooltip("Aim speed of the character in m/s")]
            [SerializeField] private float aimSpeed = 1.0f;

            [Tooltip("Move speed of the character in m/s")]
            [SerializeField] private float moveSpeed = 4.0f;

            [Tooltip("Sprint speed of the character in m/s")]
            [SerializeField] private float sprintSpeed = 6.0f;

            [Tooltip("Acceleration and deceleration")]
            [SerializeField] private float speedChangeRate = 10.0f;

            [Space(10)]
            [Tooltip("The height the player can jump")]
            [SerializeField] private float jumpHeight = 1.2f;

            [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
            [SerializeField] private float gravity = -15.0f;

            [Space(10)]
            [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
            [SerializeField] private float jumpTimeout = 0.1f;

            [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
            [SerializeField] private float fallTimeout = 0.15f;

            [Tooltip("Useful for rough ground")]
            [SerializeField] private float groundedOffset = -0.14f;

            [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
            [SerializeField] private float groundedRadius = 0.25f;

            [Tooltip("What layers the character uses as ground")]
            [SerializeField] private LayerMask groundLayers;

            [Header("Aiming")]
            [SerializeField] private float lookSensitivity = 1.0f;
            [SerializeField] private float aimSensitivity = 0.5f;

            public float AimSpeed
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.aimSpeed;
            }

            public float MoveSpeed
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.moveSpeed;
            }

            public float SprintSpeed
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.sprintSpeed;
            }

            public float SpeedChangeRate
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.speedChangeRate;
            }

            public float JumpHeight
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.jumpHeight;
            }

            public float Gravity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.gravity;
            }

            public float JumpTimeout
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.jumpTimeout;
            }

            public float FallTimeout
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.fallTimeout;
            }

            public float GroundedOffset
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.groundedOffset;
            }

            public float GroundedRadius
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.groundedRadius;
            }

            public LayerMask GroundLayers
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.groundLayers;
            }

            public float LookSensitivity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.lookSensitivity;
            }

            public float AimSensitivity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.aimSensitivity;
            }
        }
    }
}

#endif

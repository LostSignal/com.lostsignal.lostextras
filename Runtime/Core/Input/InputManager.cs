//-----------------------------------------------------------------------
// <copyright file="InputManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class InputManager : Manager<InputManager>
    {
        private const int InputCacheSize = 20;

        private readonly List<InputHandler> handlers = new List<InputHandler>();
        private readonly List<Input> fingerInputs = new List<Input>(10);
        private readonly Dictionary<int, Input> fingerIdToInputMap = new Dictionary<int, Input>();
        private readonly HashSet<int> activeFingerIdsCache = new HashSet<int>();
        private readonly List<Input> inputCache = new List<Input>(InputCacheSize);

        private Input mouseInput = null;
        private Input penInput = null;

        private bool useTouchInput;
        private bool useMouseInput;
        private bool usePenInput;

        private int inputIdCounter = 0;

        public void AddHandler(InputHandler handler)
        {
            if (handler != null && this.handlers.Contains(handler) == false)
            {
                this.handlers.Add(handler);
            }
        }

        public void RemoveHandler(InputHandler handler)
        {
            this.handlers.Remove(handler);
        }

        public override void Initialize()
        {
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();

            // Populating the input cache
            for (int i = 0; i < InputCacheSize; i++)
            {
                this.inputCache.Add(new Input());
            }

            this.useTouchInput = true;
            this.usePenInput = false;
            this.useMouseInput =
                Application.isEditor ||
                Application.platform == RuntimePlatform.OSXPlayer ||
                Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.LinuxPlayer;

            this.SetInstance(this);
        }

        private void Update()
        {
            if (this.useTouchInput)
            {
                this.UpdateTouchInputs();
            }

            if (this.useMouseInput)
            {
                this.UpdateMouseInput();
            }

            if (this.usePenInput)
            {
                this.UpdatePenInput();
            }

            // Sending inputs to all registered handlers
            for (int i = 0; i < this.handlers.Count; i++)
            {
                this.handlers[i].HandleInputs(this.fingerInputs, this.mouseInput, this.penInput);
            }
        }

        private void UpdateTouchInputs()
        {
            Debug.Assert(this.fingerInputs.Count == this.fingerIdToInputMap.Count, "Finger Inputs list and map don't match!");

            // Remove all inputs that have been marked as released
            for (int i = this.fingerInputs.Count - 1; i >= 0; i--)
            {
                if (this.fingerInputs[i].InputState == InputState.Released)
                {
                    Input input = this.fingerInputs[i];
                    this.RecycleInput(input);
                    this.fingerInputs.RemoveAt(i);
                    this.fingerIdToInputMap.Remove(input.UnityFingerId);
                }
            }

            this.activeFingerIdsCache.Clear();

            // Going through all the unity touch inputs and either creating/updating Lost.Inputs
            for (int i = 0; i < UnityEngine.InputSystem.EnhancedTouch.Touch.activeFingers.Count; i++)
            {
                var finger = UnityEngine.InputSystem.EnhancedTouch.Touch.activeFingers[i];
                var fingerId = finger.currentTouch.touchId;
                var position = finger.screenPosition;

                this.activeFingerIdsCache.Add(fingerId);

                if (this.fingerIdToInputMap.TryGetValue(fingerId, out Input input))
                {
                    input.Update(position);
                }
                else
                {
                    Input newInput = this.GetNewInput(fingerId, InputType.Touch, InputButton.Left, position);
                    this.fingerInputs.Add(newInput);
                    this.fingerIdToInputMap.Add(newInput.UnityFingerId, newInput);
                }
            }

            // Testing if any of the Lost.Inputs no longer have their unity counterparts and calling Done() on them if that's the case
            for (int i = 0; i < this.fingerInputs.Count; i++)
            {
                Input input = this.fingerInputs[i];

                if (this.activeFingerIdsCache.Contains(input.UnityFingerId) == false)
                {
                    input.Done();
                }
            }
        }

        private void UpdateMouseInput()
        {
#if USING_UNITY_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;

            Vector3 mousePosition = mouse.position.ReadValue();
            bool isLeftButtonDown = mouse.leftButton.isPressed;
            bool isRightButtonDown = mouse.rightButton.isPressed;
            bool isMiddleButtonDown = mouse.middleButton.isPressed;
#else
            Vector3 mousePosition = UnityEngine.Input.mousePosition;
            bool isLeftButtonDown = UnityEngine.Input.GetMouseButton(0);
            bool isRightButtonDown = UnityEngine.Input.GetMouseButton(1);
            bool isMiddleButtonDown = UnityEngine.Input.GetMouseButton(2);
#endif

            // Defaulting the mouse input to be in the hover state
            if (this.mouseInput == null || this.mouseInput.InputState == InputState.Released)
            {
                this.RecycleInput(this.mouseInput);
                this.mouseInput = this.GetNewInput(-1, InputType.Mouse, InputButton.None, mousePosition);
                this.mouseInput.UpdateHover(mousePosition);
            }

            if (this.mouseInput.InputState == InputState.Hover)
            {
                InputButton inputButton = InputButton.None;

                if (isLeftButtonDown)
                {
                    inputButton = InputButton.Left;
                }
                else if (isRightButtonDown)
                {
                    inputButton = InputButton.Right;
                }
                else if (isMiddleButtonDown)
                {
                    inputButton = InputButton.Middle;
                }

                if (inputButton != InputButton.None)
                {
                    this.RecycleInput(this.mouseInput);
                    this.mouseInput = this.GetNewInput(-1, InputType.Mouse, inputButton, mousePosition);
                }
                else
                {
                    this.mouseInput.UpdateHover(mousePosition);
                }
            }
            else
            {
                if (this.mouseInput.InputButton == InputButton.Left)
                {
                    if (isLeftButtonDown)
                    {
                        this.mouseInput.Update(mousePosition);
                    }
                    else
                    {
                        this.mouseInput.Done();
                    }
                }
                else if (this.mouseInput.InputButton == InputButton.Right)
                {
                    if (isRightButtonDown)
                    {
                        this.mouseInput.Update(mousePosition);
                    }
                    else
                    {
                        this.mouseInput.Done();
                    }
                }
                else if (this.mouseInput.InputButton == InputButton.Middle)
                {
                    if (isMiddleButtonDown)
                    {
                        this.mouseInput.Update(mousePosition);
                    }
                    else
                    {
                        this.mouseInput.Done();
                    }
                }
                else
                {
                    Debug.LogError("UpdateMouseInput found an invalid InputButton type!!!", this);
                }
            }
        }

        private void UpdatePenInput()
        {
            // TODO implement (may need platform dependent code)
        }

        private void RecycleInput(Input input)
        {
            if (input != null)
            {
                this.inputCache.Add(input);
            }
        }

        private Input GetNewInput(int unityFingerId, InputType inputType, InputButton inputButton, Vector2 position)
        {
            Debug.Assert(this.inputCache.Count != 0, "InputManager's input cache has run out!  Figure out why we're leaking inputs.");

            int lastIndex = this.inputCache.Count - 1;
            Input lastInput = this.inputCache[lastIndex];
            this.inputCache.RemoveAt(lastIndex);

            lastInput.Reset(this.inputIdCounter++, unityFingerId, inputType, inputButton, position);

            return lastInput;
        }
    }
}

#endif

﻿// This code is distributed under MIT license. 
// Copyright (c) 2015 George Mamaladze
// See license.txt or https://mit-license.org/

using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput.Native;
using WindowsInput;
using WindowsInput.Events;
using System.Runtime.InteropServices;

namespace WindowsInput.Native {




    /// <summary>
    ///     Contains a snapshot of a keyboard state at certain moment and provides methods
    ///     of querying whether specific keys are pressed or locked.
    /// </summary>
    /// <remarks>
    ///     This class is basically a managed wrapper of GetKeyboardState API function
    ///     http://msdn.microsoft.com/en-us/library/ms646299
    /// </remarks>
    public class KeyboardState {
        public KeyboardKeyState[] State { get; } = new KeyboardKeyState[256];

        public IDictionary<KeyCode, KeyboardKeyState> ToDictionary() {
            var ret = new Dictionary<KeyCode, KeyboardKeyState>();
            for (int i = 0; i < State.Length; i++) {
                if(State[i] != KeyboardKeyState.Default) {
                    ret[(KeyCode)i] = State[i];
                }
            }

            return ret;
        }

        public KeyboardKeyState this[byte index] {
            get => State[index];
            set => State[index] = value;
        }

        public KeyboardKeyState this[int index] {
            get => State[index];
            set => State[index] = value;
        }

        public KeyboardKeyState this[KeyCode index] {
            get => State[(int)index];
            set => State[(int)index] = value;
        }


        public KeyboardState Clone() {
            var ret = new KeyboardState();

            Array.Copy(this.State, 0, ret.State, 0, ret.State.Length);

            return ret;
        }

        private KeyboardState() {
            
        }

        public static KeyboardState Current() {
            var ret = new KeyboardState();
            var RVal = GetKeyboardState(ret.State);

            return ret;
        }

        public static KeyboardState Blank() {
            var ret = new KeyboardState();

            return ret;
        }


        /// <summary>
        ///     Indicates whether specified key was down at the moment when snapshot was created or not.
        /// </summary>
        /// <param name="key">Key (corresponds to the virtual code of the key)</param>
        /// <returns><b>true</b> if key was down, <b>false</b> - if key was up.</returns>
        public bool IsDown(KeyCode key) {
            var ret = KeyState(key)
                .HasFlag(KeyboardKeyState.KeyDown)
                ;

            return ret;
        }

        /// <summary>
        ///     Indicate weather specified key was toggled at the moment when snapshot was created or not.
        /// </summary>
        /// <param name="key">Key (corresponds to the virtual code of the key)</param>
        /// <returns>
        ///     <b>true</b> if toggle key like (CapsLock, NumLocke, etc.) was on. <b>false</b> if it was off.
        ///     Ordinal (non toggle) keys return always false.
        /// </returns>
        public bool IsToggled(KeyCode key) {
            var ret = KeyState(key)
                .HasFlag(KeyboardKeyState.Toggled)
                ;
            return ret;
        }

        /// <summary>
        ///     Indicates weather every of specified keys were down at the moment when snapshot was created.
        ///     The method returns false if even one of them was up.
        /// </summary>
        /// <param name="keys">Keys to verify whether they were down or not.</param>
        /// <returns><b>true</b> - all were down. <b>false</b> - at least one was up.</returns>
        public bool AreAllDown(IEnumerable<KeyCode> keys) {
            return keys.All(IsDown);
        }

        public KeyboardKeyState KeyState(KeyCodeModifiers key) {
            var ret = KeyboardKeyState.Default;

            if (key == KeyCodeModifiers.AltModifier) {
                ret = KeyState(KeyCode.LAlt) | KeyState(KeyCode.RAlt);
            } else if (key == KeyCodeModifiers.ShiftModifier) {
                ret = KeyState(KeyCode.LShift) | KeyState(KeyCode.RShift);
            } else if (key == KeyCodeModifiers.ControlModifier) {
                ret = KeyState(KeyCode.LControl) | KeyState(KeyCode.RControl);
            }

            return ret;
        }

        public KeyboardKeyState KeyState(KeyCode key) {
            var ret = State[(int)key];


            return ret;
        }

        /// <summary>
        ///     The GetKeyboardState function copies the status of the 256 virtual keys to the
        ///     specified buffer.
        /// </summary>
        /// <param name="pbKeyState">
        ///     [in] Pointer to a 256-byte array that contains keyboard key states.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero.
        ///     If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        /// <remarks>
        ///     http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/userinput/keyboardinput/keyboardinputreference/keyboardinputfunctions/toascii.asp
        /// </remarks>
        [DllImport("user32.dll")]
        protected static extern int GetKeyboardState(KeyboardKeyState[] pbKeyState);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern KeyboardKeyState GetAsyncKeyState(KeyCode virtualKeyCode);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern KeyboardKeyState GetKeyState(KeyCode virtualKeyCode);


    }

    [Flags]
    public enum KeyboardKeyState : byte {
        Default = 0b_0000_0000,
        KeyDown = 0b_1000_0000,
        Toggled = 0b_0000_0001,
    }

}
using RPGFramework.Core.PlayerLoop;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGFramework.Core.Input
{
    /// <summary>Runs the motors of the player's gamepad, when they have one.</summary>
    public interface IVibrationService
    {
        /// <summary>Runs the motors for a time, in place of whatever they were doing. No time stops them.</summary>
        /// <param name="low">The strength of the heavy motor, 0 to 1.</param>
        /// <param name="high">The strength of the light motor, 0 to 1.</param>
        /// <param name="seconds">How long for, in real time.</param>
        void Vibrate(float low, float high, float seconds);

        void Stop();
    }

    /// <summary>The motors themselves, so that what runs them can be tested with no pad.</summary>
    internal interface IGamepadMotors
    {
        void Run(float low, float high);

        void Stop();
    }

    internal sealed class InputSystemGamepadMotors : IGamepadMotors
    {
        void IGamepadMotors.Run(float low, float high)
        {
            Gamepad.current?.SetMotorSpeeds(low, high);
        }

        void IGamepadMotors.Stop()
        {
            InputSystem.ResetHaptics();
        }
    }

    internal sealed class VibrationService : IVibrationService, IUpdatable
    {
        private readonly IGamepadMotors m_Motors;
        private readonly IFrameClock    m_FrameClock;

        private float m_SecondsLeft;

        public VibrationService(IGamepadMotors motors, IFrameClock frameClock)
        {
            m_Motors      = motors;
            m_FrameClock  = frameClock;
            m_SecondsLeft = 0f;
        }

        void IVibrationService.Vibrate(float low, float high, float seconds)
        {
            if (seconds <= 0f)
            {
                StopMotors();

                return;
            }

            if (m_SecondsLeft <= 0f)
            {
                UpdateManager.RegisterUpdatable(this);

                Application.quitting += StopMotors;
            }

            m_SecondsLeft = seconds;

            m_Motors.Run(low, high);
        }

        void IVibrationService.Stop()
        {
            StopMotors();
        }

        void IUpdatable.Update()
        {
            m_SecondsLeft -= m_FrameClock.UnscaledDeltaTime;

            if (m_SecondsLeft <= 0f)
            {
                StopMotors();
            }
        }

        // Also as the game quits, play mode included, so a pad is never left running.
        private void StopMotors()
        {
            m_SecondsLeft = 0f;

            UpdateManager.UnregisterUpdatable(this);

            Application.quitting -= StopMotors;

            m_Motors.Stop();
        }
    }
}

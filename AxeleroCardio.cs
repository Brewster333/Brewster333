using System;

namespace NeuroSoft.Devices.ErgoDevices
{
    /// <summary>
    /// Тредмил Axelero Cardio (Польша).
    /// Отличием протокола от TrackMaster является то, что он принимает скорость бега в км/ч.
    /// </summary>
    public class AxeleroCardio : TrackMasterTreadmillErgometer
    {
        /// <summary>
        /// Устанавливает скорость ленты (км/ч) или (миль/ч).
        /// </summary>
        public override void SetSpeed(float value, TreadmillSpeedUnit unit)
        {
            CheckActive();
            if (unit == TreadmillSpeedUnit.MPH) value *= 1.609f;
            if (value < 0) value = 0;
            if (value > 16 * 1.609f) value = 16 * 1.609f;

            var speed = Convert.ToInt32(value * 10);
            SD.SendString((char)0xA3 + $"{speed:D4}");
            // запуск/остановка ленты
            if (speed > 0) RunBelt();
            else
            if (speed == 0) StopBelt();
            OnChanged();
        }

        /// <summary>
        /// Устанавливает уклон (%).
        /// </summary>
        public override void SetElevation(float value)
        {
            CheckActive();
            if (value > 25) value = 25;
            else
                if (value < 0) value = 0;

            SD.SendString((char)0xA4 + $"{Convert.ToInt32(value * 10):D4}");
            OnChanged();
        }

        /// <summary>
        /// Сигнализирует об обмене данными с эргометром.
        /// </summary>
        protected override void OnSerialDeviceData(string command, string answer)
        {
            if (!string.IsNullOrEmpty(answer) && answer.Length > 4)
            {
                switch ((byte)answer[0])
                {
                    case 0xD1:
                        if (Int32.TryParse(answer.Substring(1, 4), out var s))
                            currentSpeed = s / 10f;
                        break;
                    case 0xD2:
                        if (Int32.TryParse(answer.Substring(1, 4), out var e))
                            currentElevation = e / 10f;
                        break;
                }
            }
            base.OnSerialDeviceData(command, answer);
        }
    }
}

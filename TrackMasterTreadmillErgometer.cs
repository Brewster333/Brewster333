using System;

namespace NeuroSoft.Devices.ErgoDevices
{
	/// <summary>
	/// Тредмил "TRACKMASTER".
	/// </summary>
	public class TrackMasterTreadmillErgometer : TreadmillErgometer
	{
        // Интерфейс: RS232, 4800 бод, 8 бит, 1 старт-бит, 1 стоп-бит, full duplex
        // Кабель: 9 pin (male) <=< 9 pin (female): 2-3, 3-2, 5-5 (земля)
        // либо: 9 pin (female) <=> 5 (DIN): 2-1, 3-5, 5-2, 7-4, 8-3
        // Input Commands:
        // ---------------
        // A0	 160	Start Belt - Com. Disconnect Stop Enable
        // A1	 161	Start Belt - Com. Disconnect Stop Disable
        // A2	 162	Stop Belt
        // A3	 163	Set Speed to the next 4 bytes of ASCII Data
        // A4	 164	Set Elevation to the next 4 bytes of ASCII Data
        // A5	 165	Set Time to the next 4 bytes of ASCII Data
        // A6	 166	Set Protocol to the next 2 bytes of ASCII Data
        // A7	 167	Set Stage to the next 2 bytes of ASCII Data
        // A8	 168	Reset Distance, Total Time, Energy to 0
        // A9	 169	Set Patient Weight to the next 4 bytes of ASCII Data
        // AA    170	Auto Stop - Sets speed, elevation to min. and stops belt
        // AB	 171	Auto Cool Down - Sets speed, elevation to min.
        // AC	 172	Toggle Transmit Acknowledge Data Flag (Flag Clear On Reset) See Note 3
        //
        // Input Command Acknowledgment:
        // -----------------------------
        // B0	 176	Ack. Start Belt - Com. Disconnect Stop Enable
        // B1	 177	Ack. Start Belt - Com. Disconnect Stop Disable
        // B2	 178	Ack. Stop Belt
        // B3	 179	Ack. Set Speed
        // B4	 180	Ack. Set Elevation
        // B5	 181	Ack. Set Time
        // B6	 182	Ack. Set Protocol
        // B7	 183	Ack. Set Stage
        // B8	 184	Ack. Reset Distance, Total Time, Energy
        // B9	 185	Ack. Set Current Weight
        // BA	 186	Ack. Auto Stop
        // BB	 187	Ack. Auto Cool Down
        // BC	 188	Ack. Toggle Transmit Acknowledge Command Data Flag
        // BE	 190	Input Command Data Out of Range
        // BF    191	Illegal Command or Command not Recognized
        //
        // Status Request:
        // ---------------
        // C0	 192	Xmit Belt Status
        // C1	 193	Xmit Current actual Speed
        // C2	 194	Xmit Current actual Elevation
        // C3	 195	Xmit Current Set Speed or Stage Speed
        // C4	 196	Xmit Current Set Elevation or Stage Elevation
        // C5	 197	Xmit Current Lap Time or Stage Time
        // C6	 198	Xmit Current Total Time
        // C7	 199	Xmit Current Distance
        // C8	 200	Xmit Current Protocol
        // C9	 201	Xmit Current Stage
        // CA	 202	Xmit Current Weight
        // CB	 203	Xmit Current Calories
        // CC	 204	Xmit Current Total V02
        // CD	 205	Xmit Current Mets
        //
        // Status Responses:
        // -----------------
        // D0	 208	Belt Stopped Followed By 1 Byte of Data
        //	        31h = Belt Stopped
        //	        32h = Belt Started Com. Disc. Stop Enabled
        //	        33h = Belt Started Com. Disc. Stop Disabled
        // D1	 209	Current Belt Speed followed by 4 bytes of ASCII Data
        // D2	 210	Current Elevation followed by 4 bytes of ASCII Data
        // D3	 211	Current Set or Stage Speed, independent of belt status followed by 4 bytes of ASCII Data
        // D4	 212	Current Set or Stage Elev. fol. by 4 bytes of ASCII Data
        // D5	 213	Current Lap or Stage Time fol. by 4 bytes of ASCII Data
        // D6	 214	Current Total Time followed by 4 bytes of ASCII Data
        // D7	 215	Current Distance followed by 4 bytes of ASCII Data
        // D8	 216	Current Protocol followed by 2 bytes of ASCII Data
        // D9	 217	Current Stage followed by 2 bytes of ASCII Data
        // DA	 218	Current Weight followed by 4 bytes of ASCII Data
        // DB    219	Current Calories followed by 4 bytes of ASCII Date
        // DC	 220	Current Total V02 followed by 4 bytes of ASCII Data
        // DD	 221	Current Mets followed by 4 bytes of ASCII Data
        // DF	 223	Treadmill Status Request not Recognized
        //
        // Data Configuration:
        // -------------------
        // All data transmitted and received is in ASCII Numbers.
        // The decimal point is not transmitted.
        // Speed, Elevation, Time, Calories (bytes): 1 - Hundreds; 2 - Tens; 3 - Units; 4 - Tenths
        // Distance (bytes): 1 - Tens; 2 - Units; 3 - Tenths; 4 - Hundreds
        // Weight (bytes): 1 - Thousands; 2 - Hundreds; 3 - Tens; 4 - Units
        // Stage & Protocol (bytes): 1 - Tens; 2 - Units
        //
        // APPLICATION NOTES
        // -----------------
        // Communication Disconnect Stop (CDS) - While the CDS is enabled,
        // the treadmill must receive a command or a status request every 500ms.
        // This insures the treadmill-computer link remains intact.
        // If a request is not made in 500 ms, the treadmill will de-accelerate to minimum then stop,
        // and elevate to 0% grade. This mode is strongly recommended when using an Internal Controller.
        //
        // Transmit Acknowledge Data Flag - This option may be used to retransmit the Input Command data
        // after the Acknowledgment. This Data may be compared with the transmitted data to insure the
        // command data was sent properly. Upon Power up the flag is cleared so no data will be sent after
        // the Input Command Acknowledgment.
        //
        // The treadmill models differ by the minimum and maximum speed and maximum elevation.
        // Therefore, the command data is checked for values within the proper ranges.
        // An ASCII BEh is returned if the values are out of range.
        // In addition, the Elevation must be in 0.5% increments to be considered within range.
        //
        // ADDITIONAL APPLICATION NOTES
        // ----------------------------
        // The treadmill microcontroller serial input command buffer is limited to one command and 4 data bytes.
        // Therefore acknowledgments should be received before a second command is sent.
        // If the second command is sent before the first command is processed, the first command will be lost.
        // Speed of 999.9 is sent for emergency stop or invalid speed in control loop i.e. not responding.
        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Настройка порта.
        /// </summary>
        protected override void SetupPort() => SD.Port.BaudRate = 4800;

        void SendByte(byte b) => SD.WriteBuffer(new[] { b });

        /// <summary>
        /// Начинает эргометрию.
        /// </summary>
        public override void Start()
        {
            base.Start();
            StartTimer(1000);
        }

        /// <summary>
        /// Останавливает эргометрию.
        /// </summary>
        public override void Stop()
		{
            base.Stop();
            SendByte(0xAA);
			SendByte(0xAA);
            StopTimer();
		}

		/// <summary>
		/// Устанавливает скорость ленты (км/ч) или (миль/ч).
		/// </summary>
		public override void SetSpeed(float value, TreadmillSpeedUnit unit)
		{
			CheckActive();
			if (unit == TreadmillSpeedUnit.MPH) value *= 1.609f;
			if (value < 0) value = 0;
			if (value > 16 * 1.609f) value = 16 * 1.609f;

            var speed = Convert.ToInt32(value / 1.609f * 10);
            SD.SendString((char) 0xA3 + $"{speed:D4}");
            currentSpeed = value;

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

            SD.SendString((char) 0xA4 + $"{Convert.ToInt32(value * 10):D4}");
            currentElevation = value;
            OnChanged();
        }

		/// <summary>
		/// Запускает ленту.
		/// </summary>
		public override void RunBelt()
        {
            CheckActive();
            SendByte(0xA1);
            if (state == TreadmillState.Run) return;

            state = TreadmillState.Run;
            OnChanged();
        }

        /// <summary>
        /// Останавливаает ленту.
        /// </summary>
        public override void StopBelt() 
		{
			CheckActive();
            SendByte(0xA2);
            if (state == TreadmillState.Stop) return;

            state = TreadmillState.Stop;
            OnChanged();
        }

        /// <summary>
        /// Обработчик внутреннего таймера.
        /// </summary>
        protected override void TimerTick(object sender, EventArgs e)
        {
            SendByte(0xC0);
            SendByte(0xC1);
            SendByte(0xC2);
        }

        /// <summary>
        /// Сигнализирует об обмене данными с эргометром.
        /// </summary>
        protected override void OnSerialDeviceData(string command, string answer)
        {
            if (answer != null) OnChanged();
            base.OnSerialDeviceData(command, answer);
        }
    }
}

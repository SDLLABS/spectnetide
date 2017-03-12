﻿using System.IO;

namespace Spect.Net.SpectrumEmu.Tape.Tzx
{
    /// <summary>
    /// Represents the standard speed data block in a TZX file
    /// </summary>
    public class TzxStandardSpeedDataBlock : TzxDataBlockBase, ISupportsTapePlayback
    {
        /// <summary>
        /// Pause after this block
        /// </summary>
        public ushort PauseAfter { get; set; }

        /// <summary>
        /// Lenght of block data
        /// </summary>
        public ushort DataLenght { get; set; }

        /// <summary>
        /// Block Data
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// The ID of the block
        /// </summary>
        public override int BlockId => 0x10;

        /// <summary>
        /// Reads the content of the block from the specified binary stream.
        /// </summary>
        /// <param name="reader">Stream to read the block from</param>
        public override void ReadFrom(BinaryReader reader)
        {
            PauseAfter = reader.ReadUInt16();
            DataLenght = reader.ReadUInt16();
            Data = reader.ReadBytes(DataLenght);
        }

        /// <summary>
        /// Writes the content of the block to the specified binary stream.
        /// </summary>
        /// <param name="writer">Stream to write the block to</param>
        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write(PauseAfter);
            writer.Write(DataLenght);
            writer.Write(Data);
        }

        #region Implementation of ISupportsTapePlayback

        private const int PILOT = 2168;
        private const int SYNC1 = 667;
        private const int SYNC2 = 735;
        private const int BIT_0 = 855;
        private const int BIT_1 = 1710;
        private const ulong PAUSE_MS = 3500;

        private int _pilotEnds;
        private int _sync1Ends;
        private int _sync2Ends;
        private int _bitStarts;
        private int _bitPulseLength;
        private bool _currentBit;
        private ulong _pauseEnds;

        /// <summary>
        /// The index of the currently playing byte
        /// </summary>
        public int ByteIndex { get; private set; }

        /// <summary>
        /// The mask of the currently playing bit in the current byte
        /// </summary>
        public byte BitMask { get; private set; }

        /// <summary>
        /// The current playing phase
        /// </summary>
        public PlayPhase PlayPhase { get; private set; }

        /// <summary>
        /// The tact count of the CPU when playing starts
        /// </summary>
        public ulong StartTact { get; private set; }

        /// <summary>
        /// Initializes the player
        /// </summary>
        public void InitPlay(ulong startTact)
        {
            PlayPhase = PlayPhase.Pilot;
            StartTact = startTact;
            _pilotEnds = ((Data[0] & 0x80) == 0 ? 8063 : 3223) * PILOT;
            _sync1Ends = _pilotEnds + SYNC1;
            _sync2Ends = _sync1Ends + SYNC2;
            ByteIndex = 0;
            BitMask = 0x80;
        }

        /// <summary>
        /// Gets the EAR bit value for the specified tact
        /// </summary>
        /// <param name="currentTact">Tact to retrieve the EAR bit</param>
        /// <returns>
        /// The EAR bit value to play back
        /// </returns>
        public bool GetEarBit(ulong currentTact)
        {
            var pos = (int)(currentTact - StartTact);
            if (pos >= TapeDevice.MAX_TACT_JUMP)
            {
                // --- If the EAR bit has not been scanned for a long time, 
                // --- we mimic that the tape is faulty by completing the block
                PlayPhase = PlayPhase.Completed;
                return true;
            }

            if (PlayPhase == PlayPhase.Pilot || PlayPhase == PlayPhase.Sync)
            {
                // --- Generate the appropriate pilot or sync EAR bit
                if (pos <= _pilotEnds)
                {
                    // --- Alternating pilot pulses
                    return (pos / PILOT) % 2 == 0;
                }
                if (pos <= _sync1Ends)
                {
                    // --- 1st sync pulse
                    PlayPhase = PlayPhase.Sync;
                    return false;
                }
                if (pos <= _sync2Ends)
                {
                    // --- 2nd sync pulse
                    PlayPhase = PlayPhase.Sync;
                    return true;
                }
                PlayPhase = PlayPhase.Data;
                _bitStarts = _sync2Ends;
                _currentBit = (Data[ByteIndex] & BitMask) != 0;
                _bitPulseLength = _currentBit ? BIT_1 : BIT_0;
            }
            if (PlayPhase == PlayPhase.Data)
            {
                // --- Data block playback
                // --- Generate current bit pulse
                var bitPos = pos - _bitStarts;
                if (bitPos < _bitPulseLength)
                {
                    // --- First pulse of the bit
                    return false;
                }
                if (bitPos < 2 * _bitPulseLength)
                {
                    // --- Second pulse of the bit
                    return true;
                }

                // --- Move to the next bit, or byte
                if ((BitMask >>= 1) == 0)
                {
                    BitMask = 0x80;
                    ByteIndex++;
                }

                // --- Prepare the next bit
                if (ByteIndex < DataLenght)
                {
                    _bitStarts += 2 * _bitPulseLength;
                    _currentBit = (Data[ByteIndex] & BitMask) != 0;
                    _bitPulseLength = _currentBit ? BIT_1 : BIT_0;
                    // --- We're in the first pulse of the next bit
                    return false;
                }

                // --- We've played back all data, not, it's pause time
                PlayPhase = PlayPhase.Pause;
                _pauseEnds = currentTact + PAUSE_MS * PauseAfter;
                return true;
            }

            // --- We need to produce pause signs
            if (currentTact > _pauseEnds)
            {
                PlayPhase = PlayPhase.Completed;
            }
            return true;
        }

        #endregion
    }
}
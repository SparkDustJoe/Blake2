﻿// Original Written in 2012 by Christian Winnerlein  <codesinchaos@gmail.com>
// Rewritten Fall 2014 (for the Blake2s flavor instead of the Blake2b flavor) 
//   by Dustin J Sparks <sparkdustjoe@gmail.com>


// To the extent possible under law, the author(s) have dedicated all copyright
// and related and neighboring rights to this software to the public domain
// worldwide. This software is distributed without any warranty.

// You should have received a copy of the CC0 Public Domain Dedication along with
// this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
/*
  Based on BlakeSharp
  by Dominik Reichl <dominik.reichl@t-online.de>
  Web: http://www.dominik-reichl.de/
  If you're using this class, it would be nice if you'd mention
  me somewhere in the documentation of your program, but it's
  not required.

  BLAKE was designed by Jean-Philippe Aumasson, Luca Henzen,
  Willi Meier and Raphael C.-W. Phan.
  BlakeSharp was derived from the reference C implementation.
*/

using System;

namespace Blake2sCSharp
{
    public sealed partial class Blake2sCore
    {
        private bool _isInitialized = false;

        private int _bufferFilled;
        private byte[] _buf = new byte[64]; //

        private UInt32[] _m = new UInt32[16]; 
        private UInt32[] _h = new UInt32[8]; // stays the same
        private UInt32 _counter0;
        private UInt32 _counter1;
        private UInt32 _finalizationFlag0;
        private UInt32 _finalizationFlag1;

        //private const int NumberOfRounds = 10; //
        public const int BlockSizeInBytes = 64; //

        const UInt32 IV0 = 0x6A09E667U; //
        const UInt32 IV1 = 0xBB67AE85U; //
        const UInt32 IV2 = 0x3C6EF372U; //
        const UInt32 IV3 = 0xA54FF53AU; //
        const UInt32 IV4 = 0x510E527FU; //
        const UInt32 IV5 = 0x9B05688CU; //
        const UInt32 IV6 = 0x1F83D9ABU; //
        const UInt32 IV7 = 0x5BE0CD19U; //

        /*private static readonly int[] Sigma = new int[NumberOfRounds * 16] {
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
			14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3,
			11, 8, 12, 0, 5, 2, 15, 13, 10, 14, 3, 6, 7, 1, 9, 4,
			7, 9, 3, 1, 13, 12, 11, 14, 2, 6, 5, 10, 4, 0, 15, 8,
			9, 0, 5, 7, 2, 4, 10, 15, 14, 1, 11, 12, 6, 8, 3, 13,
			2, 12, 6, 10, 0, 11, 8, 3, 4, 13, 7, 5, 15, 14, 1, 9,
			12, 5, 1, 15, 14, 13, 4, 10, 0, 7, 6, 3, 9, 2, 8, 11,
			13, 11, 7, 14, 12, 1, 3, 9, 5, 0, 15, 4, 8, 6, 2, 10,
			6, 15, 14, 9, 11, 3, 0, 8, 12, 2, 13, 7, 1, 4, 10, 5,
			10, 2, 8, 4, 7, 6, 1, 5, 15, 11, 9, 14, 3, 12, 13, 0,
		}; //*/

        internal static UInt32 BytesToUInt32(byte[] buf, int offset) //
        {
            return
                (
                 ((UInt32)buf[offset + 3] << 24) +  //
                 ((UInt32)buf[offset + 2] << 16) + //
                 ((UInt32)buf[offset + 1] << 8) + //
                  (UInt32)buf[offset]
                ); //
        }

        private static void UInt32ToBytes(UInt32 value, byte[] buf, int offset) //
        {
            buf[offset + 3] = (byte)(value >> 24); //
            buf[offset + 2] = (byte)(value >> 16); //
            buf[offset + 1] = (byte)(value >> 8); //
            buf[offset] = (byte)value;
        }

        partial void Compress(byte[] block, int start);

        public void Initialize(UInt32[] salt) //
        {
            if (salt == null)
                throw new ArgumentNullException("salt");
            if (salt.Length != 8)
                throw new ArgumentException("salt length must be 8 words");
            _isInitialized = true;

            _h[0] = IV0;
            _h[1] = IV1;
            _h[2] = IV2;
            _h[3] = IV3;
            _h[4] = IV4;
            _h[5] = IV5;
            _h[6] = IV6;
            _h[7] = IV7;

            _counter0 = 0;
            _counter1 = 0;
            _finalizationFlag0 = 0;
            _finalizationFlag1 = 0;

            _bufferFilled = 0;

            Array.Clear(_buf, 0, _buf.Length);

            for (int i = 0; i < _h.Length; i++)
                _h[i] ^= salt[i];
        }

        public void HashCore(byte[] array, int start, int count)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Not initialized");
            if (array == null)
                throw new ArgumentNullException("array");
            if (start < 0)
                throw new ArgumentOutOfRangeException("start");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if ((long)start + (long)count > array.Length)
                throw new ArgumentOutOfRangeException("start+count");
            int offset = start;
            int bufferRemaining = BlockSizeInBytes - _bufferFilled;

            if ((_bufferFilled > 0) && (count > bufferRemaining))
            {
                Array.Copy(array, offset, _buf, _bufferFilled, bufferRemaining);
                _counter0 += BlockSizeInBytes;
                if (_counter0 == 0)
                    _counter1++;
                Compress(_buf, 0);
                offset += bufferRemaining;
                count -= bufferRemaining;
                _bufferFilled = 0;
            }

            while (count > BlockSizeInBytes)
            {
                _counter0 += BlockSizeInBytes;
                if (_counter0 == 0)
                    _counter1++;
                Compress(array, offset);
                offset += BlockSizeInBytes;
                count -= BlockSizeInBytes;
            }

            if (count > 0)
            {
                Array.Copy(array, offset, _buf, _bufferFilled, count);
                _bufferFilled += count;
            }
        }

        public byte[] HashFinal()
        {
            return HashFinal(false);
        }

        public byte[] HashFinal(bool isEndOfLayer)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Not initialized");
            _isInitialized = false;

            //Last compression
            _counter0 += (uint)_bufferFilled;
            _finalizationFlag0 = UInt32.MaxValue; //
            //if (isEndOfLayer) // tree mode
            //    _finalizationFlag1 = UInt32.MaxValue; //
            for (int i = _bufferFilled; i < _buf.Length; i++)
                _buf[i] = 0;
            Compress(_buf, 0);

            //Output
            byte[] hash = new byte[32]; //
            for (int i = 0; i < 8; i++) //
                UInt32ToBytes(_h[i], hash, i * 4); //
            return hash;
        }
    }
}

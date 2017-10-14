﻿// Original Written in 2012 by Christian Winnerlein  <codesinchaos@gmail.com>
// Rewritten Fall 2015
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
using System.Security.Cryptography;
using System.Text;

namespace Blake2bCSharp
{
    internal class Blake2bHasher : Hasher
    {
        private Blake2bCore core = new Blake2bCore();
        private UInt64[] rawConfig = null; // no longer read only
        private byte[] key = null;
        private int outputSizeInBytes;
        private static readonly Blake2bConfig DefaultConfig = new Blake2bConfig();

        public override void Init()
        {
            core.Initialize(rawConfig);
            if (key != null)
            {
                core.HashCore(key, 0, key.Length);
            }
        }

        public override byte[] Finish()
        {
            byte[] fullResult = core.HashFinal();
            if (outputSizeInBytes != fullResult.Length)
            {
                byte[] result = new byte[outputSizeInBytes];
                Array.Copy(fullResult, result, result.Length);
                return result;
            }
            else 
                return fullResult;
        }

        public Blake2bHasher(Blake2bConfig config)
        {
            if (config == null)
                config = DefaultConfig;
            rawConfig = Blake2bIvBuilder.ConfigB(config); //, null); no tree config;
            if (config.Key != null && config.Key.Length != 0)
            {
                key = new byte[Blake2bCore.BlockSizeInBytes]; 
                Array.Copy(config.Key, key, config.Key.Length);
            }
            outputSizeInBytes = config.OutputSizeInBytes;
            Init();
        }

        public override void Update(byte[] data, int start, int count)
        {
            core.HashCore(data, start, count);
        }
    }

    public abstract class Hasher
    {
        public abstract void Init();
        public abstract byte[] Finish();
        public abstract void Update(byte[] data, int start, int count);

        public void Update(byte[] data)
        {
            Update(data, 0, data.Length);
        }

        public HashAlgorithm AsHashAlgorithm()
        {
            return new HashAlgorithmAdapter(this);
        }

        internal class HashAlgorithmAdapter : HashAlgorithm
        {
            private readonly Hasher _hasher;

            protected override void HashCore(byte[] array, int ibStart, int cbSize)
            {
                _hasher.Update(array, ibStart, cbSize);
            }

            protected override byte[] HashFinal()
            {
                return _hasher.Finish();
            }

            public override void Initialize()
            {
                _hasher.Init();
            }

            public HashAlgorithmAdapter(Hasher hasher)
            {
                _hasher = hasher;
            }
        }
    }
}

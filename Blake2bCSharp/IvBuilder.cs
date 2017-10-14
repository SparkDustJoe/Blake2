// Original Written in 2012 by Christian Winnerlein  <codesinchaos@gmail.com>
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

namespace Blake2bCSharp
{
    /* from the ref C, since we are not doing any tree hashing, we care about '+' of the following:
    typedef struct __blake2b_param
    {
        uint8_t  digest_length; // 1 +
        uint8_t  key_length;    // 2 +
        uint8_t  fanout;        // 3 =1
        uint8_t  depth;         // 4 =1
        uint32_t leaf_length;   // 8 =0
        uint64_t node_offset;   // 16 =0
        uint8_t  node_depth;    // 17 =0
        uint8_t  inner_length;  // 18 =0
        uint8_t  reserved[14];  // 32
        uint8_t  salt[BLAKE2B_SALTBYTES]; // 48 +
        uint8_t  personal[BLAKE2B_PERSONALBYTES];  // 64 +
    } blake2b_param;
    //*/

    internal static class Blake2bIvBuilder
    {
        public static UInt64[] ConfigB(Blake2bConfig config)
        {

            var rawConfig = new UInt64[8]; //
            //var result = new UInt64[8]; //

            //digest length
            if (config.OutputSizeInBytes <= 0 | config.OutputSizeInBytes > 64) //
                throw new ArgumentOutOfRangeException("config.OutputSize");
            rawConfig[0] = (UInt32)config.OutputSizeInBytes; //

            //Key length
            if (config.Key != null)
            {
                if (config.Key.Length > 64) //
                    throw new ArgumentException("config.Key", "Key too long");
                rawConfig[0] |= (UInt64)(config.Key.LongLength << 8); //
            }
            // Fan Out =1 and Max Height / Depth = 1
            rawConfig[0] |= 1 << 16;
            rawConfig[0] |= 1 << 24;
            // Leaf Length and Inner Length 0, no need to worry about them
            // Salt
            if (config.Salt != null)
            {
                if (config.Salt.Length != 16)
                    throw new ArgumentException("config.Salt has invalid length");
                rawConfig[4] = Blake2bCore.BytesToUInt64(config.Salt, 0);
                rawConfig[5] = Blake2bCore.BytesToUInt64(config.Salt, 8);
            }
            // Personalization
            if (config.Personalization != null)
            {
                if (config.Personalization.Length != 16)
                    throw new ArgumentException("config.Personalization has invalid length");
                rawConfig[6] = Blake2bCore.BytesToUInt64(config.Personalization, 0);
                rawConfig[7] = Blake2bCore.BytesToUInt64(config.Personalization, 8);
            }

            return rawConfig;
        }

     }
}

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

namespace Blake2bCSharp
{
    public sealed class Blake2bConfig : ICloneable
    {
        public byte[] Personalization { get; set; }
        public byte[] Salt { get; set; }
        public byte[] Key { get; set; }
        public int OutputSizeInBytes { get; set; }
        public int OutputSizeInBits
        {
            get { return OutputSizeInBytes * 8; }
            set
            {
                if (value % 8 == 0)
                    throw new ArgumentException("Output size must be a multiple of 8 bits");
                OutputSizeInBytes = value / 8;
            }
        }

        public Blake2bConfig()
        {
            OutputSizeInBytes = 64;
        }

        public Blake2bConfig Clone()
        {
            var result = new Blake2bConfig();
            result.OutputSizeInBytes = OutputSizeInBytes;
            if (Key != null)
                result.Key = (byte[])Key.Clone();
            if (Personalization != null)
                result.Personalization = (byte[])Personalization.Clone();
            if (Salt != null)
                result.Salt = (byte[])Salt.Clone();
            return result;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
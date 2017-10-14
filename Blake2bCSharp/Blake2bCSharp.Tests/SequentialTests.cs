// Original Written in 2012 by Christian Winnerlein  <codesinchaos@gmail.com>
// Rewritten Fall 2015
//   by Dustin Sparks <sparkdustjoe@gmail.com>


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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Blake2bCSharp.Tests
{
    [TestClass]
    public class SequentialTests
    {
        byte[] input = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();


        [TestMethod]
        public void CheckTestVectors()
        {
            for (int len = 0; len < TestVectors.UnkeyedBlake2B.Length; len++)
            {
                var input = Enumerable.Range(0, len).Select(i => (byte)i).ToArray();
                var hash = Blake2bCSharp.Blake2B.ComputeHash(input);
                string actual = BitConverter.ToString(hash).Replace("-", "");
                string expected = TestVectors.UnkeyedBlake2B[len];
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void CheckKeyedTestVectors()
        {
            var key = Enumerable.Range(0, 64).Select(i => (byte)i).ToArray();
            for (int len = 0; len < TestVectors.KeyedBlake2B.Length; len++)
            {
                var input = Enumerable.Range(0, len).Select(i => (byte)i).ToArray();
                var hash = Blake2bCSharp.Blake2B.ComputeHash(input, new Blake2bCSharp.Blake2bConfig { Key = key });
                string actual = BitConverter.ToString(hash).Replace("-", "");
                string expected = TestVectors.KeyedBlake2B[len];
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void Splits()
        {
            var hasher = Blake2B.Create();
            for (int len = 0; len <= 256; len++)
            {
                hasher.Init();
                hasher.Update(input, 0, len);
                string hash0 = BitConverter.ToString(hasher.Finish());

                for (int split1 = 0; split1 <= len; split1++)
                {
                    for (int split2 = split1; split2 <= len; split2++)
                    {
                        hasher.Init();
                        hasher.Update(input, 0, split1);
                        hasher.Update(input, split1, split2 - split1);
                        hasher.Update(input, split2, len - split2);
                        string hash1 = BitConverter.ToString(hasher.Finish());
                        Assert.AreEqual(hash0, hash1);
                    }
                }
            }
        }
    }
}

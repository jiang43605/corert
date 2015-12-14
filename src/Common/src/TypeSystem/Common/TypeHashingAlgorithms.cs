﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// ---------------------------------------------------------------------------
// Generic functions to compute the hashcode value of types
// ---------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Internal.NativeFormat
{
    static public class TypeHashingAlgorithms
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int _rotl(int value, int shift)
        {
            return (int)(((uint)value << shift) | ((uint)value >> (32 - shift)));
        }

        //
        // Returns the hashcode value of the 'src' string
        //
        public static int ComputeNameHashCode(string src)
        {
            int hash1 = 0x6DA3B944;
            int hash2 = 0;

            for (int i = 0; i < src.Length; i += 2)
            {
                hash1 = (hash1 + _rotl(hash1, 5)) ^ src[i];
                if ((i + 1) < src.Length)
                    hash2 = (hash2 + _rotl(hash2, 5)) ^ src[i + 1];
            }

            hash1 += _rotl(hash1, 8);
            hash2 += _rotl(hash2, 8);

            return hash1 ^ hash2;
        }

        public static unsafe int ComputeASCIINameHashCode(byte* data, int length, out bool isAscii)
        {
            int hash1 = 0x6DA3B944;
            int hash2 = 0;
            int asciiMask = 0;

            for (int i = 0; i < length; i += 2)
            {
                int b1 = data[i];
                asciiMask |= b1;
                hash1 = (hash1 + _rotl(hash1, 5)) ^ b1;
                if ((i + 1) < length)
                {
                    int b2 = data[i];
                    asciiMask |= b2;
                    hash2 = (hash2 + _rotl(hash2, 5)) ^ b2;
                }
            }

            hash1 += _rotl(hash1, 8);
            hash2 += _rotl(hash2, 8);

            isAscii = (asciiMask & 0x80) == 0;

            return hash1 ^ hash2;
        }

        public static int ComputeArrayTypeHashCode(int elementTypeHashCode, int rank)
        {
            // Arrays are treated as generic types in some parts of our system. The array hashcodes are 
            // carefully crafted to be the same as the hashcodes of their implementation generic types.

            int hashCode;
            if (rank == 1)
            {
                hashCode = unchecked((int)0xd5313557u);
                Debug.Assert(hashCode == ComputeNameHashCode("System.Array`1"));
            }
            else
            {
                hashCode = ComputeNameHashCode("System.MDArrayRank" + rank.ToString() + "`1");
            }

            hashCode = (hashCode + _rotl(hashCode, 13)) ^ elementTypeHashCode;
            return (hashCode + _rotl(hashCode, 15));
        }

        public static int ComputeArrayTypeHashCode<T>(T elementType, int rank)
        {
            return ComputeArrayTypeHashCode(elementType.GetHashCode(), rank);
        }


        public static int ComputePointerTypeHashCode(int pointeeTypeHashCode)
        {
            return (pointeeTypeHashCode + _rotl(pointeeTypeHashCode, 5)) ^ 0x12D0;
        }

        public static int ComputePointerTypeHashCode<T>(T pointeeType)
        {
            return ComputePointerTypeHashCode(pointeeType.GetHashCode());
        }


        public static int ComputeByrefTypeHashCode(int parameterTypeHashCode)
        {
            return (parameterTypeHashCode + _rotl(parameterTypeHashCode, 7)) ^ 0x4C85;
        }

        public static int ComputeByrefTypeHashCode<T>(T parameterType)
        {
            return ComputeByrefTypeHashCode(parameterType.GetHashCode());
        }


        public static int ComputeNestedTypeHashCode(int enclosingTypeHashCode, int nestedTypeNameHash)
        {
            return (enclosingTypeHashCode + _rotl(enclosingTypeHashCode, 11)) ^ nestedTypeNameHash;
        }


        public static int ComputeGenericInstanceHashCode<ARG>(int genericDefinitionHashCode, ARG[] genericTypeArguments)
        {
            int hashcode = genericDefinitionHashCode;
            for (int i = 0; i < genericTypeArguments.Length; i++)
            {
                int argumentHashCode = genericTypeArguments[i].GetHashCode();
                hashcode = (hashcode + _rotl(hashcode, 13)) ^ argumentHashCode;
            }
            return (hashcode + _rotl(hashcode, 15));
        }

        /// <summary>
        /// Produce a hashcode for a specific method
        /// </summary>
        /// <param name="typeHashCode">HashCode of the type that owns the method</param>
        /// <param name="nameOrNameAndGenericArgumentsHashCode">HashCode of either the name of the method (for non-generic methods) or the GenericInstanceHashCode of the name+generic arguments of the method.</param>
        /// <returns></returns>
        public static int ComputeMethodHashCode(int typeHashCode, int nameOrNameAndGenericArgumentsHashCode)
        {
            // TODO! This hash combining function isn't good, but it matches logic used in the past
            // consider changing to a better combining function once all uses use this function
            return typeHashCode ^ nameOrNameAndGenericArgumentsHashCode;
        }

        /// <summary>
        /// Produce a hashcode for a generic signature variable
        /// </summary>
        /// <param name="index">zero based index</param>
        /// <param name="method">true if the signature variable describes a method</param>
        public static int ComputeSignatureVariableHashCode(int index, bool method)
        {
            if (method)
            {
                return index * 0x7822381 + 0x54872645;
            }
            else
            {
                return index * 0x5498341 + 0x832424;
            }
        }
    }
}

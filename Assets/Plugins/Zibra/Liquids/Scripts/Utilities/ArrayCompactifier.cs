using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.zibra.liquid.Utilities
{
    public static class ArrayCompactifier
    {
        public static string IntToString(this IEnumerable<int> array)
        {
            var byteArray =
                array.SelectMany(BitConverter.GetBytes)
                    .ToArray(); // possible GarbageCollector overload (GetBytes generates new array every call)

            return Convert.ToBase64String(byteArray);
        }

        public static int[] StringToInt(this string input)
        {
            var bytes = Convert.FromBase64String(input);
            var result = new int[Mathf.CeilToInt(bytes.Length / (float)sizeof(int))];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = BitConverter.ToInt32(bytes, i * sizeof(int));
            }

            return result;
        }

        public static string FloatToString(this IEnumerable<float> array)
        {
            var byteArray =
                array.SelectMany(BitConverter.GetBytes)
                    .ToArray(); // possible GarbageCollector overload (GetBytes generates new array every call)

            return Convert.ToBase64String(byteArray);
        }

        public static float[] StringToFloat(this string input)
        {
            var bytes = Convert.FromBase64String(input);
            var result = new float[Mathf.CeilToInt(bytes.Length / (float)sizeof(float))];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = BitConverter.ToSingle(bytes, i * sizeof(float));
            }

            return result;
        }

        public static string Vector3ToString(this IEnumerable<Vector3> array)
        {
            var byteArray =
                array
                    .SelectMany(vec => BitConverter.GetBytes(vec.x)
                                           .Concat(BitConverter.GetBytes(vec.y))
                                           .Concat(BitConverter.GetBytes(vec.z)))
                    .ToArray(); // possible GarbageCollector overload (GetBytes generates new array every call)

            return Convert.ToBase64String(byteArray);
        }

        public static Vector3[] StringToVector3(this string input)
        {
            var bytes = Convert.FromBase64String(input);
            var sizeofVector3 = sizeof(float) * 3;
            var result = new Vector3[Mathf.CeilToInt(bytes.Length / (float)sizeofVector3)];

            for (var i = 0; i < bytes.Length / sizeofVector3; i++)
            {
                result[i] = new Vector3(BitConverter.ToSingle(bytes, i * sizeofVector3),
                                        BitConverter.ToSingle(bytes, i * sizeofVector3 + sizeof(float)),
                                        BitConverter.ToSingle(bytes, i * sizeofVector3 + sizeof(float) * 2));
            }

            return result;
        }
    }
}
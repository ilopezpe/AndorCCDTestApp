﻿using log4net;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace LuiHardware
{
    /// <summary>
    /// Provides a variety of utility methods.
    /// </summary>
    public static class Util
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Returns the name of a property as a string.
        /// Usage: GetPropertyName(() => Property)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string GetPropertyName<T>(Expression<Func<T>> property)
        {
            var me = property.Body as MemberExpression;
            if (me == null)
            {
                throw new ArgumentException();
            }
            return me.Member.Name;
        }

        /// <summary>
        /// Provides access to the native QueryDosDevice system call.
        /// </summary>
        /// <param name="lpDeviceName"></param>
        /// <param name="lpTargetPath"></param>
        /// <param name="ucchMax"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        static extern uint QueryDosDevice(string lpDeviceName, IntPtr lpTargetPath, uint ucchMax);

        /// <summary>
        /// Enumerates the available COM ports using QueryDosDevice.
        /// </summary>
        /// <returns></returns>
        public static List<string> EnumerateSerialPorts()
        {
            const int ERROR_INSUFFICIENT_BUFFER = 122;
            // Allocate some memory to get a list of all system devices.
            // Start with a small size and dynamically give more space until we have enough room.
            int returnSize = 0;
            int maxSize = 100;
            string allDevices = null;
            IntPtr mem;
            string[] retval = null;

            while (returnSize == 0)
            {
                mem = Marshal.AllocHGlobal(maxSize);
                if (mem != IntPtr.Zero)
                {
                    // mem points to memory that needs freeing
                    try
                    {
                        returnSize = (int)QueryDosDevice(null, mem, (uint)maxSize);
                        if (returnSize != 0)
                        {
                            allDevices = Marshal.PtrToStringAnsi(mem, returnSize);
                            retval = allDevices.Split('\0');
                            break;    // not really needed, but makes it more clear...
                        }
                        else if (Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
                        {
                            maxSize *= 10;
                        }
                        else
                        {
                            Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(mem);
                    }
                }
                else
                {
                    throw new OutOfMemoryException();
                }
            }

            List<string> ports = new List<string>();

            foreach (string device in retval) if (device.Contains("COM")) ports.Add(device);

            return ports;
        }

        /// <summary>
        /// Simple Bernstein hash. Use to combine hash codes of multiple objects.
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        /// <returns></returns>
        public static int Hash(int h1, int h2)
        {
            unchecked { return (h1 << 5) * h2; } // Hash should wrap around.
        }

        /// <summary>
        /// Combines hash with object's hash after null check.
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static int Hash(int h1, object o)
        {
            if (o == null) return h1;
            return Hash(h1, o.GetHashCode());
        }

        /// <summary>
        /// Combines object hashes after null check.
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        /// <returns></returns>
        public static int Hash(object o1, object o2)
        {
            if (o1 == null && o2 == null)
            {
                return 0;
            }
            else if (o1 != null)
            {
                return o1.GetHashCode();
            }
            else if (o2 != null)
            {
                return o2.GetHashCode();
            }
            else
            {
                return Hash(o1.GetHashCode(), o2.GetHashCode());
            }
        }
    }
}

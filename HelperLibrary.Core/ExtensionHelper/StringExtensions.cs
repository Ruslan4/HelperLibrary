﻿/* 
 * FileName:    StringExtensions.cs
 * Author:      functionghw<functionghw@hotmail.com>
 * CreateTime:  3/11/2015 11:11:10 AM
 * Version:     v1.0
 * Description:
 * */

namespace HelperLibrary.Core.ExtensionHelper
{
    using System;
    using System.Diagnostics.Contracts;

    public static class StringExtensions
    {
        /// <summary>
        /// Reverse a string
        /// </summary>
        /// <param name="str">the string instance</param>
        /// <returns>the new string that reversed; 
        /// if string length less than 2, return the reference directly</returns>
        /// <exception cref="ArgumentNullException">the string instance is null</exception>
        public static string ReverseString(this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (str.Length < 2)
            {
                return str;
            }
            char[] chs = str.ToCharArray();

            CollectionTypeExtensions.InternalReverseArray(chs, 0, chs.Length);

            return new string(chs);
        }

        /// <summary>
        ///  to uppercase the first char of string.
        ///  Note that the string must at least has one char;
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">the string is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">the string is empty</exception>
        public static string FirstCharToUpper(this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (str.Length < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(str), "Length of str must greater than 0.");
            }
            return ChangeCaseOfCharInternal(str, 0, true);
        }

        /// <summary>
        ///  to lowercase the first char of string.
        ///  Note that the string must at least has one char;
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">the string is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">the string is empty</exception>
        public static string FirstCharToLower(this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (str.Length < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(str), "Length of str must greater than 0.");
            }
            return ChangeCaseOfCharInternal(str, 0, false);
        }

        /// <summary>
        /// make the char of the index to uppercase.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="indexOfChar">index of the char</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">the string is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">index out of range</exception>
        public static string CharToUpperByIndex(this string str, int indexOfChar)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (indexOfChar < 0 || indexOfChar >= str.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(indexOfChar),
                    "indexOfChar less than 0 or greater than length of string");
            }
            return ChangeCaseOfCharInternal(str, indexOfChar, true);
        }

        /// <summary>
        /// make the char of the index to lowercase.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="indexOfChar">index of the char</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">the string is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">index out of range</exception>
        public static string CharToLowerByIndex(this string str, int indexOfChar)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (indexOfChar < 0 || indexOfChar >= str.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(indexOfChar),
                    "indexOfChar less than 0 or greater than length of string");
            }
            return ChangeCaseOfCharInternal(str, indexOfChar, false);
        }

        private static string ChangeCaseOfCharInternal(string str, int indexOfChar, bool isToUpper)
        {
            Contract.Requires(str != null);
            Contract.Requires(indexOfChar >= 0 && indexOfChar < str.Length);
            char theChar = str[indexOfChar];
            if (isToUpper)
            {
                if (char.IsUpper(theChar))
                {
                    return str;
                }
                else
                {
                    char[] chs = str.ToCharArray();
                    chs[indexOfChar] = char.ToUpper(chs[indexOfChar]);
                    return new string(chs);
                }
            }
            else
            {
                if (char.IsLower(theChar))
                {
                    return str;
                }
                else
                {
                    char[] chs = str.ToCharArray();
                    chs[indexOfChar] = char.ToLower(chs[indexOfChar]);
                    return new string(chs);
                }
            }
        }
    }
}

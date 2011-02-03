﻿/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:    Utils.cs
 *  Desc:    Fundamental LATINO utilities
 *  Created: Nov-2007
 *
 *  Authors: Miha Grcar
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Xml;
using System.Web;
using System.Text.RegularExpressions;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Class Utils
       |
       '-----------------------------------------------------------------------
    */
    public static class Utils
    {
        private static bool mVerbose
            = true;
        private static VerboseDelegate mVerboseProc
            = new VerboseDelegate(DefaultVerbose);
        private static VerboseDelegate mVerboseLineProc
            = new VerboseDelegate(DefaultVerboseLine);
        private static VerboseProgressDelegate mVerboseProgressProc
            = new VerboseProgressDelegate(DefaultVerboseProgress);

        public delegate void VerboseDelegate(string format, params object[] args);
        public delegate void VerboseProgressDelegate(string format, int step, int numSteps);

        private static void DefaultVerbose(string format, params object[] args)
        {
            Console.Write(format, args); // throws ArgumentNullException, FormatException
        }

        private static void DefaultVerboseLine(string format, params object[] args)
        {
            Console.WriteLine(format, args); // throws ArgumentNullException, FormatException
        }

        private static void DefaultVerboseProgress(string format, int step, int numSteps)
        {
            ThrowException(step < 1 ? new ArgumentOutOfRangeException("step") : null);
            if (numSteps <= 0)
            {
                if (mVerbose && step % 100 == 0)
                {
                    Console.Write("\r" + format, step); // throws ArgumentNullException, FormatException
                }
            }
            else
            {
                if (mVerbose && (step % 100 == 0 || step == numSteps))
                {
                    Console.Write("\r" + format, step, numSteps); // throws ArgumentNullException, FormatException
                    if (step == numSteps) { Console.WriteLine(); }
                }
            }
        }

        public static void Verbose(string format, params object[] args)
        {
            if (mVerbose && mVerboseProc != null) { mVerboseProc(format, args); } 
        }

        public static void VerboseLine(string format, params object[] args)
        {
            if (mVerbose && mVerboseLineProc != null) { mVerboseLineProc(format, args); } 
        }

        public static void VerboseLine()
        {
            if (mVerbose && mVerboseLineProc != null) { mVerboseLineProc(""); } 
        }

        public static void VerboseProgress(string format, int step, int numSteps)
        {
            if (mVerbose && mVerboseProgressProc != null) { mVerboseProgressProc(format, step, numSteps); }
        }

        public static void VerboseProgress(string format, int step)
        {
            VerboseProgress(format, step, /*numSteps=*/-1);
        }

        public static bool VerboseEnabled
        {
            get { return mVerbose; }
            set { mVerbose = value; }
        }

        public static VerboseDelegate VerboseFunc
        {
            get { return mVerboseProc; }
            set { mVerboseProc = value; }
        }

        public static VerboseDelegate VerboseLineFunc
        {
            get { return mVerboseLineProc; }
            set { mVerboseLineProc = value; }
        }

        public static VerboseProgressDelegate VerboseProgressFunc
        {
            get { return mVerboseProgressProc; }
            set { mVerboseProgressProc = value; }
        }

        [Conditional("THROW_EXCEPTIONS")]
        public static void ThrowException(Exception exception)
        {
            if (exception != null) { throw exception; }
        }

        public static bool IsFiniteNumber(double val)
        {
            return !double.IsInfinity(val) && !double.IsNaN(val);
        }

        public static bool IsFiniteNumber(float val)
        {
            return !double.IsInfinity(val) && !double.IsNaN(val);
        }

        public static bool VerifyFileNameCreate(string fileName)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(fileName);
                FileAttributes attributes = fileInfo.Attributes; 
                if ((int)attributes == -1) { attributes = (FileAttributes)0; }
                return (attributes & FileAttributes.Directory) != FileAttributes.Directory && (attributes & FileAttributes.Offline) != FileAttributes.Offline &&
                    (attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly /*&& (attributes & FileAttributes.System) != FileAttributes.System*/;
            }
            catch
            {
                return false;
            }
        }

        public static bool VerifyFileNameOpen(string fileName)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(fileName);
                FileAttributes attributes = fileInfo.Attributes;
                if ((int)attributes == -1) { return false; }
                return (attributes & FileAttributes.Directory) != FileAttributes.Directory && (attributes & FileAttributes.Offline) != FileAttributes.Offline;
            }
            catch
            {
                return false;
            }
        }

        public static bool VerifyPathName(string pathName, bool mustExist)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(pathName);
                return mustExist ? dirInfo.Exists : true;
            }
            catch
            {
                return false;
            }
        }

        public static byte[] ReadAllBytes(Stream stream)
        {
            ThrowException(stream == null ? new ArgumentNullException("stream") : null);
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length); // throws IOException, ObjectDisposedException, NotSupportedException
                    if (read <= 0) { return ms.ToArray(); }
                    ms.Write(buffer, 0, read);
                }
            }
        }

        public static bool CheckUnicodeSignature(string fileName)
        {
            ThrowException(!VerifyFileNameOpen(fileName) ? new ArgumentValueException("fileName") : null);
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] bom = new byte[4]; // get the byte-order mark, if there is one
            file.Read(bom, 0, 4);
            file.Close();
            return ((bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) || // utf-8
                (bom[0] == 0xff && bom[1] == 0xfe) || // ucs-2le, ucs-4le, and ucs-16le
                (bom[0] == 0xfe && bom[1] == 0xff) || // utf-16 and ucs-2
                (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)); // ucs-4
        }

        public static object Clone(object obj, bool deepClone)
        {
            if (deepClone && obj is IDeeplyCloneable)
            {
                return ((IDeeplyCloneable)obj).DeepClone();
            }
            else if (obj is ICloneable)
            {
                return ((ICloneable)obj).Clone();
            }
            else
            {
                return obj;
            }
        }

        public static bool ObjectEquals(object obj1, object obj2, bool deepCmp)
        {
            if (obj1 == null && obj2 == null) { return true; }
            else if (obj1 == null || obj2 == null) { return false; }
            else if (!obj1.GetType().Equals(obj2.GetType())) { return false; }
            else if (deepCmp && obj1 is IContentEquatable)
            {
                return ((IContentEquatable)obj1).ContentEquals(obj2);
            }
            else
            {
                return obj1.Equals(obj2);
            }
        }

        public static int GetHashCode(object obj)
        {
            ThrowException(obj == null ? new ArgumentNullException("obj") : null);
            if (obj is ISerializable)
            {
                BinarySerializer memSer = new BinarySerializer();
                ((ISerializable)obj).Save(memSer); // throws serialization-related exceptions   
                byte[] buffer = ((MemoryStream)memSer.Stream).GetBuffer();
                MD5CryptoServiceProvider hashAlgo = new MD5CryptoServiceProvider();
                Guid md5Hash = new Guid(hashAlgo.ComputeHash(buffer));
                return md5Hash.GetHashCode();
            }
            else
            {
                return obj.GetHashCode();
            }
        }

        public static void SaveDictionary<KeyT, ValT>(Dictionary<KeyT, ValT> dict, BinarySerializer writer)
        {
            ThrowException(dict == null ? new ArgumentNullException("dict") : null);
            ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions   
            writer.WriteInt(dict.Count);
            foreach (KeyValuePair<KeyT, ValT> item in dict)
            {
                writer.WriteValueOrObject<KeyT>(item.Key);
                writer.WriteValueOrObject<ValT>(item.Value);
            }
        }

        public static Dictionary<KeyT, ValT> LoadDictionary<KeyT, ValT>(BinarySerializer reader)
        {
            return LoadDictionary<KeyT, ValT>(reader, /*comparer=*/null); // throws ArgumentNullException, serialization-related exceptions
        }

        public static Dictionary<KeyT, ValT> LoadDictionary<KeyT, ValT>(BinarySerializer reader, IEqualityComparer<KeyT> comparer)
        {
            ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            Dictionary<KeyT, ValT> dict = new Dictionary<KeyT, ValT>(comparer);
            // the following statements throw serialization-related exceptions   
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            { 
                KeyT key = reader.ReadValueOrObject<KeyT>();
                ValT dat = reader.ReadValueOrObject<ValT>();
                dict.Add(key, dat);
            }
            return dict;
        }

        public static object ChangeType(object obj, Type newType, IFormatProvider fmtProvider)
        {
            ThrowException(newType == null ? new ArgumentNullException("newType") : null);            
            if (newType.IsAssignableFrom(obj.GetType()))
            {              
                return obj;
            }
            else 
            {
                return Convert.ChangeType(obj, newType, fmtProvider); // throws InvalidCastException, FormatException, OverflowException
            }
        }

        public static double GetVecLenL2(SparseVector<double>.ReadOnly vec)
        {
            ThrowException(vec == null ? new ArgumentNullException("vec") : null);
            double len = 0;
            ArrayList<double> datInner = vec.Inner.InnerDat;
            foreach (double val in datInner)
            {
                len += val * val;
            }
            return Math.Sqrt(len);
        }

        public static void NrmVecL2(SparseVector<double> vec)
        {
            ThrowException(vec == null ? new ArgumentNullException("vec") : null);
            double len = GetVecLenL2(vec);
            ThrowException(len == 0 ? new InvalidOperationException() : null);
            ArrayList<double> datInner = vec.InnerDat;
            for (int i = 0; i < vec.Count; i++)
            {
                vec.SetDirect(i, datInner[i] / len);
            }
        }

        public static bool TryNrmVecL2(SparseVector<double> vec)
        {
            ThrowException(vec == null ? new ArgumentNullException("vec") : null);
            double len = GetVecLenL2(vec);
            if (len == 0) { return false; }
            ArrayList<double> datInner = vec.InnerDat;
            for (int i = 0; i < vec.Count; i++)
            {
                vec.SetDirect(i, datInner[i] / len);
            }
            return true;
        }

        public const string DATE_TIME_SIMPLE
            = "yyyy-MM-dd HH:mm:ss K"; // simple date-time format (incl. time zone, if available)
        private static KeyDat<string, string>[] mTimeZones = new KeyDat<string, string>[] {
            new KeyDat<string, string>("ACDT", "+10:30"),
            new KeyDat<string, string>("ACST", "+09:30"),
            new KeyDat<string, string>("ACT", "+08:00"),
            new KeyDat<string, string>("ADT", "-03:00"),
            new KeyDat<string, string>("AEDT", "+11:00"),
            new KeyDat<string, string>("AEST", "+10:00"),
            new KeyDat<string, string>("AFT", "+04:30"),
            new KeyDat<string, string>("AKDT", "-08:00"),
            new KeyDat<string, string>("AKST", "-09:00"),
            new KeyDat<string, string>("AMST", "+05:00"),
            new KeyDat<string, string>("AMT", "+04:00"),
            new KeyDat<string, string>("ART", "-03:00"),
            new KeyDat<string, string>("AST", "+03:00"),
            new KeyDat<string, string>("AST", "+04:00"),
            new KeyDat<string, string>("AST", "+03:00"),
            new KeyDat<string, string>("AST", "-04:00"),
            new KeyDat<string, string>("AWDT", "+09:00"),
            new KeyDat<string, string>("AWST", "+08:00"),
            new KeyDat<string, string>("AZOST", "-01:00"),
            new KeyDat<string, string>("AZT", "+04:00"),
            new KeyDat<string, string>("BDT", "+08:00"),
            new KeyDat<string, string>("BIOT", "+06:00"),
            new KeyDat<string, string>("BIT", "-12:00"),
            new KeyDat<string, string>("BOT", "-04:00"),
            new KeyDat<string, string>("BRT", "-03:00"),
            new KeyDat<string, string>("BST", "+06:00"),
            new KeyDat<string, string>("BST", "+01:00"),
            new KeyDat<string, string>("BTT", "+06:00"),
            new KeyDat<string, string>("CAT", "+02:00"),
            new KeyDat<string, string>("CCT", "+06:30"),
            new KeyDat<string, string>("CDT", "-05:00"),
            new KeyDat<string, string>("CEDT", "+02:00"),
            new KeyDat<string, string>("CEST", "+02:00"),
            new KeyDat<string, string>("CET", "+01:00"),
            new KeyDat<string, string>("CHAST", "+12:45"),
            new KeyDat<string, string>("CIST", "-08:00"),
            new KeyDat<string, string>("CKT", "-10:00"),
            new KeyDat<string, string>("CLST", "-03:00"),
            new KeyDat<string, string>("CLT", "-04:00"),
            new KeyDat<string, string>("COST", "-04:00"),
            new KeyDat<string, string>("COT", "-05:00"),
            new KeyDat<string, string>("CST", "-06:00"),
            new KeyDat<string, string>("CST", "+08:00"),
            new KeyDat<string, string>("CVT", "-01:00"),
            new KeyDat<string, string>("CXT", "+07:00"),
            new KeyDat<string, string>("ChST", "+10:00"),
            new KeyDat<string, string>("DFT", "+01:00"),
            new KeyDat<string, string>("EAST", "-06:00"),
            new KeyDat<string, string>("EAT", "+03:00"),
            new KeyDat<string, string>("ECT", "-04:00"),
            new KeyDat<string, string>("ECT", "-05:00"),
            new KeyDat<string, string>("EDT", "-04:00"),
            new KeyDat<string, string>("EEDT", "+03:00"),
            new KeyDat<string, string>("EEST", "+03:00"),
            new KeyDat<string, string>("EET", "+02:00"),
            new KeyDat<string, string>("EST", "-05:00"),
            new KeyDat<string, string>("FJT", "+12:00"),
            new KeyDat<string, string>("FKST", "-03:00"),
            new KeyDat<string, string>("FKT", "-04:00"),
            new KeyDat<string, string>("GALT", "-06:00"),
            new KeyDat<string, string>("GET", "+04:00"),
            new KeyDat<string, string>("GFT", "-03:00"),
            new KeyDat<string, string>("GILT", "+12:00"),
            new KeyDat<string, string>("GIT", "-09:00"),
            new KeyDat<string, string>("GMT", "+00:00"),
            new KeyDat<string, string>("GST", "-02:00"),
            new KeyDat<string, string>("GYT", "-04:00"),
            new KeyDat<string, string>("HADT", "-09:00"),
            new KeyDat<string, string>("HAST", "-10:00"),
            new KeyDat<string, string>("HKT", "+08:00"),
            new KeyDat<string, string>("HMT", "+05:00"),
            new KeyDat<string, string>("HST", "-10:00"),
            new KeyDat<string, string>("IRKT", "+08:00"),
            new KeyDat<string, string>("IRST", "+03:30"),
            new KeyDat<string, string>("IST", "+05:30"),
            new KeyDat<string, string>("IST", "+01:00"),
            new KeyDat<string, string>("IST", "+02:00"),
            new KeyDat<string, string>("JST", "+09:00"),
            new KeyDat<string, string>("KRAT", "+07:00"),
            new KeyDat<string, string>("KST", "+09:00"),
            new KeyDat<string, string>("LHST", "+10:30"),
            new KeyDat<string, string>("LINT", "+14:00"),
            new KeyDat<string, string>("MAGT", "+11:00"),
            new KeyDat<string, string>("MDT", "-06:00"),
            new KeyDat<string, string>("MIT", "-09:30"),
            new KeyDat<string, string>("MSD", "+04:00"),
            new KeyDat<string, string>("MSK", "+03:00"),
            new KeyDat<string, string>("MST", "+08:00"),
            new KeyDat<string, string>("MST", "-07:00"),
            new KeyDat<string, string>("MST", "+06:30"),
            new KeyDat<string, string>("MUT", "+04:00"),
            new KeyDat<string, string>("NDT", "-02:30"),
            new KeyDat<string, string>("NFT", "+11:30"),
            new KeyDat<string, string>("NPT", "+05:45"),
            new KeyDat<string, string>("NST", "-03:30"),
            new KeyDat<string, string>("NT", "-03:30"),
            new KeyDat<string, string>("OMST", "+06:00"),
            new KeyDat<string, string>("PDT", "-07:00"),
            new KeyDat<string, string>("PETT", "+12:00"),
            new KeyDat<string, string>("PHOT", "+13:00"),
            new KeyDat<string, string>("PKT", "+05:00"),
            new KeyDat<string, string>("PST", "-08:00"),
            new KeyDat<string, string>("PST", "+08:00"),
            new KeyDat<string, string>("RET", "+04:00"),
            new KeyDat<string, string>("SAMT", "+04:00"),
            new KeyDat<string, string>("SAST", "+02:00"),
            new KeyDat<string, string>("SBT", "+11:00"),
            new KeyDat<string, string>("SCT", "+04:00"),
            new KeyDat<string, string>("SLT", "+05:30"),
            new KeyDat<string, string>("SST", "-11:00"),
            new KeyDat<string, string>("SST", "+08:00"),
            new KeyDat<string, string>("TAHT", "-10:00"),
            new KeyDat<string, string>("THA", "+07:00"),
            new KeyDat<string, string>("UTC", "+00:00"),
            new KeyDat<string, string>("UYST", "-02:00"),
            new KeyDat<string, string>("UYT", "-03:00"),
            new KeyDat<string, string>("VET", "-04:30"),
            new KeyDat<string, string>("VLAT", "+10:00"),
            new KeyDat<string, string>("WAT", "+01:00"),
            new KeyDat<string, string>("WEDT", "+01:00"),
            new KeyDat<string, string>("WEST", "+01:00"),
            new KeyDat<string, string>("WET", "+00:00"),
            new KeyDat<string, string>("YAKT", "+09:00"),
            new KeyDat<string, string>("YEKT", "+05:00") };

        public static string NormalizeDateTimeStr(string dateTime)
        {
            ThrowException(dateTime == null ? new ArgumentNullException("dateTime") : null);
            dateTime = dateTime.Trim();
            foreach (KeyDat<string, string> timeZone in mTimeZones)
            {
                if (dateTime.EndsWith(" " + timeZone.Key)) { dateTime = dateTime.Replace(timeZone.Key, timeZone.Dat); break; }
            }
            DateTime dt;
            try
            {
                dt = DateTime.Parse(dateTime);
            }
            catch
            {
                return null;
            }
            return dt.ToString(DATE_TIME_SIMPLE);
        }

        public static string Trunc(string str, int len)
        {
            //ThrowException(str == null ? new ArgumentNullException("str") : null); // *** nulls are allowed
            ThrowException(len < 0 ? new ArgumentOutOfRangeException("len") : null);
            return (str != null && str.Length > len) ? str.Substring(0, len) : str;
        }

        public static string MakeOneLine(string str)
        {
            return MakeOneLine(str, /*compact=*/false);
        }

        public static string MakeOneLine(string str, bool compact)
        {
            //ThrowException(str == null ? new ArgumentNullException("str") : null); // *** nulls are allowed
            if (str == null) { return null; }
            str = str.Replace("\r", "").Replace('\n', ' ').Trim();
            if (compact)
            { 
                str = Regex.Replace(str, @"\s\s+", " ");
            }
            return str;
        }

        public static string XmlReadValue(XmlReader reader, string attrName)
        {
            ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            ThrowException(attrName == null ? new ArgumentNullException("attrName") : null);
            if (reader.IsEmptyElement) { return ""; }
            string text = "";
            while (reader.Read() && reader.NodeType != XmlNodeType.Text && reader.NodeType != XmlNodeType.CDATA && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == attrName)) ;
            if (reader.NodeType == XmlNodeType.Text)
            {
                text = HttpUtility.HtmlDecode(reader.Value);
                XmlSkip(reader, attrName);
            }
            else if (reader.NodeType == XmlNodeType.CDATA)
            {
                text = reader.Value; // no decoding for CDATA
                XmlSkip(reader, attrName);
            }
            return text;
        }

        public static void XmlSkip(XmlReader reader, string attrName)
        {
            ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            ThrowException(attrName == null ? new ArgumentNullException("attrName") : null);
            if (reader.IsEmptyElement) { return; }
            while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == attrName)) ;
        }

        // *** Delegates ***

        public delegate T UnaryOperatorDelegate<T>(T val);
        public delegate T BinaryOperatorDelegate<T>(T a, T b);
    }
}
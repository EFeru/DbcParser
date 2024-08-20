using System;
using System.Collections.Generic;
using DbcParserLib.Model;

namespace DbcParserLib
{
    public static class ExtensionsAndHelpers
    {
        public static bool Motorola(this Signal signal)
        {
            return signal.Msb();
        }

        public static bool Msb(this Signal signal)
        {
            return signal.ByteOrder == 0;
        }

        public static bool Lsb(this Signal signal)
        {
            return signal.ByteOrder != 0;
        }

        public static bool Intel(this Signal signal)
        {
            return signal.Lsb();
        }
        
        internal static ulong BitMask(this Signal signal)
        {
            return (ulong.MaxValue >> (64 - signal.Length));
        }

        internal static bool TryParseToDict(this string records, out IReadOnlyDictionary<int, string> dict)
        {
            return StringToDictionaryParser.ParseString(records, out dict);
        }
        
        internal static bool AreDoublesEqual(double a, double b, double epsilon = 1e-10)
        {
            return Math.Abs(a - b) < epsilon;
        }
        
        internal static bool IsDoubleZero(double a, double epsilon = 1e-10)
        {
            return Math.Abs(a) < epsilon;
        }
        
        public static bool IsWholeNumber(double value)
        {
            return IsDoubleZero(value % 1);
        }
    }

    internal class StringToDictionaryParser
    {
        private IDictionary<int, string> m_dictionary;

        private StringToDictionaryParser(IDictionary<int, string> dict)
        {
            m_dictionary = dict;
        }
        
        public static bool ParseString(string text, out IReadOnlyDictionary<int, string> dictionary)
        {
            dictionary = null;
            var internalDictionary = new Dictionary<int, string>();
            var parser = new StringToDictionaryParser(internalDictionary);
            if (parser.ParseKey(text, 0))
            {
                dictionary = internalDictionary;
                return true;
            }
            return false;
        }
        
        private bool ParseKey(string text, int offset)
        {
            var index = text.IndexOf("\"", offset, StringComparison.InvariantCulture);
            if(index == -1)
                return true;

            var key = text.Substring(offset, index - offset);
            offset = index + 1;
            return int.TryParse(key, out var intKey) ? ParseValue(text, offset, intKey) : false;
        }

        private bool ParseValue(string text, int offset, int key)
        {
            var index = text.IndexOf("\"", offset, StringComparison.InvariantCulture);
            if (index == -1)
                return false;

            var value = text.Substring(offset, index - offset);

            m_dictionary[key] = value;
            offset = index +1;
            return ParseKey(text, offset);
        }
    }
}
/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime {
    /// <summary>
    /// Simple implementation of ASCII encoding/decoding.  The default instance (PythonAsciiEncoding.Instance) is
    /// setup to always convert even values outside of the ASCII range.  The EncoderFallback/DecoderFallbacks can
    /// be replaced with versions that will throw exceptions instead though.
    /// </summary>
    [Serializable]
    sealed class PythonAsciiEncoding : Encoding {
        internal static readonly Encoding Instance = MakeNonThrowing();

        internal PythonAsciiEncoding()
            : base() {
        }

        internal static Encoding MakeNonThrowing() {
            // we need to Clone the new instance here so that the base class marks us as non-readonly
            Encoding enc = (Encoding)new PythonAsciiEncoding().Clone();
#if !SILVERLIGHT
            enc.DecoderFallback = new NonStrictDecoderFallback();
            enc.EncoderFallback = new NonStrictEncoderFallback();
#endif
            return enc;
        }

        public override int GetByteCount(char[] chars, int index, int count) {
#if SILVERLIGHT
            return count;
#else
            int byteCount = 0;
            int charEnd = index + count;
            while (index < charEnd) {
                char c = chars[index];
                if (c > 0x7f) {
                    EncoderFallbackBuffer efb = EncoderFallback.CreateFallbackBuffer();
                    if (efb.Fallback(c, index)) {
                        byteCount += efb.Remaining;
                    }
                } else {
                    byteCount++;
                }
                index++;
            }
            return byteCount;
#endif
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) {
            int charEnd = charIndex + charCount;
            int outputBytes = 0;
            while (charIndex < charEnd) {
                char c = chars[charIndex];
#if !SILVERLIGHT
                if (c > 0x7f) {
                    EncoderFallbackBuffer efb = EncoderFallback.CreateFallbackBuffer();
                    if (efb.Fallback(c, charIndex)) {
                        while (efb.Remaining != 0) {
                            bytes[byteIndex++] = (byte)efb.GetNextChar();
                            outputBytes++;
                        }
                    }
                } else {
                    bytes[byteIndex++] = (byte)c;
                    outputBytes++;
                }
#else
                bytes[byteIndex++] = (byte)c;
                outputBytes++;
#endif
                charIndex++;
            }
            return outputBytes;
        }

        public override int GetCharCount(byte[] bytes, int index, int count) {
            int byteEnd = index + count;
            int outputChars = 0;
            while (index < byteEnd) {
                byte b = bytes[index];
#if !SILVERLIGHT
                if (b > 0x7f) {
                    DecoderFallbackBuffer dfb = DecoderFallback.CreateFallbackBuffer();
                    if (dfb.Fallback(new byte[] { b }, index)) {
                        outputChars += dfb.Remaining;
                    }
                } else {
                    outputChars++;
                }
#else
                outputChars++;
#endif
                index++;
            }
            return outputChars;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) {
            int byteEnd = byteIndex + byteCount;
            int outputChars = 0;
            while (byteIndex < byteEnd) {
                byte b = bytes[byteIndex];
#if !SILVERLIGHT
                if (b > 0x7f) {
                    DecoderFallbackBuffer dfb = DecoderFallback.CreateFallbackBuffer();
                    if (dfb.Fallback(new byte[] { b }, byteIndex)) {
                        while (dfb.Remaining != 0) {
                            chars[charIndex++] = dfb.GetNextChar();
                            outputChars++;
                        }
                    }
                } else {
                    chars[charIndex++] = (char)b;
                    outputChars++;
                }
#else
                chars[charIndex++] = (char)b;
                outputChars++;
#endif
                byteIndex++;
            }
            return outputChars;
        }

        public override int GetMaxByteCount(int charCount) {
            return charCount * 4;
        }

        public override int GetMaxCharCount(int byteCount) {
            return byteCount;
        }

        public override string WebName {
            get {
                return "ascii";
            }
        }

#if !SILVERLIGHT
        public override string EncodingName {
            get {
                return "ascii";
            }
        }
#endif
    }

#if !SILVERLIGHT
    class NonStrictEncoderFallback : EncoderFallback {
        public override EncoderFallbackBuffer CreateFallbackBuffer() {
            return new NonStrictEncoderFallbackBuffer();
        }
        
        public override int MaxCharCount {
            get { return 1; }
        }
    }

    class NonStrictEncoderFallbackBuffer : EncoderFallbackBuffer {
        private List<char> _buffer = new List<char>();
        private int _index;

        public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index) {
            throw PythonOps.UnicodeDecodeError("'ascii' codec can't encode character '\\u{0:X}{1:04X}' in position {1}", (int)charUnknownHigh, (int)charUnknownLow, index);
        }

        public override bool Fallback(char charUnknown, int index) {
            if (charUnknown > 0xff) {
                throw PythonOps.UnicodeDecodeError("'ascii' codec can't encode character '\\u{0:X}' in position {1}", (int)charUnknown, index);
            }

            _buffer.Add(charUnknown);
            return true;
        }
        
        public override char GetNextChar() {
            return _buffer[_index++];
        }

        public override bool MovePrevious() {
            if (_index > 0) {
                _index--;
                return true;
            }
            return false;
        }

        public override int Remaining {
            get { return _buffer.Count - _index; }
        }
    }

    class NonStrictDecoderFallback : DecoderFallback {
        public override DecoderFallbackBuffer CreateFallbackBuffer() {
            return new NonStrictDecoderFallbackBuffer();
        }

        public override int MaxCharCount {
            get { return 1; }
        }
    }

    // no ctors on DecoderFallbackBuffer in Silverlight
    class NonStrictDecoderFallbackBuffer : DecoderFallbackBuffer {
        private List<byte> _bytes = new List<byte>();
        private int _index;

        public override bool Fallback(byte[] bytesUnknown, int index) {
            _bytes.AddRange(bytesUnknown);
            return true;
        }

        public override char GetNextChar() {
            if (_index == _bytes.Count) {
                return char.MinValue;
            }
            return (char)_bytes[_index++];
        }

        public override bool MovePrevious() {
            if (_index > 0) {
                _index--;
                return true;
            }
            return false;
        }

        public override int Remaining {
            get { return _bytes.Count - _index; }
        }
    }
#endif
}

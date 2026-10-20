using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LauncherPatcher.Patching;

namespace LauncherPatcher.Hmcl
{
    /// <summary>
    /// Minimal JVM class-file rewriter: replaces whole method bodies without external libraries.
    /// </summary>
    internal static class ClassPatcher
    {
        /// <summary>
        /// Replaces the body of every method in <paramref name="specs"/> with its replacement code.
        /// The class file is fully rebuilt: patched methods get a brand-new, self-consistent Code
        /// attribute (no unreachable code, no stale exception table / StackMapTable), everything
        /// else is copied verbatim.
        /// </summary>
        /// <exception cref="PatchException">The class is malformed or not every target method was found.</exception>
        public static byte[] PatchClass(byte[] classBytes, MethodSpec[] specs)
        {
            int hits;
            byte[] output;
            try
            {
                output = PatchClassInner(classBytes, specs, out hits);
            }
            catch (PatchException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PatchException("failed to parse class file: " + ex.Message, ex);
            }

            if (hits != specs.Length)
                throw new PatchException($"only matched {hits}/{specs.Length} target methods");
            return output;
        }

        /// <summary>
        /// Returns the code bytes of the given method, or <c>null</c> when the method or its
        /// Code attribute is missing.
        /// </summary>
        /// <exception cref="PatchException">The class is malformed.</exception>
        public static byte[] GetMethodCode(byte[] classBytes, string name, string desc)
        {
            try
            {
                return GetMethodCodeInner(classBytes, name, desc);
            }
            catch (PatchException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PatchException("failed to parse class file: " + ex.Message, ex);
            }
        }

        private sealed class AttrInfo
        {
            public readonly int NameIdx;
            public readonly int Length;
            public readonly int Start;

            public AttrInfo(int nameIdx, int length, int start)
            {
                NameIdx = nameIdx;
                Length = length;
                Start = start;
            }
        }

        private static byte[] PatchClassInner(byte[] b, MethodSpec[] specs, out int hits)
        {
            hits = 0;
            int pos = 0;
            string[] utf8 = ReadConstantPool(b, ref pos);
            SkipHeader(b, ref pos);

            // ---- methods (targets get a rebuilt Code attribute) ----
            int methodCount = ReadU2(b, ref pos);
            var outStream = new MemoryStream();
            int copyFrom = 0;

            for (int i = 0; i < methodCount; i++)
            {
                int mStart = pos;
                int flags = ReadU2(b, ref pos);
                int nameIdx = ReadU2(b, ref pos);
                int descIdx = ReadU2(b, ref pos);
                string methodName = utf8[nameIdx];
                string desc = utf8[descIdx];

                int attrCount = ReadU2(b, ref pos);
                var attrs = new List<AttrInfo>();
                for (int j = 0; j < attrCount; j++)
                {
                    int aNameIdx = ReadU2(b, ref pos);
                    int aLen = ReadU4(b, ref pos);
                    attrs.Add(new AttrInfo(aNameIdx, aLen, pos));
                    pos += aLen;
                }
                int mEnd = pos;

                MethodSpec target = null;
                if (HasCodeAttribute(attrs, utf8))
                {
                    foreach (var spec in specs)
                    {
                        if (methodName == spec.Name && desc == spec.Desc)
                        {
                            target = spec;
                            break;
                        }
                    }
                }

                if (target == null)
                    continue;

                outStream.Write(b, copyFrom, mStart - copyFrom);

                WriteU2(outStream, flags);
                WriteU2(outStream, nameIdx);
                WriteU2(outStream, descIdx);
                WriteU2(outStream, attrCount);
                foreach (var attr in attrs)
                {
                    bool isCode = utf8[attr.NameIdx] == "Code";
                    WriteU2(outStream, attr.NameIdx);
                    if (!isCode)
                    {
                        WriteU4(outStream, attr.Length);
                        outStream.Write(b, attr.Start, attr.Length);
                        continue;
                    }

                    // Rebuild the Code attribute: keep max_stack/max_locals, body = the
                    // replacement patch only, no exception table, no nested attributes (so no
                    // StackMapTable is required - the code is linear and fully reachable).
                    int maxStack = ReadU2(b, attr.Start);
                    int maxLocals = ReadU2(b, attr.Start + 2);
                    WriteU4(outStream, target.Code.Length + 12);
                    WriteU2(outStream, maxStack);
                    WriteU2(outStream, maxLocals);
                    WriteU4(outStream, target.Code.Length);
                    outStream.Write(target.Code, 0, target.Code.Length);
                    WriteU2(outStream, 0); // exception_table_length
                    WriteU2(outStream, 0); // attributes_count
                }

                copyFrom = mEnd;
                hits++;
            }

            if (hits == 0)
                return b;

            outStream.Write(b, copyFrom, b.Length - copyFrom);
            return outStream.ToArray();
        }

        private static byte[] GetMethodCodeInner(byte[] b, string name, string desc)
        {
            int pos = 0;
            string[] utf8 = ReadConstantPool(b, ref pos);
            SkipHeader(b, ref pos);

            int methodCount = ReadU2(b, ref pos);
            for (int i = 0; i < methodCount; i++)
            {
                pos += 2; // access_flags
                int nameIdx = ReadU2(b, ref pos);
                int descIdx = ReadU2(b, ref pos);
                bool isTarget = utf8[nameIdx] == name && utf8[descIdx] == desc;

                int attrCount = ReadU2(b, ref pos);
                for (int j = 0; j < attrCount; j++)
                {
                    int aNameIdx = ReadU2(b, ref pos);
                    int aLen = ReadU4(b, ref pos);
                    if (isTarget && utf8[aNameIdx] == "Code")
                    {
                        int codeLength = ReadU4(b, pos + 4); // skip max_stack and max_locals
                        var code = new byte[codeLength];
                        Array.Copy(b, pos + 8, code, 0, codeLength);
                        return code;
                    }
                    pos += aLen;
                }
            }
            return null;
        }

        /// <summary>Validates the magic and reads the constant pool, keeping only Utf8 entries.</summary>
        private static string[] ReadConstantPool(byte[] b, ref int pos)
        {
            if (ReadU4(b, ref pos) != unchecked((int)0xCAFEBABE))
                throw new PatchException("bad magic");

            pos += 4; // minor + major version

            int cpCount = ReadU2(b, ref pos);
            var utf8 = new string[cpCount];
            for (int idx = 1; idx < cpCount; idx++)
            {
                byte tag = b[pos++];
                switch (tag)
                {
                    case 1: // Utf8
                        int len = ReadU2(b, ref pos);
                        utf8[idx] = Encoding.UTF8.GetString(b, pos, len);
                        pos += len;
                        break;
                    case 3: // Integer
                    case 4: // Float
                        pos += 4;
                        break;
                    case 5: // Long
                    case 6: // Double
                        pos += 8;
                        idx++; // takes two constant pool slots
                        break;
                    case 15: // MethodHandle
                        pos += 3;
                        break;
                    case 7:  // Class
                    case 8:  // String
                    case 16: // MethodType
                    case 19: // Module
                    case 20: // Package
                        pos += 2;
                        break;
                    default:
                        // Fieldref(9), Methodref(10), InterfaceMethodref(11),
                        // NameAndType(12), Dynamic(17), InvokeDynamic(18)
                        pos += 4;
                        break;
                }
            }
            return utf8;
        }

        /// <summary>Skips the class header and the field table, which are copied verbatim.</summary>
        private static void SkipHeader(byte[] b, ref int pos)
        {
            pos += 2; // access_flags
            pos += 2; // this_class
            pos += 2; // super_class

            int ifaces = ReadU2(b, ref pos);
            pos += 2 * ifaces;

            int fieldCount = ReadU2(b, ref pos);
            for (int i = 0; i < fieldCount; i++)
            {
                pos += 6; // access_flags, name_index, descriptor_index
                int attrCount = ReadU2(b, ref pos);
                for (int j = 0; j < attrCount; j++)
                {
                    pos += 2; // attribute_name_index
                    int attrLen = ReadU4(b, ref pos);
                    pos += attrLen;
                }
            }
        }

        private static bool HasCodeAttribute(List<AttrInfo> attrs, string[] utf8)
        {
            for (int i = 0; i < attrs.Count; i++)
                if (utf8[attrs[i].NameIdx] == "Code")
                    return true;
            return false;
        }

        private static int ReadU2(byte[] b, int pos)
        {
            return (b[pos] << 8) | b[pos + 1];
        }

        private static int ReadU2(byte[] b, ref int pos)
        {
            int v = (b[pos] << 8) | b[pos + 1];
            pos += 2;
            return v;
        }

        private static int ReadU4(byte[] b, ref int pos)
        {
            int v = (int)(((uint)b[pos] << 24) | ((uint)b[pos + 1] << 16) | ((uint)b[pos + 2] << 8) | b[pos + 3]);
            pos += 4;
            return v;
        }

        private static int ReadU4(byte[] b, int pos)
        {
            int dummy = pos;
            return ReadU4(b, ref dummy);
        }

        private static void WriteU2(MemoryStream stream, int value)
        {
            stream.WriteByte((byte)((value >> 8) & 0xFF));
            stream.WriteByte((byte)(value & 0xFF));
        }

        private static void WriteU4(MemoryStream stream, int value)
        {
            stream.WriteByte((byte)((value >> 24) & 0xFF));
            stream.WriteByte((byte)((value >> 16) & 0xFF));
            stream.WriteByte((byte)((value >> 8) & 0xFF));
            stream.WriteByte((byte)(value & 0xFF));
        }
    }

    /// <summary>
    /// One method to patch: name, descriptor and the bytecode that replaces its body.
    /// </summary>
    internal sealed class MethodSpec
    {
        public MethodSpec(string name, string desc, byte[] code)
        {
            Name = name;
            Desc = desc;
            Code = code;
        }

        public string Name { get; }

        public string Desc { get; }

        public byte[] Code { get; }
    }
}

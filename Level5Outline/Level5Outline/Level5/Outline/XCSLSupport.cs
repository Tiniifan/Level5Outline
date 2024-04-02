using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Level5Outline.Level5.Outline
{
    public class XCSLSupport
    {
        public struct Header
        {
            public uint Magic;
            public int OutlineOffset;
            public int MeshOffset;
            public int MeshLength;
            public int CMBOffset1;
            public int CMBLength1;
            public int CMBOffset2;
            public int CMBLength2;
        }

        public struct CMBHeader
        {
            public ulong Magic;
            public short Unk1;
            public short Unk2;
        }

        public struct OutlineData
        {
            public float Unk1;
            public float Unk2;
            public int Unk3;
            public float Unk4;

            public float Unk5;
            public float Unk6;
            public float Unk7;
            public int Unk8;

            public float Unk9;
            public float Unk10;
            public float Unk11;
            public float Unk12;

            public float Unk13;
            public float Unk14;
            public float Unk15;
            public float Unk16;

            public float Unk17;
            public float Unk18;
            public int Unk19;
            public int Unk20;

            public float Unk21;
            public float Unk22;
            public float Unk23;
            public float Unk24;

            public float Unk25;
            public float Unk26;
            public float Unk27;
            public int Unk28;

            public static List<object> GetValues(OutlineData data)
            {
                List<object> values = new List<object>();
                Type outlineType = typeof(OutlineData);
                FieldInfo[] fields = outlineType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(float))
                    {
                        values.Add((float)field.GetValue(data));
                    } else if (field.FieldType == typeof(int))
                    {
                        values.Add((int)field.GetValue(data));
                    }
                }

                return values;
            }
        }
    }
}

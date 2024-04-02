using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Level5Outline.Tools;
using Level5Outline.Level5.Compression;
using Level5Outline.Level5.Compression.NoCompression;

namespace Level5Outline.Level5.Outline
{
    public class XCSL
    {
        [JsonProperty("HashName")]
        public string HashName { get; set; }

        [JsonProperty("OutlineMeshData")]
        public OutlineMeshData OutlineMeshData { get; set; }

        [JsonProperty("CMB1")]
        public int[] CMB1 { get; set; }

        [JsonProperty("CMB2")]
        public int[] CMB2 { get; set; }

        public XCSL()
        {

        }

        public XCSL(Stream stream)
        {
            OutlineMeshData = new OutlineMeshData();

            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                var header = reader.ReadStruct<XCSLSupport.Header>();

                // Outline
                reader.Seek(header.OutlineOffset);
                byte[] xcslDataDecomp = Compressor.Decompress(reader.GetSection(header.CMBOffset1 - (int)reader.Position));
                using (BinaryDataReader xcslDataDecompReader = new BinaryDataReader(xcslDataDecomp))
                {
                    HashName = xcslDataDecompReader.ReadValue<uint>().ToString("X8");
                    xcslDataDecompReader.Skip(0x04);

                    int meshCount = xcslDataDecompReader.ReadValue<int>();
                    xcslDataDecompReader.Seek(header.MeshOffset);

                    for (int i = 0; i < meshCount; i++)
                    {
                        OutlineMeshData.Hashes.Add(xcslDataDecompReader.ReadValue<uint>().ToString("X8"));
                    }

                    xcslDataDecompReader.Seek(0x0C);

                    OutlineMeshData.Values = XCSLSupport.OutlineData.GetValues(xcslDataDecompReader.ReadStruct<XCSLSupport.OutlineData>()).ToArray();
                }

                // CMB 1
                reader.Seek(header.CMBOffset1);
                reader.Skip(0x0C);
                byte[] cmbDecomp1 = Compressor.Decompress(reader.GetSection(header.CMBLength1));
                using (BinaryDataReader cmbDecompReader1 = new BinaryDataReader(cmbDecomp1))
                {
                    CMB1 = cmbDecompReader1.ReadMultipleValue<byte>(32).Select(x => (int)x).ToArray();
                }

                // CMB 2
                reader.Seek(header.CMBOffset2);
                reader.Skip(0x0C);
                byte[] cmbDecomp2 = Compressor.Decompress(reader.GetSection(header.CMBLength2));
                using (BinaryDataReader cmbDecompReader2 = new BinaryDataReader(cmbDecomp2))
                {
                    CMB2 = cmbDecompReader2.ReadMultipleValue<byte>(32).Select(x => (int)x).ToArray();
                }
            }
        }

        public void Save(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                BinaryDataWriter writer = new BinaryDataWriter(stream);

                XCSLSupport.Header header = new XCSLSupport.Header
                {
                    Magic = 0x4C534358,
                    OutlineOffset = 0x20,
                    MeshOffset = 0x0,
                    MeshLength = 0x0,
                    CMBOffset1 = 0x0,
                    CMBLength1 = 0x0,
                    CMBOffset2 = 0x0,
                    CMBLength2 = 0x0,
                };

                // Write outlineMeshDataOffset
                writer.Seek(0x20);
                using (MemoryStream outlineMeshDataStream = new MemoryStream())
                {
                    using (BinaryDataWriter outlineMeshDataWriter = new BinaryDataWriter(outlineMeshDataStream))
                    {
                        outlineMeshDataWriter.Write(Convert.ToUInt32(HashName, 16));
                        outlineMeshDataWriter.Write(0x00);
                        outlineMeshDataWriter.Write(OutlineMeshData.Hashes.Count);

                        foreach (object value in OutlineMeshData.Values)
                        {
                            if (value.GetType() == typeof(int) || value.GetType() == typeof(Int64))
                            {
                                outlineMeshDataWriter.Write(Convert.ToInt32(value));
                            }
                            else if (value.GetType() == typeof(float) || value.GetType() == typeof(double))
                            {
                                outlineMeshDataWriter.Write(Convert.ToSingle(value));
                            }
                        }

                        header.MeshOffset = (OutlineMeshData.Values.Count() + 3) * 4;
                        header.MeshLength = OutlineMeshData.Hashes.Count * 4;

                        foreach (string key in OutlineMeshData.Hashes)
                        {
                            outlineMeshDataWriter.Write(Convert.ToUInt32(key, 16));
                        }
                    }

                    writer.Write(new NoCompression().Compress(outlineMeshDataStream.ToArray()));
                }

                // Write CMB1 header
                header.CMBOffset1 = (int)writer.Position;
                writer.Write(0x0000303043424D43);
                writer.Write(0x0001000C);

                // Write CMB1 data
                using (MemoryStream CMB1DataStream = new MemoryStream())
                {
                    using (BinaryDataWriter CMB1DataWriter = new BinaryDataWriter(CMB1DataStream))
                    {
                        CMB1DataWriter.WriteMultipleStruct(CMB1.Select(x => Convert.ToByte(x)).ToArray());
                    }

                    byte[] compressedCMB1 = new NoCompression().Compress(CMB1DataStream.ToArray());
                    writer.Write(compressedCMB1);
                    header.CMBLength1 = compressedCMB1.Length + 12;
                }

                // Write CMB2 header
                header.CMBOffset2 = (int)writer.Position;
                writer.Write(0x0000303043424D43);
                writer.Write(0x0001000C);

                // Write CMB2 data
                using (MemoryStream CMB2DataStream = new MemoryStream())
                {
                    using (BinaryDataWriter CMB2DataWriter = new BinaryDataWriter(CMB2DataStream))
                    {
                        CMB2DataWriter.WriteMultipleStruct(CMB2.Select(x => Convert.ToByte(x)).ToArray());
                    }

                    byte[] compressedCMB2 = new NoCompression().Compress(CMB2DataStream.ToArray());
                    writer.Write(compressedCMB2);
                    header.CMBLength2 = compressedCMB2.Length + 12;
                }

                // Write header
                writer.Seek(0);
                writer.WriteStruct<XCSLSupport.Header>(header);
            }
        }
    }
}

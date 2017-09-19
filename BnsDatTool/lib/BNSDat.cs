using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BnsDatTool
{
    public enum BXML_TYPE
    {
        BXML_XML = 1,
        BXML_PLAIN,
        BXML_BINARY,
        BXML_UNKNOWN
    };

    class BPKG_FTE
    {
        public int FilePathLength;
        public string FilePath;
        public byte Unknown_001;
        public bool IsCompressed;
        public bool IsEncrypted;
        public byte Unknown_002;
        public int FileDataSizeUnpacked;
        public int FileDataSizeSheared; // without padding for AES
        public int FileDataSizeStored;
        public int FileDataOffset; // (relative) offset
        public byte[] Padding;

    }

    public class ExtractEventListener
    {
        public Action<int> NumberOfFiles;
        public Action ProcessedFile;

    }
    public class BNSDat
    {


        // public class BNSDat
        // {

        public string AES_KEY = "bns_obt_kr_2014#";

        public byte[] XOR_KEY = new byte[16] { 164, 159, 216, 179, 246, 142, 57, 194, 45, 224, 97, 117, 92, 75, 26, 7 };

        public string BytesToHex(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];

            byte b;

            for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
            {
                b = ((byte)(bytes[bx] >> 4));
                c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

                b = ((byte)(bytes[bx] & 0x0F));
                c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
            }

            return new string(c);
        }

        private byte[] Decrypt(byte[] buffer, int size)
        {
            // AES requires buffer to consist of blocks with 16 bytes (each)
            // expand last block by padding zeros if required...
            // -> the encrypted data in BnS seems already to be aligned to blocks
            int AES_BLOCK_SIZE = AES_KEY.Length;
            int sizePadded = size + AES_BLOCK_SIZE;
            byte[] output = new byte[sizePadded];
            byte[] tmp = new byte[sizePadded];
            buffer.CopyTo(tmp, 0);
            buffer = null;

            Rijndael aes = Rijndael.Create();
            aes.Mode = CipherMode.ECB;
            ICryptoTransform decrypt = aes.CreateDecryptor(Encoding.ASCII.GetBytes(AES_KEY), new byte[16]);
            decrypt.TransformBlock(tmp, 0, sizePadded, output, 0);
            tmp = output;
            output = new byte[size];
            Array.Copy(tmp, 0, output, 0, size);
            tmp = null;

            return output;
        }

        public byte[] Deflate(byte[] buffer, int sizeCompressed, int sizeDecompressed)
        {
            byte[] tmp = Ionic.Zlib.ZlibStream.UncompressBuffer(buffer);

            //if (tmp.Length != sizeDecompressed)
            {
                byte[] tmp2 = new byte[sizeDecompressed];

                if (tmp.Length > sizeDecompressed)
                    Array.Copy(tmp, 0, tmp2, 0, sizeDecompressed);
                else
                    Array.Copy(tmp, 0, tmp2, 0, tmp.Length);
                tmp = tmp2;
                tmp2 = null;
            }
            return tmp;
        }

        public byte[] Unpack(byte[] buffer, int sizeStored, int sizeSheared, int sizeUnpacked, bool isEncrypted, bool isCompressed)
        {
            byte[] output = buffer;

            if (isEncrypted)
            {
                output = Decrypt(output, sizeStored);
            }

            if (isCompressed)
            {
                output = Deflate(output, sizeSheared, sizeUnpacked);
            }

            // neither encrypted, nor compressed -> raw copy
            if (output == buffer)
            {
                output = new byte[sizeUnpacked];
                if (sizeSheared < sizeUnpacked)
                {
                    Array.Copy(buffer, 0, output, 0, sizeSheared);
                }
                else
                {
                    Array.Copy(buffer, 0, output, 0, sizeUnpacked);
                }
            }

            return output;
        }

        public byte[] Inflate(byte[] buffer, int sizeDecompressed, out int sizeCompressed, int compressionLevel)
        {

            MemoryStream output = new MemoryStream();
            Ionic.Zlib.ZlibStream zs = new Ionic.Zlib.ZlibStream(output, Ionic.Zlib.CompressionMode.Compress, (Ionic.Zlib.CompressionLevel)compressionLevel, true);
            zs.Write(buffer, 0, sizeDecompressed);
            zs.Flush();
            zs.Close();
            sizeCompressed = (int)output.Length;
            return output.ToArray();
        }

        private byte[] Encrypt(byte[] buffer, int size, out int sizePadded)
        {
            int AES_BLOCK_SIZE = AES_KEY.Length;
            sizePadded = size + (AES_BLOCK_SIZE - (size % AES_BLOCK_SIZE));
            byte[] output = new byte[sizePadded];
            byte[] temp = new byte[sizePadded];
            Array.Copy(buffer, 0, temp, 0, buffer.Length);
            buffer = null;
            Rijndael aes = Rijndael.Create();
            aes.Mode = CipherMode.ECB;

            ICryptoTransform encrypt = aes.CreateEncryptor(Encoding.ASCII.GetBytes(AES_KEY), new byte[16]);
            encrypt.TransformBlock(temp, 0, sizePadded, output, 0);
            temp = null;
            return output;
        }

        private byte[] Pack(byte[] buffer, int sizeUnpacked, out int sizeSheared, out int sizeStored, bool encrypt, bool compress, int compressionLevel)
        {
            byte[] output = buffer;
            buffer = null;
            sizeSheared = sizeUnpacked;
            sizeStored = sizeSheared;

            if (compress)
            {
                byte[] tmp = Inflate(output, sizeUnpacked, out sizeSheared, compressionLevel);
                sizeStored = sizeSheared;
                output = tmp;
                tmp = null;
            }

            if (encrypt)
            {
                byte[] tmp = Encrypt(output, sizeSheared, out sizeStored);
                output = tmp;
                tmp = null;
            }
            return output;
        }

        public void Extract(string FileName, Action<int, int> processedEvent, bool is64 = false)
        {

            FileStream fs = new FileStream(FileName, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            string file_path;
            byte[] buffer_packed;
            byte[] buffer_unpacked;

            byte[] Signature = br.ReadBytes(8);
            uint Version = br.ReadUInt32();

            byte[] Unknown_001 = br.ReadBytes(5);
            int FileDataSizePacked = is64 ? (int)br.ReadInt64() : br.ReadInt32();
            int FileCount = is64 ? (int)br.ReadInt64() : br.ReadInt32();
            bool IsCompressed = br.ReadByte() == 1;
            bool IsEncrypted = br.ReadByte() == 1;
            byte[] Unknown_002 = br.ReadBytes(62);
            int FileTableSizePacked = is64 ? (int)br.ReadInt64() : br.ReadInt32();
            int FileTableSizeUnpacked = is64 ? (int)br.ReadInt64() : br.ReadInt32();

            buffer_packed = br.ReadBytes(FileTableSizePacked);
            int OffsetGlobal = is64 ? (int)br.ReadInt64() : br.ReadInt32();
            OffsetGlobal = (int)br.BaseStream.Position; // don't trust value, read current stream location.

            byte[] FileTableUnpacked = Unpack(buffer_packed, FileTableSizePacked, FileTableSizePacked, FileTableSizeUnpacked, IsEncrypted, IsCompressed);
            buffer_packed = null;
            MemoryStream ms = new MemoryStream(FileTableUnpacked);
            BinaryReader br2 = new BinaryReader(ms);

            for (int i = 0; i < FileCount; i++)
            {
                BPKG_FTE FileTableEntry = new BPKG_FTE();
                FileTableEntry.FilePathLength = is64 ? (int)br2.ReadInt64() : br2.ReadInt32();
                FileTableEntry.FilePath = Encoding.Unicode.GetString(br2.ReadBytes(FileTableEntry.FilePathLength * 2));
                FileTableEntry.Unknown_001 = br2.ReadByte();
                FileTableEntry.IsCompressed = br2.ReadByte() == 1;
                FileTableEntry.IsEncrypted = br2.ReadByte() == 1;
                FileTableEntry.Unknown_002 = br2.ReadByte();
                FileTableEntry.FileDataSizeUnpacked = is64 ? (int)br2.ReadInt64() : br2.ReadInt32();
                FileTableEntry.FileDataSizeSheared = is64 ? (int)br2.ReadInt64() : br2.ReadInt32();
                FileTableEntry.FileDataSizeStored = is64 ? (int)br2.ReadInt64() : br2.ReadInt32();
                FileTableEntry.FileDataOffset = (is64 ? (int)br2.ReadInt64() : br2.ReadInt32()) + OffsetGlobal;
                FileTableEntry.Padding = br2.ReadBytes(60);

                file_path = string.Format("{0}.files\\{1}", FileName, FileTableEntry.FilePath);
                if (!Directory.Exists((new FileInfo(file_path)).DirectoryName))
                    Directory.CreateDirectory((new FileInfo(file_path)).DirectoryName);

                // Console.Write("\rExtracting Files: {0}/{1}", (i + 1), FileCount);

                br.BaseStream.Position = FileTableEntry.FileDataOffset;
                buffer_packed = br.ReadBytes(FileTableEntry.FileDataSizeStored);
                buffer_unpacked = Unpack(buffer_packed, FileTableEntry.FileDataSizeStored, FileTableEntry.FileDataSizeSheared, FileTableEntry.FileDataSizeUnpacked, FileTableEntry.IsEncrypted, FileTableEntry.IsCompressed);
                buffer_packed = null;
                FileTableEntry = null;

                if (file_path.EndsWith("xml") || file_path.EndsWith("x16"))
                {
                    // decode bxml
                    MemoryStream temp = new MemoryStream();
                    MemoryStream temp2 = new MemoryStream(buffer_unpacked);
                    BXML bns_xml = new BXML(XOR_KEY);
                    Convert(temp2, bns_xml.DetectType(temp2), temp, BXML_TYPE.BXML_PLAIN);
                    temp2.Close();
                    File.WriteAllBytes(file_path, temp.ToArray());
                    temp.Close();
                    buffer_unpacked = null;
                }
                else
                {
                    // extract raw
                    File.WriteAllBytes(file_path, buffer_unpacked);
                    buffer_unpacked = null;
                }
                processedEvent((i + 1), FileCount);
            }

            br2.Close();
            ms.Close();
            br2 = null;
            ms = null;
            br.Close();
            fs.Close();
            br = null;
            fs = null;
        }

        public void Compress(string Folder, Action<int, int> processedEvent, bool is64 = false, int compression = 9)
        {
            string file_path;
            byte[] buffer_packed;
            byte[] buffer_unpacked;
            //Stopwatch stopWatch = new Stopwatch();

            string[] files = Directory.EnumerateFiles(Folder, "*", SearchOption.AllDirectories).ToArray();

            int FileCount = files.Count();

            BPKG_FTE FileTableEntry = new BPKG_FTE();
            MemoryStream mosTablems = new MemoryStream();
            BinaryWriter mosTable = new BinaryWriter(mosTablems);
            MemoryStream mosFilesms = new MemoryStream();
            BinaryWriter mosFiles = new BinaryWriter(mosFilesms);

            for (int i = 0; i < FileCount; i++)
            {
                file_path = files[i].Replace(Folder, "").TrimStart('\\');
                FileTableEntry.FilePathLength = file_path.Length;

                if (is64)
                    mosTable.Write((long)FileTableEntry.FilePathLength);
                else
                    mosTable.Write(FileTableEntry.FilePathLength);

                FileTableEntry.FilePath = file_path;
                mosTable.Write(Encoding.Unicode.GetBytes(FileTableEntry.FilePath));
                FileTableEntry.Unknown_001 = 2;
                mosTable.Write(FileTableEntry.Unknown_001);
                FileTableEntry.IsCompressed = true;
                mosTable.Write(FileTableEntry.IsCompressed);
                FileTableEntry.IsEncrypted = true;
                mosTable.Write(FileTableEntry.IsEncrypted);
                FileTableEntry.Unknown_002 = 0;
                mosTable.Write(FileTableEntry.Unknown_002);

                FileStream fis = new FileStream(files[i], FileMode.Open);
                MemoryStream tmp = new MemoryStream();


                //stopWatch.Start();
                //Console.Write("\rCompressing Files: {0}/{1}", (i + 1), FileCount);
                processedEvent((i + 1), FileCount);

                if (file_path.EndsWith(".xml") || file_path.EndsWith(".x16"))
                {
                    // encode bxml
                    BXML bxml = new BXML(XOR_KEY);
                    Convert(fis, bxml.DetectType(fis), tmp, BXML_TYPE.BXML_BINARY);
                }
                else
                {
                    // compress raw
                    fis.CopyTo(tmp);
                }
                fis.Close();
                fis = null;

                FileTableEntry.FileDataOffset = (int)mosFiles.BaseStream.Position;
                FileTableEntry.FileDataSizeUnpacked = (int)tmp.Length;

                if (is64)
                    mosTable.Write((long)FileTableEntry.FileDataSizeUnpacked);
                else
                    mosTable.Write(FileTableEntry.FileDataSizeUnpacked);

                buffer_unpacked = tmp.ToArray();
                tmp.Close();
                tmp = null;
                buffer_packed = Pack(buffer_unpacked, FileTableEntry.FileDataSizeUnpacked, out FileTableEntry.FileDataSizeSheared, out FileTableEntry.FileDataSizeStored, FileTableEntry.IsEncrypted, FileTableEntry.IsCompressed, compression);
                buffer_unpacked = null;
                mosFiles.Write(buffer_packed);
                buffer_packed = null;

                if (is64)
                    mosTable.Write((long)FileTableEntry.FileDataSizeSheared);
                else
                    mosTable.Write(FileTableEntry.FileDataSizeSheared);

                if (is64)
                    mosTable.Write((long)FileTableEntry.FileDataSizeStored);
                else
                    mosTable.Write(FileTableEntry.FileDataSizeStored);

                if (is64)
                    mosTable.Write((long)FileTableEntry.FileDataOffset);
                else
                    mosTable.Write(FileTableEntry.FileDataOffset);

                FileTableEntry.Padding = new byte[60];
                mosTable.Write(FileTableEntry.Padding);
            }

            //Console.Write("\nWriting File Entries...\n");
            //processedEvent(0,0,"\nWriting File Entries...\n");

            MemoryStream output = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(output);
            byte[] Signature = new byte[8] { (byte)'U', (byte)'O', (byte)'S', (byte)'E', (byte)'D', (byte)'A', (byte)'L', (byte)'B' };
            bw.Write(Signature);
            int Version = 2;
            bw.Write(Version);
            byte[] Unknown_001 = new byte[5] { 0, 0, 0, 0, 0 };
            bw.Write(Unknown_001);
            int FileDataSizePacked = (int)mosFiles.BaseStream.Length;

            if (is64)
            {
                bw.Write((long)FileDataSizePacked);
                bw.Write((long)FileCount);
            }
            else
            {
                bw.Write(FileDataSizePacked);
                bw.Write(FileCount);
            }

            bool IsCompressed = true;
            bw.Write(IsCompressed);
            bool IsEncrypted = true;
            bw.Write(IsEncrypted);
            byte[] Unknown_002 = new byte[62];
            bw.Write(Unknown_002);

            int FileTableSizeUnpacked = (int)mosTable.BaseStream.Length;
            int FileTableSizeSheared = FileTableSizeUnpacked;
            int FileTableSizePacked = FileTableSizeUnpacked;
            buffer_unpacked = mosTablems.ToArray();
            mosTable.Close();
            mosTablems.Close();
            mosTable = null;
            mosTablems = null;
            buffer_packed = Pack(buffer_unpacked, FileTableSizeUnpacked, out FileTableSizeSheared, out FileTableSizePacked, IsEncrypted, IsCompressed, compression);
            buffer_unpacked = null;

            if (is64)
                bw.Write((long)FileTableSizePacked);
            else
                bw.Write(FileTableSizePacked);

            if (is64)
                bw.Write((long)FileTableSizeUnpacked);
            else
                bw.Write(FileTableSizeUnpacked);

            bw.Write(buffer_packed);
            buffer_packed = null;

            int OffsetGlobal = (int)output.Position + (is64 ? 8 : 4);

            if (is64)
                bw.Write((long)OffsetGlobal);
            else
                bw.Write(OffsetGlobal);

            buffer_packed = mosFilesms.ToArray();
            mosFiles.Close();
            mosFilesms.Close();
            mosFiles = null;
            mosFilesms = null;
            bw.Write(buffer_packed);
            buffer_packed = null;
            File.WriteAllBytes(Folder.Replace(".files", ""), output.ToArray());
            bw.Close();
            output.Close();
            bw = null;
            output = null;

            // stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            //TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            //string elapsedTime = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds,ts.Milliseconds / 10);
            //Console.WriteLine("RunTime " + elapsedTime);
            //Console.WriteLine("\r\nDone!"/* in "+ elapsedTime*/);
        }

        private void Convert(Stream iStream, BXML_TYPE iType, Stream oStream, BXML_TYPE oType)
        {
            if ((iType == BXML_TYPE.BXML_PLAIN && oType == BXML_TYPE.BXML_BINARY) || (iType == BXML_TYPE.BXML_BINARY && oType == BXML_TYPE.BXML_PLAIN))
            {
                BXML bns_xml = new BXML(XOR_KEY);
                bns_xml.Load(iStream, iType);
                bns_xml.Save(oStream, oType);
            }
            else
            {
                iStream.CopyTo(oStream);
            }
        }
    }

    class BXML_CONTENT
    {
        public byte[] XOR_KEY;

        void Xor(byte[] buffer, int size)
        {
            for (int i = 0; i < size; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ XOR_KEY[i % XOR_KEY.Length]);
            }
        }

        bool Keep_XML_WhiteSpace = true;

        public void Read(Stream iStream, BXML_TYPE iType)
        {
            if (iType == BXML_TYPE.BXML_PLAIN)
            {
                Signature = new byte[8] { (byte)'L', (byte)'M', (byte)'X', (byte)'B', (byte)'O', (byte)'S', (byte)'L', (byte)'B' };
                Version = 3;
                FileSize = 85;
                Padding = new byte[64];
                Unknown = true;
                OriginalPathLength = 0;

                // NOTE: keep whitespace text nodes (to be compliant with the whitespace TEXT_NODES in bns xml)
                // no we don't keep them, we remove them because it is cleaner
                Nodes.PreserveWhitespace = Keep_XML_WhiteSpace;
                Nodes.Load(iStream);

                // get original path from first comment node
                //fixed 18-08 
                XmlNode node = null;
                try
                {
                    node = Nodes.DocumentElement.ChildNodes.OfType<XmlComment>().First();
                }
                catch
                {
                }
                finally
                {
                    if (node != null && node.NodeType == XmlNodeType.Comment)
                    {
                        string Text = node.InnerText;
                        OriginalPathLength = Text.Length;
                        OriginalPath = Encoding.Unicode.GetBytes(Text);
                        Xor(OriginalPath, 2 * OriginalPathLength);
                        if (Nodes.PreserveWhitespace && node.NextSibling.NodeType == XmlNodeType.Whitespace)
                            Nodes.DocumentElement.RemoveChild(node.NextSibling);
                    }
                    else
                    {
                        OriginalPath = new byte[2 * OriginalPathLength];
                    }
                }
            }

            if (iType == BXML_TYPE.BXML_BINARY)
            {
                Signature = new byte[8];
                BinaryReader br = new BinaryReader(iStream);
                br.BaseStream.Position = 0;
                Signature = br.ReadBytes(8);
                Version = br.ReadInt32();
                FileSize = br.ReadInt32();
                Padding = br.ReadBytes(64);
                Unknown = br.ReadByte() == 1;
                OriginalPathLength = br.ReadInt32();
                OriginalPath = br.ReadBytes(2 * OriginalPathLength);
                AutoID = 1;
                ReadNode(iStream);

                // add original path as first comment node
                byte[] Path = OriginalPath;
                Xor(Path, 2 * OriginalPathLength);
                XmlComment node = Nodes.CreateComment(Encoding.Unicode.GetString(Path));
                Nodes.DocumentElement.PrependChild(node);
                XmlNode docNode = Nodes.CreateXmlDeclaration("1.0", "utf-8", null);
                Nodes.PrependChild(docNode);
                if (FileSize != iStream.Position)
                {
                    throw new Exception(String.Format("Filesize Mismatch, expected size was {0} while actual size was {1}.", FileSize, iStream.Position));
                }
            }
        }

        public void Write(Stream oStream, BXML_TYPE oType)
        {
            if (oType == BXML_TYPE.BXML_PLAIN)
            {
                Nodes.Save(oStream);
            }
            if (oType == BXML_TYPE.BXML_BINARY)
            {
                BinaryWriter bw = new BinaryWriter(oStream);
                bw.Write(Signature);
                bw.Write(Version);
                bw.Write(FileSize);
                bw.Write(Padding);
                bw.Write(Unknown);
                bw.Write(OriginalPathLength);
                bw.Write(OriginalPath);

                AutoID = 1;
                WriteNode(oStream);

                FileSize = (int)oStream.Position;
                oStream.Position = 12;
                bw.Write(FileSize);
            }

        }

        private void ReadNode(Stream iStream, XmlNode parent = null)
        {

            XmlNode node = null;
            BinaryReader br = new BinaryReader(iStream);
            int Type = 1;
            if (parent != null)
            {
                Type = br.ReadInt32();
            }


            if (Type == 1)
            {
                node = Nodes.CreateElement("Text");

                int ParameterCount = br.ReadInt32();
                for (int i = 0; i < ParameterCount; i++)
                {
                    int NameLength = br.ReadInt32();
                    byte[] Name = br.ReadBytes(2 * NameLength);
                    Xor(Name, 2 * NameLength);

                    int ValueLength = br.ReadInt32();
                    byte[] Value = br.ReadBytes(2 * ValueLength);
                    Xor(Value, 2 * ValueLength);

                    ((XmlElement)node).SetAttribute(Encoding.Unicode.GetString(Name), Encoding.Unicode.GetString(Value));
                }
            }

            if (Type == 2)
            {
                node = Nodes.CreateTextNode("");

                int TextLength = br.ReadInt32();
                byte[] Text = br.ReadBytes(TextLength * 2);
                Xor(Text, 2 * TextLength);

                ((XmlText)node).Value = Encoding.Unicode.GetString(Text);
            }

            if (Type > 2)
            {
                throw new Exception("Unknown XML Node Type");
            }

            bool Closed = br.ReadByte() == 1;
            int TagLength = br.ReadInt32();
            byte[] Tag = br.ReadBytes(2 * TagLength);
            Xor(Tag, 2 * TagLength);
            if (Type == 1)
            {
                node = RenameNode(node, Encoding.Unicode.GetString(Tag));
            }

            int ChildCount = br.ReadInt32();
            AutoID = br.ReadInt32();
            AutoID++;

            for (int i = 0; i < ChildCount; i++)
            {
                ReadNode(iStream, node);
            }

            if (parent != null)
            {
                if (Keep_XML_WhiteSpace || Type != 2 || !String.IsNullOrWhiteSpace(node.Value))
                {
                    parent.AppendChild(node);
                }
            }
            else
            {
                Nodes.AppendChild(node);
            }
        }

        public XmlNode RenameNode(XmlNode node, string Name)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                XmlElement oldElement = (XmlElement)node;
                XmlElement newElement =
                node.OwnerDocument.CreateElement(Name);

                while (oldElement.HasAttributes)
                {
                    newElement.SetAttributeNode(oldElement.RemoveAttributeNode(oldElement.Attributes[0]));
                }

                while (oldElement.HasChildNodes)
                {
                    newElement.AppendChild(oldElement.FirstChild);
                }

                if (oldElement.ParentNode != null)
                {
                    oldElement.ParentNode.ReplaceChild(newElement, oldElement);
                }

                return newElement;
            }
            else
                return node;
        }

        private bool WriteNode(Stream oStream, XmlNode parent = null)
        {
            BinaryWriter bw = new BinaryWriter(oStream);
            XmlNode node = null;

            int Type = 1;
            if (parent != null)
            {
                node = parent;
                if (node.NodeType == XmlNodeType.Element)
                {
                    Type = 1;
                }
                if (node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.Whitespace)
                {
                    Type = 2;
                }
                if (node.NodeType == XmlNodeType.Comment)
                {
                    return false;
                }
                bw.Write(Type);
            }
            else
            {
                node = Nodes.DocumentElement;
            }

            if (Type == 1)
            {
                int OffsetAttributeCount = (int)oStream.Position;
                int AttributeCount = 0;
                bw.Write(AttributeCount);

                foreach (XmlAttribute attribute in node.Attributes)
                {
                    string Name = attribute.Name;
                    int NameLength = Name.Length;
                    bw.Write(NameLength);
                    byte[] NameBuffer = Encoding.Unicode.GetBytes(Name);
                    Xor(NameBuffer, 2 * NameLength);
                    bw.Write(NameBuffer);

                    String Value = attribute.Value;
                    int ValueLength = Value.Length;
                    bw.Write(ValueLength);
                    byte[] ValueBuffer = Encoding.Unicode.GetBytes(Value);
                    Xor(ValueBuffer, 2 * ValueLength);
                    bw.Write(ValueBuffer);
                    AttributeCount++;
                }

                int OffsetCurrent = (int)oStream.Position;
                oStream.Position = OffsetAttributeCount;
                bw.Write(AttributeCount);
                oStream.Position = OffsetCurrent;
            }

            if (Type == 2)
            {
                string Text = node.Value;
                int TextLength = Text.Length;
                bw.Write(TextLength);
                byte[] TextBuffer = Encoding.Unicode.GetBytes(Text);
                Xor(TextBuffer, 2 * TextLength);
                bw.Write(TextBuffer);

            }

            if (Type > 2)
            {
                throw new Exception(String.Format("ERROR: XML NODE TYPE [{0}] UNKNOWN", node.NodeType.ToString()));
            }

            bool Closed = true;
            bw.Write(Closed);
            string Tag = node.Name;
            int TagLength = Tag.Length;
            bw.Write(TagLength);
            byte[] TagBuffer = Encoding.Unicode.GetBytes(Tag);
            Xor(TagBuffer, 2 * TagLength);
            bw.Write(TagBuffer);

            int OffsetChildCount = (int)oStream.Position;
            int ChildCount = 0;
            bw.Write(ChildCount);
            bw.Write(AutoID);
            AutoID++;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (WriteNode(oStream, child))
                {
                    ChildCount++;
                }
            }

            int OffsetCurrent2 = (int)oStream.Position;
            oStream.Position = OffsetChildCount;
            bw.Write(ChildCount);
            oStream.Position = OffsetCurrent2;
            return true;
        }

        byte[] Signature;                  // 8 byte
        int Version;                   // 4 byte
        int FileSize;                  // 4 byte
        byte[] Padding;                    // 64 byte
        bool Unknown;                       // 1 byte
                                            // TODO: add to CDATA ?
        int OriginalPathLength;        // 4 byte
        byte[] OriginalPath;               // 2*OriginalPathLength bytes

        int AutoID;
        XmlDocument Nodes = new XmlDocument();
    }

    //class BDAT_HEAD
    //{
    //    //public: BDAT_HEAD();
    //    //public: ~BDAT_HEAD();

    //    //public: void Read(wxInputStream* iStream, BDAT_TYPE iType);
    //    //public: void Write(wxOutputStream* oStream, BDAT_TYPE oType);

    //    /*
    //     * read/write Data only when the iStream file is not a complement file,
    //     * otherwise the data is only available in the corresponding 'parent' datafile.bin
    //     * the way to detect if this file is a complement is unknown, so we can use some hacks:
    //     *   Size_1 exceeds the filesize -> complement file
    //     *   ListCount < 200 -> complement file
    //     */
    //    bool Complement;
    //    int Size_1;                    // 4 byte
    //    int Size_2;                    // 4 byte
    //    int Size_3;                    // 4 byte
    //    byte[] Padding;                    // 62 byte
    //    byte[] Data;                       // Size_1 || Size_2 bytes ?
    //                                        /* Data
    //                                        {
    //                                            wxUint32 Size_A;
    //                                            wxUint32 Size_B;
    //                                            wxUint32 Size_C;
    //                                            ...
    //                                        }*/
    //};

    //class BDAT_LIST
    //{
    //    //public: BDAT_LIST();
    //    //public: ~BDAT_LIST();

    //    //public: void Read(wxInputStream* iStream, BDAT_TYPE iType);
    //    //public: void Write(wxOutputStream* oStream, BDAT_TYPE oType);

    //    byte[] Unknown1;                    // 1 byte
    //    short ID;                        // 2 byte
    //    ushort Unknown2;                  // 2 byte
    //    ushort Unknown3;                  // 2 byte
    //    int Size;                      // 4 byte
    //    BDAT_COLLECTION Collection;         // Size bytes
    //};
    //class BDAT_COLLECTION
    //{
    //    //public: BDAT_COLLECTION();
    //    //public: ~BDAT_COLLECTION();

    //    //public: void Read(wxInputStream* iStream, BDAT_TYPE iType);
    //    //public: void Write(wxOutputStream* oStream, BDAT_TYPE oType);

    //    bool Compressed;                    // 1 byte
    //    uint Deprecated;                 // 1 byte
    //    BDAT_ARCHIVE Archive;              // *
    //    BDAT_LOOSE Loose;                  // *
    //};

    //class BDAT_ARCHIVE
    //{
    //    //public: BDAT_ARCHIVE();
    //    //public: ~BDAT_ARCHIVE();

    //    //public: void Read(wxInputStream* iStream, BDAT_TYPE iType);
    //    //public: void Write(wxOutputStream* oStream, BDAT_TYPE oType);

    //    int SubArchiveCount;           // 4 byte
    //    ushort Unknown;                   // 2 byte
    //    BDAT_SUBARCHIVE SubArchives;       // *
    //};

    //class BDAT_LOOSE
    //{
    //    //public: BDAT_LOOSE();
    //    //public: ~BDAT_LOOSE();

    //    //public: void Read(wxInputStream* iStream, BDAT_TYPE iType);
    //    //public: void Write(wxOutputStream* oStream, BDAT_TYPE oType);

    //    int FieldCountUnfixed;         // 4 byte
    //    int FieldCount;
    //    int SizeFields;                // 4 byte
    //    int SizeLookup;                // 4 byte
    //    byte[] Unknown;                     // 1 byte
    //    BDAT_FIELDTABLE Fields;            // SizeFields bytes
    //    int SizePadding;               // 0 byte (private use only)
    //    byte[] Padding;                    // (private use only)
    //    BDAT_LOOKUPTABLE Lookup;            // SizeLookup bytes
    //};

    //class BDAT_LOOKUPTABLE
    //{
    //    //public: BDAT_LOOKUPTABLE();
    //    //public: ~BDAT_LOOKUPTABLE();

    //    //public: void Read(wxInputStream* iStream, BDAT_TYPE iType);
    //    //public: void Write(wxOutputStream* oStream, BDAT_TYPE oType);

    //    int Size; // only for private usage...
    //    byte[] Data;
    //};

    //class BDAT_SUBARCHIVE
    //{
    //    //public: BDAT_SUBARCHIVE();
    //    //public: ~BDAT_SUBARCHIVE();

    //    //public: void Read(wxInputStream* iStream, BDAT_TYPE iType);
    //    //public: void Write(wxOutputStream* oStream, BDAT_TYPE oType);

    //    byte[] Unknown;                    // 16 byte (shorts?)
    //    ushort SizeCompressed;            // 2 byte
    //                                      //                                  // SizeCompressed bytes
    //    ushort SizeDecompressed;          // 2 byte
    //    int FieldLookupCount;          // 4 byte
    //    BDAT_FIELDTABLE Fields;            // *
    //    BDAT_LOOKUPTABLE Lookups;          // *
    //};
    //class BDAT_FIELDTABLE
    //{
    //    //public: BDAT_FIELDTABLE();
    //    //public: ~BDAT_FIELDTABLE();

    //    //public: void Read(wxInputStream* iStream, BDAT_TYPE iType);
    //    //public: void Write(wxOutputStream* oStream, BDAT_TYPE oType);

    //    ushort Unknown1;                  // 2 byte
    //    ushort Unknown2;                  // 2 byte
    //    int Size;                      // 4 byte
    //    byte[] Data;                       // Size-8 bytes
    //};
    //class BDAT_CONTENT
    //{
    //    void Read(Stream iStream, BDAT_TYPE iType)
    //    {
    //        Signature = new byte[8];

    //        Stream->Read(Signature, 8);
    //        Stream->Read(&Version, 4);
    //        Unknown = new Byte[9];
    //        Stream->Read(Unknown, 9);
    //        Stream->Read(&ListCount, 4);
    //        HeadList.Complement = false;
    //        if (ListCount < 20)
    //        {
    //            HeadList.Complement = true;
    //        }
    //        HeadList.Read(iStream, iType);
    //        Lists = new BDAT_LIST[ListCount];

    //        for (int l = 0; l < ListCount; l++)
    //        {
    //            Lists[l].Read(iStream, iType);
    //        }
    //    }
    //    byte[] Signature;                  // 8 byte

    //    int Version;                   // 4 byte
    //    byte[] Unknown;                    // 9 byte
    //    int ListCount;                 // 4 byte
    //    BDAT_HEAD HeadList;                 // *
    //    BDAT_LIST Lists;                   // *
    //    //uint32=int
    //}
    //class BDAT
    //{
    //    private BDAT_CONTENT _content = new BDAT_CONTENT;

    //    private void Convert(Stream iStream, BDAT_TYPE iType, Stream oStream, BDAT_TYPE oType)
    //    {
    //        BDAT bns_dat;
    //        bns_dat.Load(iStream, iType);
    //        bns_dat.Save(oStream, oType);
    //    }

    //    private void Load(Stream iStream, BDAT_TYPE iType)
    //    {
    //        if (iType == BDAT_PLAIN)
    //        {
    //            //wxPrintf(wxT("BDAT_PLAIN import not yet supported!\n"));
    //            // FIXME: currently load as binary
    //            _content.Read(iStream, BDAT_BINARY);
    //            DetectIndices();
    //        }

    //        if (iType == BDAT_BINARY)
    //        {
    //            _content.Read(iStream, BDAT_BINARY);
    //            DetectIndices();
    //        }
    //    }

    //    private  void Save(Stream oStream, BDAT_TYPE oType)
    //    {
    //        if (oType == BDAT_PLAIN)
    //        {
    //            //wxPrintf(wxT("BDAT_PLAIN export not yet supported!\n"));
    //            //FIXME: currently save as binary
    //            _content.Write(oStream, BDAT_BINARY);
    //        }

    //        if (oType == BDAT_BINARY)
    //        {
    //            _content.Write(oStream, BDAT_BINARY);
    //        }
    //    }

    //}
    class BXML
    {
        private BXML_CONTENT _content = new BXML_CONTENT();

        private byte[] XOR_KEY { get { return _content.XOR_KEY; } set { _content.XOR_KEY = value; } }

        public BXML(byte[] xor)
        {
            XOR_KEY = xor;
        }

        public void Load(Stream iStream, BXML_TYPE iType)
        {
            _content.Read(iStream, iType);
        }

        public void Save(Stream oStream, BXML_TYPE oType)
        {
            _content.Write(oStream, oType);
        }

        public BXML_TYPE DetectType(Stream iStream)
        {
            int offset = (int)iStream.Position;
            iStream.Position = 0;
            byte[] Signature = new byte[13];
            iStream.Read(Signature, 0, 13);
            iStream.Position = offset;

            BXML_TYPE result = BXML_TYPE.BXML_UNKNOWN;

            if (
                BitConverter.ToString(Signature).Replace("-", "").Replace("00", "").Contains(BitConverter.ToString(new byte[] { (byte)'<', (byte)'?', (byte)'x', (byte)'m', (byte)'l' }).Replace("-", ""))
            )
            {
                result = BXML_TYPE.BXML_PLAIN;
            }

            if (
                Signature[7] == 'B' &&
                Signature[6] == 'L' &&
                Signature[5] == 'S' &&
                Signature[4] == 'O' &&
                Signature[3] == 'B' &&
                Signature[2] == 'X' &&
                Signature[1] == 'M' &&
                Signature[0] == 'L'
            )
            {
                result = BXML_TYPE.BXML_BINARY;
            }

            return result;
        }
    }

}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Web;
using static BnsDatTool.lib.TranslateFileDal;
using System.Text.RegularExpressions;

namespace BnsDatTool
{
    public class BDat
    {
        public BDAT_CONTENT _content = new BDAT_CONTENT();
        private BNSDat m_bnsDat = new BNSDat();

        private int _indexFaqs;
        private int _indexCommons;
        private int _indexCommands;

        public BDat()
        {
            _indexFaqs = -1;
            _indexCommons = -1;
            _indexCommands = -1;
        }

        public void Dump(string FileName, string saveFolder, BXML_TYPE format, bool is64)
        {
            FileStream fs = new FileStream(FileName, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            Load(br, format, is64);

            Directory.CreateDirectory(saveFolder);

            DumpFAQ(saveFolder + @"\lookup_faq.txt");
            DumpGeneral(saveFolder + @"\lookup_general.txt");
            DumpCommand(saveFolder + @"\lookup_command.txt");
            DumpXML(saveFolder);

            fs.Close();
            br.Close();
        }


        public void ExportTranslate(string FileName, string saveFolder, BXML_TYPE format, bool is64)
        {
            Console.Write("\rExporting Translation XML...");
            FileStream fs = new FileStream(FileName, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            Load(br, format, is64);

            using (StreamWriter outfile = new StreamWriter(saveFolder))
            {
                outfile.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                int empty = 0;
                string alias = string.Empty;
                string text = string.Empty;
                for (int l = 0; l < _content.ListCount; l++)
                {
                    if (l == _indexCommons)//enable for xml.dat translation
                    {
                        Console.Write("\rDumping Translation XML: {0}/{1}", (l + 1), _content.ListCount);
                        BDAT_LIST blist = _content.Lists[l];
                        BDAT_COLLECTION bcollection = blist.Collection;
                    
                        if (bcollection.Compressed > 0)
                        {
                            outfile.WriteLine("<table>");
                            int autoId = 1;
                            BDAT_ARCHIVE barchive = bcollection.Archive;
                            for (int a = 0; a < barchive.SubArchiveCount; a++)
                            {
                                BDAT_SUBARCHIVE bsubarchive = barchive.SubArchives[a];
                                for (int f = 0; f < bsubarchive.FieldLookupCount; f++)
                                {
                                    BDAT_FIELDTABLE bfield = bsubarchive.Fields[f];
                                    BDAT_LOOKUPTABLE blookup = bsubarchive.Lookups[f];
                                    string[] words = LookupSplitToWords(blookup.Data, blookup.Size);
                                    empty = 0;
                                    alias = string.Empty;
                                    text = string.Empty;
                                    for (int w = 0; w < words.Length; w++)
                                    {
                                        if (words[w] != null && words[w].Length > 0)
                                        {
                                            switch (w)
                                            {
                                                case 0:
                                                    alias = string.Format(
                                                        "\t<text autoId=\"{0}\" alias=\"{1}\" priority=\"0\">", autoId,
                                                        words[w]);
                                                    autoId++;
                                                    break;
                                                default:
                                                    text = "<![CDATA[" + words[w] +
                                                           "]]>"; //System.Web.HttpUtility.HtmlEncode(words[w]);
                                                    break;
                                            }

                                        }
                                        else
                                        {
                                            empty++;
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(alias) && !string.IsNullOrEmpty(text))
                                    {
                                        outfile.WriteLine(alias);
                                        outfile.WriteLine("\t\t<original>" + text + "</original>");
                                        outfile.WriteLine("\t\t<replacement>" + text + "</replacement>");
                                        outfile.WriteLine("\t</text>");
                                    }
                                }

                            }

                            outfile.WriteLine("</table>");
                        }
                    }
                }
                Console.Write("\rDone!!");
            }

            fs.Close();
            br.Close();
        }
        public void Translate(string dir, string lang,bool isDatafile, bool is64)
        {
            string bin;
            string newdat;
            Console.Write("\rTranslating: Content...");
            if (isDatafile)
            {
                bin = is64 ? @"xml64.dat.files\datafile64.bin" : @"xml.dat.files\datafile.bin";
                newdat = is64 ? "datafile64_new.bin" : "datafile_new.bin";
            }
            else
            {
                bin = is64 ? @"local64.dat.files\localfile64.bin" : @"local.dat.files\localfile.bin";
                newdat = is64 ? "localfile64_new.bin" : "localfile_new.bin";
            }

            FileStream fslocalfile = new FileStream(dir + bin, FileMode.Open);
            BinaryReader brlocalfile = new BinaryReader(fslocalfile);
            Load(brlocalfile, BXML_TYPE.BXML_BINARY, is64);

            TranslateReader translateControl = new TranslateReader();
            translateControl.Load(lang);

            TranslateGeneral(translateControl);
            brlocalfile.Close();

            BinaryWriter bw = new BinaryWriter(File.Open(dir + newdat, FileMode.Create));

            Save(bw, BXML_TYPE.BXML_BINARY, is64);
            bw.Close();
            Console.WriteLine("\rDone!!");
        }

        public void TranslateGeneral(TranslateReader translator)
        {
            if (_indexCommons > -1)
            {
                int index = 0;
                int index2 = index;

                BDAT_LIST blist = _content.Lists[_indexCommons];

                if (blist.Collection.Compressed > 0)
                {
                    BDAT_ARCHIVE barchive = blist.Collection.Archive;
                    BDAT_SUBARCHIVE bsubarchive;
                    List<BDAT_SUBARCHIVE> subNews = new List<BDAT_SUBARCHIVE>();

                    for (int s = 0; s < barchive.SubArchiveCount; s++)
                    {
                        bsubarchive = barchive.SubArchives[s];
                        // Console.Write("\rTranslateGeneral SubArchive: {0}/{1}", (s + 1), barchive.SubArchiveCount);
                        for (int f = 0; f < bsubarchive.FieldLookupCount; f++)
                        {
                            //Console.Write("\rTranslateGeneral FieldLookup: {0}/{1}", (f + 1), bsubarchive.FieldLookupCount);
                            BDAT_FIELDTABLE field = bsubarchive.Fields[f];
                            BDAT_LOOKUPTABLE blookup = bsubarchive.Lookups[f];

                            string[] words = LookupSplitToWords(blookup.Data, blookup.Size);
                            string translated = translator.Translate(words[1], words[0]);

                            // alias
                            //Buffer.BlockCopy(BitConverter.GetBytes(words[0].Length), 0, field.Data, 12, 4);

                            //translate
                            if (translated != null)
                            {
                                words[1] = translated;
                                blookup.Data = LookupWorldsToBytes(words);
                                blookup.Size = blookup.Data.Length;
                                // set new test size   
                                //Buffer.BlockCopy(BitConverter.GetBytes(words[1].Length), 0, field.Data, 8, 4);
                            }
                        }

                        if (bsubarchive.NeedSplit(ref index))
                        {
                            BDAT_SUBARCHIVE bClone1 = new BDAT_SUBARCHIVE()
                            {
                                StartAndEndFieldId = new byte[16],
                                Fields = new BDAT_FIELDTABLE[index],
                                Lookups = new BDAT_LOOKUPTABLE[index],
                                FieldLookupCount = index
                            };

                            Array.Copy(bsubarchive.Fields, 0, bClone1.Fields, 0, index);
                            Array.Copy(bsubarchive.Lookups, 0, bClone1.Lookups, 0, index);

                            // set new start field id
                            Buffer.BlockCopy(BitConverter.GetBytes(bClone1.Fields[0].ID), 0, bClone1.StartAndEndFieldId, 0, 4);
                            // set new end field id
                            Buffer.BlockCopy(BitConverter.GetBytes(bClone1.Fields[index - 1].ID), 0, bClone1.StartAndEndFieldId, 8, 4);
                            subNews.Add(bClone1);

                            //part 2
                            index2 = bsubarchive.FieldLookupCount - index;
                            BDAT_SUBARCHIVE bClone2 = new BDAT_SUBARCHIVE()
                            {
                                StartAndEndFieldId = new byte[16],
                                Fields = new BDAT_FIELDTABLE[index2],
                                Lookups = new BDAT_LOOKUPTABLE[index2],
                                FieldLookupCount = index2
                            };
                            Array.Copy(bsubarchive.Fields, index, bClone2.Fields, 0, index2);
                            Array.Copy(bsubarchive.Lookups, index, bClone2.Lookups, 0, index2);

                            // set new start field id
                            Buffer.BlockCopy(BitConverter.GetBytes(bClone2.Fields[0].ID), 0, bClone2.StartAndEndFieldId, 0, 4);
                            // set new end field id
                            Buffer.BlockCopy(BitConverter.GetBytes(bClone2.Fields[index2 - 1].ID), 0, bClone2.StartAndEndFieldId, 8, 4);
                            subNews.Add(bClone2);

                            Console.WriteLine("A:{0}<>B:{1}.OK!", m_bnsDat.BytesToHex(bClone1.StartAndEndFieldId), m_bnsDat.BytesToHex(bClone2.StartAndEndFieldId));

                        }
                        else
                        {
                            subNews.Add(bsubarchive);
                        }
                    }
                    barchive.SubArchiveCount = subNews.Count;
                    barchive.SubArchives = subNews.ToArray();
                    // Console.WriteLine("IF A==B that mean have something wrong! Check source code and fix it.");
                    //Console.WriteLine("\rDone!!");
                }
                else
                {
                    BDAT_LOOSE bloose = blist.Collection.Loose;
                    BDAT_LOOKUPTABLE blookup = bloose.Lookup;
                    string[] words = LookupSplitToWords(bloose.Lookup.Data, bloose.SizeLookup);

                    for (int w = 0; w < words.Length; w += 2)
                    {
                        string translated = translator.Translate(words[w + 1], words[w]);
                        if (translated != null)
                        {
                            words[w + 1] = translated;
                        }
                        //Console.WriteLine("words[w + 1]: " + words[w + 1]);
                    }

                    blookup.Data = LookupWorldsToBytes(words);
                    blookup.Size = blookup.Data.Length;
                }
            }
        }

        public void Split<T>(T[] source, int index, out T[] first, out T[] last)
        {
            int len2 = source.Length - index;
            first = new T[index];
            last = new T[len2];
            Array.Copy(source, 0, first, 0, index);
            Array.Copy(source, index, last, 0, len2);
        }

        public void Load(BinaryReader br, BXML_TYPE iType, bool is64 = false)
        {
            if (iType == BXML_TYPE.BXML_PLAIN || iType == BXML_TYPE.BXML_BINARY)
            {
                if (is64)
                {
                    _content.Read64(br, BXML_TYPE.BXML_BINARY);
                }
                else
                {
                    _content.Read(br, BXML_TYPE.BXML_BINARY);
                }
                DetectIndices();
            }
        }

        public void Save(BinaryWriter bw, BXML_TYPE iType, bool is64)
        {
            if (is64)
            {
                _content.Write64(bw, iType);
            }
            else
            {
                _content.Write(bw, iType);
            }
            bw.Flush();
        }

        public void DumpFAQ(string file)
        {
            Console.Write("\rDumping FAQ...");
            if (_indexFaqs > -1)
            {
                BDAT_LIST blist = _content.Lists[_indexFaqs];

                BDAT_LOOSE bloose = blist.Collection.Loose;

                string[] words = LookupSplitToWords(bloose.Lookup.Data, bloose.SizeLookup);

                using (StreamWriter outfile = new StreamWriter(file))
                {
                    foreach (string text in words)
                    {

                        outfile.WriteLine("<alias>");
                        outfile.WriteLine("</alias>");
                        outfile.WriteLine("<text>");
                        outfile.WriteLine(text);
                        outfile.WriteLine("</text>");
                    }
                }
            }
        }

        public void DumpGeneral(string file)
        {
            Console.Write("\rDumping GENERAL...");
            if (_indexCommons > -1)
            {
                using (StreamWriter outfile = new StreamWriter(file))
                {
                    BDAT_LIST blist = _content.Lists[_indexCommons];

                    if (blist.Collection.Compressed > 0)
                    {
                        BDAT_ARCHIVE barchive = blist.Collection.Archive;
                        for (int s = 0; s < barchive.SubArchiveCount; s++)
                        {
                            BDAT_SUBARCHIVE bsubarchive = barchive.SubArchives[s];
                            for (int f = 0; f < bsubarchive.FieldLookupCount; f++)
                            {
                                BDAT_LOOKUPTABLE blookup = bsubarchive.Lookups[f];
                                string[] words = LookupSplitToWords(blookup.Data, blookup.Size);
                                for (int w = 0; w < words.Length; w++)
                                {
                                    if (w % 2 == 0)
                                    {
                                        outfile.WriteLine("<alias>");
                                        outfile.WriteLine(words[w]);
                                        outfile.WriteLine("</alias>");
                                    }
                                    else
                                    {
                                        outfile.WriteLine("<text>");
                                        outfile.WriteLine(words[w]);
                                        outfile.WriteLine("</text>");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        BDAT_LOOSE bloose = blist.Collection.Loose;
                        string[] words = LookupSplitToWords(bloose.Lookup.Data, bloose.SizeLookup);
                        for (int w = 0; w < words.Length; w++)
                        {
                            if (w % 2 == 0)
                            {
                                outfile.WriteLine("<alias>");
                                outfile.WriteLine(words[w]);
                                outfile.WriteLine("</alias>");
                            }
                            else
                            {
                                outfile.WriteLine("<text>");
                                outfile.WriteLine(words[w]);
                                outfile.WriteLine("</text>");
                            }
                        }
                    }
                }
            }
        }

        public void DumpCommand(string file)
        {
            Console.Write("\rDumping COMMAND...");
            if (_indexCommands > -1)
            {
                using (StreamWriter outfile = new StreamWriter(file))
                {
                    BDAT_LIST blist = _content.Lists[_indexCommands];

                    BDAT_LOOSE bloose = blist.Collection.Loose;
                    string[] words = LookupSplitToWords(bloose.Lookup.Data, bloose.SizeLookup);
                    for (int w = 0; w < words.Length; w++)
                    {
                        outfile.WriteLine("<alias>");
                        outfile.WriteLine("</alias>");
                        outfile.WriteLine("<text>");
                        outfile.WriteLine(words[w]);
                        outfile.WriteLine("</text>");
                    }
                }
            }
            //Console.WriteLine("_indexCommands: " + _indexCommands);
        }

        public void DumpXML(string dir)
        {

            for (int l = 0; l < _content.ListCount; l++)
            {
                //processedEvent((l + 1), _content.ListCount);
                Console.Write($@"Dumping XML: {(l + 1)}/{_content.ListCount}");

                BDAT_LIST blist = _content.Lists[l];

                using (StreamWriter outfile = new StreamWriter(dir + "\\datafile_" + blist.ID + ".xml"))
                {
                    outfile.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    outfile.WriteLine(
                        $"<list id=\"{blist.ID}\" size=\"{blist.Size}\" unk1=\"{blist.Unknown1}\" unk2=\"{blist.Unknown2}\" unk3=\"{blist.Unknown3}\">");
                    BDAT_COLLECTION bcollection = blist.Collection;
                    outfile.WriteLine($"\t<collection compressed=\"{bcollection.Compressed}\">");
                    if (bcollection.Compressed > 0)
                    {
                        BDAT_ARCHIVE barchive = bcollection.Archive;
                        outfile.WriteLine($"\t\t<archive count=\"{barchive.SubArchiveCount}\">");
                        for (int a = 0; a < barchive.SubArchiveCount; a++)
                        {
                            BDAT_SUBARCHIVE bsubarchive = barchive.SubArchives[a];
                            outfile.WriteLine(
                                $"\t\t\t<subarchive count=\"{bsubarchive.FieldLookupCount}\" StartAndEndFieldId=\"{m_bnsDat.BytesToHex(bsubarchive.StartAndEndFieldId)}\">");
                            for (int f = 0; f < bsubarchive.FieldLookupCount; f++)
                            {
                                BDAT_FIELDTABLE bfield = bsubarchive.Fields[f];
                                outfile.Write("\t\t\t\t<field ID=\"{3}\" size=\"{0}\" unk1=\"{1}\" unk2=\"{2}\">", bfield.Size, bfield.Unknown1, bfield.Unknown2, bfield.ID);
                                outfile.Write(m_bnsDat.BytesToHex(bfield.Data));//00 00 00 00 00 00 00 00 0c 00 00 00 50 00 00 00
                                outfile.WriteLine("</field>");

                                BDAT_LOOKUPTABLE blookup = bsubarchive.Lookups[f];
                                string[] words = LookupSplitToWords(blookup.Data, blookup.Size);
                                outfile.WriteLine($"\t\t\t\t<lookup count=\"{words.Length}\">");
                                int empty = 0;
                                for (int w = 0; w < words.Length; w++)
                                {
                                    if (words[w] != null && words[w].Length > 0)
                                    {
                                        outfile.Write("\t\t\t\t\t<word>");
                                        outfile.Write(words[w]);
                                        outfile.WriteLine("</word>");
                                    }
                                    else
                                    {
                                        empty++;
                                    }
                                }
                                outfile.WriteLine($"\t\t\t\t\t<empty count=\"{empty}\"/>");
                                outfile.WriteLine("\t\t\t\t</lookup>");
                            }
                            outfile.WriteLine("\t\t\t</subarchive>");
                        }
                        outfile.WriteLine("\t\t</archive>");
                    }
                    else
                    {
                        BDAT_LOOSE bloose = bcollection.Loose;
                        outfile.WriteLine(
                            $"\t\t<loose countFields=\"{bloose.FieldCount}\" sizeFields=\"{bloose.SizeFields}\" sizePadding=\"{bloose.SizePadding}\" sizeLookup=\"{bloose.SizeLookup}\" unk=\"{bloose.Unknown}\">");
                        for (int f = 0; f < bloose.FieldCount; f++)
                        {
                            BDAT_FIELDTABLE bfield = bloose.Fields[f];
                            outfile.Write(
                                $"\t\t\t<field size=\"{bfield.Size}\" unk1=\"{bfield.Unknown1}\" unk2=\"{bfield.Unknown2}\">");
                            outfile.Write(m_bnsDat.BytesToHex(bfield.Data));
                            outfile.WriteLine("</field>");
                        }
                        outfile.Write("\t\t\t<padding>");
                        if (bloose.Padding != null)
                            outfile.Write(m_bnsDat.BytesToHex(bloose.Padding));
                        outfile.WriteLine("</padding>");

                        string[] words = LookupSplitToWords(bloose.Lookup.Data, bloose.Lookup.Size);
                        outfile.WriteLine($"\t\t\t<lookup count=\"{words.Length}\">");

                        int empty = 0;
                        for (int w = 0; w < words.Length; w++)
                        {
                            // only add non-empty words
                            if (words[w] != null && words[w].Length > 0)
                            {
                                outfile.Write("\t\t\t\t<word>");
                                outfile.Write(words[w]);//WebUtility.HtmlEncode(
                                outfile.WriteLine("</word>");
                            }
                            else
                            {
                                empty++;
                            }
                        }
                        outfile.WriteLine($"\t\t\t\t<empty count=\"{empty}\"/>");
                        outfile.WriteLine("\t\t\t</lookup>");
                        outfile.WriteLine("\t\t</loose>");
                    }
                    outfile.WriteLine("\t</collection>");
                    outfile.WriteLine("</list>");
                }
            }
            Console.Write("\rDone!!");
        }

        public void DetectIndices()
        {
            for (int l = 0; l < _content.ListCount; l++)
            {
                BDAT_LIST blist = _content.Lists[l];
                int fieldsize;
                if (blist.Unknown1 == 2 && blist.Unknown2 == 0)
                {
                    fieldsize = 0;
                    if (blist.Collection.Archive != null && blist.Collection.Archive.SubArchiveCount > 0 && blist.Collection.Archive.SubArchives[0].FieldLookupCount > 0)
                    {
                        fieldsize = blist.Collection.Archive.SubArchives[0].Fields[0].Size;
                    }
                    else if (blist.Collection.Loose != null && blist.Collection.Loose.FieldCount > 0)
                    {
                        fieldsize = blist.Collection.Loose.Fields[0].Size;
                    }
                    if (fieldsize == 32 || fieldsize == 40)
                    {
                        _indexFaqs = l;
#if DEBUG
                        Console.Write(
                            $"+++ FAQ +++\nIndex: {l}\nID:{blist.ID}\nSIZE: {blist.Size}\nUNK1: {blist.Unknown1}\nUNK2: {blist.Unknown2}\nUNK3: {blist.Unknown3}\nCompr: {blist.Collection.Compressed}\nFieldSize: {fieldsize}\n\n");
#endif
                    }
                }

                if (blist.Size > 5000000 && blist.Unknown1 == 1 && blist.Unknown2 == 0/* && blist.Unknown3 == 6*/)
                {
                    fieldsize = 0;
                    if (blist.Collection.Archive != null && blist.Collection.Archive.SubArchiveCount > 0 && blist.Collection.Archive.SubArchives[0].FieldLookupCount > 0)
                    {
                        fieldsize = blist.Collection.Archive.SubArchives[0].Fields[0].Size;
                    }
                    else if (blist.Collection.Loose != null && blist.Collection.Loose.FieldCount > 0)
                    {
                        fieldsize = blist.Collection.Loose.Fields[0].Size;
                    }
                    if (fieldsize == 28 || fieldsize == 36)
                    {
                        _indexCommons = l;
#if DEBUG
                        Console.Write(
                            $"+++ GENERAL +++\nIndex: {l}\nID:{blist.ID}\nSIZE: {blist.Size}\nUNK1: {blist.Unknown1}\nUNK2: {blist.Unknown2}\nUNK3: {blist.Unknown3}\nCompr: {blist.Collection.Compressed}\nFieldSize: {fieldsize}\n\n");
#endif
                    }
                }

                if (_indexCommons > -1 && (int)_indexCommons < l && blist.Unknown1 == 1 && blist.Unknown2 == 0)
                {
                    fieldsize = 0;
                    if (blist.Collection.Archive != null && blist.Collection.Archive.SubArchiveCount > 0 && blist.Collection.Archive.SubArchives[0].FieldLookupCount > 0)
                    {
                        fieldsize = blist.Collection.Archive.SubArchives[0].Fields[0].Size;
                        //Console.WriteLine("1=> _indexCommands fieldsize = " + fieldsize);
                    }
                    else if (blist.Collection.Loose != null && blist.Collection.Loose.FieldCount > 0)
                    {
                        fieldsize = blist.Collection.Loose.Fields[0].Size;
                        //Console.WriteLine("2 =>_indexCommands fieldsize = " + fieldsize);
                    }

                    if (fieldsize == 28)
                    {
                        _indexCommands = l;
#if DEBUG
                        Console.Write(
                            $"+++ COMMANDS +++\nIndex: {l}\nID:{blist.ID}\nSIZE: {blist.Size}\nUNK1: {blist.Unknown1}\nUNK2: {blist.Unknown2}\nUNK3: {blist.Unknown3}\nCompr: {blist.Collection.Compressed}\nFieldSize: {fieldsize}\n\n");
#endif
                    }
                }
            }
        }

        public string[] LookupSplitToWords(byte[] data, int size)
        {
            List<string> words = new List<string>();
            int start = 0;
            int end = 0;
            while (end + 1 < data.Length)
            {
                if (data[end] == 0 && data[end + 1] == 0)
                {
                    byte[] tmp = new byte[end - start];

                    Array.Copy(data, start, tmp, 0, end - start);
                    words.Add(Encoding.Unicode.GetString(tmp));
                    start = end + 2;
                }
                end += 2;
            }
            return words.ToArray();
        }

        public byte[] LookupWorldsToBytes(string[] strArr)
        {
            List<byte> listBytes = new List<byte>();
            foreach (string str in strArr)
            {
                listBytes.AddRange(Encoding.Unicode.GetBytes(str));
                listBytes.Add(0);
                listBytes.Add(0);
            }

            return listBytes.ToArray();
        }
    }

    public class BDAT_HEAD
    {
        public bool Complement;
        public int Size_1;
        public int Size_2;
        public int Size_3;
        public byte[] Padding;
        public byte[] Data;

        public void Read(BinaryReader br, BXML_TYPE iType)
        {
            Size_1 = br.ReadInt32();
            Size_2 = br.ReadInt32();
            Size_3 = br.ReadInt32();
            Padding = br.ReadBytes(62);
            Data = new byte[Size_1];
            if (!Complement)
                Data = br.ReadBytes(Size_1);
        }
        public void Read64(BinaryReader br, BXML_TYPE iType)
        {
            Size_1 = (int)br.ReadInt64();
            Size_2 = (int)br.ReadInt64();
            Size_3 = (int)br.ReadInt64();
            Padding = br.ReadBytes(62);
            Data = new byte[Size_1];
            if (!Complement)
                Data = br.ReadBytes(Size_1);
        }
        public void Write(BinaryWriter bw, BXML_TYPE iType)
        {
            bw.Write(Size_1);
            bw.Write(Size_2);
            bw.Write(Size_3);
            bw.Write(Padding);
            if (!Complement)
                bw.Write(Data);
        }
        public void Write64(BinaryWriter bw, BXML_TYPE iType)
        {
            bw.Write((long)Size_1);
            bw.Write((long)Size_2);
            bw.Write((long)Size_3);
            bw.Write(Padding);
            if (!Complement)
                bw.Write(Data);
        }
    }

    public class BDAT_LOOKUPTABLE
    {
        public int Size;
        public byte[] Data;

        public void Read(BinaryReader br)
        {
            Data = br.ReadBytes(Size);
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(Data);
        }
    }

    public class BDAT_FIELDTABLE
    {
        public int Unknown1; //short
        public int Unknown2; //short
        public int Size;
        public byte[] Data;
        public int ID;

        public void Read(BinaryReader br)
        {
            Unknown1 = br.ReadInt16();
            Unknown2 = br.ReadInt16();
            Size = br.ReadInt32();
            ID = br.ReadInt32();
            //fixed strungoutrules
            if (Size >= 12)
                Data = br.ReadBytes(Size - 12);
            else
                Data = new byte[0];

            //Console.WriteLine("Size: {0}, ID: {1}", Size, ID);
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(Ultily.WriteIntTo2Bytes(Unknown1));
            bw.Write(Ultily.WriteIntTo2Bytes(Unknown2));
            bw.Write(Size);
            bw.Write(ID);
            bw.Write(Data);
        }
    }

    public class BDAT_LOOSE
    {
        public int FieldCountUnfixed;         // 4 byte
        public int FieldCount;
        public int SizeFields;                // 4 byte
        public int SizeLookup;                // 4 byte
        public byte Unknown;                     // 1 byte
        public BDAT_FIELDTABLE[] Fields;            // SizeFields bytes
        public int SizePadding;               // 0 byte (private use only)
        public byte[] Padding;                    // (private use only)
        public BDAT_LOOKUPTABLE Lookup;            // SizeLookup bytes
        public bool Is64;

        public void Read(BinaryReader br)
        {
            FieldCount = br.ReadInt32();
            FieldCountUnfixed = FieldCount;
            SizeFields = br.ReadInt32();
            SizeLookup = br.ReadInt32();
            Unknown = br.ReadByte();

            if (FieldCount > 0 && SizeFields <= 0)
            {
                br.BaseStream.Position -= 13;
                FieldCount = (int)br.ReadInt64();
                FieldCountUnfixed = FieldCount;
                SizeFields = br.ReadInt32();
                SizeLookup = br.ReadInt32();
                Unknown = br.ReadByte();
                Is64 = true;
            }

            long offsetCurrent = 0;
            long offsetStart = br.BaseStream.Position;
            long offsetExpected = offsetStart + SizeFields;


            Fields = new BDAT_FIELDTABLE[FieldCount];
            for (int i = 0; i < FieldCount; i++)
            {
                offsetCurrent = br.BaseStream.Position;

                if (offsetCurrent >= offsetExpected)
                {
                    FieldCount = i;
                    br.BaseStream.Seek(offsetExpected - offsetCurrent, SeekOrigin.Current);
                    break;
                }
                Fields[i] = new BDAT_FIELDTABLE();
                Fields[i].Read(br);
            }

            //Console.WriteLine("//FieldCountUnfixed: " + FieldCountUnfixed + "|FieldCount: " + FieldCount + "|SizeFields: " + SizeFields + "|SizeLookup: " + SizeLookup + "|offsetExpected: " + offsetExpected);

            offsetCurrent = br.BaseStream.Position;
            SizePadding = (int)(offsetExpected - offsetCurrent);
            if (SizePadding < 0)
                return;

            if (SizePadding > 0)
            {
                Padding = br.ReadBytes(SizePadding);
            }
            Lookup = new BDAT_LOOKUPTABLE();
            Lookup.Size = SizeLookup;
            Lookup.Read(br);
        }

        public void Write(BinaryWriter bw)
        {
            if (Is64)
                bw.Write((long)FieldCountUnfixed);
            else
                bw.Write(FieldCountUnfixed);
            int offsetSize = (int)bw.BaseStream.Position;
            bw.Write(SizeFields);
            bw.Write(SizeLookup);
            bw.Write(Unknown);
            int offsetStart = (int)bw.BaseStream.Position;
            for (int i = 0; i < FieldCount; i++)
            {
                Fields[i].Write(bw);
            }
            if (SizePadding < 0)
                return;
            if (SizePadding > 0)
                bw.Write(Padding);

            SizeFields = (int)bw.BaseStream.Position - offsetStart;
            Lookup.Size = SizeLookup;
            Lookup.Write(bw);
            SizeLookup = (int)bw.BaseStream.Position - offsetStart - SizeFields;

            bw.BaseStream.Seek(offsetSize, SeekOrigin.Begin);
            bw.Write(SizeFields);
            bw.Write(SizeLookup);
            bw.BaseStream.Seek(1 + SizeFields + SizeLookup, SeekOrigin.Current);
        }

    }

    public class BDAT_SUBARCHIVE
    {
        public byte[] StartAndEndFieldId;                    // 16 byte (shorts?)
        public int SizeCompressed;            //short
        public int SizeDecompressed;          //short
        public int FieldLookupCount;          // 4 byte
        public BDAT_FIELDTABLE[] Fields;            // *
        public BDAT_LOOKUPTABLE[] Lookups;          // *
        private BNSDat m_bnsDat = new BNSDat();

        public void Read(BinaryReader br)
        {
            StartAndEndFieldId = br.ReadBytes(16);
			SizeCompressed = Ultily.ReadIntFrom2Bytes(br.ReadBytes(2));
			byte[] DataCompressed = br.ReadBytes(SizeCompressed);
			SizeDecompressed = Ultily.ReadIntFrom2Bytes(br.ReadBytes(2));

            if (SizeDecompressed < 0)
            {
                Console.WriteLine("SizeCompressed: " + SizeCompressed + "|SizeDecompressed: " + SizeDecompressed);
                //throw new Exception("SizeDecompressed is wrong!");
            }

            byte[] DataDecompressed = m_bnsDat.Deflate(DataCompressed, SizeCompressed, SizeDecompressed);
            FieldLookupCount = br.ReadInt32();
            Fields = new BDAT_FIELDTABLE[FieldLookupCount];
            Lookups = new BDAT_LOOKUPTABLE[FieldLookupCount];

            //Console.WriteLine("FieldLookupCount: " + FieldLookupCount);

            BinaryReader mis = new BinaryReader(new MemoryStream(DataDecompressed));
            int DataOffset = br.ReadInt16();
            for (int i = 1; i <= FieldLookupCount; i++)
            {
                mis.BaseStream.Seek(DataOffset, SeekOrigin.Begin);
                //Console.WriteLine("test: " + mis.BaseStream.Position + " - " + DataOffset);

                Fields[i - 1] = new BDAT_FIELDTABLE();
                Fields[i - 1].Read(mis);

                if (i < FieldLookupCount)
                {
                    DataOffset = Ultily.ReadIntFrom2Bytes(br.ReadBytes(2));
                }
                else
                {
                    DataOffset = SizeDecompressed;
                }

                Lookups[i - 1] = new BDAT_LOOKUPTABLE();
                Lookups[i - 1].Size = DataOffset - (int)mis.BaseStream.Position;
                Lookups[i - 1].Read(mis);
            }
        }
        private int m_maxSize = 0;
        public void Write(BinaryWriter bw)
        {
            BinaryWriter mos = new BinaryWriter(new MemoryStream());
            int[] DataOffsets = new int[FieldLookupCount];
            DataOffsets[0] = 0;
            for (int i = 1; i <= FieldLookupCount; i++)
            {
                Fields[i - 1].Write(mos);
                Lookups[i - 1].Write(mos);
                if (i < FieldLookupCount)
                {
                    DataOffsets[i] = (int)mos.BaseStream.Position;
                }
                if ((int)mos.BaseStream.Length < 65535)
                {
                    m_maxSize = i;
                }
            }

            SizeDecompressed = (int)mos.BaseStream.Length;

            byte[] DataDecompressed = new byte[SizeDecompressed];

            Array.Copy(((MemoryStream)mos.BaseStream).ToArray(), DataDecompressed, SizeDecompressed);//mos.CopyTo(DataDecompressed, SizeDecompressed);

            int SizeCompressedNew;
            byte[] DataCompressed = m_bnsDat.Inflate(DataDecompressed, SizeDecompressed, out SizeCompressedNew, 6);
            SizeCompressed = SizeCompressedNew;

            bw.Write(StartAndEndFieldId);
            bw.Write(Ultily.WriteIntTo2Bytes(SizeCompressed));
            bw.Write(DataCompressed);
            bw.Write(Ultily.WriteIntTo2Bytes(SizeDecompressed));
            bw.Write(FieldLookupCount);
            for (int i = 0; i < FieldLookupCount; i++)
            {
                bw.Write(Ultily.WriteIntTo2Bytes(DataOffsets[i]));
            }
        }

        public bool NeedSplit(ref int maxSize)
        {
            BinaryWriter bw = new BinaryWriter(new MemoryStream());
            Write(bw);
            maxSize = m_maxSize;
            return SizeDecompressed > 65535;
        }
    }

    public class BDAT_ARCHIVE
    {
        public int SubArchiveCount;           // 4 byte
        public int Unknown; //short
        public BDAT_SUBARCHIVE[] SubArchives;       // *

        public void Read(BinaryReader br)
        {
            SubArchiveCount = br.ReadInt32();
            Unknown = br.ReadInt16();
            SubArchives = new BDAT_SUBARCHIVE[SubArchiveCount];

            //string DebugText = "";

            for (int i = 0; i < SubArchiveCount; i++)
            {
                SubArchives[i] = new BDAT_SUBARCHIVE();
                SubArchives[i].Read(br);

                //DebugText += SubArchives[i].SizeDecompressed + "|";
            }

            //Console.WriteLine("Count SubArchive in Archive: " + SubArchiveCount);
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(SubArchiveCount);
            bw.Write(Ultily.WriteIntTo2Bytes(Unknown));
            for (int i = 0; i < SubArchiveCount; i++)
            {
                SubArchives[i].Write(bw);
            }
        }
    }

    public class BDAT_COLLECTION
    {
        public byte Compressed;                    // 1 byte
        public byte Deprecated;                 // 1 byte
        public BDAT_ARCHIVE Archive;              // *
        public BDAT_LOOSE Loose;                  // *

        public void Read(BinaryReader br)
        {
            Compressed = br.ReadByte();

            if (Convert.ToBoolean(Compressed))
            {
                if (Compressed > 1)
                    br.BaseStream.Seek(br.BaseStream.Position - 1, SeekOrigin.Begin);

                Archive = new BDAT_ARCHIVE();
                Archive.Read(br);
                Loose = null;
                if (Compressed > 1)
                    Deprecated = br.ReadByte();
            }
            else
            {
                Loose = new BDAT_LOOSE();
                Loose.Read(br);
                Archive = null;
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(Compressed);
            if (Convert.ToBoolean(Compressed))
            {
                if (Compressed > 1)
                    bw.BaseStream.Seek(bw.BaseStream.Position - 1, SeekOrigin.Begin);
                Archive.Write(bw);
                if (Compressed > 1)
                    bw.Write(Deprecated);
            }
            else
            {
                Loose.Write(bw);
            }
        }
    }

    public class BDAT_LIST
    {
        public byte Unknown1;
        public int ID; //short
        public int Unknown2; //short
        public int Unknown3; //short
        public int Size;
        public BDAT_COLLECTION Collection;

        public void Read(BinaryReader br)
        {
            Unknown1 = br.ReadByte();
            this.ID = Ultily.ReadIntFrom2Bytes(br.ReadBytes(2));
            this.Unknown2 = Ultily.ReadIntFrom2Bytes(br.ReadBytes(2));
            this.Unknown3 = Ultily.ReadIntFrom2Bytes(br.ReadBytes(2));
            Size = br.ReadInt32();

            long offsetStart = br.BaseStream.Position;
            Collection = new BDAT_COLLECTION();
            Collection.Read(br);
            long offsetEnd = br.BaseStream.Position;

            if (offsetStart + Size != offsetEnd)
            {
                br.BaseStream.Seek(offsetStart + Size, SeekOrigin.Begin);
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(Unknown1);
            bw.Write(Ultily.WriteIntTo2Bytes(ID));
            bw.Write(Ultily.WriteIntTo2Bytes(Unknown2));
            bw.Write(Ultily.WriteIntTo2Bytes(Unknown3));
            bw.Write(Size);
            long offsetStart = bw.BaseStream.Position;
            Collection.Write(bw);
            long offsetEnd = bw.BaseStream.Position;
            bw.Seek((int)offsetStart - 4, SeekOrigin.Begin);
            Size = (int)(offsetEnd - offsetStart);
            bw.Write(Size);
            bw.Seek(Size, SeekOrigin.Current);
        }

    }

    public class BDAT_CONTENT
    {
        public byte[] Signature;
        public int Version;
        public byte[] Unknown;
        public int ListCount;
        public BDAT_HEAD HeadList;
        public BDAT_LIST[] Lists;

        public void Read(BinaryReader br, BXML_TYPE iType)
        {
            Signature = br.ReadBytes(8);
            Version = br.ReadInt32();
            Unknown = br.ReadBytes(9);
            ListCount = br.ReadInt32();
            HeadList = new BDAT_HEAD();
            HeadList.Complement = false;
            if (ListCount < 20)
                HeadList.Complement = true;
            HeadList.Read(br, iType);
            Lists = new BDAT_LIST[ListCount];
#if DEBUG
            Console.WriteLine("32bit Signature: {0}, Version: {1}, Unknown: {2}, ListCount: {3}", Signature, Version, Unknown, ListCount);
#endif
            for (int l = 0; l < ListCount; l++)
            {
                Lists[l] = new BDAT_LIST();
                Lists[l].Read(br);
            }
        }

        public void Read64(BinaryReader br, BXML_TYPE iType)
        {
            Signature = br.ReadBytes(8);
            Version = br.ReadInt32();
            Unknown = br.ReadBytes(13);
            ListCount = (int)br.ReadInt64();
            HeadList = new BDAT_HEAD();
            HeadList.Complement = false;
            if (ListCount < 20)
                HeadList.Complement = true;
            HeadList.Read64(br, iType);
            Lists = new BDAT_LIST[ListCount];
            //Console.WriteLine("64bit Signature: {0}, Version: {1}, Unknown: {2}, ListCount: {3}", Signature, Version, Unknown, ListCount);
            for (int l = 0; l < ListCount; l++)
            {
                Lists[l] = new BDAT_LIST();
                Lists[l].Read(br);
            }
        }

        public void Write(BinaryWriter bw, BXML_TYPE iType)
        {
            bw.Write(Signature);
            bw.Write(Version);
            bw.Write(Unknown);
            bw.Write(ListCount);
            HeadList.Write(bw, iType);
            for (int l = 0; l < ListCount; l++)
            {
                Lists[l].Write(bw);
            }
        }
        public void Write64(BinaryWriter bw, BXML_TYPE iType)
        {
            bw.Write(Signature);
            bw.Write(Version);
            bw.Write(Unknown);
            bw.Write((long)ListCount);
            HeadList.Write64(bw, iType);
            for (int l = 0; l < ListCount; l++)
            {
                Lists[l].Write(bw);
            }
        }
    }

    public static class Ultily
    {
        public static int ReadIntFrom2Bytes(byte[] bytes)
        {
            return bytes[0] | (bytes[1] << 8);
        }

        public static byte[] WriteIntTo2Bytes(int value)
        {
            byte[] data = new byte[2];

            data[0] = (byte)(value & 0xFF);
            data[1] = (byte)((value >> 8) & 0xFF);

            return data;
        }
    }

    public class TranslateReader
    {
        public Dictionary<string, string> _trans_alias = new Dictionary<string, string>();
        public Dictionary<string, string> _trans_text = new Dictionary<string, string>();
        public Dictionary<string, int> _trans_priority = new Dictionary<string, int>();
        public Dictionary<string, string> Trans_alias
        {
            get
            {
                return _trans_alias;
            }
        }

        private XmlDocument _xDoc = new XmlDocument();

        public XmlDocument Doc
        {
            get
            {
                return _xDoc;
            }
        }
        public void Load(string filename)
        {
            Load(filename, false);
        }
        public void Load(string filename, bool loadOnly)
        {
            _xDoc.Load(filename);
            if (_xDoc == null) throw new Exception("translation.xml not found!");

            XmlNodeList xNodeList = _xDoc.SelectNodes("table/child::node()");

            foreach (XmlNode xNode in xNodeList)
            {
                string alias = xNode.Attributes["alias"].Value;
                string priority = xNode.Attributes["priority"].Value;
                string original = xNode.FirstChild.InnerText;//.Trim();
                string translated = xNode.LastChild.InnerText;//.Trim();

                if (alias != null && !_trans_alias.ContainsKey(alias))
                {

                    _trans_alias.Add(alias, translated);
                }
                else
                {
                    _trans_alias[alias] = translated;
                }
                if (!_trans_text.ContainsKey(original) || _trans_priority[original] < int.Parse(priority))
                {
                    _trans_text.Add(original, translated);
                    _trans_priority.Add(original, int.Parse(priority));
                }

                //MessageBox.Show("alias: " + alias + "|priority: " + priority + "|original: " + original + "|translated: " + translated);
            }
        }

        public string Translate(string original, string alias)
        {
            string translated = null;
            if (!string.IsNullOrEmpty(alias) && _trans_alias.ContainsKey(alias))
            {
                translated = _trans_alias[alias];
            }
            if (translated == null && !string.IsNullOrEmpty(original) && _trans_text.ContainsKey(original))
            {
                translated = _trans_text[original];
            }
            return translated;
        }
        public static class CharExtension
        {
            public static bool IsChineseChar(string elem)
            {
                // return elem >= '\u4E00' && elem <= '\u9FFF';
                return elem.Any(c => c >= 0x3300 && c <= 0x33FF) ||
                       elem.Any(c => c >= 0x3400 && c <= 0x4DB5) ||
                       elem.Any(c => c >= 0x4E00 && c <= 0x9FFF);
            }
            public static bool IsJapanChar(string elem)
            {
                //return (elem >= '\u3040' && elem <= '\u309F') || (elem >= '\u30A0' && elem <= '\u30FF');
                return elem.Any(c => c >= 0x3040 && c <= 0x309F) ||
                       elem.Any(c => c >= 0x30A0 && c <= 0x30FF);
            }
            public static bool IsKoreaChar(string elem)
            {
                //return (elem >= '\u3130' && elem <= '\u318F')||(elem >= '\uAC00' && elem <= '\uD7AF');
                return elem.Any(c => c >= 0x3130 && c <= 0x318F) ||
                       elem.Any(c => c >= 0xAC00 && c <= 0xD7AF) ||
                       elem.Any(c => c >= 0x1100 && c <= 0x11FF);
            }
            public static bool IsThaiChar(string elem)
            {
                //return elem >= '\u0E00' && elem <= '\u0E7F';
                return elem.Any(c => c >= 0x0E00 && c <= 0x0E7F);
            }
            public static bool IsVietChar(string elem)
            {
                //return (elem >= '\u00C0' && elem <= '\u00FF')|| (elem >= '\u1EA0' && elem <= '\u1EFF');
                return elem.Any(c => c >= 0x00C0 && c <= 0x00FF) ||
                       elem.Any(c => c >= 0x1EA0 && c <= 0x1EFF);
            }
        }
        public bool IsRunesChar(string text)
        {
            if (CharExtension.IsKoreaChar(text) ||
                CharExtension.IsChineseChar(text) ||
                CharExtension.IsJapanChar(text) ||
                CharExtension.IsThaiChar(text)||
                CharExtension.IsVietChar(text))
                return false;
            return true;
        }

        public bool CanTran(string alias)
        {
            //For Frontier
            //if (alias.IndexOf("Usercommand.TalkSocial") >= 0 ||
            //    alias.IndexOf("Usercommand.Emoticon") >= 0 ||
            //    alias.IndexOf("Usercommand.chat") >= 0)
            //    return true;
            //if (alias.IndexOf("Usercommand") == 0 || alias.IndexOf("Emoticon") == 0)
            //    return true;
            //if (alias.IndexOf("UI") == 0)
            //    return true;
            //if (alias.IndexOf("Npc") == 0)
            //    return true;
            //if (alias.IndexOf("Unit_") == 0)
            //    return true;
            //if (alias.IndexOf("Quest.Name2") == 0)
            //    return true;
            //if (alias.IndexOf("gm_322") == 0)
            //    return true;
            //if (alias.IndexOf("Msg") == 0)
            //    return true;
            //if (alias.IndexOf("FieldZone") == 0)
            //    return true;
            //if (alias.IndexOf("Map_") == 0)
            //    return true;
            //if (alias.IndexOf("CH_") == 0)
            //    return true;
            //if (alias.IndexOf("lobby_account") == 0)
            //    return true;
            //if (alias.IndexOf("authfail") == 0)
            //    return true;
            //if (alias.IndexOf("Name.npc") == 0)
            //    return true;
            //if (alias.IndexOf("Name.Faction") == 0)
            //    return true;
            //if (alias.IndexOf("Name.Auction") == 0)
            //    return true;
            //if (alias.IndexOf("Name.item") == 0)
            //    return true;
            //if (alias.IndexOf("Name.LoadingTip") == 0)
            //    return true;
            //if (alias.IndexOf("QLooting") == 0)
            //    return true;
            //if (alias.IndexOf("daesamak_") == 0)
            //    return true;
            //if (alias.IndexOf("Skill.Name2") == 0)
            //    return true;
            //if (alias.IndexOf("select_") == 0)
            //    return true;
            //if (alias.IndexOf("Text.JobData") == 0)
            //    return true;
            //if (alias.IndexOf("QG_BladeMaster") == 0)
            //    return true;
            //if (alias.IndexOf("desc_") == 0)
            //    return true;
            //if (alias.IndexOf("customize_") == 0)
            //    return true;
            //if (alias.IndexOf("PcVoice") == 0)
            //    return true;
            //if (alias.IndexOf("Discovery.Name2") == 0)
            //    return true;
            //if (alias.IndexOf("Text.EnterWorld") == 0)
            //    return true;
            //if (alias.IndexOf("connecting") == 0)
            //    return true;
            //if (alias.IndexOf("Mission.Name2") == 0)
            //    return true;
            //if (alias.IndexOf("Name.mapunit") == 0)
            //    return true;
            //if (alias.IndexOf("Name.ContentsGuide") == 0)
            //    return true;
            //if (alias.IndexOf("chatdomain") == 0)
            //    return true;
            //if (alias.IndexOf("Discovery.Name2") == 0)
            //    return true;
            //if (alias.IndexOf("Name.job") == 0)
            //    return true;
            //if (alias.IndexOf("Name.Item.") == 0)
            //    return true;
            //if (alias.IndexOf("Name.ItemTooltip") == 0)
            //    return true;
            //if (alias.IndexOf("Name.ItemTransform") == 0)
            //    return true;
            //if (alias.IndexOf("Name.ItemBrand") == 0)
            //    return true;
            //if (alias.IndexOf("Name.EquipSlot") == 0)
            //    return true;
            //if (alias.IndexOf("Name2.Teleport") == 0)
            //    return true;
            //if (alias.IndexOf("Name.GemCompose") == 0)
            //    return true;
            //if (alias.IndexOf("Name.KeyCommand") == 0)
            //    return true;
            //if (alias.IndexOf("Name.PhotoAlbum") == 0)
            //    return true;
            //if (alias.IndexOf("Name.GameTipBalloon") == 0)
            //    return true;
            //if (alias.IndexOf("Name.Option") == 0)
            //    return true;
            //if (alias.IndexOf("Name.KeyCommand") == 0)
            //    return true;
            //if (alias.IndexOf("IBN.ItemBrand") == 0)
            //    return true;
            //if (alias.IndexOf("AQ.") == 0)
            //    return true;
            //if (alias.IndexOf("PartyMatch.") == 0)
            //    return true;
            //if (alias.IndexOf("InterD.Desc") == 0)
            //    return true;
            //if (alias.IndexOf("Item.DescTitle") == 0)
            //    return true;
            //if (alias.IndexOf("Effect.Name2.SP") == 0 ||
            //    alias.IndexOf("Effect.Desc2.SP") == 0)
            //    return true;
            //if (alias.Contains("Store2.Tendency"))
            //    return true;
            //if (alias.Contains("Item.LimitUsePlace"))
            //    return true;
            //if (alias.Contains("Location.name"))
            //    return true;
            //if (alias.Contains("QG_"))
            //    return true;
            //if (alias.Contains("_env_"))
            //    return true;
            ////Translate only items
            //if (alias.IndexOf("Item.Name2") == 0)
            //    return true;
            return true;
        }

        public void MergeTranslation(TranslateReader translated, string savePath)
        {
            XElement texts = new XElement("table");

            XmlNodeList xNodeList = _xDoc.SelectNodes("table/child::node()");
            foreach (XmlNode xNode in xNodeList)
            {
                string alias = xNode.Attributes[NodeNames.Alias].Value;
                string autoId = xNode.Attributes[NodeNames.AutoId].Value;
                string priority = xNode.Attributes[NodeNames.Priority].Value;
                string replacement = xNode.LastChild.InnerText;
                //string original = xNode.FirstChild.InnerText;
                
                if (!string.IsNullOrEmpty(alias) && translated.Trans_alias.ContainsKey(alias) && CanTran(alias)/*&& IsRunesChar(translated.Trans_alias[alias])*/)
                {
                    replacement = translated.Trans_alias[alias];
                }
                else
                {
#if DEBUG
                   // Console.WriteLine("MergeTranslation: {0}", alias);
#endif
                }
                XElement temp_xml = new XElement(NodeNames.Root,
                   new XAttribute(NodeNames.AutoId, autoId),
                   new XAttribute(NodeNames.Alias, alias),
                   new XAttribute(NodeNames.Priority, priority));

                temp_xml.Add(new XElement(NodeNames.Original, new XCData(xNode.FirstChild.InnerText)),
                             new XElement(NodeNames.Replacement, new XCData(replacement)));

                texts.Add(temp_xml);
            }
            //Doc.Save(savePath);
            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Encoding = Encoding.UTF8,
                Indent = true
            };
#if DEBUG
            Console.WriteLine("savePath: {0}", savePath);
#endif
            using (XmlWriter xw = XmlWriter.Create(savePath, settings))
            {
                texts.Save(xw);
            }
            Console.WriteLine("\rDone!!");
        }
    }
}

using System;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace BnsDatTool
{
    public class StreamWriter : TextWriter
    {
        RichTextBox _output = null;

        public StreamWriter(RichTextBox output)
        {
            _output = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            _output.AppendText(value.ToString());

        }
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}

using System.IO.Pipelines;
using System.Text;

namespace LLE.Frontend.Writers
{
    public class HtmlSink(PipeWriter writer)
    {
        private static readonly Encoding Utf8 = Encoding.UTF8;

        public void Append(string value)
        {
            var span = writer.GetSpan(Utf8.GetMaxByteCount(value.Length));
            var written = Utf8.GetBytes(value, span);
            writer.Advance(written);
        }

        public void Append(ReadOnlySpan<char> value)
        {
            var span = writer.GetSpan(Utf8.GetMaxByteCount(value.Length));
            var written = Utf8.GetBytes(value, span);
            writer.Advance(written);
        }
    }
}
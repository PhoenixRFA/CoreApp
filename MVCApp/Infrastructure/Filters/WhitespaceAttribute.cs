using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MVCApp.Infrastructure.Filters
{
    public class WhitespaceAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var response = context.HttpContext.Response;

            if (response.Body == null) return;

            response.Body = new SpaceCleaner(response.Body);
        }
    }

    public class SpaceCleaner : Stream
    {
        private readonly Stream _outputStream;

        public SpaceCleaner(Stream filterStream)
        {
            if (filterStream == null)
            {
                throw new ArgumentNullException(nameof(filterStream));
            }

            _outputStream = filterStream;
        }

        public override void Flush()
        {
            _outputStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            string html = Encoding.UTF8.GetString(buffer, offset, count);
            var regx = new Regex(@"(?<=\s)\s+(?![^<>]*</pre>)");
            html = regx.Replace(html, string.Empty);
            buffer = Encoding.UTF8.GetBytes(html);

            int checkedCount = count > buffer.Length ? buffer.Length : count;
            await _outputStream.WriteAsync(buffer, offset, checkedCount, cancellationToken);
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotImplementedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}

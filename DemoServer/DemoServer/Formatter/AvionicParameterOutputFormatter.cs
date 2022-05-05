//
// Copyright (c) Deutsche Lufthansa AG.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

using System;
using System.Text;
using System.Threading.Tasks;
using DemoServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace DemoServer.Formatter
{
    public class AvionicParameterOutputFormatter : TextOutputFormatter
    {
        public AvionicParameterOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/avionic"));
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanWriteType(Type type)
        {
            if (typeof(AvionicParameters).IsAssignableFrom(type))
            {
                return base.CanWriteType(type);
            }

            return false;
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            IServiceProvider serviceProvider = context.HttpContext.RequestServices;
            var logger = serviceProvider.GetService(typeof(ILogger<AvionicParameterOutputFormatter>)) as ILogger;
            var response = context.HttpContext.Response;
            var buffer = new StringBuilder();
            var parameters = context.Object as AvionicParameters;
            FormatAvionic(buffer, parameters, logger);
            await response.WriteAsync(buffer.ToString());
        }

        private void FormatAvionic(StringBuilder buffer, AvionicParameters parameters, ILogger logger)
        {
            foreach (var p in parameters.Parameters)
            {
                buffer.AppendLine($"data,{p.Timestamp},{p.Settable},{p.Name},{p.Value}");
            }
        }
    }
}
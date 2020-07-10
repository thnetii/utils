using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Text;
using System.Threading;

namespace THNETII.Utils.GhMultiSync
{
    partial class Program
    {
        public static ICommandHandler Handler { get; } = CommandHandler.Create(
        (IHost host, CancellationToken cancelToken) =>
        {

        });
    }
}

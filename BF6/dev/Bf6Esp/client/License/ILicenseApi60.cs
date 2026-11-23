using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bf6Esp;

internal interface ILicenseApi60 : IDisposable
{
	Task<LicenseReply60> Call(string path, Dictionary<string, object> payload, string token, CancellationToken cancel);
}

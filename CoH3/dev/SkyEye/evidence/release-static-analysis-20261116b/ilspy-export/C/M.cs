using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using a;

namespace C;

internal sealed class M : H, IDisposable
{
	public const string A = "0.60.8";

	private readonly HttpClient m_A;

	public M()
		: this(L.a)
	{
	}

	internal M(Uri P_0)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
		this.m_A = new HttpClient((HttpMessageHandler)new HttpClientHandler
		{
			AllowAutoRedirect = false,
			UseCookies = false
		})
		{
			BaseAddress = L.A(P_0.AbsoluteUri),
			Timeout = TimeSpan.FromSeconds(5.0),
			MaxResponseContentBufferSize = 32768L
		};
		this.m_A.DefaultRequestHeaders.UserAgent.ParseAdd(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XJ());
	}

	public async Task<o> A(string P_0, Dictionary<string, object> P_1, string P_2, CancellationToken P_3)
	{
		if (!new string[4]
		{
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XK(),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xo(),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XO(),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XP()
		}.A(P_0))
		{
			throw new ArgumentException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Yg());
		}
		J j = null;
		if (P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xo() || P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XO())
		{
			if (P_1 == null)
			{
				throw new ArgumentException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.YH());
			}
			P_1 = new Dictionary<string, object>(P_1);
			P_1[_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dI()] = J.A();
			j = new J(P_1, P_2, P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xo());
		}
		long num = a.k.A;
		HttpRequestMessage val = new HttpRequestMessage((P_1 == null) ? HttpMethod.Get : HttpMethod.Post, P_0);
		try
		{
			if (P_1 != null)
			{
				val.Content = (HttpContent)new StringContent(new JavaScriptSerializer().Serialize(P_1), Encoding.UTF8, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Yh());
			}
			if (!string.IsNullOrEmpty(P_2))
			{
				val.Headers.Authorization = new AuthenticationHeaderValue(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.YI(), P_2);
			}
			HttpResponseMessage val2 = await this.m_A.SendAsync(val, (HttpCompletionOption)0, P_3).ConfigureAwait(continueOnCapturedContext: false);
			try
			{
				if (val2.StatusCode >= HttpStatusCode.InternalServerError || val2.StatusCode == (HttpStatusCode)429)
				{
					throw new HttpRequestException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Yi());
				}
				string input = await val2.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
				o o = new o((new JavaScriptSerializer
				{
					MaxJsonLength = 32768
				}.DeserializeObject(input) as Dictionary<string, object>) ?? throw new HttpRequestException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.YJ()), num);
				if (o.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dk())
				{
					o.A(j);
				}
				if (!val2.IsSuccessStatusCode && string.IsNullOrEmpty(o.A))
				{
					throw new HttpRequestException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Yj());
				}
				return o;
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void Dispose()
	{
		((HttpMessageInvoker)this.m_A).Dispose();
	}
}

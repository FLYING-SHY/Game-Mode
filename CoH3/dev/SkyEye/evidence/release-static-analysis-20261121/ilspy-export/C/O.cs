using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using a;

namespace C;

internal sealed class O : I, IDisposable
{
	public const string A = "0.60.8";

	private readonly HttpClient m_A;

	public O()
		: this(M.a)
	{
	}

	internal O(Uri P_0)
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
			BaseAddress = M.A(P_0.AbsoluteUri),
			Timeout = TimeSpan.FromSeconds(5.0),
			MaxResponseContentBufferSize = 32768L
		};
		this.m_A.DefaultRequestHeaders.UserAgent.ParseAdd(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xn());
	}

	public async Task<p> A(string P_0, Dictionary<string, object> P_1, string P_2, CancellationToken P_3)
	{
		if (!new string[4]
		{
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xo(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XT(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xs(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xt()
		}.A(P_0))
		{
			throw new ArgumentException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YM());
		}
		K k = null;
		if (P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XT() || P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xs())
		{
			if (P_1 == null)
			{
				throw new ArgumentException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ym());
			}
			P_1 = new Dictionary<string, object>(P_1);
			P_1[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dl()] = K.A();
			k = new K(P_1, P_2, P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XT());
		}
		long num = a.l.A;
		HttpRequestMessage val = new HttpRequestMessage((P_1 == null) ? HttpMethod.Get : HttpMethod.Post, P_0);
		try
		{
			if (P_1 != null)
			{
				val.Content = (HttpContent)new StringContent(new JavaScriptSerializer().Serialize(P_1), Encoding.UTF8, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YN());
			}
			if (!string.IsNullOrEmpty(P_2))
			{
				val.Headers.Authorization = new AuthenticationHeaderValue(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yn(), P_2);
			}
			HttpResponseMessage val2 = await this.m_A.SendAsync(val, (HttpCompletionOption)0, P_3).ConfigureAwait(continueOnCapturedContext: false);
			try
			{
				if (val2.StatusCode >= HttpStatusCode.InternalServerError || val2.StatusCode == (HttpStatusCode)429)
				{
					throw new HttpRequestException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YO());
				}
				string input = await val2.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
				p p = new p((new JavaScriptSerializer
				{
					MaxJsonLength = 32768
				}.DeserializeObject(input) as Dictionary<string, object>) ?? throw new HttpRequestException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yo()), num);
				if (p.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dO())
				{
					p.A(k);
				}
				if (!val2.IsSuccessStatusCode && string.IsNullOrEmpty(p.A))
				{
					throw new HttpRequestException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YP());
				}
				return p;
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

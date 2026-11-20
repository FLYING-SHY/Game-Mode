using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
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
		this.m_A.DefaultRequestHeaders.UserAgent.ParseAdd(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XJ());
	}

	public async Task<o> A(string P_0, Dictionary<string, object> P_1, string P_2, CancellationToken P_3)
	{
		if (!new string[4]
		{
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XK(),
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xo(),
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XO(),
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XP()
		}.A(P_0))
		{
			throw new ArgumentException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Yg());
		}
		J j = null;
		if (P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xo() || P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XO())
		{
			if (P_1 == null)
			{
				throw new ArgumentException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.YH());
			}
			P_1 = new Dictionary<string, object>(P_1);
			P_1[C4318269_002D39F2_002D4549_002D851A_002D418926B36435.dI()] = J.A();
			j = new J(P_1, P_2, P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xo());
		}
		long num = a.k.A;
		HttpRequestMessage val = new HttpRequestMessage((P_1 == null) ? HttpMethod.Get : HttpMethod.Post, P_0);
		try
		{
			if (P_1 != null)
			{
				val.Content = (HttpContent)new StringContent(new JavaScriptSerializer().Serialize(P_1), Encoding.UTF8, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Yh());
			}
			if (!string.IsNullOrEmpty(P_2))
			{
				val.Headers.Authorization = new AuthenticationHeaderValue(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.YI(), P_2);
			}
			HttpResponseMessage val2 = await this.m_A.SendAsync(val, (HttpCompletionOption)0, P_3).ConfigureAwait(continueOnCapturedContext: false);
			try
			{
				if (val2.StatusCode >= HttpStatusCode.InternalServerError || val2.StatusCode == (HttpStatusCode)429)
				{
					throw new HttpRequestException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Yi());
				}
				string input = await val2.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
				o o = new o((new JavaScriptSerializer
				{
					MaxJsonLength = 32768
				}.DeserializeObject(input) as Dictionary<string, object>) ?? throw new HttpRequestException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.YJ()), num);
				if (o.A == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.dk())
				{
					o.A(j);
				}
				if (!val2.IsSuccessStatusCode && string.IsNullOrEmpty(o.A))
				{
					throw new HttpRequestException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Yj());
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

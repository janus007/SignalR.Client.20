﻿using System.Collections.Generic;
using Newtonsoft.Json;
using SignalR.Client._20.Transports;
using Newtonsoft.Json.Linq;

namespace SignalR.Client._20.Hubs
{
	public class HubConnection : Connection
	{
		private readonly Dictionary<string, HubProxy> m_hubs = new Dictionary<string, HubProxy>();

		public HubConnection(string url)
			: base(GetUrl(url))
		{
		}

        public HubConnection(string url, IDictionary<string, string> queryString)
            : base(GetUrl(url), queryString)
        {
        }

		public override void Start(IClientTransport transport)
		{
			Sending += OnConnectionSending;
			base.Start(transport);
		}

		public override void Stop()
		{
			Sending -= OnConnectionSending;
			base.Stop();
		}

		protected override void OnReceived(JToken message)
		{
			var _invocation = message.ToObject<HubInvocation>();
			HubProxy _hubProxy;

			if (m_hubs.TryGetValue(_invocation.Hub, out _hubProxy))
			{
				if (_invocation.State != null)
				{
					foreach (var state in _invocation.State)
					{
						_hubProxy[state.Key] = state.Value;
					}
				}
				_hubProxy.InvokeEvent(_invocation.Method, _invocation.Args);
			}
			base.OnReceived(message);
		}

		public IHubProxy CreateProxy(string hubName)
		{
			HubProxy _hubProxy;
			if (!m_hubs.TryGetValue(hubName, out _hubProxy))
			{
				_hubProxy = new HubProxy(this, hubName);
				m_hubs[hubName] = _hubProxy;
			}
			return _hubProxy;
		}

		private string OnConnectionSending()
		{
			var _data = new List<HubRegistrationData>();
			foreach (var p in m_hubs)
			{
				_data.Add(new HubRegistrationData { Name = p.Key });
			}			
			return JsonConvert.SerializeObject(_data);
		}

		private static string GetUrl(string url)
		{
			if (!url.EndsWith("/"))
				url += "/";
			return url + "signalr";
		}
	}
}

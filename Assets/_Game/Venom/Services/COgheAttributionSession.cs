using System;
using System.Collections.Generic;
using System.Globalization;
using GravityBox.Venom;

namespace GravityBox.Services
{
    public interface ICOgheAttributionTransport
    {
        void Connect();
        void OptOut();
        void Event(string name, string integerValue);
    }

    /// <summary>No queue, no pre-consent SDK initialization, no dependency on Firebase readiness.</summary>
    public sealed class COgheAttributionSession : IDisposable
    {
        private readonly Func<ICOgheAttributionTransport> create;
        private ICOgheAttributionTransport transport;
        private int permit = -1;
        public bool Active => permit >= 0 && COghePrivacy.AdMeasurementPermitValid(permit);

        public COgheAttributionSession(Func<ICOgheAttributionTransport> create) => this.create = create;

        public void ApplyPrivacy()
        {
            if (!COghePrivacy.AdMeasurementAllowed) { Suspend(); return; }
            if (Active) return;
            Suspend();
            transport ??= create();
            if (transport == null) return;
            permit = COghePrivacy.Revision;
            try { transport.Connect(); }
            catch { Suspend(); throw; }
        }

        public void Foreground()
        {
            if (Active) transport.Connect();
            else ApplyPrivacy();
        }

        public void Send(string name, IReadOnlyDictionary<string, object> values)
        {
            if (!Active || !COghePrivacy.AnalyticsAllowed || values == null) return;
            // Only bounded puzzle progress. No age, attempt UUID, free text or purchase claims.
            if (name != "level_start" && name != "level_complete" && name != "level_fail") return;
            if (!values.TryGetValue("level", out var value) || !(value is int level) || level < 1 || level > 10000) return;
            transport.Event(name, level.ToString(CultureInfo.InvariantCulture));
        }

        private void Suspend()
        {
            bool wasConnected = permit >= 0;
            permit = -1;
            if (wasConnected) transport?.OptOut();
        }

        public void Dispose() => Suspend();
    }
}

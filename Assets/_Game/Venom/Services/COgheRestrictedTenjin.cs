using System.Collections.Generic;
using GravityBox.Venom;

namespace GravityBox.Services
{
    /// <summary>Tell Tenjin that identifiers are intentionally withheld, including on reconnect.</summary>
    public sealed class COgheRestrictedTenjin : AndroidTenjin
    {
        public override void Connect()
        {
            if (!COghePrivacy.AdMeasurementAllowed) { OptOut(); return; }
            // SDK 1.23.2 remembers OptIn/OptOut flags and passes them to native connect,
            // which would overwrite the parameter policy. Clear those flags, then
            // explicitly opt out before every connect (Tenjin support, 18 Nov 2026).
            // OptInParams alone causes server error 202 when advertising_id is absent.
            optIn = false;
            optOut = false;
            OptOutParams(new List<string> {
                "developer_device_id", "developer_device_id_scope",
                "ip_address", "imei", "oaid", "android_id", "ad_id_local",
                "firebase_app_instance_id", "customer_user_id",
                "device", "device_info", "device_model", "device_manufacturer",
                "device_brand", "device_product", "screen_width", "screen_height",
                "build_id", "os_version", "os_version_release", "locale", "language",
                "timezone", "country", "carrier", "connection_type", "connectivity",
                "meta_referrer", "meta_referrer_clickthrough", "meta_referrer_ts",
                "iab_tcf", "ad_personalization", "ad_user_data"
            });
            // Only the separately consented adult GAID is allowed. No Android ID, App Set ID
            // or IP fallback; a missing/deleted GAID must never be reported as proven attribution.
            // Keep Play referrer + SDK installation/session fields and tracking-limit
            // status. The SDK is pinned; audit new optional fields before any upgrade.
            base.Connect();
        }
    }
}

# Alien Pet embedded SDK patch

Upstream: https://github.com/tenjin/tenjin-unity-sdk
Pinned commit: 58e074aca12fa34c1a47174c68add1e6cc6a783d
Unity SDK 1.23.2 / Android SDK 2.0.2. Original MIT license retained.

On 18 November 2026, removed play-services-appset from Tenjin's Editor/Dependencies.xml, as directed by Tenjin support. No runtime vendor sources changed. Embedding preserves this change across clean checkouts and Unity package resolution.

The owner subsequently approved restoring Google's transitive App Set library because Google Mobile Ads 25.4.0 crashed without it. COghePrivacyBuild no longer globally excludes that module. Google may process App Set ID; Tenjin excludes developer_device_id and developer_device_id_scope through explicit OptOutParams. See Docs/Monetization/COghe/GOOGLE_SERVICES.md.

Do not upgrade without re-auditing native parameter providers, consent and request payloads. Tenjin remains disabled in the production configuration. Its adult test path requires separate, explicit 18+ ad-measurement consent; Analytics consent alone never grants that permission.

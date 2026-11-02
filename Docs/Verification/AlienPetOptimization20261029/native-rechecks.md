# Isolated native functional rechecks

Three complete repeats per APK of the additional failed cases and changed-step level 27. All observations are retained. These are functional replays, not additional primary FPS samples.

| Level | Original final full sweep | Baseline isolated | Final isolated | Interpretation |
|---|---|---:|---:|---|
| 10 | Failed | 3/3 | 3/3 | Both isolated sets pass; original long-run failure remains unresolved |
| 27 | Failed | 0/3 | 0/3 | Failure observed on both versions; exact causes may differ |
| 32 | Failed | 1/3 | 1/3 | Failure observed on both versions; exact causes may differ |
| 56 | Failed | 2/3 | 1/3 | Failure observed on both versions; exact causes may differ |
| 57 | Failed | 0/3 | 0/3 | Failure observed on both versions; exact causes may differ |

## First errors observed only in the final isolated cohort

A failure on both APKs does **not** rule out regression when the failing step differs. These signatures require separate interpretation; the small sequential cohort does not establish cause.

- Level 27: `System.InvalidOperationException: Across the back road onto the right shelf, clear of P2`
- Level 56: `System.InvalidOperationException: Handle C rejected: Cơ quan đang thực hiện`

Original full-sweep failures remain **10, 18, 27, 32, 48, 51, 56, 57**. These rechecks do not replace that result.

The APK with SHA-256 `26df2c98969cd38f500708e2c364e9a82dd9090bbb1407b5babdd6efe4e5fb39` was restored, verified by pulling the installed APK, and launched in normal Product mode after the rechecks.

Isolated launches reset process lifetime and input history; matching or differing outcomes do not by themselves prove optimization causality. The detailed JSON retains each repeat, tap count and first error. Player progress was not reset.

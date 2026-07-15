# Manual smoke test (Windows 11 VM) - run before every release

Automated tests cover catalog parsing, engine logic, and ViewModel behavior
against fakes. These steps exercise the real OS integration that cannot be
safely run in CI.

1. Launch `RazoWinslim.exe` - confirm exactly one UAC elevation prompt appears,
   then the main window opens.
2. Confirm `wuauserv` / Windows Update never appears anywhere in the tweak list.
3. For each Safe-tier tweak (Services, Scheduled tasks, Telemetry, Startup
   apps, Bloatware uninstall):
   - Toggle it off, confirm no error is shown.
   - Confirm the underlying OS state actually changed (service start mode /
     task enabled state / registry value / startup approval flag / Appx
     package removed, as applicable).
   - Toggle it back on (revert), confirm the OS state is restored to what it
     was before step 1.
4. For the one Advanced-tier tweak (Microsoft Defender Antivirus Service):
   - Toggle it off, confirm the second-confirmation modal appears and the
     Apply button stays disabled until the checkbox is checked.
   - Cancel once, confirm nothing changed.
   - Confirm, confirm the service is actually disabled, then revert and
     confirm it's restored to Automatic.
5. Confirm `winslim.log` recorded one line per apply/revert action performed
   above, with the correct tweak id and success/error status.
6. Kill the app mid-apply (End Task right after clicking a toggle) - relaunch,
   confirm the tweak's current state is read correctly (whatever it settled
   at) and Revert still restores the pre-toggle original.
7. Run the publish command from `README.md`, copy the resulting publish
   folder to a machine with no .NET installed, confirm it launches without
   requiring a runtime install.

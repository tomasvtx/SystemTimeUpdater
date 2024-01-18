using System.Reflection;
using System.Threading;

namespace SystemTimeUpdater.Services
    {
    public class TimeUpdate
    {
        private SemaphoreSlim UpdateSemaphore = new SemaphoreSlim(1, 1);
        public async Task UpdateTime(MainWindowViewModel mainWindowViewModel)
            {
                await UpdateSemaphore.WaitAsync( );
            try
                {
                await Task.Run(async ( ) =>
                {
                    var utc = await GetTime(mainWindowViewModel);

                    if (utc > new DateTimeOffset(2024, 1, 18, 0, 0, 0, new TimeSpan(0, 0, 0)))
                    {
                        // Aktualizace času na Windows
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            try
                            {
                                // Získání aktuálního času
                                DateTime newTime = utc.LocalDateTime;

                                // Nastavení aktuálního času
                                SystemTime systemTime = new SystemTime
                                {
                                    Year = (ushort)newTime.Year,
                                    Month = (ushort)newTime.Month,
                                    Day = (ushort)newTime.Day,
                                    Hour = (ushort)newTime.Hour,
                                    Minute = (ushort)newTime.Minute,
                                    Second = (ushort)newTime.Second,
                                };

                                var Result = SetSystemTime(ref systemTime);
                                if (Result)
                                {
                                    // Logika na základě úspěšnosti aktualizace
                                    await HandleExitCode(0, String.Empty, "Windows");
                                }
                                else
                                {
                                    await HandleExitCode(10, String.Empty, "Windows");
                                }
                            }
                            catch (Exception ex)
                            {
                                // Logika pro případ chyby při aktualizaci času
                                await HandleExitCode(10, ex.Message, "Windows");
                            }
                        }

                        // Aktualizace času na Linux
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            uint euid = geteuid();
                            if (euid != 0)
                            {
                                await Dispatcher.UIThread.InvokeAsync(async () =>
                                {
                                    string appName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

                                    var box = MessageBoxManager.GetMessageBoxStandard("UpdateTime",
                                        $"Aplikace '{appName}' nespadá pod zvýšená oprávnění (Efektivní UID: {euid}).\nPro aktualizaci je nutné spustit program jako správce.\n\nPoužijte příkaz sudo v terminálu:\n\nsudo {Process.GetCurrentProcess().MainModule.FileName}",
                                        ButtonEnum.Ok);

                                    await box.ShowWindowDialogAsync(App.Main);
                                }, DispatcherPriority.Background);
                                UpdateSemaphore.Release( );
                                return;
                            }

                            var linuxProcessStartInfo = new ProcessStartInfo
                            {
                                FileName = "sudo",
                                Arguments = $"date -s \"{utc.LocalDateTime:yyyy-MM-dd HH:mm:ss}\"",
                                RedirectStandardInput = true,
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };


                            var linuxProcess = new Process { StartInfo = linuxProcessStartInfo };
                            linuxProcess.Start();
                            await linuxProcess.WaitForExitAsync();
                            string output = await linuxProcess.StandardOutput.ReadToEndAsync();

                            var exitCode = linuxProcess.ExitCode;

                            // Logika na základě návratového kódu
                            await HandleExitCode(exitCode, output, "Linux");
                        }
                        // Aktualizace času na macOS
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            var macProcessStartInfo = new ProcessStartInfo
                            {
                                FileName = "sudo",
                                Arguments = $"date {utc.LocalDateTime:MMddHHmmyyyy.ss}",
                                RedirectStandardInput = true,
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };

                            var macProcess = new Process { StartInfo = macProcessStartInfo };
                            macProcess.Start();
                            await macProcess.WaitForExitAsync();
                            string output = await macProcess.StandardOutput.ReadToEndAsync();

                            var exitCode = macProcess.ExitCode;

                            // Logika na základě návratového kódu
                            await HandleExitCode(exitCode, output, "macOS");
                        }

                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                var box = MessageBoxManager.GetMessageBoxStandard("UpdateTime",
                                    "Aktualizace času není podporována na tomto operačním systému.", ButtonEnum.Ok);
                                await box.ShowWindowDialogAsync(App.Main);
                            }, DispatcherPriority.Background);
                        }

                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(async ( ) =>
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("UpdateTime",
                                "Chyba při získávání aktuálního času", ButtonEnum.Ok);
                            await box.ShowWindowDialogAsync(App.Main);
                        } , DispatcherPriority.Background);
                    }
                });
                }
            catch (Exception ex)
                {
                // Zpracování výjimky nebo zobrazení chybové zprávy
                await HandleException(ex);
                }
                UpdateSemaphore.Release( );
            }

        private static async Task HandleExitCode(int exitCode , string Err , string platform)
            {
            await Dispatcher.UIThread.InvokeAsync(async ( ) =>
            {
                var message = exitCode == 0
            ? $"{platform} proces byl úspěšně dokončen."
            : $"{platform} proces byl dokončen s chybou. Exit Code: {exitCode} {Err}";

                var box = MessageBoxManager.GetMessageBoxStandard("UpdateTime", message, ButtonEnum.Ok);
                await box.ShowWindowDialogAsync(App.Main);
            } , DispatcherPriority.Background);
            }

        private static async Task HandleException(Exception ex)
            {
            await Dispatcher.UIThread.InvokeAsync(async ( ) =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("UpdateTime", ex.Message, ButtonEnum.Ok);
                await box.ShowWindowDialogAsync(App.Main);
            } , DispatcherPriority.Background);
            }

        public async Task<DateTimeOffset> GetTime(MainWindowViewModel mainWindowViewModel)
        {
            await UpdateSemaphore.WaitAsync();
            if (mainWindowViewModel.SelectedNtpServers?.IPAddress is not null)
                {
                try
                    {
                    return await Task.Run(async ( ) =>
                    {
                        // Získání UTC času z NTP serveru
                        var client = new GuerrillaNtp.NtpClient(mainWindowViewModel.SelectedNtpServers?.IPAddress);
                        var clock = await client.QueryAsync();
                        var local = clock.Now;
                        var utc = clock.UtcNow;

                        mainWindowViewModel.SyncError = "Bez chyby";

                        UpdateSemaphore.Release();

                        return utc > DateTimeOffset.MinValue ? utc : DateTimeOffset.MinValue;
                    });
                    }
                catch (Exception ss)
                    {
                    mainWindowViewModel.SyncError = "chyba " + ss.Message;
                    UpdateSemaphore.Release( );

                    return DateTimeOffset.MinValue;
                    }
                }
            else
                {
                mainWindowViewModel.SyncError = "Prosím zvolte server";
                UpdateSemaphore.Release( );

                return DateTimeOffset.MinValue;
                }
            }

        [DllImport("libc" , SetLastError = true)]
        private static extern uint geteuid( );

        // Definice struktury pro SystemTime
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
            {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Millisecond;
            }

        // Deklarace metody SetSystemTime ze kernel32.dll
        [DllImport("kernel32.dll" , SetLastError = true)]
        public static extern bool SetSystemTime(ref SystemTime systemTime);
        }
    }

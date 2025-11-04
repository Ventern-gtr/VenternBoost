using SharpMonoInjector;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            AllocConsole();
            HideWindow();
            Console.Title = "Ventern Boost";
            if (!IsRunAsAdministrator())
            {
                ShowWindow();
                PrintLine("Please run this program as Administrator.", ConsoleColor.Red);
                PauseExit();
                return;
            }

            byte[] dllBytes;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VenternBoost.Assets.VenternBoost.dll"))
            {
                if (stream == null)
                {
                    ShowWindow();
                    PrintLine("Embedded DLL not found", ConsoleColor.Red);
                    PauseExit();
                }

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    dllBytes = ms.ToArray();
                }
            }
            Thread.Sleep(1000);
            Inject(dllBytes);
        }
        catch (Exception ex)
        {
            ShowWindow();
            PrintLine($"An error occurred: {ex.Message}", ConsoleColor.Red);
            PauseExit();
        }
    }

    private static void Inject(byte[] dllBytes)
    {
        try
        {
            Injector injector = new Injector("Gorilla Tag");
            if (injector != null)
            {
                IntPtr injectionCode = injector.Inject(dllBytes, "Loader", "Loader", "Load");
                injector.Dispose();
                if (injectionCode != 0)
                {
                    PrintLine($"Injection Complete : {injectionCode}", ConsoleColor.Green, 5);
                    Console.Beep(100, 200);
                    CountdownExit(3);
                }
                else
                {
                    ShowWindow();
                    PrintLine($"Injection failed : {injectionCode}", ConsoleColor.Red, 5);
                    PauseExit();
                }
            }

        }
        catch (Exception ex)
        {
            ShowWindow();
            PrintLine($"Injection failed | Message:{ex.Message} Expection: {ex.InnerException}", ConsoleColor.Red);
            PauseExit();
        }
    }

    #region Imports
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;


    private static void HideWindow()
    {
        IntPtr console = GetConsoleWindow();
        ShowWindow(console, SW_HIDE);
    }
    private static void ShowWindow()
    {
        IntPtr console = GetConsoleWindow();
        ShowWindow(console, SW_SHOW);
    }
    #endregion

    #region Useful Shit
    private static bool IsRunAsAdministrator()
    {
        try
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    private static void PrintLine(string message, ConsoleColor color, int typingDelay = 3, bool noline = false)
    {
        try
        {
            Console.ForegroundColor = color;
            foreach (char c in message)
            {
                Console.Write(c);
                Thread.Sleep(typingDelay);
            }
            Console.ResetColor();

            if (!noline)
                Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.ResetColor();
            Console.WriteLine($"Error printing message: {ex.Message}");
        }
    }

    private static void CountdownExit(int seconds = 3)
    {
        try
        {
            for (int i = seconds; i > 0; i--)
            {
                PrintLine($"Closing in {i}...", ConsoleColor.Red, 10);
                Thread.Sleep(1000);
            }
        }
        catch (Exception ex)
        {
            PrintLine($"Error during countdown: {ex.Message}", ConsoleColor.Red);
        }
    }

    private static void PauseExit()
    {
        try
        {
            PrintLine("", ConsoleColor.Red);
            PrintLine("Press any key to close...", ConsoleColor.Red, 10);
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            PrintLine($"Error while pausing for exit: {ex.Message}", ConsoleColor.Red);
        }
    }

    private static int GetProcessPIDByName(string processName)
    {
        try
        {
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName.ToLower() == processName.ToLower())
                {
                    PrintLine($"Found process with name '{processName}' found. PID: {process.Id}", ConsoleColor.Green);
                    return process.Id;
                }
            }
            PrintLine($"No process with name '{processName}' found. Checking again in 15 seconds...", ConsoleColor.Yellow);
            return 0;
        }
        catch (Exception ex)
        {
            PrintLine($"An error occurred: {ex.Message}", ConsoleColor.Yellow);
            return 0;
        }
    }
    #endregion
}
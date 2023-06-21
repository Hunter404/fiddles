using System.Runtime.InteropServices;

/// <summary>
/// Enables ANSI 256 color mode for windows console, works in rider terminal as well.
/// </summary>
public class ConsoleSettings
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    private const int STD_OUTPUT_HANDLE = -11;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

    /// <summary>
    /// Call this once, normally in Program.Main
    /// </summary>
    /// <returns>Success</returns>
    public bool SetupConsole()
    {
        var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
        if (!GetConsoleMode(iStdOut, out var outConsoleMode))
        {
            Console.WriteLine("failed to get output console mode");
            return false;
        }

        outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
        if (!SetConsoleMode(iStdOut, outConsoleMode))
        {
            Console.WriteLine($"failed to set output console mode, error code: {Marshal.GetLastWin32Error()}");
            return false;
        }

        return true;
    }
}

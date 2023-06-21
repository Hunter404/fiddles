using Console = System.Console;

/// <summary>
/// Thread safe, asynchronous and concurrent implementation of the C# console.
/// See ConsoleSettings.cs on how to enable ANSI 256 color mode.
/// </summary>
public class ConcurrentConsole
{
    private readonly char[] _inputBuffer = new char[4096];
    private readonly SemaphoreSlim _lock = new (1, 1);
    
    private int _inputPosition = 0;
    private int _currentPosition = 0;

    /// <summary>
    /// Writes a line to console.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="cancellationToken"></param>
    public async Task WriteLineAsync(string line, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            Console.CursorLeft = 0;
            Console.Write(new string(' ', _inputPosition + 3));

            Console.SetCursorPosition(0, _currentPosition);
    
            Console.WriteLine(line);
    
            _currentPosition++;
    
            Console.SetCursorPosition(0, _currentPosition + 3);
            Console.Write("> ");
            Console.Write(new string(_inputBuffer, 0, _inputPosition));
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Reads a line from console.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> ReadLineAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!Console.KeyAvailable)
            {
                await Task.Delay(1, cancellationToken); // Release thread if no key is present.
                continue;
            }
            
            
            await _lock.WaitAsync(cancellationToken);
            try
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace) // Key is backspace
                {
                    if (_inputPosition <= 0)
                    {
                        continue;
                    }

                    Console.CursorLeft--;
                    Console.Write(' ');
                    Console.CursorLeft--;
                    _inputPosition--;

                    continue;
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                
                _inputBuffer[_inputPosition] = key.KeyChar;
                _inputPosition++;
        
                Console.Write(key.KeyChar);
            }
            finally
            {
                _lock.Release();
            }
        }

        await _lock.WaitAsync(cancellationToken);

        try
        {
            Console.CursorLeft = 0;
            Console.Write(new string(' ', _inputPosition + 2));
            Console.SetCursorPosition(0, _currentPosition + 3);
    
            var result = new string(_inputBuffer, 0, _inputPosition);
            _inputPosition = 0;

            return result;
        }
        finally
        {
            _lock.Release();
        }
    }
}

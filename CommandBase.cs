using System;
using System.Windows.Input;

/// <summary>
/// Boilerplate ICommand implementation
/// </summary>
public class CommandBase : ICommand
{
    /// <summary>
    /// Callback for when command is executed
    /// </summary>
    public delegate void ExecuteMethod(object? parameter = null);

    /// <summary>
    /// Callback for when checking if command can execute
    /// </summary>
    public delegate bool CanExecuteMethod(object? parameter = null);

    private readonly ExecuteMethod _execute;
    private readonly CanExecuteMethod? _canExecute;

    /// <summary>
    /// Carbon implementation of ICommand with basic callbacks
    /// </summary>
    /// <param name="execute">Callback when executed</param>
    /// <param name="canExecute">Callback when checking if command can execute</param>
    public CommandBase(ExecuteMethod execute, CanExecuteMethod? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <summary>
    /// Calls can execute callback and checks if button is clickable or not.
    /// </summary>
    /// <param name="parameter">Leave as null</param>
    /// <returns></returns>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() != false;

    /// <summary>
    /// Calls callback when executed.
    /// </summary>
    /// <param name="parameter">Leave as null</param>
    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    /// <summary>
    ///
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}

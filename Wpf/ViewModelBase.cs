using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
/// This is a base ViewModel class that implements the INotifyPropertyChanged interface.
/// This interface is a key part of the Model-View-ViewModel (MVVM) design pattern that is often used in .NET applications,
/// especially ones that have a graphical user interface.
/// </summary>
public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Returns false if stored value equals new value
    /// </summary>
    /// <param name="storage">Field to update</param>
    /// <param name="value">Value to update field with</param>
    /// <param name="propertyName">Leave it</param>
    /// <typeparam name="T">Field type</typeparam>
    /// <returns></returns>
    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (storage != null && storage.Equals(value)) return false;

        storage = value;

        NotifyPropertyChanged(propertyName);

        return true;
    }

    protected void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

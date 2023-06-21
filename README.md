# fiddles
Fiddles used personally or professionally

## Console
### ConcurrentConsole.cs
Basic implementation of a multi-threaded async console manager.

### ConsoleSettings.cs
Enables ANSI 256 color mode for windows console.

## Data
### CircularBufferStream.cs
The CircularBufferStream class is a custom implementation of a Stream using a circular buffer, which is a data structure that uses a single, fixed-size buffer as if it were connected end-to-end.
This kind of data structure is very useful for situations where you want a fixed-sized buffer and old data can be automatically overwritten when the buffer is full.

### ScatterRegister.cs
A class for aggregating reads and writes to a scatter register.

## Helpers
### ConcurrentAsyncEventHandler.cs
The ConcurrentAsyncEventHandler<T> class is an implementation of an event handler that can handle asynchronous operations.

Addition Operators +: Two + operator overloads are provided to add either a callback (Func<object, T, Task>) or another AsyncEventHandler<T> instance to the event handler. If the event handler e is null, it's initialized with a new instance. The callback or other handler is then added to the appropriate set.

Subtraction Operators -: Two - operator overloads are provided to remove either a callback (Func<object, T, Task>) or another AsyncEventHandler<T> instance from the event handler. If the event handler e or the callback/other handler is null, no operation is performed and e is returned as is. Otherwise, the callback or other handler is removed from the appropriate set.

### ParallelTest.cs
Example of running asynchronous and synchronising steps.

## Wpf
### CommandBase.cs
This is a base Command class that implements the ICommand interface, which is a key part of the Model-View-ViewModel (MVVM) design pattern used in WPF, UWP, Avalonia and Xamarin.Forms applications.

### ViewModelBase.cs
This is a base ViewModel class that implements the INotifyPropertyChanged interface. This interface is a key part of the Model-View-ViewModel (MVVM) design pattern that is often used in .NET applications, especially ones that have a graphical user interface.
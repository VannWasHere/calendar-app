namespace RoomBook.Components.UI;

public sealed class ToastService
{
    public event Action<ToastMessage>? OnShow;
    public void Success(string message) => OnShow?.Invoke(new(message, "success"));
    public void Error(string message) => OnShow?.Invoke(new(message, "error"));
    public void Info(string message) => OnShow?.Invoke(new(message, "info"));
}

public sealed record ToastMessage(string Text, string Tone);

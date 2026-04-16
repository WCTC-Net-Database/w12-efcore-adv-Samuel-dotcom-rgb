namespace ConsoleRpg.Helpers;

public class OutputManager
{
    private readonly List<(string message, ConsoleColor color)> _outputBuffer; // A list of messages with associated colors

    public OutputManager()
    {
        _outputBuffer = new List<(string message, ConsoleColor color)>();
    }

    public void Clear()
    {
        Console.Clear();
        _outputBuffer.Clear();
    }

    public void Display()
    {
        foreach (var (message, color) in _outputBuffer)
        {
            WriteColorToConsole(message, color); // Write stored messages with color
        }

        _outputBuffer.Clear(); // Clear the buffer after displaying
    }

    public void Write(string message, ConsoleColor color = ConsoleColor.White)
    {
        _outputBuffer.Add((message, color));
    }

    public void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
    {
        _outputBuffer.Add((message + Environment.NewLine, color));
    }

    private void WriteColorToConsole(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color; // Set the text color
        Console.Write(message); // Write the message to the console
        Console.ResetColor(); // Reset the console color back to default
    }
}

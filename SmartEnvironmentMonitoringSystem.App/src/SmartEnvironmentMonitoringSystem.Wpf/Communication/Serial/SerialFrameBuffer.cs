using System.Text;

namespace SmartEnvironmentMonitoringSystem.Wpf.Communication.Serial;

public sealed class SerialFrameBuffer
{
    private readonly StringBuilder buffer = new();

    public IReadOnlyList<string> Append(string text)
    {
        var frames = new List<string>();
        buffer.Append(text);

        while (true)
        {
            var current = buffer.ToString();
            var newlineIndex = current.IndexOfAny(['\r', '\n']);
            if (newlineIndex < 0)
            {
                break;
            }

            var frame = current[..newlineIndex].Trim();
            buffer.Remove(0, newlineIndex + 1);

            while (buffer.Length > 0 && (buffer[0] == '\r' || buffer[0] == '\n'))
            {
                buffer.Remove(0, 1);
            }

            if (!string.IsNullOrWhiteSpace(frame))
            {
                frames.Add(frame);
            }
        }

        return frames;
    }
}

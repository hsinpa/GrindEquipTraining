using System;
using System.Threading.Tasks;
using TMPro;

namespace Hsinpa
{
    public class TextCountdown 
    {
        public static async Task Countdown(TextMeshProUGUI source, string content, int durationSeconds)
        {
            for (int remaining = Math.Max(0, durationSeconds);
                remaining >= 0;
                remaining--)
            {
                source.text = string.Format(content, remaining);

                if (remaining > 0)
                    await Task.Delay(1000);
            }
        }
    }
}
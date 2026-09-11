using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace Hsinpa.Utility
{
    public class UtilityFunc : MonoBehaviour
    {

		public static async Task DoDelayWork(float p_delay, System.Action p_action)
		{
			await DoDelayWork(p_delay, p_action, CancellationToken.None);
		}

		public static async Task DoDelayWork(float p_delay, System.Action p_action, CancellationToken token)
		{
			try
			{
				await Task.Delay(System.TimeSpan.FromSeconds(p_delay), token);

				if (p_action != null)
					p_action();
			}
			catch (System.Exception)
            {

			}
		}
    }    
}

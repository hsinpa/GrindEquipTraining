using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hsinpa
{
    public class LastSceneUI : MonoBehaviour
    {
        [SerializeField]
        private Sprite check_icon;

        [SerializeField]
        private Sprite cross_icon;

        [SerializeField]
        private Image table_dust_status;

        [SerializeField]
        private Image table_mask_status;

        [SerializeField]
        private Image table_step_status;

        private Dictionary<TaskStatus, bool> statusDict = new Dictionary<TaskStatus, bool>();

        public void SetUp()
        {
            statusDict.Clear();
            Utility.SimpleEventSystem.CustomEventListener -= OnCustomEvent;
            Utility.SimpleEventSystem.CustomEventListener += OnCustomEvent;
        }

        public void ShowResult()
        {
            table_dust_status.sprite = (statusDict.ContainsKey(TaskStatus.Dusk) && statusDict[TaskStatus.Dusk]) ? check_icon : cross_icon;
            table_mask_status.sprite = (statusDict.ContainsKey(TaskStatus.Mask) && statusDict[TaskStatus.Mask]) ? check_icon : cross_icon;
            table_step_status.sprite = (statusDict.ContainsKey(TaskStatus.Step) && statusDict[TaskStatus.Step]) ? check_icon : cross_icon;
        }

        private void OnCustomEvent(string eventName, object[] parameters)
        {
            if (eventName == StaticEvent.TaskStatusEvent && parameters.Length == 2)
            {
                TaskStatus taskStatusEnum = (TaskStatus)parameters[0];
                bool taskStatusResult = (bool)parameters[1];

                if (!statusDict.ContainsKey(taskStatusEnum))
                {
                    statusDict.Add(taskStatusEnum, taskStatusResult);
                }
            }
        }
    }
}
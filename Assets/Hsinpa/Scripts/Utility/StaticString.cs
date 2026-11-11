using System.Collections.Generic;

public class StaticString
{
    public const string SCENE0TITLE = "體驗前注意事項";
    public const string SCENE0NOTICE = @"1. 體驗過程需佩戴 VR 眼鏡，如有不適(噁心嘔吐等)請立即告知工作人員。
2.請學員評估自身狀況，若有以下生理狀況不得體驗：
(1)懷孕    (2)心臟病    (3)高血壓    (4)身體不適";

    public const string SCENE1TITLE = "任務提示";
    public const string SCENE1NOTICE = "進入作業區前，請先完成危害辨識。";

    public const string SCENE2NOTICETITLE = "找出粉塵來源";
    public const string SCENE2NOTICE = "請仔細觀察四周，找出可能產生粉塵的位置。找到後用手把點選它";
    public const string SCENE2COMPLETETITLE = "確認完成";
    public const string SCENE2COMPLETENOTICE = "很好，粉塵來源已全數確認";

    public const string SCENE4TITLE = "選用提示";
    public const string SCENE4NOTICE = "請依照這次研磨作業的粉塵風險，從架上選擇適合的呼吸防護具。";

    public const string SCENE4MISTAKE = "防護不足，無法有效降低粉塵吸入風險。";    
    public const string SCENE4CORRECT = "選擇正確。這款防護具能有效阻隔研磨粉塵，接下來請完成配戴。";      
  
    public const string SCENE4COMPLETE = "配戴完成, 請回到研磨區域";

    public const string SCENE6NOTICE = "致災原因： N95 口罩未確實密合，導致火災濃煙中的懸浮微粒經由口罩與臉部間的縫隙進入呼吸道，造成煙霧吸入危害。";
    public const string SCENE6BUTTON = "再試一次";

    public const string SCENE7NOSETITLE = "密合檢點";
    public const string SCENE7NOSENOTICE= "完成正壓／負壓密合檢點";

    public const string SCENE7MOUTHTITLE = "最後一道防線";
    public const string SCENE7MOUTHNOTICE = "面罩防護具是最後防線，研磨前務必以正確姿勢配戴";

    public const string N95_TITLE = "N95";
    public const string N95_NOTICE = "可過濾非油性粉塵及細微粒子，適合一般金屬乾式研磨";

    public const string HALF_MASK_NOTICE = "氣體/蒸氣防護，因未搭配濾棉，無法有效過濾粉塵";
    public const string HALF_MASK_TITLE = "半面式呼吸防護具（搭配濾毒罐）";

    public const string NORMAL_MASK_NOTICE = "一般異味防護，無法提供足夠的粉塵防護";
    public const string NORMAL_MASK_TITLE = "活性碳口罩";

    public const string SAFE_GLASS_MASK_NOTICE = "可降低粉塵與飛散碎屑傷眼風險";
    public const string SAFE_GLASS_MASK_TITLE = "安全護目鏡";

    public const string GRIND_TOOL_NOTICE = "請拿取工具進行研磨";
    public const string GRIND_TOOL_RENOTICE = "請再次拿取工具進行研磨";
    public const string MASK_CHECK_NOTICE = "點擊確認口罩密合點";


    public static Dictionary<string, string> GlobalStrDict = new Dictionary<string, string>()   
    {
        {"0_system", "系統"},
        {"1_system", "System"},
        {"0_manager", "主管"},
        {"1_manager", "Manager"},
    };
}

public class StaticEvent
{
    public const string TriggerEvent = "Trigger_Event";
    public const string TaskStatusEvent = "Task_Status_Event";
}

public class GestureState
{
    public const string GRAB_STICK_EVENT = "bThumbsy";
    public const string GRAB_MASK_EVENT = "bThumbsy";
    public const string POINT_EVENT = "bComeHere";
    public const string DEFAULT = "default";
}

public class HandState
{
    public const string RIGHTHAND = "RIGHT_HAND";
    public const string LEFTHAND = "LEFT_HAND";
}

public class Audio
{
    public const string TAG = "UNIVERSAL";
    public const string WRONG_CHOICE = "WRONG_CHOICE";
    public const string UI_CLICK = "UI_CLICK";
    public const string WEAR = "WEAR";
    public const string COMPLETE = "COMPLETE";
}

public enum TaskStatus
{
    Dusk, Mask, Step
}



public class StaticString
{
    public const string SCENE0TITLE = "體驗前注意事項";
    public const string SCENE0NOTICE = @"1. 體驗過程需佩戴 VR 眼鏡，如有不適(噁心嘔吐等)請立即告知工作人員。
2.請學員評估自身狀況，若有以下生理狀況不得體驗：
(1)懷孕    (2)心臟病    (3)高血壓    (4)身體不適";

    public const string SCENE1TITLE = "任務提示";
    public const string SCENE1NOTICE = "進入作業區前，請先完成危害辨識。";

    public const string SCENE2NOTICETITLE = "找出粉塵來源";
    public const string SCENE2NOTICE = "請仔細觀察四周，找出可能產生粉塵的位置——研磨點、工作台、地面堆積處，都有可能。找到後用手把點選它";
    public const string SCENE2COMPLETETITLE = "確認完成";
    public const string SCENE2COMPLETENOTICE = "很好，粉塵來源已全數確認";

    public const string SCENE4TITLE = "選用提示";
    public const string SCENE4NOTICE = "請依照這次研磨作業的粉塵風險，從架上選擇適合的呼吸防護具。";

    public const string SCENE4MISTAKE = "防護不足，無法有效降低粉塵吸入風險。";    
    public const string SCENE4CORRECT = "選擇正確。這款防護具能有效阻隔研磨粉塵，接下來請完成配戴。";      
  
    public const string SCENE4COMPLETE = "配戴完成, 請回到研磨區域";

    public const string SCENE6NOTICE = "呼吸防護具應依作業危害正確選用。選對防護具、依規定佩戴，並於每次使用前完成密合檢點";
    public const string SCENE6BUTTON = "再試一次";

    public const string SCENE7NOSETITLE = "密合檢點";
    public const string SCENE7NOSENOTICE= "完成正壓／負壓密合檢點";

    public const string SCENE7MOUTHTITLE = "最後一道防線";
    public const string SCENE7MOUTHNOTICE = "防護具是最後防線，研磨前務必確認局部排氣有效";

    public const string N95_TITLE = "NP5";
    public const string N95_NOTICE = "可過濾非油性粉塵及細微粒子，適合一般金屬乾式研磨";

    public const string HALF_MASK_NOTICE = "面罩需搭配適當的粒狀物濾材，才能防護研磨粉塵";
    public const string HALF_MASK_TITLE = "半面罩(未裝濾材)";

    public const string NORMAL_MASK_NOTICE = "主要阻隔飛沫，無法提供研磨粉塵所需的密合防護";
    public const string NORMAL_MASK_TITLE = "一般醫療口罩";

}

public class StaticEvent
{
    public const string TriggerEvent = "Trigger_Event";
}

public class GestureState
{
    public const string GRAB_STICK_EVENT = "bThumbsy";
    public const string GRAB_MASK_EVENT = "bThumbsy";
    public const string POINT_EVENT = "bComeHere";
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
}
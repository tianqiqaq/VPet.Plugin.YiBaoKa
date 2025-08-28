using System.Runtime.InteropServices.JavaScript;
using System.Windows;
using System.Windows.Controls;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.YiBaoKa;

public class YiBaoKa: MainPlugin
{
    public override string PluginName => "医保卡";

    public const int ValidDuring = 7;

    public DateTime ValidityPeriod
    {
        get
        {
            DateTime validityPeriod = MW.GameSavesData.GetDateTime("YiBaoKa.ValidityPeriod", DateTime.MinValue);
            return validityPeriod;
        }
        set
        {
            MW.GameSavesData.SetDateTime("YiBaoKa.ValidityPeriod", value);
            // if (!MW.DynamicResources.TryAdd("YiBaoKa.ValidityPeriod", value))
            // {
            //     MW.DynamicResources["YiBaoKa.ValidityPeriod"] = value;
            // }
        }
    }
    /// <summary>
    /// 医保卡是否过期
    /// </summary>
    public bool IsMedicalInsuranceCardValid
    {
        get
        {
            if (ValidityPeriod >= DateTime.Now)
            { // 医保卡效果未过期
                return true;
            }
            return false;
        }
    }

    public YiBaoKa(IMainWindow mainwin) : base(mainwin)
    {
        
    }

    public override void LoadPlugin()
    {
        MW.Event_TakeItem += TakeItem;
        
        MenuItem rootMenu = MW.Main.ToolBar.MenuFeed;
        // modset.Visibility = Visibility.Visible;
        var mainMenuItem = new MenuItem()
        {
            Header = "医保卡",
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };
        // 查看医保卡余额
        var balanceMenuItem = new MenuItem()
        {
            Header = "状态",
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };
        balanceMenuItem.Click += (s, e) =>
        {
            MW.Main.Say(String.Format("让我看看... 主人，医保卡的有效期限到{0:D} {0:t}，现在{1}", 
                ValidityPeriod, IsMedicalInsuranceCardValid? "还没有过期喵！" : "已经过期啦喵。。"));
        };
        mainMenuItem.Items.Add(balanceMenuItem);
        
        // 立即注销医保卡
        var cancelMenuItem = new MenuItem()
        {
            Header = "注销",
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };
        cancelMenuItem.Click += (s, e) =>
        {
            var timeSpan = ValidityPeriod.Subtract(DateTime.Now);
            if (timeSpan.TotalDays > 0)
            {
                var result = MessageBoxX.Show($"确定要注销医保卡吗喵？主人，我的医保卡还剩余 {timeSpan.Days} 天喵~", "注销医保卡",  MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    ValidityPeriod = DateTime.Now;
                    MW.Main.Say(IsMedicalInsuranceCardValid ? "注销失败了喵。。" : "注销成功啦喵！");
                }
            }
            else
            {
                MW.Main.Say("医保卡无效，无法注销喵~");
            }
            
        };
        mainMenuItem.Items.Add(cancelMenuItem);
        
        // modset.Items.Add(mainMenuItem);
        rootMenu.Items.Add(mainMenuItem);
    }

    public void TakeItem(Food food)
    {
        if (food.Name.Equals("医保卡"))
        {
            // 应该为一个月，DEBUG时设置为60秒
            if (!IsMedicalInsuranceCardValid)
            { // 若医保卡失效，则从今天开始加七天有效期
                ValidityPeriod = DateTime.Now + TimeSpan.FromDays(ValidDuring);
            }
            else
            { // 若医保卡有效，则在有效期上再加七天有效期
                ValidityPeriod += TimeSpan.FromDays(ValidDuring);
            }
        }
        else
        { // 若买的不是医保卡，才减免！
            if (IsMedicalInsuranceCardValid && food.Type.Equals(Food.FoodType.Drug))
            {
                double discountedPrice = food.Price * 0.8;
                MW.Core.Save.Money += discountedPrice;
            
                MW.Main.Say(String.Format("当当~ 我有医保卡，减免了 {0:C2} 元！§(*￣▽￣*)§".Translate(), discountedPrice));
                // MessageBoxX.Show();
            }
            else
            {
                // MessageBoxX.Show(String.Format("{0}: {1}, {2}\nIsValid: {3}", 
                //    food.Name, food.Type, food.Price, IsMedicalInsuranceCardValid));
            }
        }
    }
}
using GeekDesk.Constant;
using GeekDesk.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeekDesk.Util
{
    public static class FrequentStats
    {
        public static List<IconInfo> GetTopIcons(AppData data, int topN)
        {
            List<IconInfo> all = new List<IconInfo>();
            foreach (MenuInfo menu in data.MenuList)
            {
                if (menu.MenuType == MenuType.FREQUENT) continue;
                foreach (IconInfo icon in menu.IconList)
                {
                    all.Add(icon);
                }
            }
            return all
                .OrderByDescending(i => i.Count)
                .ThenBy(i => i.Name)
                .Take(Math.Max(0, topN))
                .ToList();
        }


        public static void Refresh(AppData data)
        {
            try
            {
                data.EnsureFrequentMenu();
                List<IconInfo> top = GetTopIcons(data, data.AppConfig.FrequentIconCount);
                var target = data.FrequentMenu.IconList;

                for (int i = target.Count - 1; i >= 0; i--)
                {
                    if (!top.Contains(target[i]))
                    {
                        target.RemoveAt(i);
                    }
                }
                foreach (IconInfo icon in top)
                {
                    if (!target.Contains(icon))
                    {
                        target.Add(icon);
                    }
                }

                var sorted = top.Where(i => target.Contains(i)).ToList();
                for (int i = 0; i < sorted.Count && i < target.Count; i++)
                {
                    if (!ReferenceEquals(target[i], sorted[i]))
                    {
                        int oldIdx = target.IndexOf(sorted[i]);
                        if (oldIdx >= 0)
                        {
                            target.Move(oldIdx, i);
                        }
                    }
                }
            }
            catch (Exception) { }
        }
    }
}

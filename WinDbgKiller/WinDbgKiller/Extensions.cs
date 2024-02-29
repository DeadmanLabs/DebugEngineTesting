using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace WinDbgKiller.Extensions
{
    public static class ControlExtensions
    {
        public static void SafeOperation(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }

    public static class ListViewExtensions
    {
        public static void UpdateOrAdd(this ListView listView, string key, string value)
        {
            foreach (ListViewItem item in listView.Items)
            {
                if (item.Text == key)
                {
                    item.SubItems[1].Text = value;
                    return;
                }
            }
            ListViewItem lvi = new ListViewItem(key);
            lvi.SubItems.Add(value);
            listView.Items.Add(lvi);
        }

        public static void UpdateOrAdd(this ListView listView, string key, String[] collection)
        {
            ListViewItem item = listView.Items.OfType<ListViewItem>().FirstOrDefault(i => i.Text == key);
            if (item != null)
            {
                for (int i = 0; i < collection.Length; i++)
                {
                    if (i < item.SubItems.Count - 1)
                    {
                        item.SubItems[i + 1].Text = collection[i];
                    }
                    else
                    {
                        item.SubItems.Add(collection[i]);
                    }
                }
                return;
            }
            item = new ListViewItem(key);
            foreach (string subitem in collection)
            {
                item.SubItems.Add(subitem);
            }
            listView.Items.Add(item);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit;
using MyFTP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using muxc = Microsoft.UI.Xaml.Controls;

namespace MyFTP.Utils
{
    public interface IDragTarget
    {
        string Name { get; }
    }
    public class DragHelper
    {
		public static string DragItemsFormatId { get; } = "DragItemsFormatId";
		public static readonly DependencyProperty IsDragItemsEnabledProperty = DependencyProperty.RegisterAttached("IsDragItemsEnabled", typeof(bool), typeof(DragAndDropHelper), new PropertyMetadata(false));
		public static readonly DependencyProperty DragOverBackgroundProperty = DependencyProperty.RegisterAttached("DragOverBackground", typeof(Brush), typeof(DragAndDropHelper), new PropertyMetadata(null));
		public static readonly DependencyProperty DragLeaveBackgroundProperty = DependencyProperty.RegisterAttached("DragLeaveBackground", typeof(Brush), typeof(DragAndDropHelper), new PropertyMetadata(null));
		public static bool GetIsDragItemsEnabled(UIElement element) => (bool)element.GetValue(IsDragItemsEnabledProperty);
		public static void SetIsDragItemsEnabled(UIElement element, bool value)
		{
			element.SetValue(IsDragItemsEnabledProperty, value);
			switch (element)
			{
				case ListViewBase lvb:
					lvb.DragItemsStarting -= OnListviewDragItemsStarting;
					lvb.DragItemsCompleted -= OnListviewDragItemsCompleted;
					lvb.CanDragItems = value;
					if (value)
					{
						lvb.DragItemsStarting += OnListviewDragItemsStarting;
						lvb.DragItemsCompleted += OnListviewDragItemsCompleted;
					}
					break;

				case muxc.TreeView tv when value:
					tv.DragItemsStarting -= OnTreeviewDragItemsStarting;
					tv.DragItemsCompleted -= OnTreeviewDragItemsCompleted;
					tv.CanDragItems = value;
					if (value)
					{
						tv.DragItemsStarting += OnTreeviewDragItemsStarting;
						tv.DragItemsCompleted += OnTreeviewDragItemsCompleted;
					}
					break;
			}
		}
		private static void OnListviewDragItemsStarting(object sender, DragItemsStartingEventArgs args)
		{
			// Need to contains IDragTarget
			args.Cancel = !args.Items.Any(x => x is IDragTarget);
			args.Data.Properties.Add(DragItemsFormatId, args.Items);
			args.Data.SetData(DragItemsFormatId, DragItemsFormatId);
		}
		private static void OnTreeviewDragItemsStarting(muxc.TreeView sender, muxc.TreeViewDragItemsStartingEventArgs args)
		{
			// Need to contains IDragTarget
			args.Cancel = !args.Items.Any(x => x is IDragTarget);
			args.Data.Properties.Add(DragItemsFormatId, args.Items);
			args.Data.SetData(DragItemsFormatId, DragItemsFormatId);
		}
		private static void OnListviewDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args) { }
		private static void OnTreeviewDragItemsCompleted(muxc.TreeView sender, muxc.TreeViewDragItemsCompletedEventArgs args) { }
		public static Brush GetDragOverBackground(Panel panel) => (Brush)panel.GetValue(DragOverBackgroundProperty);
		public static void SetDragOverBackground(Panel panel, Brush value) => panel.SetValue(DragOverBackgroundProperty, value);
		public static Brush GetDragLeaveBackground(Panel panel) => (Brush)panel.GetValue(DragLeaveBackgroundProperty);
		public static void SetDragLeaveBackground(Panel panel, Brush value) => panel.SetValue(DragLeaveBackgroundProperty, value);
	}
}

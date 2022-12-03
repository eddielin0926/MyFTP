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
	public class DragAndDropHelper
	{
		#region enabled drag items from app		
		
		#endregion

		#region enabled drop from app and system
		public static bool GetIsDropItemsEnabled(UIElement element) => (bool)element.GetValue(IsDropItemsEnabledProperty);
		public static void SetIsDropItemsEnabled(UIElement element, bool value)
		{
			element.SetValue(IsDropItemsEnabledProperty, value);
			element.DragOver -= OnElementDragEnter;
			element.DragLeave -= OnElementLeave;
			element.Drop -= OnElementDrop;
			if (value)
			{
				element.AllowDrop = true;
				element.DragOver += OnElementDragEnter;
				element.DragLeave += OnElementLeave;
				element.Drop += OnElementDrop;
			}
			else
			{
				element.AllowDrop = false;
			}
		}

		private static void OnElementDragEnter(object sender, DragEventArgs args)
		{
			var element = (UIElement)sender;
			var target = GetDropTarget(element) as IDropTarget;
			if (target == null)
				return;

			if (args.DataView.Contains(DragHelper.DragItemsFormatId)
							&& args.DataView.Properties.TryGetValue(DragHelper.DragItemsFormatId, out var value)
							&& value is IList<object> list) // dragging items from app
			{
				args.AcceptedOperation = DataPackageOperation.Move;
				var message = GetLocalized("MoveItemTo");
				args.DragUIOverride.Caption = string.Format("{0} {1}", message, target.Name.Truncate(80, true));
				if (element is Panel panel && DragHelper.GetDragOverBackground(panel) is Brush brush)
				{
					panel.Background = brush;
				}
			}

			else if (args.DataView.Contains(StandardDataFormats.StorageItems)) // dragging files from system
			{
				args.AcceptedOperation = DataPackageOperation.Copy;
				var message = GetLocalized("UploadItemTo");
				args.DragUIOverride.Caption = string.Format("{0} {1}", message, target.Name.Truncate(80, true));
				if (element is Panel panel && DragHelper.GetDragOverBackground(panel) is Brush brush)
				{
					panel.Background = brush;
				}
			}
			else
				args.AcceptedOperation = DataPackageOperation.None;
		}

		private static void OnElementLeave(object sender, DragEventArgs e)
		{
			if (sender is Panel panel && DragHelper.GetDragOverBackground(panel) is Brush brush)
			{
				RevertBrush(panel);
			}
		}
		private async static void OnElementDrop(object sender, DragEventArgs args)
		{
			var element = (UIElement)sender;
			var target = GetDropTarget(element) as IDropTarget;
			if (target == null)
				return;
			if (args.DataView.Contains(DragHelper.DragItemsFormatId)
							&& args.DataView.Properties.TryGetValue(DragHelper.DragItemsFormatId, out var value)
							&& value is IList<object> list) // dragging items from app
			{
				var items = list.OfType<IDragTarget>().Where(item => target.IsDragItemSupported(item));
				target.DropItems(items);
			}
			else if (args.DataView.Contains(StandardDataFormats.StorageItems)) // dragging files from system
			{
				var items = await args.DataView.GetStorageItemsAsync();
				target.DropItems(items);
			}
			if (sender is Panel panel && DragHelper.GetDragOverBackground(panel) is Brush brush)
			{
				RevertBrush(panel);
			}
		}

		private static void RevertBrush(Panel panel)
		{
			Brush brush = DragHelper.GetDragLeaveBackground(panel);
			panel.Background = brush;
		}

		public static readonly DependencyProperty IsDropItemsEnabledProperty = DependencyProperty.RegisterAttached("IsDropItemsEnabled", typeof(bool), typeof(DragAndDropHelper), new PropertyMetadata(false));
		public static IDropTarget GetDropTarget(UIElement obj) => (IDropTarget)obj.GetValue(DropTargetProperty);
		public static void SetDropTarget(UIElement obj, IDropTarget value) => obj.SetValue(DropTargetProperty, value);
		public static readonly DependencyProperty DropTargetProperty = DependencyProperty.RegisterAttached("DropTarget", typeof(IDropTarget), typeof(DragAndDropHelper), new PropertyMetadata(null));

		private static string GetLocalized(string resourceName)
		{
			var settings = App.Current.Services.GetService<ISettings>();
			if (settings == null)
				return "[Error: No ISettings service]";
			return settings.GetStringFromResource(resourceName, "Messages");
		}
		#endregion
	}

	public interface IDropTarget
	{
		string Name { get; }
		void DropItems(IEnumerable<IDragTarget> items);
		void DropItems(IReadOnlyList<IStorageItem> items);
		bool IsDragItemSupported(IDragTarget item);
	}
}

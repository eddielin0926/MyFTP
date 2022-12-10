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
		DragHelper Drag;
		DropHelper Drop;
	}
}

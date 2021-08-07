// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace ZestyKitchenHelper.iOS
{
	[Register ("MainPageController")]
	partial class MainPageController
	{
		[Outlet]
		UIKit.UIButton CloudAccountButton { get; set; }

		[Outlet]
		UIKit.UIButton HelpButton { get; set; }

		[Outlet]
		UIKit.UIButton LocalAccountButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CloudAccountButton != null) {
				CloudAccountButton.Dispose ();
				CloudAccountButton = null;
			}

			if (LocalAccountButton != null) {
				LocalAccountButton.Dispose ();
				LocalAccountButton = null;
			}

			if (HelpButton != null) {
				HelpButton.Dispose ();
				HelpButton = null;
			}
		}
	}
}

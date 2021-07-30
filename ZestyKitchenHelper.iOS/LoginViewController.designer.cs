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
	[Register ("LoginViewController")]
	partial class LoginViewController
	{
		[Outlet]
		UIKit.UIButton CloudAccountButton { get; set; }

		[Outlet]
		UIKit.UIButton HelpButton { get; set; }

		[Outlet]
		UIKit.UIButton LocalAcountButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CloudAccountButton != null) {
				CloudAccountButton.Dispose ();
				CloudAccountButton = null;
			}

			if (LocalAcountButton != null) {
				LocalAcountButton.Dispose ();
				LocalAcountButton = null;
			}

			if (HelpButton != null) {
				HelpButton.Dispose ();
				HelpButton = null;
			}
		}
	}
}

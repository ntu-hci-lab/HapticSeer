//
// App.xaml.h
// App 類別的宣告。
//

#pragma once

#include "App.g.h"

namespace HapticAgent
{
	/// <summary>
	/// 提供應用程式專屬行為以補充預設的應用程式類別。
	/// </summary>
	ref class App sealed
	{
	protected:
		virtual void OnLaunched(Windows::ApplicationModel::Activation::LaunchActivatedEventArgs^ e) override;

	internal:
		App();

	private:
		void OnSuspending(Platform::Object^ sender, Windows::ApplicationModel::SuspendingEventArgs^ e);
		void OnNavigationFailed(Platform::Object ^sender, Windows::UI::Xaml::Navigation::NavigationFailedEventArgs ^e);
	};
}

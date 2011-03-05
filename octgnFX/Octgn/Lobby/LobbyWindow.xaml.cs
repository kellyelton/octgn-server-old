using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media.Animation;

namespace Octgn.Lobby
{
	public partial class LobbyWindow : NavigationWindow
	{
		private static readonly Duration TransitionDuration = new Duration(TimeSpan.FromMilliseconds(500));
		private readonly AnimationTimeline OutAnimation = new DoubleAnimation(0, TransitionDuration);
		private readonly AnimationTimeline InAnimation = new DoubleAnimation(0, 1, TransitionDuration);

		private static readonly object BackTarget = new object();
		private bool isInTransition = false;
		private object transitionTarget;

        public LobbyWindow()
		{
			InitializeComponent();
            var navWnd = Application.Current.MainWindow as Octgn.Launcher.LauncherWindow;
            if (navWnd != null)
            {
                navWnd.Closing += new System.ComponentModel.CancelEventHandler(navWnd_Closing);
            }
            okToCloseMainWindow = false;
			NavigationCommands.BrowseBack.InputGestures.Clear();

			OutAnimation.Completed += delegate
			{
				isInTransition = false;
				if (transitionTarget == BackTarget)
					GoBack();
				else
					Navigate(transitionTarget);
			};
			OutAnimation.Freeze();

			Navigating += delegate(object sender, NavigatingCancelEventArgs e)
			{
				// FIX (jods): prevent further navigation when a navigation is already in progress
				//						 (e.g. double-click a button in the main menu). This would break the transitions.
				if (isInTransition)
				{ e.Cancel = true; return; }

				if (transitionTarget != null)
				{
					transitionTarget = null;
					return;
				}

				var page = Content as Page;
				if (page == null) return;

				e.Cancel = true;
				isInTransition = true;
				if (e.NavigationMode == NavigationMode.Back)
					transitionTarget = BackTarget;
				else
					transitionTarget = e.Content;
				page.BeginAnimation(UIElement.OpacityProperty, OutAnimation, HandoffBehavior.SnapshotAndReplace);
			};

			Navigated += delegate
			{
				var page = Content as Page;
				if (page == null) return;

				page.BeginAnimation(UIElement.OpacityProperty, InAnimation);
			};
		}
        public void navWnd_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!okToCloseMainWindow)
            {
                e.Cancel = true;
            }
        }

        public bool okToCloseMainWindow { get; set; }

        private void NavigationWindow_Closed(object sender, EventArgs e)
        {
            var navWnd = Application.Current.MainWindow as Octgn.Launcher.LauncherWindow;
            if (navWnd != null)
            {
                navWnd.Closing -= navWnd_Closing;
            }
        }
    }
}

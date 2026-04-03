using ScriptLauncher.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

namespace ScriptLauncher.Views
{
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;
        private bool _isExitRequested;
        private Point _dragStartPoint;
        private ListBoxItem _lastIndicatorItem;
        private bool _lastIndicatorWasLeft;
        private int _currentInsertIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            DataContext = vm;

            // Wire Add
            vm.AddRequested += () =>
            {
                var win = new CommandEditWindow { Owner = this };
                if (win.ShowDialog() == true)
                    vm.AddItem(win.Result);
            };

            // Wire Edit
            vm.EditRequested += (itemVm) =>
            {
                var win = new CommandEditWindow(itemVm) { Owner = this };
                if (win.ShowDialog() == true)
                    vm.UpdateItem(itemVm, win.Result);
            };

            InitNotifyIcon();
        }

        private void InitNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon(
                    System.Windows.Application.GetResourceStream(
                        new Uri("pack://application:,,,/Resources/app.ico")
                    ).Stream),
                Text = "Script Launcher",
                Visible = true
            };

            _notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left) RestoreWindow();
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Open", null, (_, __) => RestoreWindow());
            menu.Items.Add("Exit", null, (_, __) => ExitApplication());
            _notifyIcon.ContextMenuStrip = menu;
        }

        private void RestoreWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void ExitApplication()
        {
            _isExitRequested = true;
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized) Hide();
        }

        private void Window_Closing(object sender, CancelEventArgs e) { }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void ListBox_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed) return;

            var pos = e.GetPosition(null);
            if (Math.Abs(pos.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(pos.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            var listBox = sender as System.Windows.Controls.ListBox;
            var container = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (container == null) return;

            var item = listBox.ItemContainerGenerator.ItemFromContainer(container);
            if (item != null)
                DragDrop.DoDragDrop(container, item, System.Windows.DragDropEffects.Move);
        }

        private void ListBox_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (_lastIndicatorItem != null)
            {
                SetDropIndicator(_lastIndicatorItem, visible: false, left: true);
                _lastIndicatorItem = null;
            }

            if (!(DataContext is MainViewModel vm)) return;

            int insertIndex = GetInsertIndex(e);
            _currentInsertIndex = insertIndex;

            // Decide which card to draw on and which side
            ListBoxItem indicatorItem;
            bool drawLeft;

            if (insertIndex < vm.Commands.Count)
            {
                // Draw LEFT bar on the card at insertIndex
                indicatorItem = GetListBoxItem(insertIndex);
                drawLeft = true;
            }
            else
            {
                // Inserting at the end → draw RIGHT bar on the last card
                indicatorItem = GetListBoxItem(vm.Commands.Count - 1);
                drawLeft = false;
            }

            if (indicatorItem != null)
            {
                SetDropIndicator(indicatorItem, visible: true, left: drawLeft);
                _lastIndicatorItem = indicatorItem;
                _lastIndicatorWasLeft = drawLeft;
            }

            e.Effects = System.Windows.DragDropEffects.Move;
            e.Handled = true;
        }

        private void ListBox_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            if (_lastIndicatorItem != null)
            {
                SetDropIndicator(_lastIndicatorItem, visible: false, left: true);
                _lastIndicatorItem = null;
                _currentInsertIndex = -1;
            }
        }

        private void ListBox_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (_lastIndicatorItem != null)
            {
                SetDropIndicator(_lastIndicatorItem, visible: false, left: true);
                _lastIndicatorItem = null;
            }

            if (!(e.Data.GetData(typeof(CommandItemViewModel)) is CommandItemViewModel dragged)) return;

            MainViewModel vm = DataContext as MainViewModel;
            int oldIndex = vm.Commands.IndexOf(dragged);
            int newIndex = _currentInsertIndex < 0
                            ? GetInsertIndex(e)
                            : _currentInsertIndex;

            _currentInsertIndex = -1;

            newIndex = Math.Max(0, Math.Min(newIndex, vm.Commands.Count));

            if (oldIndex < newIndex) newIndex--;

            if (oldIndex != newIndex)
            {
                vm.Commands.Move(oldIndex, newIndex);
                vm.SaveCommands();
            }
        }

        private int GetInsertIndex(System.Windows.DragEventArgs e)
        {
            if (!(DataContext is MainViewModel vm)) return 0;

            for (int i = 0; i < vm.Commands.Count; i++)
            {
                var item = GetListBoxItem(i);
                if (item == null) continue;

                var pos = e.GetPosition(item);
                if (pos.X < 0 || pos.X > item.ActualWidth ||
                    pos.Y < 0 || pos.Y > item.ActualHeight)
                    continue; // cursor not over this card

                // Left half  → insert BEFORE i
                // Right half → insert AFTER  i  (= before i+1)
                return pos.X < item.ActualWidth / 2.0 ? i : i + 1;
            }

            return vm.Commands.Count;
        }

        private ListBoxItem GetListBoxItem(int index)
        {
            return CommandListBox.ItemContainerGenerator
                       .ContainerFromIndex(index) as ListBoxItem;
        }

        private void SetDropIndicator(ListBoxItem item, bool visible, bool left)
        {
            if (!(VisualTreeHelper.GetChild(item, 0) is Grid grid) || grid.Children.Count < 2) return;

            if (grid.Children[1] is Border indicator)
            {
                indicator.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

                if (visible)
                {
                    indicator.Style = left
                        ? (Style)FindResource("DropIndicatorLeft")
                        : (Style)FindResource("DropIndicatorRight");
                }
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T t) return t;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
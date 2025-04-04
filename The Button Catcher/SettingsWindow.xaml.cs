using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace The_Button_Catcher {
    public partial class SettingsWindow : Window {
        private Canvas colorObject;
        private Player player;

        public SettingsWindow(Canvas colorObject, Player player) {
            InitializeComponent();
            this.colorObject = colorObject;
            this.player = player;

            usernameTextBox.Text = player.Name;

            try {
                foreach (ComboBoxItem item in ColorComboBox.Items) {
                    string colorName = item.Tag.ToString();
                    if (((Color)ColorConverter.ConvertFromString(colorName)) == (colorObject.Background as SolidColorBrush).Color) {
                        ColorComboBox.SelectedItem = item;
                    }
                    //item.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorName));

                }
            } catch { }
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ColorComboBox.SelectionBoxItem is ComboBoxItem selectionItem) {
                string colorName = selectionItem.Tag.ToString();
                colorObject.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorName));
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e) {
            if (sender is Button button) {
                SolidColorBrush brush = button.Background as SolidColorBrush;

                if (brush != null) {
                    Color color = (Color)ColorConverter.ConvertFromString("#CCC");
                    ColorAnimation animation = new ColorAnimation {
                        To = color,
                        Duration = new Duration(TimeSpan.FromSeconds(0.3))
                    };

                    brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                }
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e) {
            if (sender is Button button) {
                SolidColorBrush brush = button.Background as SolidColorBrush;
                if (brush != null) {
                    Color color = (Color)ColorConverter.ConvertFromString("#AAA");

                    ColorAnimation animation = new ColorAnimation {
                        To = color,
                        Duration = new Duration(TimeSpan.FromSeconds(0.3))
                    };

                    brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                }
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e) {
            if (usernameTextBox.Text.Trim() != "")
                player.Name = usernameTextBox.Text;
            if (ColorComboBox.SelectedItem is ComboBoxItem item) {
                string colorName = item.Tag.ToString();
                colorObject.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorName));
            }
            this.Close();
        }
    }
}

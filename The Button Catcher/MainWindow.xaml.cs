using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace The_Button_Catcher {
    public partial class MainWindow : Window {
        SettingsWindow settingsWindow;

        Random random;
        DispatcherTimer timer;

        TextBlock currentCountdown;
        bool menuOpened = false;

        public MainWindow() {
            InitializeComponent();
            random = new Random();
            timer = new DispatcherTimer();
            GameManagement.Login(new Player());
        }

        private void StartGame() {
            GameManagement.CurrentGame.StartGame();
        }

        private void OpenMenu(object sender, RoutedEventArgs e) {
            GameManagement.CurrentGame.PauseGame();
            currentCountdown.Visibility = Visibility.Collapsed;
            menuOpened = true;
            finalText.Visibility = Visibility.Collapsed;

            menuRoot.Visibility = Visibility.Visible;
            DoubleAnimation anim = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromSeconds(0.2)) };
            menuRoot.BeginAnimation(Grid.OpacityProperty, anim);

            DoubleAnimation anim2 = new DoubleAnimation { To = 6, Duration = new Duration(TimeSpan.FromSeconds(0.2)) };
            gameRoot.Effect.BeginAnimation(BlurEffect.RadiusProperty, anim2);
        }

        private void LevelSelected(object sender, RoutedEventArgs e) {
            DoubleAnimation anim = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromSeconds(0.2)) };
            anim.Completed += (object sender2, EventArgs e2) => {
                levelSelectionMenu.Visibility = Visibility.Collapsed;
            };
            levelSelectionMenu.BeginAnimation(Grid.OpacityProperty, anim);

            DoubleAnimation anim2 = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromSeconds(0.2)) };
            gameRoot.Effect.BeginAnimation(BlurEffect.RadiusProperty, anim2);

            SolidColorBrush brush = (sender as Button).Background as SolidColorBrush;
            GameComplexities gameComplexity = GameComplexities.Easy;

            if (sender == hardSelectButton) gameComplexity = GameComplexities.Hard;
            else if (sender == normallySelectButton) gameComplexity = GameComplexities.Normally;

            GameManagement.CurrentGame = new Game(gameCanvas, gameComplexity);
            GameManagement.CurrentGame.ButtonClicked += ButtonClicked;
            GameManagement.CurrentGame.IsGameOver += GameOver;

            UpdateScoreText();

            DoubleAnimation anim3 = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromSeconds(0.2)) };
            anim3.Completed += (object sender2, EventArgs e2) => {
                menuRoot.Visibility = Visibility.Collapsed;
            };
            menuRoot.BeginAnimation(Grid.OpacityProperty, anim3);

            menuOpened = false;
            CountdownStart(3);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e) {
            DoubleAnimation anim = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromSeconds(0.2)) };
            anim.Completed += (object sender2, EventArgs e2) => {
                menuRoot.Visibility = Visibility.Collapsed;
            };
            menuRoot.BeginAnimation(Grid.OpacityProperty, anim);

            if (GameManagement.CurrentGame == null) {
                levelSelectionMenu.Visibility = Visibility.Visible;
                DoubleAnimation anim3 = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
                levelSelectionMenu.BeginAnimation(Grid.OpacityProperty, anim3);
            } else {
                menuOpened = false;
                DoubleAnimation anim2 = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromSeconds(0.2)) };
                gameRoot.Effect.BeginAnimation(BlurEffect.RadiusProperty, anim2);
                CountdownStart(3);
            }
        }

        private void ReplayButton_Click(object sender, RoutedEventArgs e) {
            if (GameManagement.CurrentGame != null) GameManagement.CurrentGame.EndGame();

            DoubleAnimation anim = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromSeconds(0.2)) };
            anim.Completed += (object sender2, EventArgs e2) => {
                menuRoot.Visibility = Visibility.Collapsed;
            };
            menuRoot.BeginAnimation(Grid.OpacityProperty, anim);

            levelSelectionMenu.Visibility = Visibility.Visible;
            DoubleAnimation anim3 = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            levelSelectionMenu.BeginAnimation(Grid.OpacityProperty, anim3);
        }

        private void OpenSettings(object sender, RoutedEventArgs e) {
            settingsWindow = new SettingsWindow(gameCanvas, GameManagement.CurrentPlayer);
            settingsWindow.Show();
        }

        private void Exit(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void CountdownStart(int seconds) {
            if (seconds < 0 || menuOpened) {
                return;
            }
            else if (seconds == 0) {
                this.StartGame();
                return;
            }

            TextBlock block = CreateCountdownText(seconds.ToString());
            currentCountdown = block;
            mainRoot.Children.Add(block);

            DoubleAnimation anim = new DoubleAnimation { From = 10, To = 100, Duration = new Duration(TimeSpan.FromSeconds(0.8)) };
            anim.Completed += (object sender2, EventArgs e2) => {
                DoubleAnimation anim2 = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromSeconds(0.2)) };
                anim2.Completed += (object sender3, EventArgs e3) => {
                    mainRoot.Children.Remove(block);
                    CountdownStart(seconds - 1);
                };
                block.BeginAnimation(TextBlock.OpacityProperty, anim2);
            };
            block.BeginAnimation(TextBlock.FontSizeProperty, anim);
        }

        private TextBlock CreateCountdownText(string text) {
            return new TextBlock {
                Text = text, FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }

        private void ButtonClicked() {
            UpdateScoreText();
        }

        private void GameOver() {
            if (menuOpened) return;
            finalText.Visibility = Visibility.Visible;
            finalText.Text = $"Игра окончена!\nОбщий счет: {GameManagement.CurrentGame.CurrentScore}";
            DoubleAnimation anim = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            finalText.BeginAnimation(TextBlock.OpacityProperty, anim);
        }

        private void UpdateScoreText() {
            if (GameManagement.CurrentGame == null) return;
            scoreText.Text = $"Счет: {GameManagement.CurrentGame.CurrentScore}";
        }
    }
}
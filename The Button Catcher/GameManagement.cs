using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Input;
using System.Windows.Shapes;


namespace The_Button_Catcher {
    static class GameManagement {
        public static Player CurrentPlayer { get; set; }
        public static Game CurrentGame { get; set; }

        public static void Login(Player player) {
            CurrentPlayer = player;
        }

        public static void StartGame() {
            if (CurrentGame == null) return;
            CurrentGame.StartGame();
        }
    }

    public class Player {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int RecordScore { get; set; }

        public Player() {
            Id = Guid.NewGuid();
            Name = $"Player{Id}";
            RecordScore = 0;
        }

        public Player(string name, int currentScore = 0, int recordScore = 0, List<Game> games = null) {
            Id = Guid.NewGuid();
            Name = name;
            RecordScore = recordScore;
        }
    }

    public class Game {
        private DispatcherTimer endgameButton;
        private DispatcherTimer buttonTimer;

        public Guid Id { get; set; }
        public GameStates GameState { get; set; }
        public GameComplexities GameComplexity { get; set; }
        public int CurrentScore { get; set; }

        public delegate void GameDelegate();
        public event GameDelegate IsGameActive;
        public event GameDelegate IsGamePaused;
        public event GameDelegate IsGameOver;
        public event GameDelegate ButtonClicked;

        Canvas canvas;
        Button currentButton;
        bool isButtonCreating = false;

        double size;
        SolidColorBrush color;

        public Game(Canvas canvas, GameComplexities gameComplexities = GameComplexities.Easy) {
            Id = Guid.NewGuid();
            GameState = GameStates.Pending;
            CurrentScore = 0;
            this.canvas = canvas;
            this.GameComplexity = gameComplexities;

            endgameButton = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            buttonTimer = new DispatcherTimer { Interval = GameComplexity == GameComplexities.Easy ? TimeSpan.FromSeconds(1)
                : (GameComplexity == GameComplexities.Normally ? TimeSpan.FromSeconds(0.7) : TimeSpan.FromSeconds(0.5))
            };
            endgameButton.Tick += (object sender, EventArgs e) => {
                this.EndGame();
            };
            buttonTimer.Tick += (object sender, EventArgs e) => {
                CreateNewButton();
            };

            IsGameActive += () => {
                Console.WriteLine("Игра запущена");
            };

            IsGamePaused += () => {
                Console.WriteLine("Игра на паузе");
            };

            IsGameOver += () => {
                Console.WriteLine("Игра окончена");
            };

            ButtonClicked += () => Console.WriteLine("Кнопка нажата");

            size = GameComplexity == GameComplexities.Hard ? 50 : (GameComplexity == GameComplexities.Normally ? 70 : 90);
            color = new SolidColorBrush(GameComplexity == GameComplexities.Hard ? Colors.DarkRed : (GameComplexity == GameComplexities.Normally ? Colors.Orange : Colors.Green));
        }

        public void StartGame() {
            GameState = GameStates.Running;
            endgameButton.Start();
            buttonTimer.Start();

            CreateNewButton();

            IsGameActive.Invoke();
        }

        public void PauseGame() {
            GameState = GameStates.Pause;
            endgameButton.Stop();
            buttonTimer.Stop();
            IsGamePaused.Invoke();
        }

        public void EndGame() {
            GameState = GameStates.Over;
            canvas.Children.Remove(currentButton);
            buttonTimer.Stop();
            IsGameOver.Invoke();
        }

        public void CreateNewButton() {
            if (isButtonCreating || GameState == GameStates.Over) return;

            isButtonCreating = true;
            buttonTimer.Stop();

            HideButton(currentButton);

            currentButton = new Button { Width = size, Height = size, Cursor = Cursors.Hand, Opacity = 0, Background = color };
            ControlTemplate template = new ControlTemplate(typeof(Button));

            FrameworkElementFactory ellipse = new FrameworkElementFactory(typeof(Ellipse));
            ellipse.SetValue(Ellipse.WidthProperty, new Binding("Width") { Source = currentButton });
            ellipse.SetValue(Ellipse.HeightProperty, new Binding("Height") { Source = currentButton });
            ellipse.SetValue(Ellipse.FillProperty, new SolidColorBrush(
                GameComplexity == GameComplexities.Hard ? Colors.DarkRed :
                (GameComplexity == GameComplexities.Normally ? Colors.Orange : Colors.Green)
            ));

            template.VisualTree = ellipse;
            currentButton.Template = template;

            currentButton.Click += Button_Click;

            Random random = new Random();
            double x = random.Next(0, (int)canvas.ActualWidth - (int)currentButton.Width);
            double y = random.Next(0, (int)canvas.ActualHeight - (int)currentButton.Height);
            Canvas.SetLeft(currentButton, x);
            Canvas.SetTop(currentButton, y);

            ShowButton(currentButton);

            DoubleAnimation anim = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            anim.Completed += (s, e) => {
                isButtonCreating = false;
                buttonTimer.Start();
            };
            currentButton.BeginAnimation(Button.OpacityProperty, anim);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (GameState != GameStates.Running) return;
            CurrentScore += 1;
            CreateNewButton();
            ButtonClicked.Invoke();
        }

        private void ShowButton(Button button) {
            if (button == null) return;
            canvas.Children.Add(button);

            DoubleAnimation anim = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            button.BeginAnimation(Button.OpacityProperty, anim);
        }

        private void HideButton(Button button) {
            if (button == null) return;

            DoubleAnimation anim2 = new DoubleAnimation { To = 40, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            button.BeginAnimation(Button.WidthProperty, anim2);

            DoubleAnimation anim3 = new DoubleAnimation { To = 30, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            button.BeginAnimation(Button.HeightProperty, anim3);

            DoubleAnimation anim = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromSeconds(0.3)) };
            anim.Completed += (s, e) => {
                canvas.Children.Remove(button);
            };
            button.BeginAnimation(Button.OpacityProperty, anim);
        }
    }

    public enum GameStates { Pending, Pause, Running, Over }
    public enum GameComplexities { Easy, Normally, Hard };
}
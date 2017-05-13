using MemoryTheGame.Persistence;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MemoryTheGame
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GameScreen : ContentPage
    {
        //co z aktualizowaniem labela przez timer
        //if else na switch z Color.Example albo inna metoda?
        //ostatni kolor z listy zawsze jest przypisany do ostatniego buttona
        //problem z pusta baza danych przy otwieraniu listview
        //pushing navigation from modal
        //transparent button?
        //
        //  easy   medium    hard
        //12 3x4 / 16 4x4 / 20 4x5

        List<Color> buttonHiddenColors = new List<Color>();
        Stopwatch stopwatch = new Stopwatch();
        Random random = new Random();
        MyButton clickedButton = new MyButton();
        MyButton[] buttons;
        bool isFinished, isOtherClicked, isNextGame = false;
        int iterator, moves, toEnd;
        string currentDifficulty;
        private SQLiteAsyncConnection _connection;

        public GameScreen(string chosenDifficulty)
        {
            InitializeComponent();

            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();

            currentDifficulty = chosenDifficulty.ToLower();

            SetUp(currentDifficulty);
        }

        protected async override void OnAppearing()
        {
            await _connection.CreateTableAsync<Highscores>();
        }

        async void BackPressedAlert()
        {
            var result = await DisplayAlert(null, "Go to the Menu?", "Yes", "No");
            if (result)
            {
                await Navigation.PopModalAsync();
            }
        }
        protected override bool OnBackButtonPressed()
        {
            //return base.OnBackButtonPressed();
            BackPressedAlert();
            return true;
        }

        //initiall game set up
        private void SetUp(string difficulty)
        {
            moves = 0;
            isFinished = false;
            isOtherClicked = false;

            AddColorsToList(difficulty);
            switch (difficulty)
            {
                case "easy":
                    toEnd = 6;
                    CreateButtons(12);
                    break;
                case "medium":
                    toEnd = 8;
                    CreateButtons(16);
                    break;
                case "hard":
                    toEnd = 10;
                    CreateButtons(20);
                    break;
            }
        }

        //create new buttons and puts them in the grid
        private void CreateButtons(int howMany)
        {
            buttons = new MyButton[howMany];
            iterator = 0;

            int column = 0, row = 0;

            switch (howMany)
            {
                case 12:
                    column = 3;
                    row = 4;
                    break;
                case 16:
                    column = 4;
                    row = 4;
                    break;
                case 20:
                    column = 4;
                    row = 5;
                    break;
            }

            for (int i = 0; i < column; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    buttons[iterator] = new MyButton();
                    buttons[iterator].Image = "red100x100.png";
                    buttons[iterator].Clicked += GameScreen_Clicked;
                    buttons[iterator].BackgroundColor = Color.Transparent;
                    buttons[iterator].BorderColor = Color.Transparent; //bez efektu
                    buttons[iterator].HorizontalOptions = LayoutOptions.Center;
                    buttons[iterator].VerticalOptions = LayoutOptions.Center;
                    AssignButtonHiddenColor(buttons[iterator]);
                    gameGrid.Children.Add(buttons[iterator], i, j);
                    iterator++;
                }
            }
        }

        //adds all the color pairs to the list depending on the difficulty level
        private void AddColorsToList(string difficulty)
        {
            buttonHiddenColors.Add(Color.Blue);
            buttonHiddenColors.Add(Color.Brown);
            buttonHiddenColors.Add(Color.Blue);
            buttonHiddenColors.Add(Color.Brown);
            buttonHiddenColors.Add(Color.Gray);
            buttonHiddenColors.Add(Color.Green);
            buttonHiddenColors.Add(Color.Gray);
            buttonHiddenColors.Add(Color.Green);
            if (isNextGame)
            {
                buttonHiddenColors.Add(Color.Orange);
                buttonHiddenColors.Add(Color.Pink);
                buttonHiddenColors.Add(Color.Orange);
                buttonHiddenColors.Add(Color.Pink);
            }
            else
            {
                buttonHiddenColors.Add(Color.Pink);
                buttonHiddenColors.Add(Color.Orange);
                buttonHiddenColors.Add(Color.Pink);
                buttonHiddenColors.Add(Color.Orange);
            }


            if (difficulty == "medium" || difficulty == "hard")
            {
                if (isNextGame)
                {
                    buttonHiddenColors.Add(Color.Violet);
                    buttonHiddenColors.Add(Color.Yellow);
                    buttonHiddenColors.Add(Color.Violet);
                    buttonHiddenColors.Add(Color.Yellow);
                }
                else
                {
                    buttonHiddenColors.Add(Color.Yellow);
                    buttonHiddenColors.Add(Color.Violet);
                    buttonHiddenColors.Add(Color.Yellow);
                    buttonHiddenColors.Add(Color.Violet);
                }
            }

            if (difficulty == "hard")
            {
                if (isNextGame)
                {
                    buttonHiddenColors.Add(Color.SkyBlue);
                    buttonHiddenColors.Add(Color.Black);
                    buttonHiddenColors.Add(Color.SkyBlue);
                    buttonHiddenColors.Add(Color.Black);
                }
                else
                {
                    buttonHiddenColors.Add(Color.Black);
                    buttonHiddenColors.Add(Color.SkyBlue);
                    buttonHiddenColors.Add(Color.Black);
                    buttonHiddenColors.Add(Color.SkyBlue);
                }
            }
        }

        //assigns random hidden color pairs to buttons from colors list
        private void AssignButtonHiddenColor(MyButton btn)
        {
            Color newHiddenColor = new Color();

            var index = random.Next(0, buttonHiddenColors.Count - 1);

            newHiddenColor = buttonHiddenColors[index];
            buttonHiddenColors.RemoveAt(index);

            btn.HiddenColor = newHiddenColor;
        }

        //disable all buttons on the grid
        private void DisableAllButtons()
        {
            foreach (var button in buttons)
            {
                button.IsEnabled = false;
            }
        }

        //enable all buttons with red image after x time in ms
        private async Task EnableAllButtons(int howLong)
        {
            await Task.Delay(howLong);
            foreach (var button in buttons)
            {
                if (button.Image == "red100x100.png")
                {
                    button.IsEnabled = true;
                }
            }
        }

        //change specified button image to red after 1s / 1000ms
        private async Task ChangeImageToRed(MyButton btn)
        {
            await Task.Delay(1000);
            btn.Image = "red100x100.png";
        }

        //enables specified button after x time in ms
        private async Task EnableAfter(MyButton button, int howLong)
        {
            await Task.Delay(howLong);
            button.IsEnabled = true;
        }

        //finish the game -> reset stopwatch, stop udpating time label, show alert with score
        private async void GameFinished()
        {
            if (isNextGame)
            {
                isNextGame = false;
            }
            else
            {
                isNextGame = true;
            }

            isFinished = true;

            var score = 10000 * (Math.Pow(0.95, (double)moves)) / ((60 * stopwatch.Elapsed.Minutes + stopwatch.Elapsed.Seconds) * 0.3);
            switch (currentDifficulty)
            {
                case "easy":
                    score *= 0.5;
                    break;
                case "medium":
                    score *= 1.4;
                    break;
                case "hard":
                    score *= 2.8;
                    break;
            }

            var timePassed = String.Format("{0}:{1}:{2}", stopwatch.Elapsed.Minutes.ToString(), stopwatch.Elapsed.Seconds.ToString(), stopwatch.Elapsed.Milliseconds);

            stopwatch.Reset();

            //var newScore = new Highscores { score = score, moves = moves, difficulty = currentDifficulty, dateTime = DateTime.Now, playerName = "Rob", duration = timePassed };
            //await _connection.InsertAsync(newScore);

            //var result = await DisplayActionSheet(String.Format("You won! Score: {0:F}", score), null, null, "Play Again", "Menu", "High Scores");
            //if (result == "Play Again")
            //{
            //    ResetGame();
            //}
            //else if (result == "Menu")
            //{
            //    await Navigation.PopModalAsync();
            //}
            //else if (result == "High Scores")
            //{
            //    Navigation.PopModalAsync();
            //    await Navigation.PushModalAsync(new NavigationPage(new HighScoreSreen(currentDifficulty)));
            //}

            await Navigation.PushModalAsync(new VictoryScreen(currentDifficulty, score, moves, timePassed));
        }

        //reset all the things and let player start a new game
        public void ResetGame()
        {
            movesLabel.Text = "Moves: 0";
            timeLabel.Text = "Time: 0:00:00";

            ClearGameGrid();
            SetUp(currentDifficulty);
        }

        //removes all children from the grid view
        private void ClearGameGrid()
        {
            foreach (var button in buttons)
            {
                gameGrid.Children.Remove(button);
            }
        }
        //handling all button clicks
        private void GameScreen_Clicked(object sender, System.EventArgs e) //could improve
        {
            //seting stopwatch for the first time, formatting output to timeLabel
            if (!stopwatch.IsRunning)
            {
                Device.StartTimer(new TimeSpan(0, 0, 0, 0, 100), () =>
                {
                    timeLabel.Text = String.Format("Time: {0}:{1}:{2}",
                        stopwatch.Elapsed.Minutes,
                        stopwatch.Elapsed.Seconds,
                        stopwatch.Elapsed.Milliseconds);
                    return !isFinished;
                });
                stopwatch.Start();
            }

            MyButton btn = (MyButton)sender; //reference the button clicked
            btn.IsEnabled = false; //make it impossible to click
            movesLabel.Text = String.Format("Moves: {0}", ++moves);


            if (btn.HiddenColor == Color.Blue)
            {
                btn.Image = "blue100x100.png";
            }
            else if (btn.HiddenColor == Color.Brown)
            {
                btn.Image = "brown100x100.png";
            }
            else if (btn.HiddenColor == Color.Gray)
            {
                btn.Image = "gray100x100.png";
            }
            else if (btn.HiddenColor == Color.Green)
            {
                btn.Image = "green100x100.png";
            }
            else if (btn.HiddenColor == Color.Orange)
            {
                btn.Image = "orange100x100.png";
            }
            else if (btn.HiddenColor == Color.Pink)
            {
                btn.Image = "pink100x100.png";
            }
            else if (btn.HiddenColor == Color.Violet)
            {
                btn.Image = "violet100x100.png";
            }
            else if (btn.HiddenColor == Color.Yellow)
            {
                btn.Image = "yellow100x100.png";
            }
            else if (btn.HiddenColor == Color.SkyBlue)
            {
                btn.Image = "skyblue100x100.png";
            }
            else if (btn.HiddenColor == Color.Black)
            {
                btn.Image = "black100x100.png";
            }//if else colors, try to find a better way

            //check if any other tile was clicked
            if (isOtherClicked) //if yes
            {
                DisableAllButtons(); //for safety
                if (btn.HiddenColor == clickedButton.HiddenColor) //compare colors
                {
                    EnableAllButtons(1);
                    isOtherClicked = false;

                    //checking if the game is finished
                    if (--toEnd == 0)
                    {
                        GameFinished();
                    }
                }
                else
                {
                    EnableAllButtons(100); //delay for safety
                    ChangeImageToRed(btn); ChangeImageToRed(clickedButton); //reset both tiles color - red
                    EnableAfter(btn, 900); EnableAfter(clickedButton, 900); //make them clickable again after x ms
                    isOtherClicked = false;
                }
            }
            else //if no
            {
                clickedButton = btn; //save current button in temp variable
                isOtherClicked = true;
            }
        }
    }
}

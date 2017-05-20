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
        //
        //  easy   medium    hard
        //12 3x4 / 16 4x4 / 20 4x5

        List<Color> buttonHiddenColors = new List<Color>();
        Stopwatch stopwatch = new Stopwatch();
        Random random = new Random();
        MyButton clickedButton = new MyButton();
        MyButton[] buttons;
        bool isFinished, isOtherClicked, isNextGame = false;
        int index, moves, toEnd;
        string currentDifficulty;
        private SQLiteAsyncConnection _connection;

        public GameScreen(string chosenDifficulty)
        {
            InitializeComponent();

            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            _connection.CreateTableAsync<Highscores>();

            currentDifficulty = chosenDifficulty.ToLower();

            SetUp(currentDifficulty);
        }

        //ask user if he is sure he wants to go back to main menu
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
            BackPressedAlert();
            return true;
        }

        //initial game set up
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

        //create new buttons and add them to the grid
        private void CreateButtons(int howMany)
        {
            buttons = new MyButton[howMany];
            index = 0;

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
                    buttons[index] = new MyButton();
                    buttons[index].Clicked += GameScreen_Clicked;
                    buttons[index].BackgroundColor = Color.Gray;
                    AssignButtonHiddenColor(buttons[index]);
                    gameGrid.Children.Add(buttons[index], i, j);
                    index++;
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
            buttonHiddenColors.Add(Color.Red);
            buttonHiddenColors.Add(Color.Green);
            buttonHiddenColors.Add(Color.Red);
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
                button.InputTransparent = true;
            }
        }

        //enable all buttons with red image after x time in ms
        private async Task EnableGrayButtons(int howLong)
        {
            await Task.Delay(howLong);
            foreach (var button in buttons)
            {
                if (button.BackgroundColor == Color.Gray)
                {
                    button.InputTransparent = false;
                }
            }
        }

        //change specified button image to red after 1s / 1000ms
        private async Task ChangeColorToGrayAsync(MyButton btn)
        {
            await Task.Delay(1000);
            btn.BackgroundColor = Color.Gray;
        }

        //enables specified button after x time in ms
        private async Task EnableButtonAsync(MyButton button, int howLong)
        {
            await Task.Delay(howLong);
            button.InputTransparent = false;
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

            MyButton btn = (MyButton)sender;
            btn.InputTransparent = true;
            movesLabel.Text = String.Format("Moves: {0}", ++moves);


            if (btn.HiddenColor == Color.Blue)
            {
                btn.BackgroundColor = Color.Blue;
            }
            else if (btn.HiddenColor == Color.Brown)
            {
                btn.BackgroundColor = Color.Brown;
            }
            else if (btn.HiddenColor == Color.Red)
            {
                btn.BackgroundColor = Color.Red;
            }
            else if (btn.HiddenColor == Color.Green)
            {
                btn.BackgroundColor = Color.Green;
            }
            else if (btn.HiddenColor == Color.Orange)
            {
                btn.BackgroundColor = Color.Orange;
            }
            else if (btn.HiddenColor == Color.Pink)
            {
                btn.BackgroundColor = Color.Pink;
            }
            else if (btn.HiddenColor == Color.Violet)
            {
                btn.BackgroundColor = Color.Violet;
            }
            else if (btn.HiddenColor == Color.Yellow)
            {
                btn.BackgroundColor = Color.Yellow;
            }
            else if (btn.HiddenColor == Color.SkyBlue)
            {
                btn.BackgroundColor = Color.SkyBlue;
            }
            else if (btn.HiddenColor == Color.Black)
            {
                btn.BackgroundColor = Color.Black;
            }//if else colors, try to find a better way

            //check if any other button is clicked
            if (isOtherClicked)
            {
                DisableAllButtons(); //for safety
                if (btn.HiddenColor == clickedButton.HiddenColor) //compare colors
                {
                    EnableGrayButtons(1);
                    isOtherClicked = false;

                    //checking if the game is finished
                    if (--toEnd == 0)
                    {
                        GameFinished();
                    }
                }
                else
                {
                    EnableGrayButtons(100); //delay for safety
                    ChangeColorToGrayAsync(btn); ChangeColorToGrayAsync(clickedButton); //reset both buttons color
                    EnableButtonAsync(btn, 980); EnableButtonAsync(clickedButton, 980); //make them clickable again after x ms
                    isOtherClicked = false;
                }
            }
            else
            {
                clickedButton = btn; //save current button in temp variable
                isOtherClicked = true;
            }
        }
    }
}

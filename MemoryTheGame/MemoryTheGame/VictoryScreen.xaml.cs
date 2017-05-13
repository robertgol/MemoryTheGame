using MemoryTheGame.Persistence;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MemoryTheGame
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VictoryScreen : ContentPage
    {
        string _difficulty, _timePassed;
        double _score;
        int _moves;
        SQLiteAsyncConnection _connection;
        public VictoryScreen(string difficulty, double score, int moves, string timePassed)
        {
            InitializeComponent();

            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();

            _difficulty = difficulty;
            _timePassed = timePassed;
            _moves = moves;
            _score = score;

            scoreLabel.Text = String.Format("Your score: {0:F0}", score);
            movesLabel.Text = String.Format("Moves: {0}", moves);
            timeLabel.Text = String.Format("Time: {0}", timePassed);
        }

        private async Task DatabaseInsert()
        {
            var _playerName = entry.Text;
            if (String.IsNullOrEmpty(_playerName))
            {
                _playerName = "Anonymous";
            }

            var newScore = new Highscores
            {
                difficulty = _difficulty,
                duration = _timePassed,
                moves = _moves,
                playerName = _playerName,
                dateTime = DateTime.Now,
                score = _score
            };

            await _connection.InsertAsync(newScore);
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private async void Menu_Clicked(object sender, EventArgs e)
        {
            await DatabaseInsert();

            Navigation.PopModalAsync();
            Navigation.PopModalAsync();
        }

        private async void PlayAgain_Clicked(object sender, EventArgs e)
        {
            var stack = Navigation.ModalStack;
            var page = stack.ElementAt(stack.Count - 2) as GameScreen;
            page.ResetGame();

            await DatabaseInsert();

            await Navigation.PopModalAsync();
        }

        private async void Highscores_Clicked(object sender, EventArgs e)
        {
            await DatabaseInsert();

            Navigation.PopModalAsync();
            Navigation.PopModalAsync();
            Navigation.PushModalAsync(new NavigationPage(new HighScoreSreen(_difficulty)));
        }
    }
}
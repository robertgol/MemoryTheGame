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

    public class Highscores
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double score { get; set; }
        public int moves { get; set; }
        public DateTime dateTime { get; set; }
        public string difficulty { get; set; }
        public string playerName { get; set; }
    }

    public partial class HighScoreSreen : TabbedPage
    {
        private SQLiteAsyncConnection _connection;
        public HighScoreSreen(string difficultyPage)
        {
            InitializeComponent();

            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();

            switch (difficultyPage)
            {
                case "easy":
                    CurrentPage = easyPage;
                    break;
                case "medium":
                    CurrentPage = mediumPage;
                    break;
                case "hard":
                    CurrentPage = hardPage;
                    break;
            }
        }

        protected async override void OnAppearing()
        {
            var highscores = await _connection.Table<Highscores>().ToListAsync();

            if (highscores.Count > 0)
            {
                var sortedHighscores = highscores.OrderByDescending(o => o.score).ToList();

                easyListView.ItemsSource = sortedHighscores.Where(s => s.difficulty == "easy").ToList();
                mediumListView.ItemsSource = sortedHighscores.Where(s => s.difficulty == "medium").ToList();
                hardListView.ItemsSource = sortedHighscores.Where(s => s.difficulty == "hard").ToList();
            }
        }

        private async void allListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }

            var listView = sender as ListView;
            var highscore = e.SelectedItem as Highscores;
            await Navigation.PushModalAsync(new NavigationPage(new HighScoreDetail(highscore)));

            listView.SelectedItem = null;
        }

        private async void ToolbarItem_Activated(object sender, EventArgs e)
        {
            var result = await DisplayAlert("Clear Highscores", "Do you want to remove all the highscores?", "Yes", "No");

            if (result)
            {
                var highscores = await _connection.Table<Highscores>().ToListAsync();
                foreach (var item in highscores)
                {
                    await _connection.DeleteAsync(item);
                }
            }
        }
    }
}

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
    public partial class HomeScreen : ContentPage
    {
        public HomeScreen()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            await Navigation.PushModalAsync(new GameScreen(button.Text));
        }

        private async void HighScores_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new HighScoreSreen("medium"));
        }
    }
}

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
    public partial class HighScoreDetail : ContentPage
    {
        public HighScoreDetail(Highscores highscore)
        {
            if (highscore == null)
            {
                throw new ArgumentNullException();
            }

            InitializeComponent();

            BindingContext = highscore;
        }
    }
}

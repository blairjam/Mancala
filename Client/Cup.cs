using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Client
{
    public class Cup
    {
        private const string APP_ROOT  = "pack://application:,,,/Client;component/";
        private const string EMPTY     = "Res/Empty.png";
        private const string GEM_ONE   = "Res/GemOne.png";
        private const string GEM_TWO   = "Res/GemTwo.png";
        private const string GEM_THREE = "Res/GemThree.png";
        private const string GEM_FOUR  = "Res/GemFour.png";
        private const string GEM_FIVE  = "Res/GemFive.png";

        private Image image;
        private Label label;
        private int gems;

        public int Gems
        {
            get
            {
                return gems;
            }

            set
            {
                gems = Math.Abs(value);
                label.Content = gems.ToString();

                switch (gems)
                {
                    case 0:
                    {
                        ChangeSource(EMPTY);
                        break;
                    }
                    case 1:
                    {
                        ChangeSource(GEM_ONE);
                        break;
                    }
                    case 2:
                    {
                        ChangeSource(GEM_TWO);
                        break;
                    }
                    case 3:
                    {
                        ChangeSource(GEM_THREE);
                        break;
                    }
                    case 4:
                    {
                        ChangeSource(GEM_FOUR);
                        break;
                    }
                    default:
                    {
                        ChangeSource(GEM_FIVE);
                        break;
                    }
                }
            }
        }

        public Cup(Image image, Label label)
        {
            this.image = image;
            this.label = label;
        }

        private void ChangeSource(string source)
        {
            image.Source = new BitmapImage(new Uri(APP_ROOT + source, UriKind.Absolute));
        }
    }
}

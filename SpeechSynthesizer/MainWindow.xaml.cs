using System;
using System.IO;
using System.Media;
using System.Windows;
using Microsoft.Speech.Synthesis;

namespace SpeechSynthesizerSample
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        SpeechSynthesizer ss = new SpeechSynthesizer();
        SoundPlayer player = new SoundPlayer();

        public MainWindow()
        {
            InitializeComponent();

            ss.SpeakCompleted +=new EventHandler<SpeakCompletedEventArgs>( ss_SpeakCompleted );
        }

        private void button1_Click( object sender, RoutedEventArgs e )
        {
            try {
                player.Stream = new MemoryStream();
                ss.SetOutputToWaveStream( player.Stream );
                ss.SpeakAsync( textBox1.Text );
            }
            catch ( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }

        void ss_SpeakCompleted( object sender, SpeakCompletedEventArgs e )
        {
            try {
                player.Stream.Position = 0;
                player.Play();
            }
            catch ( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }
    }
}

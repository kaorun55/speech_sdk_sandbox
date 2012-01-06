using System;
using System.IO;
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using SpeechRecognizer;

namespace KinectSpeechRecognize
{
    class Program
    {
        static void Main( string[] args )
        {
            try {
                using ( var source = new KinectAudioSource() ) {
                    source.FeatureMode = true;
                    source.AutomaticGainControl = false; //Important to turn this off for speech recognition
                    source.SystemMode = SystemMode.OptibeamArrayOnly; //No AEC for this sample

                    var colors = new Choices();
                    colors.Add( "red" );
                    colors.Add( "green" );
                    colors.Add( "blue" );
                    colors.Add( "end" );
                    colors.Add( "赤" );
                    colors.Add( "みどり" );
                    colors.Add( "あお" );
                    colors.Add( "終わり" );

                    Recognizer r = new Recognizer( "ja-JP", colors );
                    r.SpeechRecognized += SreSpeechRecognized;
                    r.SpeechHypothesized += SreSpeechHypothesized;
                    r.SpeechRecognitionRejected += SreSpeechRecognitionRejected;
                    Console.WriteLine( "Using: {0}", r.Name );

                    using ( Stream s = source.Start() ) {
                        r.SetInputToAudioStream( s,
                                                    new SpeechAudioFormatInfo(
                                                        EncodingFormat.Pcm, 16000, 16, 1,
                                                        32000, 2, null ) );

                        Console.WriteLine( "Recognizing. Say: 'red', 'green' or 'blue'. Press ENTER to stop" );

                        r.RecognizeAsync( RecognizeMode.Multiple );
                        Console.ReadLine();
                        Console.WriteLine( "Stopping recognizer ..." );
                        r.RecognizeAsyncStop();
                    }
                }
            }
            catch ( Exception ex ) {
                Console.WriteLine( ex.Message );
            }
        }

        static void SreSpeechRecognitionRejected( object sender, SpeechRecognitionRejectedEventArgs e )
        {
            Console.WriteLine( "\nSpeech Rejected" );
        }

        static void SreSpeechHypothesized( object sender, SpeechHypothesizedEventArgs e )
        {
            Console.Write( "\rSpeech Hypothesized: \t{0}", e.Result.Text );
        }

        static void SreSpeechRecognized( object sender, SpeechRecognizedEventArgs e )
        {
            //This first release of the Kinect language pack doesn't have a reliable confidence model, so 
            //we don't use e.Result.Confidence here.
            Console.WriteLine( "\nSpeech Recognized: \t{0}", e.Result.Text );
        }
    }
}

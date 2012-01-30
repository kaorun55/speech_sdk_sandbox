using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.Recognition;
using System.IO;
using Microsoft.Speech.AudioFormat;

namespace KinectSpeechDictation
{
    class Program
    {
        private const string RecognizerId = "ja-JP";

        static void Main( string[] args )
        {
            try {
                using ( var source = new KinectAudioSource() ) {
                    source.FeatureMode = true;
                    source.AutomaticGainControl = false; //Important to turn this off for speech recognition
                    source.SystemMode = SystemMode.OptibeamArrayOnly; //No AEC for this sample

                    RecognizerInfo ri = SpeechRecognitionEngine.InstalledRecognizers().Where( r => "ja-JP".Equals( r.Culture.Name, StringComparison.InvariantCultureIgnoreCase ) ).FirstOrDefault();

                    if ( ri == null ) {
                        Console.WriteLine( "Could not find speech recognizer: {0}. Please refer to the sample requirements.", RecognizerId );
                        return;
                    }

                    Console.WriteLine( "Using: {0}", ri.Name );

                    using ( var sre = new SpeechRecognitionEngine( ri.Id ) ) {
                        GrammarBuilder dictaphoneGB = new GrammarBuilder();

                        GrammarBuilder dictation = new GrammarBuilder();
                        dictation.AppendDictation();

                        dictaphoneGB.Append( new SemanticResultKey( "StartDictation", new SemanticResultValue( "Start Dictation", true ) ) );
                        dictaphoneGB.Append( new SemanticResultKey( "dictationInput", dictation ) );
                        dictaphoneGB.Append( new SemanticResultKey( "EndDictation", new SemanticResultValue( "Stop Dictation", false ) ) );

                        GrammarBuilder spellingGB = new GrammarBuilder();

                        GrammarBuilder spelling = new GrammarBuilder();
                        spelling.AppendDictation( "spelling" );

                        spellingGB.Append( new SemanticResultKey( "StartSpelling", new SemanticResultValue( "Start Spelling", true ) ) );
                        spellingGB.Append( new SemanticResultKey( "spellingInput", spelling ) );
                        spellingGB.Append( new SemanticResultKey( "StopSpelling", new SemanticResultValue( "Stop Spelling", true ) ) );

                        GrammarBuilder both = GrammarBuilder.Add( (GrammarBuilder)new SemanticResultKey( "Dictation", dictaphoneGB ),
                                                                (GrammarBuilder)new SemanticResultKey( "Spelling", spellingGB ) );

                        Grammar grammar = new Grammar( new SemanticResultKey( "Dictation", dictaphoneGB ) );
                        grammar.Enabled = true;
                        grammar.Name = "Dictaphone and Spelling ";

                        sre.LoadGrammar( grammar ); // Exception thrown here

                        
                        sre.SpeechRecognized += SreSpeechRecognized;
                        sre.SpeechHypothesized += SreSpeechHypothesized;
                        sre.SpeechRecognitionRejected += SreSpeechRecognitionRejected;

                        using ( Stream s = source.Start() ) {
                            sre.SetInputToAudioStream( s, new SpeechAudioFormatInfo(
                                                        EncodingFormat.Pcm, 16000, 16, 1,
                                                        32000, 2, null ) );

                            Console.WriteLine( "Recognizing. Say: 'red', 'green' or 'blue'. Press ENTER to stop" );

                            sre.RecognizeAsync( RecognizeMode.Multiple );
                            Console.ReadLine();
                            Console.WriteLine( "Stopping recognizer ..." );
                            sre.RecognizeAsyncStop();
                        }
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
            if ( e.Result != null )
                DumpRecordedAudio( e.Result.Audio );
        }

        static void SreSpeechHypothesized( object sender, SpeechHypothesizedEventArgs e )
        {
            Console.Write( "\rSpeech Hypothesized: \t{0}    ", e.Result.Text );
        }

        static void SreSpeechRecognized( object sender, SpeechRecognizedEventArgs e )
        {
            //This first release of the Kinect language pack doesn't have a reliable confidence model, so 
            //we don't use e.Result.Confidence here.
            Console.Write( "\nSpeech Recognized: \t{0}", e.Result.Text );
        }

        private static void DumpRecordedAudio( RecognizedAudio audio )
        {
            if ( audio == null )
                return;

            int fileId = 0;
            string filename;
            while ( File.Exists( (filename = "RetainedAudio_" + fileId + ".wav") ) )
                fileId++;

            Console.WriteLine( "\nWriting file: {0}", filename );
            using ( var file = new FileStream( filename, System.IO.FileMode.CreateNew ) )
                audio.WriteToWaveStream( file );
        }

    }
}

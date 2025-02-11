namespace Terrabit
{
    internal class Music
    {
        public static string[] Read(int songId)
        {
            String[] OST = { "DAY", "FinalSunset", "Boss0", "UniversalCollapse" };
            String Song = "data/" + OST[songId] + ".song"; //Useless? Only currently //I'm probably not gonna spend another 5-7 hours just to add another song... // r/thisagedpoorly (that probably doesnt exist)
            String[] Lines = File.ReadAllText(Song).Split("\n"); //Get the converted and manually altered MIDI file, and splits it into lines for easier manipulation //Update this to ReadAllLines()
            int[] ongoingNotes = new int[Lines.Length]; //Doesnt really need to be this long
            int[] timeValues = new int[Lines.Length]; //Among us?

            String[] audioData = new String[Lines.Length]; //Final output to be played by BEEPS
            int dataCounter = 0;

            for (int i = 0; i < Lines.Length; i++)
            {
                String Note = Lines[i]; //Line in series of lines
                String[] noteInfo = Note.Split(","); //Split into all metadata
                int oldIndex;
                for (int j = 0; j < noteInfo.Length; j++) //check all metadata for important info
                {
                    switch (j)
                    {
                        //The program used to convert MIDI to text gives 6 points of metadata
                        /*
                        1. Channel (always 1 in this case)
                        2. Universal time 
                        3. Note start or end
                        4. Some value that's always zero? (apparently channel again?)
                        5. Note
                        6. Velocity (discarded due to limited capabilities of Console.beep

                        Cases are the MINUS ONE because of arrays or something idk /// What the fuck does this mean? //// I get it now
                        */
                        case 2: //third
                            {
                                if (noteInfo[j] == " Note_on_c") //Turns note on in ongoing notes
                                {
                                    ongoingNotes[i] = Convert.ToInt32(noteInfo[4]);
                                    timeValues[i] = Convert.ToInt32(noteInfo[1]);
                                }
                                else if (noteInfo[j] == " Note_off_c") //Guess
                                {
                                    oldIndex = Array.IndexOf(ongoingNotes, Convert.ToInt32(noteInfo[4]));

                                    if (oldIndex != -1)
                                    {
                                        dataCounter++; //Don't override data
                                        audioData[dataCounter] = ($"{Convert.ToInt32(noteInfo[1]) - timeValues[oldIndex]},{noteInfo[4]},{timeValues[oldIndex]}"); //Save pitch and length

                                        ongoingNotes[oldIndex] = 0; //note nullified
                                        timeValues[oldIndex] = 0; //time nullfied
                                    }
                                }
                                break;
                            }
                    }
                }
            }
            return audioData;
        }
        public static void Play(string[] AudioDATA, int timeValue)
        {
            if (OperatingSystem.IsWindows())
            {
                String[] Note;
                int counter = 1;
                int time = 0;
                String currentData;

                while (true) //idfk...
                {
                    currentData = AudioDATA[counter];
                    if (currentData == null)
                        break;

                    if (Convert.ToInt32(currentData.Split(",")[2]) < time) //Tempo is around 111, and if using beepbox notes literally cannot be next to eachother
                    {
                        Note = currentData.Split(","); //Should roughly map to frequency's from 261 to 493 // 65-3983... //NOPE APPARENTLY 8-12543 FROM WIKIPEDIA //Except that sounds like shit... //I had no idea how this worked when I wrote this
                        Console.Beep(MidiToFrequency(Convert.ToInt32(Note[1]) + 2), 100 + (Convert.ToInt32(Note[0])) * 8); //First pitch to frequency then length
                        counter++;
                        time += Convert.ToInt32(Note[0]); //output = output_start + ((output_end - output_start) / (input_end - input_start)) * (input - input_start)
                        //Thanks stack overflow                 //frequency = 261 + ((493 - 261) / (127 - 0) * (Note[1] - 127)
                        //Thanks that one idk I forgot how I found the stuff I did I'm writing this in the future
                    }
                    time += timeValue;
                    Thread.Sleep(1);
                }
            }
        }
        public static int MidiToFrequency(int MidiNumber)
        {
            return (int)(Math.Pow(2, (MidiNumber - 69) / 12.0) * 440);
        }
    }
}

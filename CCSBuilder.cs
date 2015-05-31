using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMDEditor
{
    public class CCSBuilder
    {
        public CCSBuilder()
        {
        }

        public static string BuildCCS(LibMMM.VsqFile vf,int StartP)
        {
            List<string> TashTable = new List<string>();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<Scenario Code=\"7251BC4B6168E7B2992FA620BD3E1E77\">");
            sb.AppendLine("  <Sequence Id=\"\">");
            sb.AppendLine("    <Scene Id=\"\">");
            sb.AppendLine("      <Units>");
            foreach(LibMMM.VsqTrack t in vf.Tracks)
            {
                Guid g = System.Guid.NewGuid();
                
                string gTash=g.ToString();
                TashTable.Add(gTash);
                sb.AppendLine("        <Unit Version=\"1.0\" Id=\"\" Category=\"SingerSong\" Group=\""+gTash+"\" StartTime=\"00:00:00\" Duration=\"00:10:00\">");
                sb.AppendLine("          <Song Version=\"1.02\">");
                sb.AppendLine("            <Tempo>");
                foreach (LibMMM.TempoData td in t.TempoList)
                {
                    double ttx = 60000000.0 / (int)td.tempo;
                    sb.AppendLine("              <Sound Clock=\"" + td.time.ToString() + "\" Tempo=\"" + Math.Round(ttx).ToString("#0") + "\" />");
                }
                sb.AppendLine("            </Tempo>");
                sb.AppendLine("            <Beat>");
                sb.AppendLine("              <Time Clock=\"0\" Beats=\"4\" BeatType=\"4\" />");
                sb.AppendLine("            </Beat>");
                sb.AppendLine("            <Score>");
                sb.AppendLine("              <Key Clock=\"0\" Fifths=\"0\" Mode=\"0\" />");
                foreach (LibMMM.vsqEvent td in t.events)
                {
                    int a = (int)(td.info.Note / 12)-2;
                    int b = td.info.Note % 12;
                    if (a < 1) continue;
                    if (td.info.Length<30) continue;
                    sb.AppendLine("              <Note Clock=\"" + (td.time - StartP + 3840).ToString() + "\" PitchStep=\"" + b.ToString() + "\" PitchOctave=\"" + a.ToString() + "\" Duration=\"" + td.info.Length.ToString() + "\" Lyric=\"" + td.lyric.Lyric + "\" />");
                }
                sb.AppendLine("            </Score>");
                sb.AppendLine("          </Song>"); 
                sb.AppendLine("        </Unit>");
            }
            sb.AppendLine("      </Units>");
            int i=0;
            foreach (string s in TashTable)
            {
                i++;
                sb.AppendLine("      <Groups>");
                sb.AppendLine("        <Group Version=\"1.0\" Id=\""+s+"\" Category=\"SingerSong\" Name=\"ソング "+i.ToString()+"\" Color=\"#FFFF0000\" Volume=\"0\" Pan=\"0\" IsSolo=\"false\" IsMuted=\"false\" CastId=\"A\" />");
                sb.AppendLine("      </Groups>");
            }
            double ttt = 60000000.0 / (int)vf.Tracks[0].TempoList[0].tempo;
            sb.AppendLine("      <SoundSetting Rhythm=\"4/4\" Tempo=\"" + Math.Round(ttt).ToString("#0") + "\" />");
            sb.AppendLine("    </Scene>");
            sb.AppendLine("  </Sequence>");
            sb.AppendLine("</Scenario>");
            return sb.ToString();
        }
    }
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PingCastle
{
    internal class FixZircolite
    {
        public static void CombineZircoliteFiles()
        {
            //Switch Release
           string path = Directory.GetCurrentDirectory();

            //Switch normal
           //string path = Directory.GetCurrentDirectory();
           //path = path.Substring(0,path.LastIndexOf(@"\") + 1);

            //Switch Release
            File.Copy(Directory.GetCurrentDirectory() + @"\RuntimeModules\ZircoliteGUI\tempdata.js", Directory.GetCurrentDirectory() + @"\RuntimeModules\ZircoliteGUI\data.js", true);
            Globals.unionText = File.ReadAllText(Directory.GetCurrentDirectory() + @"\RuntimeModules\ZircoliteGUI\data.js");
            
            //Switch normal  
            //File.Copy(Directory.GetCurrentDirectory() + @"\ZircoliteGUI\tempdata.js", Directory.GetCurrentDirectory() + @"\ZircoliteGUI\data.js", true);
            //Globals.unionText = File.ReadAllText(Directory.GetCurrentDirectory() + @"\ZircoliteGUI\data.js");

            foreach (string subFolder in Directory.GetDirectories(path + @"\RisxClientsOutput"))
            {

                if (subFolder.Contains("Decrypted"))
                {
                    if(File.Exists(subFolder + @"\ThreatHuntingData.js"))
                    {
                        Console.WriteLine(subFolder + @"\ThreatHuntingData.js");
                        string fileText = File.ReadAllText(subFolder + @"\ThreatHuntingData.js");
                        Globals.useZircolite = true;
                        LoadWORK.LoadData(fileText);
                    }
                    //Console.WriteLine(subFolder);
                }
            }

            if(Globals.useZircolite)
            {
                SaveWORK.InsertData();
                //insert
                //Release
                File.WriteAllText(Directory.GetCurrentDirectory() + @"\RuntimeModules\ZircoliteGUI\data.js", Globals.unionText);
                //Normal
                //File.WriteAllText(Directory.GetCurrentDirectory() + @"\\ZircoliteGUI\data.js", Globals.unionText);
            }
        }

        public static void InsertZircoliteFunc()
        {
            /*
            //release
            string path = Directory.GetCurrentDirectory() + @"\10RootPingCastle\ZircoliteGUI\Index.html";
            //normal
            //string path = Directory.GetCurrentDirectory() + @"\ZircoliteGUI\Index.html";
            string text = File.ReadAllText(path);

            string st = "<span>Copyright &copy; <a href=\"https://github.com/wagga40\">https://github.com/wagga40</a></span>";
            if (text.Contains(st))
            {
                string replacedText = text.Replace(st, "");
                File.WriteAllText(path, replacedText);
            }

            text = File.ReadAllText(path);
            if(text.Contains("<div class=\"col-lg-12\">"))
            {
                string temp = text.Substring(text.IndexOf("<div class=\"col-lg-12\">"), 1092);
                string replacedText = text.Replace(temp, "");
                File.WriteAllText(path, replacedText);
            }

            //Add Cummunication zircolite html side script
            text = File.ReadAllText(path);
            string newScript = @"<script>
let height;
const sendPostMessage = () => {
  if (height !== document.getElementById('content').scrollHeight) {
    height = document.getElementById('content').scrollHeight;
    window.parent.postMessage({
      frameHeight: height
    }, '*');
    console.log(height) // check the message is being sent correctly
  }
}

  window.onload = () => sendPostMessage();
  window.onresize = () => sendPostMessage();
  document.getElementById('other').onclick = () => sendPostMessage();
  document.getElementById('reconnaissance').onclick = () => sendPostMessage();
  document.getElementById('resource_development').onclick = () => sendPostMessage();
  document.getElementById('initial_access').onclick = () => sendPostMessage();
  document.getElementById('persistence').onclick = () => sendPostMessage();
  document.getElementById('privilege_escalation').onclick = () => sendPostMessage();
  document.getElementById('defense_evasion').onclick = () => sendPostMessage();
  document.getElementById('credential_access').onclick = () => sendPostMessage();
  document.getElementById('discovery').onclick = () => sendPostMessage();
  document.getElementById('lateral_movement').onclick = () => sendPostMessage();
  document.getElementById('collection').onclick = () => sendPostMessage();
  document.getElementById('command_and_control').onclick = () => sendPostMessage();
  document.getElementById('exfiltration').onclick = () => sendPostMessage();
  document.getElementById('impact').onclick = () => sendPostMessage();
  document.getElementById('execution').onclick = () => sendPostMessage();
  document.getElementById('low').onclick = () => sendPostMessage();
  document.getElementById('medium').onclick = () => sendPostMessage();
  document.getElementById('high').onclick = () => sendPostMessage();
  document.getElementById('critical').onclick = () => sendPostMessage();
</script>";
            if(!text.Contains(newScript))
            {
                string newText = text.Insert(text.LastIndexOf("</html>"), newScript + "\n");
                File.WriteAllText(path, newText);
            }

            */
            //Add html communication pingcastle report side
            string path = "";
            var d = new DirectoryInfo(Directory.GetCurrentDirectory());
            foreach (FileInfo fi in d.GetFiles())
                if(fi.Name.Contains("ad_hc") && fi.Name.Contains("html"))
                {
                    path = Directory.GetCurrentDirectory() + @"\" + fi.Name;
                    break;
                }
            
            string text = File.ReadAllText(path);
            string newScript = @"<script>
window.onmessage = (e) => {
  if (e.data.hasOwnProperty(""frameHeight"")) {
    document.getElementById(""iFrameSucks"").style.height = String(e.data.frameHeight - 200) + ""px"";;
        }
  if (e.data.hasOwnProperty(""click"")) {
  window.open(""RuntimeModules/ZircoliteGUI/Index.html"")
        }
    };
</script>";

            string newText2 = text.Insert(text.LastIndexOf("</body>"), newScript + "\n");
            File.WriteAllText(path, newText2);
        }
    }
}

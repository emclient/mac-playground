using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using MailClient.Common.UI;

namespace FormsTest
{
    public partial class TextBoxForm : Form
    {
        public TextBoxForm()
        {
            InitializeComponent();

            // Mac layout debugging
            //this.DebugAllControls();
        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var systemAssembly = typeof(System.Net.WebClient).Assembly;
			var loggingType = systemAssembly.GetType("System.Net.Logging", false);

			var nonPublicStaticProperties = loggingType.GetMembers(System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			var nonPublicStaticFields = loggingType.GetMembers(System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			var nonPublicStaticMethods = loggingType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

			textbox4.AppendText("\n\nNon Public Static Properties:");
			foreach (var p in nonPublicStaticProperties)
				textbox4.AppendText($"\n{p.Name}");

			textbox4.AppendText("\n\nNon Public Static Fileds:");
			foreach (var f in nonPublicStaticFields)
				textbox4.AppendText($"\n{f.Name}");

			textbox4.AppendText("\n\nNon Public Static Methods:");
			foreach (var m in nonPublicStaticMethods)
				textbox4.AppendText($"\n{m.Name}()");

			// textbox5
			//textbox5.Rtf = "{\\rtf1\\ansi{\\fonttbl\\f0\\fswiss Helvetica;}\\f0\\pard\\line This is some {\\b bold} text.\\par}";
			textbox5.Rtf = "{\\rtf1\\ansi\\ansicpg1252\\cocoartf1504\\cocoasubrtf830{\\fonttbl\\f0\\fswiss\\fcharset0 Helvetica;}{\\colortbl;\\red255\\green255\\blue255;\\red71\\green71\\blue71;\\red0\\green0\\blue0;}{\\*\\expandedcolortbl; ;\\cssrgb\\c34902\\c34902\\c34902;\\cssrgb\\c0\\c0\\c0;}\\deftab720\\pard\\pardeftab720\\partightenfactor0\\f0\\fs26 \\cf2 \\expnd0\\expndtw0\\kerning0V\\'a0r\\'e1mci produkce {\\b ultrazvukov\\'fdch odpuzova\\uc0\\u269 \\u367}  najdete v na\\'9a\\'ed nab\\'eddce odpuzova\\u269 e (pla\\'9ai\\u269 e) hlodavc\\u367  a kun, kter\\'e9 pracuj\\'ed s ultrazvukov\\'fdm sign\\'e1lem nastaven\\'fdm vysoko nad hranic\\'ed sly\\'9aitelnosti pro \\u269 lov\\u283 ka, ale velmi nep\\u345 \\'edjemn\\'fdm pr\\'e1v\\u283  pro kuny a drobn\\'e9 hlodavce. Doporu\\u269 en\\'fdm typem z t\\'e9to kategorie je \\'a0{\\field{\\*\\fldinst{HYPERLINK \"http://www.deramax.cz/deramax-profi-ultrazvukovy-odpuzovac-plasic-kun-a-hlodavcu\"}}{\\fldrslt \\ul odpuzova\\uc0\\u269  kun a hlodavc\\u367  Deramax-Profi}}\\'a0. Obdobn\\'e9 p\\uc0\\u345 \\'edstroje vyr\\'e1b\\'edme i s nastaven\\'edm proti nap\\u345 . toulav\\'fdm ps\\u367 m a ko\\u269 k\\'e1m. \\'a0{\\field{\\*\\fldinst{HYPERLINK \"http://www.deramax.cz/deramax-kitty-ultrazvukovy-odpuzovac-plasic-kocek-a-psu\"}}{\\fldrslt \\ul Odpuzova\\uc0\\u269  (pla\\'9ai\\u269 ) ps\\u367  a ko\\u269 ek Deramax-Kitty}}\\'a0 neru\\'9a\\'ed sv\\'e9 okol\\'ed nep\\uc0\\u345 \\'edjemn\\'fdm hlukem, ale ochr\\'e1n\\'ed prostor vymezen\\'fd odpuzova\\u269 em p\\u345 ed pohybem nev\\'edtan\\'fdch ko\\u269 ek a ps\\u367 . Do automobil\\u367  pak vyr\\'e1b\\'edme speci\\'e1ln\\'ed typ odoln\\'e9ho \\'a0{\\field{\\*\\fldinst{HYPERLINK \"http://www.deramax.cz/deramax-auto-ultrazvukovy-odpuzovac-plasic-kun-a-hlodavcu-do-auta\"}}{\\fldrslt \\ul odpuzova\\uc0\\u269 e kun a my\\'9a\\'ed Deramax-Auto}}\\'a0, kter\\'fd udr\\'9e\\'ed 9V baterie v chodu po dobu a\\'9e 2 let! D\\'e1le najdete v na\\'9a\\'ed nab\\'eddce tak\\'e9 odpuzova\\uc0\\u269 e (pla\\'9ai\\u269 e) pt\\'e1k\\u367 , divok\\'e9 zv\\u283 \\u345 e, \\u269 i netop\\'fdr\\u367 . Pro pou\\'9eit\\'ed na zahrad\\u283 , jako ochranu tr\\'e1vn\\'edku nebo u\\'9eitkov\\'e9 zahrady vyr\\'e1b\\'edme {\\field{\\*\\fldinst{HYPERLINK \"http://www.deramax.cz/deramax-cvrcek-elektronicky-odpuzovac-plasic-krtku-a-hryzcu\"}}{\\fldrslt \\ul elektronick\\'fd odpuzova\\uc0\\u269  (pla\\'9ai\\u269 ) krtk\\u367 , hryzc\\u367  a hrabo\\'9a\\u367  Deramax-Cvr\\u269 ek}}\\'a0elektronick\\'fd odpuzova\\uc0\\u269  (pla\\'9ai\\u269 ) krtk\\u367 , hryzc\\u367  a hrabo\\'9a\\u367  Deramax-Cvr\\u269 ek.}{\\rtf1\\ansi\\ansicpg1252\\cocoartf1504\\cocoasubrtf830{\\fonttbl\\f0\\fswiss\\fcharset0 Helvetica;}{\\colortbl;\\red255\\green255\\blue255;\\red71\\green71\\blue71;\\red0\\green0\\blue0;}{\\*\\expandedcolortbl; ;\\cssrgb\\c34902\\c34902\\c34902;\\cssrgb\\c0\\c0\\c0;}\\deftab720\\pard\\pardeftab720\\partightenfactor0\\f0\\fs26 \\cf2 \\expnd0\\expndtw0\\kerning0V\\'a0r\\'e1mci produkce ultrazvukov\\'fdch odpuzova\\uc0\\u269 \\u367  najdete v na\\'9a\\'ed nab\\'eddce odpuzova\\u269 e (pla\\'9ai\\u269 e) hlodavc\\u367  a kun, kter\\'e9 pracuj\\'ed s ultrazvukov\\'fdm sign\\'e1lem nastaven\\'fdm vysoko nad hranic\\'ed sly\\'9aitelnosti pro \\u269 lov\\u283 ka, ale velmi nep\\u345 \\'edjemn\\'fdm pr\\'e1v\\u283  pro kuny a drobn\\'e9 hlodavce. Doporu\\u269 en\\'fdm typem z t\\'e9to kategorie je \\'a0{\\field{\\*\\fldinst{HYPERLINK \"http://www.deramax.cz/deramax-profi-ultrazvukovy-odpuzovac-plasic-kun-a-hlodavcu\"}}{\\fldrslt \\ul odpuzova\\uc0\\u269  kun a hlodavc\\u367  Deramax-Profi}}\\'a0. Obdobn\\'e9 p\\uc0\\u345 \\'edstroje vyr\\'e1b\\'edme i s nastaven\\'edm proti nap\\u345 . toulav\\'fdm ps\\u367 m a ko\\u269 k\\'e1m. \\'a0{\\field{\\*\\fldinst{HYPERLINK \"http://www.deramax.cz/deramax-kitty-ultrazvukovy-odpuzovac-plasic-kocek-a-psu\"}}{\\fldrslt \\ul Odpuzova\\uc0\\u269  (pla\\'9ai\\u269 ) ps\\u367  a ko\\u269 ek Deramax-Kitty}}\\'a0 neru\\'9a\\'ed sv\\'e9 okol\\'ed nep\\uc0\\u345 \\'edjemn\\'fdm hlukem, ale ochr\\'e1n\\'ed prostor vymezen\\'fd odpuzova\\u269 em p\\u345 ed pohybem nev\\'edtan\\'fdch ko\\u269 ek a ps\\u367 . Do automobil\\u367  pak vyr\\'e1b\\'edme speci\\'e1ln\\'ed typ odoln\\'e9ho \\'a0{\\field{\\*\\fldinst{HYPERLINK \"http://www.deramax.cz/deramax-auto-ultrazvukovy-odpuzovac-plasic-kun-a-hlodavcu-do-auta\"}}{\\fldrslt \\ul odpuzova\\uc0\\u269 e kun a my\\'9a\\'ed Deramax-Auto}}\\'a0, kter\\'fd udr\\'9e\\'ed 9V baterie v chodu po dobu a\\'9e 2 let! D\\'e1le najdete v na\\'9a\\'ed nab\\'eddce tak\\'e9 odpuzova\\uc0\\u269 e (pla\\'9ai\\u269 e) pt\\'e1k\\u367 , divok\\'e9 zv\\u283 \\u345 e, \\u269 i netop\\'fdr\\u367 . Pro pou\\'9eit\\'ed na zahrad\\u283 , jako ochranu tr\\'e1vn\\'edku nebo u\\'9eitkov\\'e9 zahrady vyr\\'e1b\\'edme {\\field{\\*\\fldinst{HYPERLINK \"http://www.deramax.cz/deramax-cvrcek-elektronicky-odpuzovac-plasic-krtku-a-hryzcu\"}}{\\fldrslt \\ul elektronick\\'fd odpuzova\\uc0\\u269  (pla\\'9ai\\u269 ) krtk\\u367 , hryzc\\u367  a hrabo\\'9a\\u367  Deramax-Cvr\\u269 ek}}\\'a0elektronick\\'fd odpuzova\\uc0\\u269  (pla\\'9ai\\u269 ) krtk\\u367 , hryzc\\u367  a hrabo\\'9a\\u367  Deramax-Cvr\\u269 ek.}";
		}

		private void button1_Click(object sender, System.EventArgs e)
        {
			Console.WriteLine(textbox5.Rtf);
			//MessageBox.Show(textbox5.Rtf, "RTF", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
    }
}

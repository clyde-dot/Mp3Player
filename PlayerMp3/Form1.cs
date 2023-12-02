using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;
namespace PlayerMp3
{
    public partial class Form1 : Form
    {
        //Ссылка на расположение музыки
        string path;
        string linkFilePath = $"C:\\Users\\{Environment.UserName}\\Documents\\path.txt";

        //Работа с музыкой
        WindowsMediaPlayer player = new WindowsMediaPlayer();
        //Получаем все ссылки в музыки
        public string[] allfiles;
        List<string> playList = new List<string>();
        List<string> musicName = new List<string>();

        //Индекс музыки 
        int music = 0;
        //Вид Play/Pause
        static int playerMode = 0;

        //При загрузке формы
        public Form1()
        {
            InitializeComponent();

            try
            {
                //Проверка ссылки
                if (File.ReadAllText(linkFilePath) != String.Empty)
                {
                    //Ставим ссылку из документа
                    path = File.ReadAllText(linkFilePath);
                    
                }
                else
                {
                    //Ставим обычную ссылку в отдел музыки
                    path = $"C:\\Users\\{Environment.UserName}\\Music";
                    File.WriteAllText(linkFilePath,path);
                }
            }
            catch
            {
                MessageBox.Show("Что-то пошло не так :(");
            }
            linkGetFiles(path);
            musicCheck();
            
            player.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(WMP_PlayStateChange);

        }
        //Button Play
        private void btnPlay_Click(object sender, EventArgs e)
        {
            listBox1.SelectedIndex = music;
            playOrStopMode();
        }

        //Button Next
        private void btnNext_Click(object sender, EventArgs e)
        {
            musicNext();
            listBox1.SelectedIndex = music;
        }

        //Button Undo
        private void btnUndo_Click(object sender, EventArgs e)
        {
            
            if (playList.Count == 1)
            {
                player.URL = playList[music];
            }
            else
            {
                playerMode = 0;
                playOrStopMode();

                if (music == 0)
                    music = playList.Count - 1;
                else if (music <= playList.Count - 1)
                    music--;
                player.URL = playList[music];
                label2.Text = musicName[music];
            }
            listBox1.SelectedIndex = music;
        }

        //Button FolderSet
        private void btnFolderSet_Click(object sender, EventArgs e)
        {
            
            playerMode = 1;
            playOrStopMode();
            
            bool mode =linkCreate();
            if (mode)
            {
                
                //Получение ссылок
                linkGetFiles(path);
                //Очистка файлов
                playList.Clear();
                musicName.Clear();
                listBox1.Items.Clear();
                //Перепроверка 
                musicCheck();
                //Меняем меню
                music = 0;
                label2.Text = musicName[music];
            }
            else
            {
                playerMode = 0;
                playOrStopMode();
                player.controls.play();
            }
            
        }

        private void playOrStopMode()
        {
            switch (playerMode)
            {
                case 0:
                    playerMode = 1;
                    player.controls.play();
                    btnPlay.Text = " X";
                    
                    break;
                case 1:
                    playerMode = 0;
                    player.controls.pause();
                    btnPlay.Text = " ♫";
                    break;
                default:
                    break;
            }
        }
        //Music Time
        async void musicTime()
        {

            
            while (true)
            {
                progressBar1.Maximum = (int)player.controls.currentItem.duration;
                progressBar1.Value = (int)player.controls.currentPosition;

                lblEndMusicTime.Text = player.controls.currentItem.durationString;
                if (playerMode == 1)
                {
                    lblMusicTime.Text = player.controls.currentPositionString;
                }

                
                await Task.Delay(1000);
            }
        }

        //Выбераем путь к музыке
        private bool linkCreate()
        {
            bool reNameMode = false;
            try
            {
                using (FolderBrowserDialog dlg = new FolderBrowserDialog())
                {
                    dlg.Description = "Select a folder";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        MessageBox.Show("Успешно сохранено  !", "Successfully !", MessageBoxButtons.OK,MessageBoxIcon.Information);
                        path = dlg.SelectedPath.ToString();
                        savePath(linkFilePath);
                        reNameMode = true;
                    }
                    
                }
                
            }
            catch
            {
                MessageBox.Show("Ошибка 404 ! Сообщите разработчику !","Error ",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                Close();
            }
            return reNameMode;
        }

        //Сохраняем путь в документе
        private void savePath(string folder)
        {
            File.WriteAllText(folder, String.Empty);
            File.WriteAllText(folder, path);
        }

        //Чтение и сохранение 
        private void musicCheck()
        {
            foreach (var item in allfiles)
            {
                playList.Add(item);

            }
            foreach (var item in playList)
            {
                musicName.Add(Path.GetFileName(item));
            }
            for (int i = 0; i < musicName.Count; i++)
            {
                listBox1.Items.Add(musicName[i]);
            }
            if (playList.Count != 0)
            {
                
                player.URL = playList[music];
                player.controls.stop();
                label2.Text = musicName[music];
                player.settings.volume = trackBar1.Value;
            }
        }
        //Фильтруем mp3
        //public bool filter(string musicName)
        //{
        //    bool filterResult;
        //    filterResult = musicName.EndsWith(".mp3") ? true : false;
        //    return filterResult;
        //}
        //Уровень громкости 
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            player.settings.volume = trackBar1.Value;
            label3.Text = $"Громкость {trackBar1.Value}%";
        }
        //Получение всех ссылок
        private void linkGetFiles(string fileToPath)
        {
            try
            {

                if (Directory.GetFiles(fileToPath, "*.mp3").Length != 0)
                    allfiles = Directory.GetFiles(fileToPath, "*.mp3");
                else
                {
                    MessageBox.Show("Путь не содержит нужных файлов \n The path does not contain the necessary files","Ошибка !",MessageBoxButtons.OK,MessageBoxIcon.Hand);
                    linkCreate();
                    Application.Restart();
                }
            }
            catch
            {
                
                DialogResult result=MessageBox.Show("Выбранный путь до этого был утерян ! \n Пожалуйста переопределите путь !","Ошибка !", MessageBoxButtons.OKCancel,MessageBoxIcon.Error);
                if (result == DialogResult.OK)
                {
                    btnFolderSet_Click(null, null);
                    
                }

                
            }
            
        }
        //Next Function
        private void musicNext()
        {
            if (playList.Count == 1)
            {
                player.URL = playList[music];
            }
            else
            {
                playerMode = 0;
                playOrStopMode();
                if (music == playList.Count - 1)
                    music = 0;
                else if (music < playList.Count - 1)
                    music++;
                player.URL = playList[music];
                label2.Text = musicName[music];

            }
            
        }
        
        //Проверка окончания музыки 
        void WMP_PlayStateChange(int NewState)
        {
            switch (player.playState)
            {
                case WMPPlayState.wmppsMediaEnded:
                    if (listBox1.SelectedIndex != listBox1.Items.Count - 1)
                    {
                        BeginInvoke(new Action(() => {
                            listBox1.SelectedIndex = listBox1.SelectedIndex + 1;
                        }));
                    }
                    break;
                
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            musicTime();
            cmBoxOpacity.SelectedIndex = 0;
            cmBoxOpacity.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void cmBoxOpacity_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmBoxOpacity.Text)
            {
                case "100":
                    this.Opacity = 1.00;
                    break;
                case "95":
                    this.Opacity = 0.95;
                    break;
                case "90":
                    this.Opacity = 0.90;
                    break;
                case "85":
                    this.Opacity = 0.85;
                    break;
                case "80":
                    this.Opacity = 0.80;
                    break;
                case "75":
                    this.Opacity = 0.75;
                    break;
                case "70":
                    this.Opacity = 0.70;
                    break;
                case "65":
                    this.Opacity = 0.65;
                    break;
                case "60":
                    this.Opacity = 0.60;
                    break;
                case "55":
                    this.Opacity = 0.55;
                    break;
                case "50":
                    this.Opacity = 0.50;
                    break;
                case "45":
                    this.Opacity = 0.45;
                    break;
                case "40":
                    this.Opacity = 0.40;
                    break;
                case "35":
                    this.Opacity = 0.35;
                    break;
                case "30":
                    this.Opacity = 0.30;
                    break;
                case "25":
                    this.Opacity = 0.25;
                    break;
                case "20":
                    this.Opacity = 0.20;
                    break;
                case "15":
                    this.Opacity = 0.15;
                    break;
                default:
                    this.Opacity = 1.00;
                    break;
            }
        }

        

        private void progressBar1_MouseDown(object sender, MouseEventArgs e)
        {
            player.controls.currentPosition = player.currentMedia.duration * e.X / progressBar1.Width;
        }
        //Play Set Listbox Music
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            player.URL = playList[listBox1.SelectedIndex];
            music = listBox1.SelectedIndex;
            label2.Text = musicName[music];
            //////
            player.controls.play();
        }

        private void checkWindow_CheckedChanged(object sender, EventArgs e)
        {
            if (checkWindow.Checked)
                this.FormBorderStyle = FormBorderStyle.None;
            else
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            btnColor.Text=Convert.ToString(player.controls.currentPosition);

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }
    }
}

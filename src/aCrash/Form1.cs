using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace aCrash
{
    public partial class pHelper : Form
    {
        bool[] cPlus = { true, true, true };
        int[] cCode = { 0x00, 0x80, 0xFE };

        Point point;
        bool mov = false;

        Memory m = new Memory();

        public pHelper()
        {
            InitializeComponent();
        }

        private void colorTimer_Tick(object sender, EventArgs e)
        {
            for (int c = 0; c < 3; c++)
            {
                if (cCode[c] == 0xFF) cPlus[c] = false;
                if (cCode[c] == 0x00) cPlus[c] = true;
                if (cPlus[c]) cCode[c]++;
                if (!cPlus[c]) cCode[c]--;
            }
            BackColor = Color.FromArgb(cCode[0], cCode[1], cCode[2]);
            ForeColor = Color.FromArgb(0xFF - cCode[0], 0xFF - cCode[1], 0xFF - cCode[2]);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //pictureBox1.Image = weatherlist[comboBox1.SelectedIndex];
            if (!m.CH()) return;
            m.WriteMEM((IntPtr)0xC81320, weatherListComboBox.SelectedIndex);
        }

        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!mov) return;

            Panel p = (Panel)sender;
            p.Top += e.Y - point.Y;
            p.Left += e.X - point.X;
        }

        private void panel_MouseDown(object sender, MouseEventArgs e)
        {
            mov = true;
            point = e.Location;
        }

        private void panel_MouseUp(object sender, MouseEventArgs e)
        {
            if (!mov) return;
            mov = false;
        }

        private void weatherUpdateButton_Click(object sender, EventArgs e)
        {
            if (!m.CH()) return;
            weatherIdTextBox.Text = m.ReadDWORD((IntPtr)0xC81320).ToString();
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (!m.CH()) return;
            IntPtr settingsOffset = (IntPtr)m.ReadDWORD((IntPtr)m.ReadDWORD(m.dwSAMP + 0x20DFEC) + 0x3C6);
            m.WriteMEM(settingsOffset + 0x34, (timeListComboBox.SelectedIndex + 1) * 12);
        }

        private void weatherSetButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!m.CH()) return;
                m.WriteMEM((IntPtr)0xC81320, Int32.Parse(weatherIdTextBox.Text));
            }
            catch { MessageBox.Show("Произошла ошибка: ID не число", "pHelper"); }
        }

        private void timeUpdateButton_Click(object sender, EventArgs e)
        {
            if (!m.CH()) return;
            IntPtr settingsOffset = (IntPtr)m.ReadDWORD((IntPtr)m.ReadDWORD(m.dwSAMP + 0x20DFEC) + 0x3C6);
            timeIntTextBox.Text = m.ReadDWORD(settingsOffset + 0x34).ToString();
        }

        private void timeSetButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!m.CH()) return;
                IntPtr settingsOffset = (IntPtr)m.ReadDWORD((IntPtr)m.ReadDWORD(m.dwSAMP + 0x20DFEC) + 0x3C6);
                m.WriteMEM(settingsOffset + 0x34, Int32.Parse(timeIntTextBox.Text));
            }
            catch { MessageBox.Show("Произошла ошибка: время не число", "pHelper"); }
        }

        private void closeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (closeCheckBox.Checked)
                closeTimer.Start();
            else
                closeTimer.Stop();
        }

        private void closeTimer_Tick(object sender, EventArgs e)
        {
            if (m.IsKeyDown(Keys.LMenu) && (m.IsKeyDown(Keys.End)))
            {
                Process[] ps1 = Process.GetProcessesByName("grand_theft_auto_san_andreas.dll");
                foreach (Process p1 in ps1)
                    p1.Kill();
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x84:
                    base.WndProc(ref m);
                    if ((int)m.Result == 0x1)
                        m.Result = (IntPtr)0x2;
                    return;
            }
            base.WndProc(ref m);
        }

        private void antiCrashButton_Click(object sender, EventArgs e)
        {
            if (!m.CH()) return;
            m.WriteMEM(m.dwSAMP + 0x5A170, new byte[] { 0xC2, 0x0C, 0x00, 0x15, 0x9C }); // Антикрашер
            m.WriteMEM(m.dwSAMP + 0x5A1D9, new byte[] { 0x90 }); // Антикрашер
            m.WriteMEM(m.dwSAMP + 0x5A1E1, new byte[] { 0x90, 0x90, 0x90, 0x90 }); // Антикрашер
            m.WriteMEM(m.dwSAMP + 0x12A80, new byte[] { 0xC3, 0x90 }); // NOP SetPlayerDrunkLevel
            m.WriteMEM((IntPtr)0x635DA0, new byte[] { 0xB8, 0x00, 0x00, 0x00, 0x00, 0xC3, 0x90 }); // Смерть от парашюта
            m.WriteMEM((IntPtr)0x00544CFE, new byte[] { 0xD9, 0x05, 0x24, 0x86, 0x85, 0x00, 0xD8, 0x64, 0x24, 0x04 }); // Зависания при полете
            m.WriteMEM((IntPtr)0x005E91CE, new byte[] { 0xC7, 0x46, 0x4C, 0x00, 0x00, 0x80, 0x3E }); // Зависания при полете
            m.WriteMEM((IntPtr)0x5BCD50, new byte[] { 0x90 }); // Фикс памяти
            m.WriteMEM((IntPtr)0x5BCD78, new byte[] { 0x90 }); // Фикс памяти
            m.WriteMEM((IntPtr)0x5B8E64, new byte[] { 0x90 }); // Фикс памяти
            m.WriteMEM((IntPtr)0x74872D, new byte[] { 0x90 }); // Многоядерность
            m.WriteMEM((IntPtr)0x5BCD50, new byte[] { 0x90 }); // Многоядерность
            m.WriteMEM((IntPtr)0x406946, new byte[] { 0x00 }); // Многоядерность
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Process.Start("https://vk.com/iddrygok");
        }
    }


}
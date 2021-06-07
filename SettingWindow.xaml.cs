using System;
using System.Windows;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;

namespace toolstray
{
    /// <summary>
    /// 設定画面
    /// </summary>
    public partial class SettingWindow : Window, IWindow
    {
        public static SettingWindow ins;
        // 全ショートカット
        static Dictionary<Key, IProc> _shortcut = null;
        // Alt+Win のショートカット
        static Dictionary<Key, IProc> _shortcutAltWin = null;
        // Ctrl のショートカット
        static Dictionary<Key, IProc> _shortcutAlt = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SettingWindow()
        {
            ins = this;
            InitializeComponent();

            // 設定ファイル読み込み
            textBox.Text = _ReadSCSettings();
        }

        /// <summary>
        /// デフォルト値設定
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">データ</param>
        void _Default(object sender, RoutedEventArgs e)
        {
            textBox.Text = _default;
        }

        /// <summary>
        /// Alt のショートカットをパースした内容取得
        /// </summary>
        public static Dictionary<Key, IProc> Shortcut
        {
            get
            {
                if (_shortcut == null)
                {
                    _shortcut = new Dictionary<Key, IProc>()
                        .Concat(ShortcutCtrl)
                        .Concat(ShortcutAltWin)
                        .ToDictionary(p => p.Key, p => p.Value);
                }
                return _shortcut;
            }
        }

        /// <summary>
        /// Alt+Win のショートカットをパースした内容取得
        /// </summary>
        public static Dictionary<Key, IProc> ShortcutAltWin
        {
            get
            {
                if (_shortcutAltWin == null)
                {
                    _shortcutAltWin = _Parse();
                }
                return _shortcutAltWin;
            }
        }

        /// <summary>
        /// Ctrl のショートカットをパースした内容取得
        /// </summary>
        public static Dictionary<Key, IProc> ShortcutCtrl
        {
            get
            {
                if (_shortcutAlt == null)
                {
                    _shortcutAlt = _Parse(isCtrl: true);
                }
                return _shortcutAlt;
            }
        }

        /// <summary>
        /// パース
        /// </summary>
        /// <param name="isCtrl">true:Ctrlキー false: Alt + win</param>
        /// <returns>キーと処理の辞書</returns>
        static Dictionary<Key, IProc> _Parse(bool isCtrl = false)
        {
            var keyToProc = new Dictionary<Key, IProc>();

            var iWinType = typeof(IWindow);
            var winTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => iWinType.IsAssignableFrom(p))
                .ToList();
            var iScType = typeof(IShortcut);
            var scTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => iScType.IsAssignableFrom(p))
                .ToList();

            var fileContent = _ReadSCSettings();
            var lines = fileContent.Split('\n');
            foreach (var line in File.ReadAllLines(AppDefines.PATH_SHORTCUT))
            {
                var modLine = line.ReplaceAll("\\s+", " ");
                var eles = modLine.Split(' ');
                if (eles.Length >= 2)
                {
                    // 先頭が % のときは Ctrl、無い時は Win + Alt
                    var keyStr = _ModifyKeyStr(eles[0], isCtrl);
                    if (keyStr != null)
                    {
                        var key = keyStr switch
                        {
                            "esc" => Key.Escape,
                            "space" => Key.Space,
                            ";" => Key.OemSemicolon,
                            "," => Key.OemComma,
                            "." => Key.OemPeriod,
                            string s => (Key)Enum.Parse(typeof(Key), s, true)
                        };

                        var procStr = eles[1];
                        var winType = winTypes.Where(e => e.Name == procStr).FirstOrDefault();
                        var scType = scTypes.Where(e => e.Name == procStr).FirstOrDefault();
                        if (winType != null)
                        {
                            object proc = null;
                            if (eles.Length >= 3)
                            {
                                var args = eles[2..^0];

                                proc = Activator.CreateInstance(winType, args);
                            }
                            else
                            {
                                proc = Activator.CreateInstance(winType);
                            }
                            keyToProc[key] = proc as IProc;
                        }
                        else if (scType != null)
                        {
                            keyToProc[key] = Activator.CreateInstance(scType) as IProc;
                        }
                        else if (procStr.StartsWith("http"))
                        {
                            keyToProc[key] = new OpenURL(procStr);
                        }
                        else if (procStr.StartsWith("ime"))
                        {
                            keyToProc[key] = new ImeSwitch();
                        }
                        else
                        {
                            keyToProc[key] = new Exe(procStr, eles.Length <= 2);
                        }
                    }
                }
            }

            return keyToProc;
        }

        /// <summary>
        /// キー文字列を状態によって変更
        /// </summary>
        /// <param name="keyStr">キー文字列</param>
        /// <param name="isAltOnly">true:Altキーだけ false: Alt + win</param>
        /// <returns>キー文字列</returns>
        static string _ModifyKeyStr(string keyStr, bool isAltOnly)
        {
            if (isAltOnly)
            {
                if (keyStr[0] == '%')
                {
                    return keyStr.Remove(0, 1);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (keyStr[0] == '%')
                {
                    return null;
                }
            }
            return keyStr;
        }

        /// <summary>
        /// ショートカット設定取得
        /// </summary>
        /// <returns>ファイル内容</returns>
        static string _ReadSCSettings()
        {
            if (Directory.Exists(AppDefines.WK_PATH) == false)
            {
                Directory.CreateDirectory(AppDefines.WK_PATH);
            }
            if (File.Exists(AppDefines.PATH_SHORTCUT) == false)
            {
                File.WriteAllText(AppDefines.PATH_SHORTCUT, _default);
            }
            return File.ReadAllText(AppDefines.PATH_SHORTCUT);
        }

        /// <summary>
        /// キーマップのデフォルト値
        /// </summary>
        static string _default => @"esc SettingWindow
h GoogleWindow
i https://translate.google.co.jp/?hl=ja&sl=en&tl=ja&op=translate
u https://translate.google.co.jp/?hl=ja&sl=ja&tl=en&op=translate
%space ime 
";

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender">送信元</param>
        /// <param name="e">データ</param>
        void _Save(object sender, RoutedEventArgs e)
        {
            var text = textBox.Text;
            File.WriteAllText(AppDefines.PATH_SHORTCUT, text);
        }
    }
}

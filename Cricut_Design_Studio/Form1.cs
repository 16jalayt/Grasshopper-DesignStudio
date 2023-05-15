using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Cricut_Design_Studio.com.cricut.webservice;
using Cricut_Design_Studio.WebReference;
using FileDialogExtender;
using Microsoft.Win32;
using SingleInstance;

namespace Cricut_Design_Studio
{
    public class Form1 : Form, IMessageFilter
    {
        public class RijndaelSimple
        {
            public static string Encrypt(string plainText, string passPhrase, string saltValue, string hashAlgorithm, int passwordIterations, string initVector, int keySize)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(initVector);
                byte[] bytes2 = Encoding.ASCII.GetBytes(saltValue);
                byte[] array = new byte[plainText.Length / 2];
                try
                {
                    for (int i = 0; i < plainText.Length; i += 2)
                    {
                        array[i / 2] = byte.Parse(plainText.Substring(i, 2), NumberStyles.HexNumber);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return "";
                }
                PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(passPhrase, bytes2, hashAlgorithm, passwordIterations);
                byte[] bytes3 = passwordDeriveBytes.GetBytes(keySize / 8);
                RijndaelManaged rijndaelManaged = new RijndaelManaged();
                rijndaelManaged.Mode = CipherMode.CBC;
                ICryptoTransform transform = rijndaelManaged.CreateEncryptor(bytes3, bytes);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
                cryptoStream.Write(array, 0, array.Length);
                cryptoStream.FlushFinalBlock();
                byte[] array2 = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                string text = "";
                for (int j = 0; j < array2.Length; j++)
                {
                    text += array2[j].ToString("X").PadLeft(2, '0');
                }
                return text;
            }

            public static string Decrypt(string cipherText, string passPhrase, string saltValue, string hashAlgorithm, int passwordIterations, string initVector, int keySize)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(initVector);
                byte[] bytes2 = Encoding.ASCII.GetBytes(saltValue);
                byte[] array = new byte[cipherText.Length / 2];
                try
                {
                    for (int i = 0; i < cipherText.Length; i += 2)
                    {
                        array[i / 2] = byte.Parse(cipherText.Substring(i, 2), NumberStyles.HexNumber);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return "";
                }
                PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(passPhrase, bytes2, hashAlgorithm, passwordIterations);
                byte[] bytes3 = passwordDeriveBytes.GetBytes(keySize / 8);
                RijndaelManaged rijndaelManaged = new RijndaelManaged();
                rijndaelManaged.Mode = CipherMode.CBC;
                ICryptoTransform transform = rijndaelManaged.CreateDecryptor(bytes3, bytes);
                MemoryStream memoryStream = new MemoryStream(array);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
                byte[] array2 = new byte[array.Length];
                int num = 0;
                try
                {
                    num = cryptoStream.Read(array2, 0, array2.Length);
                    cryptoStream.Close();
                }
                catch
                {
                }
                memoryStream.Close();
                string text = "";
                for (int j = 0; j < num; j++)
                {
                    text += array2[j].ToString("X").PadLeft(2, '0');
                }
                return text;
            }
        }

        public class ProjectProperties
        {
            public enum FontLibraryMode
            {
                Favorites,
                All_by_Category,
                My_Cartridges,
                Keywords,
                This_Project
            }

            public enum Category
            {
                unassigned,
                Family,
                Holiday,
                Seasonal,
                Vacation,
                Baby,
                Life_Event,
                Party,
                Miscellaneous,
                Teen,
                Sports,
                School,
                Home
            }

            public enum Difficulty
            {
                unassigned,
                Super_Easy,
                Easy,
                Moderate,
                Advanced,
                Super_Advanced
            }

            public string filename;

            public string name;

            public string authorName;

            public string description;

            public DateTime dateTime;

            public Category category;

            public Difficulty difficulty;

            public Bitmap thumbnail;

            public ProjectProperties()
            {
                name = "untitled project";
                authorName = "none";
                description = "none";
                dateTime = DateTime.Now;
                category = Category.unassigned;
                difficulty = Difficulty.unassigned;
            }

            public bool read(BinaryReader br)
            {
                bool flag = true;
                do
                {
                    string strA = null;
                    try
                    {
                        strA = br.ReadString();
                    }
                    catch
                    {
                        flag = false;
                    }
                    if (string.Compare(strA, "Name") == 0)
                    {
                        name = br.ReadString();
                    }
                    else if (string.Compare(strA, "AuthorName") == 0)
                    {
                        authorName = br.ReadString();
                    }
                    else if (string.Compare(strA, "DateTime") == 0)
                    {
                        dateTime = DateTime.Parse(br.ReadString());
                    }
                    else if (string.Compare(strA, "Description") == 0)
                    {
                        description = br.ReadString();
                    }
                    else if (string.Compare(strA, "Category") == 0)
                    {
                        category = (Category)Enum.Parse(typeof(Category), br.ReadString(), ignoreCase: false);
                    }
                    else if (string.Compare(strA, "Difficulty") == 0)
                    {
                        difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), br.ReadString(), ignoreCase: false);
                    }
                    else if (string.Compare(strA, "ProjectPropertiesEnd") == 0)
                    {
                        break;
                    }
                }
                while (flag);
                return flag;
            }
        }

        private class NudgeAutorepeatTimer : System.Windows.Forms.Timer
        {
            private Form1 form;

            private float nudgeDirectionX;

            private float nudgeDirectionY;

            public NudgeAutorepeatTimer(Form1 f, float ndx, float ndy)
            {
                base.Interval = 1000;
                form = f;
                nudgeDirectionX = ndx;
                nudgeDirectionY = ndy;
                base.Tick += Timer_Tick;
            }

            public void Timer_Tick(object sender, EventArgs e)
            {
                form.nudge(nudgeDirectionX, nudgeDirectionY);
                base.Interval = 100;
            }
        }

        private class Fake
        {
            public string size;

            public string keyname;

            public string name;

            public string value;

            public string addr;
        }

        private class ButtonState
        {
            public bool pressed;

            public bool mouseOver;

            public Bitmap normalImg;

            public Bitmap hotImg;

            public Bitmap pressedImg;

            public int baselineAdj;
        }

        private const string passPhrase = "SG8WU1J8e4GZp4F6q8nOi1bC5w2XcR3q5y8P";

        private const string initVector = "trI1lylmsEPWoGTe";

        private const string DefaultPageName = "untitled page";

        private const int WM_KEYDOWN = 256;

        private const int WM_KEYUP = 257;

        private const int WM_LEFTMOUSEDOWN = 513;

        private const int WM_LEFTMOUSEUP = 514;

        private const int WM_LEFTMOUSEDBL = 515;

        private const int WM_SYSKEYDOWN = 260;

        private const int WM_SYSKEYUP = 261;

        public static RegistryKey progKey = null;

        public static RegistryKey userKey = null;

        public ProjectProperties projectProperties = new ProjectProperties();

        public string appDataFolderPath;

        public string exeFolderPath;

        public string userDataFolderPath;

        public string fontFolderPath;

        public string imageFolderPath;

        public string projectFolderPath;

        public ArrayList fromtagsList;

        public int fromtagsIndex;

        public Bitmap featuresPicBox_backStore;

        public Bitmap fontPicBox_backStore;

        public CuttingProgressForm cuttingProgress;

        private global::FileDialogExtender.FileDialogExtender _extender = new global::FileDialogExtender.FileDialogExtender();

        public string searchTag1 = "";

        public string searchTag2 = "";

        public static Form1 myRootForm = null;

        public FontLoading fontLoading;

        public bool trialMode = true;

        public int matSize;

        public int viewIndex;

        public int multiCut;

        public bool enableSounds = true;

        public bool showRuler;

        public bool keepPreview;

        public bool paperSaver;

        public bool realSize;

        public bool enableBalloonHelp = true;

        public bool shiftLock;

        public float smallMoveValue = 1f / 64f;

        public float largeMoveValue = 0.125f;

        public float nudgeValue = 1f / 64f;

        private bool keepShapeAspectRatio;

        private float shapeAspectRatio;

        private System.Windows.Forms.Timer nudgeTimer;

        public float sizeValue = 1f;

        public int sizeCounter = 12;

        private Color darkMenuText = Color.FromArgb(35, 50, 74);

        private Color lightMenuText = Color.FromArgb(53, 75, 111);

        private Color trackBarBkgnd = Color.FromArgb(255, 248, 219);

        private Color buttonBackColor = Color.FromArgb(176, 211, 147);

        private Point cartridgeLibraryPanelLocation;

        private Point keypadPanelLocation;

        private Point propertiesPanelLocation;

        private Size matTabControlSize;

        public bool messageBoxOpen;

        private CustomTrackBar sizeTrackBar;

        private static Fake fake = new Fake();

        private static string fakeMACAddress = null;

        private string traceFilename;

        private int matTabControlRightMargin;

        private int matTabControlBottomMargin;

        private bool loadWasCalled;

        public bool projectDirty;

        private PointF mouseDownPnt = new PointF(0f, 0f);

        private PointF mouseMovePnt = new PointF(0f, 0f);

        private bool mouseDown;

        private int fontPicBox_mouseX;

        private int fontPicBox_mouseY;

        private int lastKeyId = -1;

        private PictureBox hoverBox = new PictureBox();

        private bool fontPicBox_inside;

        private static int utilityTimerMode = 0;

        public UpdatingProgressForm updatingProgress;

        public bool updateStatus;

        private long updateStartCounter;

        private long updateFreq;

        private int lastSecs;

        public bool weAreKerning;

        private IContainer components;

        private ToolStripPanel BottomToolStripPanel;

        private ToolStripPanel TopToolStripPanel;

        private ToolStripPanel RightToolStripPanel;

        private ToolStripPanel LeftToolStripPanel;

        private ToolStripContentPanel ContentPanel;

        private Panel keypadPanel;

        private Label label6;

        private Label label5;

        private Label label4;

        private Label label3;

        private Label label2;

        private Label label1;

        private Button revertShapePropertiesButton;

        private Button applyKerningButton;

        private Label label9;

        public TreeView fontTreeView;

        private Label key2Label;

        private Label key1Label;

        public ComboBox fontsChooseByComboBox;

        private Label viewLabel;

        private Panel cartridgeLibraryPanel;

        public Button nextKeywordButton;

        private MenuStrip menuStrip1;

        private Panel panel2;

        private Panel panel10;

        private Panel panel16;

        private Panel panel14;

        private Panel panel15;

        private Label backspaceKeyLabel;

        private Label spaceKeyLabel;

        private Panel keypadOutlinePanel;

        private Panel panel67;

        public Button prevKeywordButton;

        private Panel menuStripSeparator;

        private ToolStripMenuItem fileToolStripMenuItem;

        private ToolStripMenuItem newProjectMenuItem;

        private ToolStripMenuItem openProjectMenuItem;

        private ToolStripSeparator toolStripSeparator;

        private ToolStripMenuItem saveProjectMenuItem;

        private ToolStripMenuItem saveAsProjectMenuItem;

        private ToolStripSeparator toolStripSeparator1;

        private ToolStripMenuItem cutWithCricutMenuItem;

        private ToolStripMenuItem exitMenuItem;

        private ToolStripMenuItem editToolStripMenuItem;

        private ToolStripSeparator toolStripSeparator3;

        private ToolStripMenuItem helpToolStripMenuItem;

        private ToolStripMenuItem checkForUpdatesMenuItem;

        private ToolStripMenuItem storeLocatorMenuItem;

        private ToolStripMenuItem aboutToolStripMenuItem;

        private ToolStripMenuItem closeProjectMenuItem;

        private ToolStripSeparator toolStripSeparator6;

        private ToolStripMenuItem viewToolStripMenuItem;

        private ToolStripMenuItem previewMenuItem;

        private ToolStripMenuItem deleteShapesMenuItem;

        private ToolStripMenuItem deleteAllOnThisPageToolStripMenuItem;

        private ToolStripMenuItem keepPreviewAsMatBackgroundToolStripMenuItem;

        private ToolStripMenuItem showRulerToolStripMenuItem;

        private ToolStripSeparator toolStripSeparator7;

        private ToolStripMenuItem helpMenuItem;

        private ToolStripSeparator toolStripSeparator8;

        private ToolStripMenuItem matViewToolStripMenuItem;

        private ToolStripMenuItem fitToPageToolStripMenuItem;

        private ToolStripMenuItem percent100ToolStripMenuItem;

        private ToolStripMenuItem percent200ToolStripMenuItem;

        private ToolStripMenuItem prefsToolStripMenuItem;

        private ToolStripMenuItem multiCutToolStripMenuItem;

        private ToolStripMenuItem multiCut1ToolStripMenuItem;

        private ToolStripMenuItem multiCut2ToolStripMenuItem;

        private ToolStripMenuItem multiCut3ToolStripMenuItem;

        private ToolStripMenuItem multiCut4ToolStripMenuItem;

        private ToolStripMenuItem newPageToolStripMenuItem;

        private ToolStripMenuItem deletePageToolStripMenuItem;

        private ToolStripSeparator toolStripSeparator9;

        private ToolStripMenuItem clearPreviewMenuItem;

        private ToolStripSeparator toolStripSeparator5;

        private ToolStripMenuItem updateFirmwareMenuItem;

        private ToolStripSeparator toolStripSeparator10;

        private Label label13;

        private Label label7;

        private Label label14;

        private Label label15;

        private Button rotate90Button;

        private Label label16;

        private ToolStripMenuItem matSizeToolStripMenuItem;

        private ToolStripMenuItem x6ToolStripMenuItem;

        private ToolStripMenuItem x12ToolStripMenuItem;

        private ToolStripMenuItem x24ToolStripMenuItem;

        private ToolStripSeparator toolStripSeparator4;

        private CustomTabControl matTabControl;

        public Label sizeLabel;

        private CricutButton newPageButton;

        private CricutButton copyGroupButton;

        private CricutButton pasteGroupButton;

        private CricutButton deleteGroupButton;

        private CricutButton previewButton;

        private ToolTip form1ToolTips;

        private ToolStripMenuItem enableBalloonHelpMenuItem;

        private ToolStripSeparator toolStripSeparator11;

        private CricutButton fitToPageButton;

        private CricutButton percent100Button;

        private CricutButton percent200Button;

        public ImageList fontImageList;

        public ComboBox fontsTag2ComboBox;

        public ComboBox fontsTag1ComboBox;

        public PictureBox featuresPicBox;

        public PictureBox fontPicBox;

        public ToolStripMenuItem fontFeaturesMenuItem;

        public Panel propertiesPanel;

        private System.Windows.Forms.Timer hoverTimer;

        public Label fontNameLabel;

        private ToolStripSeparator toolStripSeparator12;

        public ToolStripMenuItem myCartridgeMenuItem;

        public ToolStripMenuItem favoriteCartridgeMenuItem;

        private TabPage activateByInternetTabPage;

        private Panel trialVersionPanel;

        private Label label8;

        private Button continueTrialButton;

        private Button activateByInternetButton;

        public Label shiftKeyLabel;

        private SaveFileDialog saveProjectDialog;

        private OpenFileDialog openProjectDialog;

        private ContextMenuStrip glyphContextMenu;

        private ContextMenuStrip pageContextMenu;

        private ToolStripTextBox pageNameTextBox;

        private ToolStripMenuItem changePageNameMenuItem;

        private ToolStripSeparator toolStripSeparator14;

        private ToolStripSeparator toolStripSeparator15;

        private ToolStripMenuItem orderSubMenuItem;

        private ToolStripMenuItem bringPagetoFrontMenuItem;

        private ToolStripMenuItem sendPagetoBackMenuItem;

        private ToolStripMenuItem bringPageForwardMenuItem;

        private ToolStripMenuItem sendPageBackwardMenuItem;

        public Button nudgeLeftButton;

        public Button nudgeRightButton;

        public ToolStripMenuItem showFontMenuItem;

        private ToolStripSeparator toolStripSeparator17;

        private ToolStripMenuItem addKeywordMenuItem;

        private ToolStripMenuItem removeKeywordMenuItem;

        public ToolStripComboBox glyphKeywordComboBox;

        private ToolStripMenuItem pagePreviewMenuItem;

        public TextBox xPositionTextBox;

        public TextBox shearTextBox;

        public TextBox angleTextBox;

        public TextBox heightTextBox;

        public TextBox widthTextBox;

        public TextBox yPositionTextBox;

        public CheckBox flipShapesCheckBox;

        public CheckBox weldingCheckBox;

        public TextBox kerningTextBox;

        public CricutButton cricutCutButton;

        private TextBox iSerialNum5;

        private TextBox iSerialNum2;

        private TextBox iSerialNum1;

        private GroupBox groupBox1;

        private Label label18;

        private Label label17;

        private Label label12;

        private TextBox iSerialNum3;

        private Label label11;

        private TextBox iSerialNum4;

        private Label label10;

        private TextBox iSerialNum6;

        private Label label19;

        private Label label28;

        private TextBox iFirstNameTextBox;

        private Label label31;

        private Label label30;

        private Label label29;

        private TextBox iEmailTextBox;

        private TextBox iLastNameTextBox;

        private Label label32;

        private TabPage activateByPhoneTabPage;

        private Panel panel1;

        private Label label34;

        private Label label35;

        private Label label36;

        private TextBox pEmailTextBox;

        private TextBox pLastNameTextBox;

        private TextBox pFirstNameTextBox;

        private Label label37;

        private GroupBox groupBox3;

        private Label label38;

        private Label label39;

        private TextBox actKey8;

        private TextBox actKey7;

        private TextBox actKey6;

        private TextBox actKey5;

        private TextBox actKey4;

        private TextBox actKey3;

        private TextBox actKey2;

        private Label label40;

        private TextBox actKey1;

        private Label label41;

        private Label label42;

        private Label label43;

        private Label label44;

        private Label label46;

        private GroupBox groupBox4;

        private TextBox pSerialNum2;

        private Label label47;

        private TextBox pSerialNum1;

        private Label label48;

        private TextBox pSerialNum5;

        private Label label49;

        private TextBox pSerialNum3;

        private Label label50;

        private TextBox pSerialNum4;

        private Label label51;

        private TextBox pSerialNum6;

        private Label label52;

        private Label label20;

        private Button activateByPhoneButton;

        private GroupBox groupBox2;

        private Label label21;

        private Button verifyActKeyButton;

        private GroupBox groupBox5;

        private Label activationPhoneNumberLabel;

        private Label label24;

        private Label activationKeyStatusLabel;

        private Button pContinueTrialButton;

        private Button goBackToInternetActivationButton;

        private Label registrationCodeLabel;

        private TabPage activationCompleteTabPage;

        private Label label22;

        private Button activationCompleteOKButton;

        private Label label57;

        private GroupBox groupBox6;

        private Label label27;

        private Label label33;

        private TextBox aActKey8;

        private TextBox aActKey7;

        private TextBox aActKey6;

        private TextBox aActKey5;

        private TextBox aActKey4;

        private TextBox aActKey3;

        private TextBox aActKey2;

        private Label label45;

        private TextBox aActKey1;

        private Label label53;

        private Label label54;

        private Label label55;

        private Label label56;

        private Label label23;

        private Label label25;

        private Label label26;

        private TextBox aEmailTextBox;

        private TextBox aLastNameTextBox;

        private TextBox aFirstNameTextBox;

        private GroupBox groupBox7;

        private TextBox aSerialNum2;

        private Label label58;

        private TextBox aSerialNum1;

        private Label label59;

        private TextBox aSerialNum5;

        private Label label60;

        private TextBox aSerialNum3;

        private Label label61;

        private TextBox aSerialNum4;

        private Label label62;

        private TextBox aSerialNum6;

        private System.Windows.Forms.Timer utilityTimer;

        public BackgroundWorker cuttingBackgroundWorker;

        public CheckBox realSizeCheckBox;

        public CheckBox paperSaverCheckBox;

        private TabPage updateCricutFirmwareTabPage;

        private Label label63;

        private Label label64;

        private RadioButton v2RadioButton;

        private RadioButton v1RadioButton;

        private Label label65;

        private Label label66;

        private Button beginUpdatingButton;

        private Button updateLaterButton;

        public BackgroundWorker firmwareBackgroundWorker;

        private Label successfulUpdateLabel;

        private ToolStripMenuItem pageColorMenuItem;

        public ToolStripMenuItem copyMenuItem;

        public ToolStripMenuItem pasteMenuItem;

        private ToolStripMenuItem revertAllMenuItem;

        private ToolStripSeparator toolStripSeparator13;

        public ToolStripMenuItem undoToolStripMenuItem;

        public ToolStripMenuItem redoToolStripMenuItem;

        private Panel panel3;

        private Label label68;

        private Label label67;

        private Panel panel4;

        public Button nudgeDownButton;

        public Button nudgeUpButton;

        private Button keepAspectRatioButton;

        private RadioButton v3RadioButton;

        private ToolStripSeparator toolStripSeparator2;

        private ToolStripMenuItem invisContourMenuItem;

        private ToolStripSeparator toolStripSeparator16;

        private ToolStripMenuItem nextShapeToolStripMenuItem;

        private ToolStripSeparator toolStripSeparator18;

        private ToolStripMenuItem loadPaperToolStripMenuItem;

        private ToolStripMenuItem unloadPaperToolStripMenuItem;

        private Label label71;

        private Label label69;

        private Label label70;

        private ToolStripSeparator toolStripSeparator20;

        private Panel cartridgeLibraryHeaderPanel;

        private Panel propertiesHeaderPanel;

        public bool shiftKeyDown;

        public bool ctrlKeyDown;

        private bool allowLocalFocus;

        public int textBoxKeyChar = -1;

        public static void setRegistryValue(RegistryKey key, string name, string value)
        {
            key.SetValue(name, value);
        }

        public static string getRegistryValue(RegistryKey key, string name)
        {
            if (key == null)
            {
                return null;
            }
            return (string)key.GetValue(name);
        }

        public bool setComboBoxFromReg(ComboBox cb, RegistryKey key, string name)
        {
            string registryValue = getRegistryValue(key, name);
            if (registryValue != null)
            {
                for (int i = 0; i < cb.Items.Count; i++)
                {
                    if (registryValue.CompareTo((string)cb.Items[i]) == 0)
                    {
                        cb.SelectedIndex = i;
                        return true;
                    }
                }
            }
            return false;
        }

        public object checkMenuItemFromReg(ToolStripMenuItem parentMenu, RegistryKey key, string name)
        {
            string registryValue = getRegistryValue(key, name);
            object result = null;
            if (registryValue != null)
            {
                foreach (ToolStripMenuItem dropDownItem in parentMenu.DropDownItems)
                {
                    if (registryValue.CompareTo(dropDownItem.Text) == 0)
                    {
                        dropDownItem.Checked = true;
                        result = dropDownItem;
                    }
                    else
                    {
                        dropDownItem.Checked = false;
                    }
                }
                return result;
            }
            return result;
        }

        private void initRegistry()
        {
            RegistryKey registryKey = Registry.LocalMachine;
            try
            {
                registryKey = registryKey.OpenSubKey("SOFTWARE", writable: true);
                progKey = registryKey.OpenSubKey("Cognitive Devices", writable: true);
                if (progKey != null)
                {
                    progKey = progKey.OpenSubKey("Cricut PC", writable: true);
                }
            }
            catch
            {
                registryKey = registryKey.OpenSubKey("SOFTWARE", writable: false);
                progKey = registryKey.OpenSubKey("Cognitive Devices", writable: false);
                if (progKey != null)
                {
                    progKey = progKey.OpenSubKey("Cricut PC", writable: false);
                }
            }
            registryKey = Registry.CurrentUser;
            registryKey = registryKey.OpenSubKey("Software", writable: true);
            userKey = registryKey.OpenSubKey("Cognitive Devices", writable: true);
            if (userKey == null)
            {
                userKey = registryKey.CreateSubKey("Cognitive Devices");
            }
            registryKey = userKey;
            userKey = registryKey.OpenSubKey("Cricut PC", writable: true);
            if (userKey == null)
            {
                userKey = registryKey.CreateSubKey("Cricut PC");
                setRegistryValue(userKey, "currentFont", "George and Basic Shapes");
                setRegistryValue(userKey, "currentFontFeature", "0");
                setRegistryValue(userKey, "allowTagging", "False");
            }
            registryKey = userKey;
            string text = checkProgThenUserKey("regNum");
            string text2 = checkProgThenUserKey("actNum");
            if (text != null && text2 != null)
            {
                verifyActivation(text, text2);
            }
        }

        private void verifyActivation(string regNumStr, string actNumStr)
        {
            /*if (regNumStr == null)
            {
                regNumStr = checkProgThenUserKey("regNum");
            }
            if (actNumStr == null)
            {
                actNumStr = checkProgThenUserKey("actNum");
            }
            if (regNumStr != null && 41 == regNumStr.Length && actNumStr != null && 39 == actNumStr.Length)
            {
                regNumStr = regNumStr.Replace("-", "").Substring(0, 30);
                actNumStr = actNumStr.Replace("-", "");
                string macAddr = getMACAddress().Replace(":", "");
                string text = checkActKey(actNumStr, macAddr);
                if (text.CompareTo(regNumStr) == 0)
                {
                    trialMode = false;
                    PcCache.trialMode = false;
                }
            }*/
            trialMode = false;
            PcCache.trialMode = false;
        }

        private void saveActData(bool actOkay)
        {
            string regNumFromTextBoxes = getRegNumFromTextBoxes();
            string actNumFromTextBoxes = getActNumFromTextBoxes();
            string text = "";
            string text2 = "";
            string text3 = "";
            int num = 0;
            if (activateByInternetTabPage == matTabControl.SelectedTab)
            {
                text = iFirstNameTextBox.Text;
                text2 = iLastNameTextBox.Text;
                text3 = iEmailTextBox.Text;
            }
            else if (activateByPhoneTabPage == matTabControl.SelectedTab)
            {
                text = pFirstNameTextBox.Text;
                text2 = pLastNameTextBox.Text;
                text3 = pEmailTextBox.Text;
            }
            setRegistryValue(userKey, "firstName", text);
            setRegistryValue(userKey, "lastName", text2);
            setRegistryValue(userKey, "email", text3);
            setRegistryValue(userKey, "regNum", regNumFromTextBoxes);
            setRegistryValue(userKey, "actNum", actNumFromTextBoxes);
            setRegistryValue(userKey, "size", fake.size);
            if (!actOkay)
            {
                return;
            }
            using (Process process = new Process())
            {
                text = escapeArgument(text);
                text2 = escapeArgument(text2);
                text3 = escapeArgument(text3);
                regNumFromTextBoxes = escapeArgument(regNumFromTextBoxes);
                actNumFromTextBoxes = escapeArgument(actNumFromTextBoxes);
                process.StartInfo.FileName = exeFolderPath + "\\Cricut Registration Helper.exe";
                process.StartInfo.Verb = "runas";
                process.StartInfo.Arguments = "\"" + text + "\"";
                ProcessStartInfo startInfo = process.StartInfo;
                startInfo.Arguments = startInfo.Arguments + " \"" + text2 + "\"";
                ProcessStartInfo startInfo2 = process.StartInfo;
                startInfo2.Arguments = startInfo2.Arguments + " \"" + text3 + "\"";
                ProcessStartInfo startInfo3 = process.StartInfo;
                startInfo3.Arguments = startInfo3.Arguments + " \"" + regNumFromTextBoxes + "\"";
                ProcessStartInfo startInfo4 = process.StartInfo;
                startInfo4.Arguments = startInfo4.Arguments + " \"" + actNumFromTextBoxes + "\"";
                ProcessStartInfo startInfo5 = process.StartInfo;
                startInfo5.Arguments = startInfo5.Arguments + " \"" + fake.size + "\"";
                ProcessStartInfo startInfo6 = process.StartInfo;
                startInfo6.Arguments = startInfo6.Arguments + " \"" + fake.keyname + "\"";
                ProcessStartInfo startInfo7 = process.StartInfo;
                startInfo7.Arguments = startInfo7.Arguments + " \"" + fake.name + "\"";
                ProcessStartInfo startInfo8 = process.StartInfo;
                startInfo8.Arguments = startInfo8.Arguments + " \"" + fake.value + "\"";
                try
                {
                    process.Start();
                    process.WaitForExit();
                    num = process.ExitCode;
                }
                catch
                {
                    num = -1;
                }
                if (num != 0)
                {
                    MessageBox.Show("Cricut Registration Helper failed!\nError code is A" + num);
                }
            }
        }

        private string escapeArgument(string arg)
        {
            char[] array = arg.ToCharArray();
            arg = "";
            char[] array2 = array;
            foreach (char c in array2)
            {
                if (c >= ' ' && c < '\u007f')
                {
                    arg += c;
                }
            }
            arg = arg.Replace("\"", "\\\"");
            return arg;
        }

        private string getRegNumFromTextBoxes()
        {
            string text = "";
            if (activateByInternetTabPage == matTabControl.SelectedTab)
            {
                text = text + iSerialNum1.Text.PadLeft(6, '0') + "-";
                text = text + iSerialNum2.Text.PadLeft(6, '0') + "-";
                text = text + iSerialNum3.Text.PadLeft(6, '0') + "-";
                text = text + iSerialNum4.Text.PadLeft(6, '0') + "-";
                text = text + iSerialNum5.Text.PadLeft(6, '0') + "-";
                text += iSerialNum6.Text.PadLeft(6, '0');
            }
            else if (activateByPhoneTabPage == matTabControl.SelectedTab)
            {
                text = text + pSerialNum1.Text.PadLeft(6, '0') + "-";
                text = text + pSerialNum2.Text.PadLeft(6, '0') + "-";
                text = text + pSerialNum3.Text.PadLeft(6, '0') + "-";
                text = text + pSerialNum4.Text.PadLeft(6, '0') + "-";
                text = text + pSerialNum5.Text.PadLeft(6, '0') + "-";
                text += pSerialNum6.Text.PadLeft(6, '0');
            }
            return text;
        }

        private string getActNumFromTextBoxes()
        {
            string text = "";
            text = text + actKey1.Text.PadLeft(4, '0') + "-";
            text = text + actKey2.Text.PadLeft(4, '0') + "-";
            text = text + actKey3.Text.PadLeft(4, '0') + "-";
            text = text + actKey4.Text.PadLeft(4, '0') + "-";
            text = text + actKey5.Text.PadLeft(4, '0') + "-";
            text = text + actKey6.Text.PadLeft(4, '0') + "-";
            text = text + actKey7.Text.PadLeft(4, '0') + "-";
            return text + actKey8.Text.PadLeft(4, '0');
        }

        private void setRegNum(string s)
        {
            if (s != null && s.Length >= 36)
            {
                s = s.Replace("-", "");
                TextBox textBox = aSerialNum1;
                TextBox textBox2 = iSerialNum1;
                string text2 = (pSerialNum1.Text = s.Substring(0, 6));
                string text5 = (textBox.Text = (textBox2.Text = text2));
                TextBox textBox3 = aSerialNum2;
                TextBox textBox4 = iSerialNum2;
                string text7 = (pSerialNum2.Text = s.Substring(6, 6));
                string text10 = (textBox3.Text = (textBox4.Text = text7));
                TextBox textBox5 = aSerialNum3;
                TextBox textBox6 = iSerialNum3;
                string text12 = (pSerialNum3.Text = s.Substring(12, 6));
                string text15 = (textBox5.Text = (textBox6.Text = text12));
                TextBox textBox7 = aSerialNum4;
                TextBox textBox8 = iSerialNum4;
                string text17 = (pSerialNum4.Text = s.Substring(18, 6));
                string text20 = (textBox7.Text = (textBox8.Text = text17));
                TextBox textBox9 = aSerialNum5;
                TextBox textBox10 = iSerialNum5;
                string text22 = (pSerialNum5.Text = s.Substring(24, 6));
                string text25 = (textBox9.Text = (textBox10.Text = text22));
                TextBox textBox11 = aSerialNum6;
                TextBox textBox12 = iSerialNum6;
                string text27 = (pSerialNum6.Text = s.Substring(30, 6));
                string text30 = (textBox11.Text = (textBox12.Text = text27));
            }
        }

        private void setActNum(string s)
        {
            if (s != null && s.Length >= 32)
            {
                s = s.Replace("-", "");
                string text3 = (aActKey1.Text = (actKey1.Text = s.Substring(0, 4)));
                string text6 = (aActKey2.Text = (actKey2.Text = s.Substring(4, 4)));
                string text9 = (aActKey3.Text = (actKey3.Text = s.Substring(8, 4)));
                string text12 = (aActKey4.Text = (actKey4.Text = s.Substring(12, 4)));
                string text15 = (aActKey5.Text = (actKey5.Text = s.Substring(16, 4)));
                string text18 = (aActKey6.Text = (actKey6.Text = s.Substring(20, 4)));
                string text21 = (aActKey7.Text = (actKey7.Text = s.Substring(24, 4)));
                string text24 = (aActKey8.Text = (actKey8.Text = s.Substring(28, 4)));
            }
        }

        private string createActNum(string regNum, string macAddr)
        {
            return RijndaelSimple.Encrypt(regNum, "SG8WU1J8e4GZp4F6q8nOi1bC5w2XcR3q5y8P", macAddr, "SHA1", 2, "trI1lylmsEPWoGTe", 128);
        }

        private string checkActKey(string actKey, string macAddr)
        {
            return RijndaelSimple.Decrypt(actKey, "SG8WU1J8e4GZp4F6q8nOi1bC5w2XcR3q5y8P", macAddr, "SHA1", 2, "trI1lylmsEPWoGTe", 128);
        }

        public string checkProgThenUserKey(string keyname)
        {
            string text = null;
            if (progKey != null)
            {
                text = getRegistryValue(progKey, keyname);
            }
            if (text == null)
            {
                text = getRegistryValue(userKey, keyname);
            }
            return text;
        }

        private void loadRegistrySettings()
        {
            object obj = null;
            if (!setComboBoxFromReg(fontsChooseByComboBox, userKey, "fontView"))
            {
                fontsChooseByComboBox.SelectedIndex = 1;
            }
            fontsChooseByComboBox_SelectedIndexChanged(fontsChooseByComboBox, null);
            if ((obj = checkMenuItemFromReg(matSizeToolStripMenuItem, userKey, "matSize")) != null)
            {
                changeMatSize_Click(obj, null);
            }
            else
            {
                changeMatSize_Click(x6ToolStripMenuItem, null);
            }
            if ((obj = checkMenuItemFromReg(matViewToolStripMenuItem, userKey, "viewSize")) != null)
            {
                changeView_Click(obj, null);
            }
            else
            {
                changeView_Click(fitToPageToolStripMenuItem, null);
            }
            string registryValue = getRegistryValue(userKey, "paperSaver");
            if (registryValue != null && (string.Compare(registryValue, "True") == 0 || string.Compare(registryValue, "False") == 0))
            {
                paperSaverCheckBox.Checked = bool.Parse(registryValue);
            }
            registryValue = getRegistryValue(userKey, "realSize");
            if (registryValue != null && (string.Compare(registryValue, "True") == 0 || string.Compare(registryValue, "False") == 0))
            {
                realSizeCheckBox.Checked = bool.Parse(registryValue);
            }
            registryValue = getRegistryValue(userKey, "shapeSize");
            if (registryValue != null)
            {
                try
                {
                    sizeCounter = int.Parse(registryValue);
                }
                catch
                {
                    sizeCounter = 12;
                }
            }
            sizeTrackBar_ValueChanged(0);
            registryValue = getRegistryValue(userKey, "showRuler");
            if (registryValue != null && (string.Compare(registryValue, "True") == 0 || string.Compare(registryValue, "False") == 0))
            {
                showRulerToolStripMenuItem.Checked = (showRuler = bool.Parse(registryValue));
            }
            registryValue = getRegistryValue(userKey, "keepPreview");
            if (registryValue != null && (string.Compare(registryValue, "True") == 0 || string.Compare(registryValue, "False") == 0))
            {
                keepPreviewAsMatBackgroundToolStripMenuItem.Checked = (keepPreview = bool.Parse(registryValue));
            }
            registryValue = getRegistryValue(userKey, "realSize");
            if (registryValue != null && (string.Compare(registryValue, "True") == 0 || string.Compare(registryValue, "False") == 0))
            {
                realSizeCheckBox.Checked = (realSize = bool.Parse(registryValue));
            }
            registryValue = getRegistryValue(userKey, "paperSaver");
            if (registryValue != null && (string.Compare(registryValue, "True") == 0 || string.Compare(registryValue, "False") == 0))
            {
                paperSaverCheckBox.Checked = (paperSaver = bool.Parse(registryValue));
            }
            registryValue = getRegistryValue(userKey, "balloonHelp");
            if (registryValue != null && (string.Compare(registryValue, "True") == 0 || string.Compare(registryValue, "False") == 0))
            {
                enableBalloonHelpMenuItem.Checked = (enableBalloonHelp = bool.Parse(registryValue));
                form1ToolTips.Active = enableBalloonHelp;
            }
            registryValue = checkProgThenUserKey("firstName");
            if (registryValue != null)
            {
                string text3 = (iFirstNameTextBox.Text = (pFirstNameTextBox.Text = registryValue));
            }
            registryValue = checkProgThenUserKey("lastName");
            if (registryValue != null)
            {
                string text6 = (iLastNameTextBox.Text = (pLastNameTextBox.Text = registryValue));
            }
            registryValue = checkProgThenUserKey("email");
            if (registryValue != null)
            {
                string text9 = (iEmailTextBox.Text = (pEmailTextBox.Text = registryValue));
            }
            if (progKey != null)
            {
                registryValue = getRegistryValue(progKey, "actPhoneNum");
                if (registryValue != null)
                {
                    activationPhoneNumberLabel.Text = registryValue;
                }
            }
            registryValue = checkProgThenUserKey("regNum");
            if (registryValue != null)
            {
                setRegNum(registryValue.Replace("-", ""));
            }
            registryValue = checkProgThenUserKey("actNum");
            if (registryValue != null)
            {
                setActNum(registryValue.Replace("-", ""));
            }
            trace("LR 0001");
            registryValue = getMACAddress();
            if (registryValue != null)
            {
                registrationCodeLabel.Text = registryValue.Replace(":", "");
            }
            activationKeyStatusLabel.Visible = false;
            trace("LR 0002");
        }

        private void createCartridgeLibraryPanel(string mode)
        {
            bool flag = false;
            cartridgeLibraryPanel.SuspendLayout();
            cartridgeLibraryPanel.Controls.Clear();
            if (mode.CompareTo("Favorites") == 0)
            {
                fontLoading.showFontList(FontLoading.FontLibraryMode.Favorites, null, myRootForm.fontLoading.shapes);
            }
            else if (mode.CompareTo("All by Category") == 0)
            {
                fontLoading.showFontList(FontLoading.FontLibraryMode.All_by_Category, null, myRootForm.fontLoading.shapes);
            }
            else if (mode.CompareTo("My Cartridges") == 0)
            {
                fontLoading.showFontList(FontLoading.FontLibraryMode.My_Cartridges, null, myRootForm.fontLoading.shapes);
            }
            else if (mode.CompareTo("Keywords") == 0)
            {
                flag = true;
                setKeywordSelection();
            }
            else if (mode.CompareTo("This Project") == 0)
            {
                ArrayList fontNames = new ArrayList();
                ArrayList shapes = new ArrayList();
                foreach (Control control in matTabControl.Controls)
                {
                    TabPage tabPage = (TabPage)control;
                    PictureBox pictureBox = (PictureBox)tabPage.Tag;
                    if (pictureBox == null)
                    {
                        continue;
                    }
                    Canvas canvas = (Canvas)pictureBox.Tag;
                    foreach (SceneGroup sceneGroup in canvas.sceneGroups)
                    {
                        sceneGroup.listCarts(fontNames, shapes);
                    }
                }
                fontLoading.showFontList(FontLoading.FontLibraryMode.This_Project, null, shapes);
            }
            int left = fontTreeView.Left;
            int num = 6;
            if (flag)
            {
                fontTreeView.Location = new Point(fontTreeView.Left, fontsTag2ComboBox.Top + fontsTag2ComboBox.Height + num);
                fontTreeView.Size = new Size(cartridgeLibraryPanel.Width - fontTreeView.Left - left, cartridgeLibraryPanel.Height - fontTreeView.Top - left);
                cartridgeLibraryPanel.Controls.Add(cartridgeLibraryHeaderPanel);
                cartridgeLibraryPanel.Controls.Add(prevKeywordButton);
                cartridgeLibraryPanel.Controls.Add(nextKeywordButton);
                cartridgeLibraryPanel.Controls.Add(fontTreeView);
                cartridgeLibraryPanel.Controls.Add(fontsChooseByComboBox);
                cartridgeLibraryPanel.Controls.Add(key2Label);
                cartridgeLibraryPanel.Controls.Add(viewLabel);
                cartridgeLibraryPanel.Controls.Add(fontsTag2ComboBox);
                cartridgeLibraryPanel.Controls.Add(fontsTag1ComboBox);
                cartridgeLibraryPanel.Controls.Add(key1Label);
            }
            else
            {
                fontTreeView.Location = new Point(fontTreeView.Left, fontsChooseByComboBox.Top + fontsChooseByComboBox.Height + num);
                fontTreeView.Size = new Size(cartridgeLibraryPanel.Width - fontTreeView.Left - left, cartridgeLibraryPanel.Height - fontTreeView.Top - left);
                cartridgeLibraryPanel.Controls.Add(cartridgeLibraryHeaderPanel);
                cartridgeLibraryPanel.Controls.Add(fontTreeView);
                cartridgeLibraryPanel.Controls.Add(fontsChooseByComboBox);
                cartridgeLibraryPanel.Controls.Add(viewLabel);
            }
            cartridgeLibraryPanel.ResumeLayout();
        }

        private void renderFeatures()
        {
            featuresPicBox_backStore = new Bitmap(featuresPicBox.Width, featuresPicBox.Height);
            Graphics g = Graphics.FromImage(featuresPicBox_backStore);
            fontLoading.showFeatures(g, featuresPicBox.Width / 2, featuresPicBox.Height / 3);
        }

        private void renderFonts()
        {
            fontPicBox_backStore = new Bitmap(fontPicBox.Width, fontPicBox.Height);
            Graphics g = Graphics.FromImage(fontPicBox_backStore);
            fontLoading.dspyFont(g, fontLoading.FontId, GlyphButtonWidth(), GlyphButtonHeight());
        }

        private void DrawControlBackingStore(Graphics graphics, Image bitmap, Control dest)
        {
            graphics.DrawImage(bitmap, 0, 0, dest.Width, dest.Height);
        }

        private void featuresPicBox_Paint(object sender, PaintEventArgs e)
        {
            if (featuresPicBox_backStore == null)
            {
                Cursor.Current = Cursors.WaitCursor;
                renderFeatures();
                Cursor.Current = Cursors.Default;
            }
            DrawControlBackingStore(e.Graphics, featuresPicBox_backStore, featuresPicBox);
        }

        private void fontPicBox_Paint(object sender, PaintEventArgs e)
        {
            if (fontPicBox_backStore == null)
            {
                Cursor.Current = Cursors.WaitCursor;
                renderFonts();
                Cursor.Current = Cursors.Default;
            }
            DrawControlBackingStore(e.Graphics, fontPicBox_backStore, fontPicBox);
        }

        private void cricut_LoadPaper()
        {
            PcControl pcControl = new PcControl();
            if (pcControl.bgn())
            {
                pcControl.gypsyCommand(1);
                pcControl.end();
            }
        }

        private void cricut_UnloadPaper()
        {
            PcControl pcControl = new PcControl();
            if (pcControl.bgn())
            {
                pcControl.gypsyCommand(3);
                pcControl.end();
            }
        }

        private void cutWithCricut()
        {
            if (cuttingBackgroundWorker.IsBusy || cuttingProgress != null || getCanvas() == null)
            {
                return;
            }
            PcControl pcControl = new PcCache();
            if (!pcControl.bgn())
            {
                if (pcControl.badUSBDriver)
                {
                    messageBoxOpen = true;
                    MessageBox.Show(myRootForm, "Your Cricut was not found.\n\nThe USB drivers that this computer uses to communicate with the Cricut\nmay be out of date or not properly installed. Please refer to the \"Getting Started\"\ndocumentation for help on properly connecting this computer to your Cricut.", "Cricut Not Found", MessageBoxButtons.OK);
                    messageBoxOpen = false;
                }
                else
                {
                    messageBoxOpen = true;
                    MessageBox.Show(myRootForm, "Your Cricut was not found.\n\nPlease make sure that your Cricut is connected by a USB cable to this computer.\nAnd that it is turned on and has the correct size mat and paper loaded.\n\nThen try your cut again.", "Cricut Not Found", MessageBoxButtons.OK);
                    messageBoxOpen = false;
                }
                return;
            }
            cutWithCricutMenuItem.Enabled = false;
            cricutCutButton.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            int machine = 0;
            int major = 0;
            int minor = 0;
            int ox = 0;
            int oy = 0;
            int cx = 0;
            int cy = 0;
            int cartIsProgrammed = 0;
            ushort err = 0;
            ushort paper = 0;
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            bool flag = false;
            switch (matSize)
            {
                case 0:
                    num3 = 115;
                    num4 = 55;
                    break;

                case 1:
                    num3 = 115;
                    num4 = 115;
                    break;

                case 2:
                    num3 = 235;
                    num4 = 115;
                    break;
            }
            USB_Interface.setTimeout(4000);
            bool flag2 = false;
            int num5 = 0;
            do
            {
                flag2 = pcControl.NOOP(out err, out paper);
            }
            while (!flag2 && num5++ < 3);
            if (!flag2)
            {
                flag = true;
            }
            else
            {
                pcControl.getCartridge(ref cartIsProgrammed);
                PcCache.cricutModel = 0;
                if (pcControl.getVersion(ref machine, ref major, ref minor))
                {
                    PcCache.cricutModel = machine;
                    flag = true;
                    if (20 == machine && major >= 2 && minor >= 34)
                    {
                        flag = false;
                    }
                    else if (10 == machine && major >= 1 && minor >= 34)
                    {
                        flag = false;
                    }
                    else if (15 == machine && major >= 3 && minor >= 4)
                    {
                        flag = false;
                    }
                }
            }
            if (flag)
            {
                messageBoxOpen = true;
                MessageBox.Show(myRootForm, "Your Cricut machine's firmware requires updating.\n\nPlease choose \"Update Cricut Firmware...\" from the Help menu and\nfollow the instructions to update your Cricut machine's firmware.\n\nThen try your cut again.", "Cricut Firmware Needs Updating", MessageBoxButtons.OK);
                messageBoxOpen = false;
                cutWithCricutMenuItem.Enabled = true;
                cricutCutButton.Enabled = true;
                pcControl.end();
            }
            else
            {
                if (pcControl.getCanvas(ref ox, ref oy, ref cx, ref cy))
                {
                    pcControl.PaperOriginX = ox;
                    pcControl.PaperOriginY = oy;
                    pcControl.PaperCornerX = cx;
                    pcControl.PaperCornerY = cy;
                    num = (cx - ox) * 10 / 404;
                    num2 = (cy - oy) * 10 / 404;
                }
                if (paper == 0)
                {
                    messageBoxOpen = true;
                    MessageBox.Show(myRootForm, "The mat and paper are not loaded.\n\nPlease make sure that your Cricut has the correct size mat and paper loaded.\n\nThen try your cut again.", "Paper Not Loaded", MessageBoxButtons.OK);
                    messageBoxOpen = false;
                    pcControl.end();
                }
                else if (num4 > num2 || num3 > num)
                {
                    messageBoxOpen = true;
                    MessageBox.Show(myRootForm, "The selected mat size is too large.\n\nThe mat size selected in the Cricut DesignStudio is larger than\nthe mat size loaded in the Cricut. Please select a smaller mat size\nor make sure that your Cricut has the correct size paper and mat loaded.\n\nThen try your cut again.", "Mat Size Too Large", MessageBoxButtons.OK);
                    messageBoxOpen = false;
                    pcControl.end();
                }
                else
                {
                    USB_Interface.setTimeout(10000);
                    switch (machine)
                    {
                        case 10:
                            getCanvas().cutInit(202, 290);
                            break;

                        case 15:
                            getCanvas().cutInit(202, 290);
                            break;

                        case 20:
                            getCanvas().cutInit(202, 214);
                            break;
                    }
                    if (!getCanvas().cut(pcControl))
                    {
                        messageBoxOpen = true;
                        MessageBox.Show(myRootForm, "A Cutting/Welding error has occured.\n\nSome shapes or parts of shapes may be skipped in your cut.", "Cutting/Welding Error", MessageBoxButtons.OK);
                        messageBoxOpen = false;
                    }
                    Cursor.Current = Cursors.Default;
                    switch (matSize)
                    {
                        case 0:
                            getCanvas().init(14f, 7f, 12f, 6f, 1f);
                            break;

                        case 1:
                            getCanvas().init(14f, 13f, 12f, 12f, 1f);
                            break;

                        case 2:
                            getCanvas().init(26f, 13f, 24f, 12f, 1f);
                            break;
                    }
                    cuttingBackgroundWorker.RunWorkerAsync(pcControl);
                    cuttingProgress = new CuttingProgressForm();
                    cuttingProgress.Text = "Cutting - 0% complete";
                    cuttingProgress.cuttingProgressBar.Value = 0;
                    cuttingProgress.ShowDialog(this);
                    if (cuttingBackgroundWorker.IsBusy)
                    {
                        cuttingBackgroundWorker.CancelAsync();
                    }
                }
            }
            pcControl = null;
            cuttingProgress = null;
            cutWithCricutMenuItem.Enabled = true;
            cricutCutButton.Enabled = true;
            myRootForm.Invalidate(invalidateChildren: true);
        }

        private void cuttingBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (cuttingProgress != null && !cuttingProgress.Disposing)
            {
                cuttingProgress.Close();
            }
        }

        private void cuttingBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _ = (BackgroundWorker)sender;
            if (cuttingProgress != null && !cuttingProgress.Disposing)
            {
                cuttingProgress.Text = "Cutting - " + e.ProgressPercentage + "% complete";
                cuttingProgress.cuttingProgressBar.Value = e.ProgressPercentage;
            }
        }

        private void cuttingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _ = (BackgroundWorker)sender;
            PcCache pcCache = (PcCache)e.Argument;
            pcCache.end();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            _extender.WndProc(ref m);
        }

        private void saveGypsyProject(string filename)
        {
            Cursor.Current = Cursors.WaitCursor;
            bool flag = Canvas.previewBitmap != null;
            bool flag2 = true;
            int num = 0;
            FileStream fileStream = new FileStream(filename, FileMode.Create);
            GypsyWriter gypsyWriter = new GypsyWriter(fileStream);
            Canvas.clearPreview();
            createPreview(drawBkgnd: true);
            gypsyWriter.WriteHeader(filename, myRootForm.matSize);
            foreach (Control control3 in matTabControl.Controls)
            {
                TabPage tabPage = (TabPage)control3;
                PictureBox pictureBox = (PictureBox)tabPage.Tag;
                if (pictureBox != null)
                {
                    Canvas canvas = (Canvas)pictureBox.Tag;
                    canvas.saveGypsy(gypsyWriter, flag2, num);
                    if (flag2)
                    {
                        num = 2;
                        flag2 = false;
                    }
                    else
                    {
                        num++;
                    }
                }
            }
            gypsyWriter.WriteLayersStart();
            flag2 = true;
            num = 0;
            foreach (Control control4 in matTabControl.Controls)
            {
                TabPage tabPage2 = (TabPage)control4;
                PictureBox pictureBox2 = (PictureBox)tabPage2.Tag;
                if (pictureBox2 != null)
                {
                    Canvas canvas2 = (Canvas)pictureBox2.Tag;
                    gypsyWriter.WriteLayer(canvas2.layerProperties.layerName, num, canvas2.layerProperties.includeInPreview);
                    if (flag2)
                    {
                        num = 2;
                        flag2 = false;
                    }
                    else
                    {
                        num++;
                    }
                }
            }
            gypsyWriter.WriteLayersEnd();
            gypsyWriter.WriteEOF();
            gypsyWriter.Close();
            fileStream.Close();
            projectProperties.filename = filename;
            saveProjectMenuItem.Enabled = true;
            Text = getAppName(Path.GetFileNameWithoutExtension(projectProperties.filename));
            Canvas.clearPreview();
            if (flag)
            {
                createPreview(drawBkgnd: false);
            }
            refreshMattePicBox();
            Cursor.Current = Cursors.Default;
        }

        private void saveProject(string filename)
        {
            if (!myRootForm.trialMode && string.Compare(Path.GetExtension(filename).ToLower(), ".gypsy") == 0)
            {
                saveGypsyProject(filename);
            }
            else
            {
                saveCDSProject(filename);
            }
        }

        private void saveCDSProject(string filename)
        {
            Cursor.Current = Cursors.WaitCursor;
            FileStream fileStream = new FileStream(filename, FileMode.Create);
            BinaryWriter binaryWriter = new BinaryWriter(fileStream);
            bool saveThumb = true;
            bool flag = Canvas.previewBitmap != null;
            Canvas.clearPreview();
            createPreview(drawBkgnd: true);
            binaryWriter.Write("Cricut DesignStudio File Version");
            binaryWriter.Write("1.0.0");
            binaryWriter.Write("ProjectPropertiesBegin");
            binaryWriter.Write("Name");
            binaryWriter.Write(projectProperties.name);
            binaryWriter.Write("AuthorName");
            binaryWriter.Write(projectProperties.authorName);
            binaryWriter.Write("DateTime");
            binaryWriter.Write(projectProperties.dateTime.ToString());
            binaryWriter.Write("Description");
            binaryWriter.Write(projectProperties.description);
            binaryWriter.Write("Category");
            binaryWriter.Write(projectProperties.category.ToString());
            binaryWriter.Write("Difficulty");
            binaryWriter.Write(projectProperties.difficulty.ToString());
            binaryWriter.Write("ProjectPropertiesEnd");
            foreach (Control control in matTabControl.Controls)
            {
                TabPage tabPage = (TabPage)control;
                PictureBox pictureBox = (PictureBox)tabPage.Tag;
                if (pictureBox != null)
                {
                    Canvas canvas = (Canvas)pictureBox.Tag;
                    canvas.save(binaryWriter, filename, saveThumb);
                    saveThumb = false;
                }
            }
            binaryWriter.Close();
            fileStream.Close();
            projectProperties.filename = filename;
            saveProjectMenuItem.Enabled = true;
            Text = getAppName(Path.GetFileNameWithoutExtension(projectProperties.filename));
            Canvas.clearPreview();
            if (flag)
            {
                createPreview(drawBkgnd: false);
            }
            refreshMattePicBox();
            Cursor.Current = Cursors.Default;
            cleanProject();
        }

        private void saveAsProjectButton_Click(object sender, EventArgs e)
        {
            saveProjectDialog.DefaultExt = ".cut";
            if (myRootForm.trialMode)
            {
                saveProjectDialog.Filter = "Cricut files (*.cut)|*.cut|All files (*.*)|*.*";
            }
            else
            {
                saveProjectDialog.Filter = "Cricut files (*.cut)|*.cut|Gypsy files (*.gypsy)|*.gypsy|All files (*.*)|*.*";
            }
            saveProjectDialog.InitialDirectory = projectFolderPath;
            saveProjectDialog.FileName = projectProperties.filename;
            if (saveProjectDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveProjectDialog.FileName;
                saveProject(fileName);
            }
        }

        private void openGypsyProject(string filename)
        {
            SceneGroup.CutCartRecord.reset();
            Canvas.clearPreview();
            StreamReader streamReader = File.OpenText(filename);
            string text;
            try
            {
                streamReader.ReadLine();
                streamReader.ReadLine();
                text = streamReader.ReadLine();
            }
            catch (Exception ex)
            {
                messageBoxOpen = true;
                MessageBox.Show(myRootForm, "Cricut DesignStudio cannot read that file.\nThe file may be corrupt or empty. The error that Windows returned is:\n\n" + ex.Message, "Cannot Read File", MessageBoxButtons.OK);
                messageBoxOpen = false;
                streamReader?.Close();
                return;
            }
            matTabControl.Hide();
            matTabControl.Controls.Clear();
            addEmptyLayer();
            addLayer("untitled page");
            int num = matTabControl.Controls.Count - 1;
            TabPage tabPage = (TabPage)matTabControl.Controls[num];
            PictureBox pictureBox = (PictureBox)tabPage.Tag;
            Canvas canvas = (Canvas)pictureBox.Tag;
            char[] separator = new char[1] { ' ' };
            string[] array = text.Split(separator);
            bool flag = false;
            bool flag2 = false;
            int num2 = int.Parse(array[0]);
            int num3 = int.Parse(array[1]);
            string text2 = "";
            float num4 = 0f;
            float num5 = 0f;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            SceneGroup sceneGroup = null;
            canvas.removeAll();
            do
            {
                string text3 = streamReader.ReadLine();
                bool flag6 = false;
                string text4 = "";
                string text5 = text3;
                foreach (char c in text5)
                {
                    if ('"' == c)
                    {
                        flag6 = !flag6;
                    }
                    text4 = ((' ' != c || !flag6) ? (text4 + c) : (text4 + '_'));
                }
                array = text4.Split(separator);
                switch (array.Length)
                {
                    case 1:
                        if (string.Compare(array[0], "EOF") == 0)
                        {
                            flag = true;
                        }
                        else if (string.Compare(array[0], "layers") == 0)
                        {
                            flag2 = true;
                        }
                        else if (array[0].Length > 0)
                        {
                            text2 = array[0].Replace("_", " ");
                        }
                        break;

                    case 5:
                        {
                            float.Parse(array[0]);
                            num4 = float.Parse(array[1]);
                            float.Parse(array[2]);
                            num5 = float.Parse(array[3]);
                            int.Parse(array[4]);
                            int num24 = int.Parse(array[4]);
                            if (num24 > 0)
                            {
                                num24--;
                            }
                            while (num + num24 >= matTabControl.Controls.Count)
                            {
                                addEmptyLayer();
                                addLayer("untitled page");
                            }
                            tabPage = (TabPage)matTabControl.Controls[num + num24];
                            pictureBox = (PictureBox)tabPage.Tag;
                            canvas = (Canvas)pictureBox.Tag;
                            break;
                        }
                    case 13:
                    case 14:
                        {
                            sceneGroup = new SceneGroup();
                            canvas.sceneGroups.Add(sceneGroup);
                            int fontId = int.Parse(array[0]);
                            int num6 = int.Parse(array[1]);
                            int num7 = int.Parse(array[2]);
                            float num8 = float.Parse(array[3]);
                            float num9 = float.Parse(array[4]);
                            float num10 = float.Parse(array[5]);
                            float num11 = float.Parse(array[6]);
                            num4 = float.Parse(array[7]);
                            float.Parse(array[8]);
                            num5 = float.Parse(array[9]);
                            flag3 = ((int.Parse(array[10]) != 0) ? true : false);
                            flag4 = ((int.Parse(array[11]) != 0) ? true : false);
                            flag5 = ((int.Parse(array[12]) != 0) ? true : false);
                            if (num6 < 0 || num7 < 0)
                            {
                                break;
                            }
                            SceneGlyph sceneGlyph = new SceneGlyph();
                            int num12 = 0;
                            char[] trimChars = new char[1] { '"' };
                            foreach (Shape item in Shape.shapesById)
                            {
                                string text6 = text2.TrimStart(trimChars).TrimEnd(trimChars).Replace("Cricut(R) ", "");
                                if (text6.CompareTo(item.header.cartHeader.fontName.Replace("Cricut(R) ", "")) == 0)
                                {
                                    sceneGlyph.shape = item;
                                }
                            }
                            if (sceneGlyph.shape == null)
                            {
                                break;
                            }
                            sceneGlyph.fontId = fontId;
                            sceneGlyph.keyId = num7 * 14 + 2 + num6;
                            sceneGlyph.isSpace = false;
                            Glyph glyph = sceneGlyph.shape.getGlyph(sceneGlyph.fontId, sceneGlyph.keyId);
                            if (glyph != null)
                            {
                                sceneGlyph.contourInvis = new bool[glyph.nContours];
                                for (int j = 0; j < glyph.nContours; j++)
                                {
                                    if (array.Length > 13 && array[13].Length > j)
                                    {
                                        sceneGlyph.contourInvis[j] = array[13][j] == 'H';
                                    }
                                    else
                                    {
                                        sceneGlyph.contourInvis[j] = false;
                                    }
                                }
                                _ = glyph.xMin;
                                _ = glyph.yMin;
                                _ = glyph.xMax;
                                _ = glyph.yMax;
                            }
                            float num13 = 800f;
                            float num14 = (float)glyph.xMin / num13;
                            float num15 = (float)glyph.yMin / num13;
                            float num16 = (float)(glyph.xMax - glyph.xMin) / num13;
                            float num17 = (float)(glyph.yMax - glyph.yMin) / num13;
                            _ = 1f / num16;
                            _ = 1f / num17;
                            sceneGlyph.bbox.Location = new PointF(0f, 0f);
                            sceneGlyph.bbox.Size = new SizeF(1f, 1f);
                            sceneGlyph.ox = 0f;
                            sceneGlyph.oy = 0f;
                            sceneGlyph.size = 1f;
                            sceneGlyph.glyphToWorld = Canvas.glyphToWorld(0f, 0f, 1f);
                            sceneGroup.add(sceneGlyph, num12);
                            Matrix matrix = new Matrix();
                            float num18 = 0f + num16 / 2f;
                            float num19 = 0f + num17 / 2f;
                            sceneGroup.flipShapes = flag4 ^ flag5;
                            if (flag5)
                            {
                                num5 += 180f;
                            }
                            matrix.Translate(num18, num19, MatrixOrder.Prepend);
                            matrix.Rotate(0f - num5, MatrixOrder.Prepend);
                            float num20 = num10 / num16;
                            float num21 = num11 / num17;
                            matrix.Scale(num20, num21, MatrixOrder.Prepend);
                            matrix.Translate(0f - num14 - num18, 0f - num15 - num19, MatrixOrder.Prepend);
                            float num22 = canvas.drawR.X + num8;
                            float num23 = canvas.drawR.Y + (float)num3 - num9 - num11;
                            num22 -= num18 - num18 * num20;
                            num23 -= num19 - num19 * num21;
                            matrix.Translate(num22, num23, MatrixOrder.Append);
                            num4 *= num21 / num20;
                            Matrix matrix2 = new Matrix();
                            PointF[] array2 = new PointF[1]
                            {
                        new PointF(num14, num19)
                            };
                            PointF[] array3 = new PointF[1]
                            {
                        new PointF(num14, num19)
                            };
                            matrix2.Reset();
                            matrix2.Shear(0f - num4, 0f, MatrixOrder.Append);
                            matrix2.TransformPoints(array2);
                            matrix.TransformPoints(array2);
                            matrix.TransformPoints(array3);
                            matrix.Translate(array2[0].X - array3[0].X, array2[0].Y - array3[0].Y, MatrixOrder.Append);
                            sceneGroup.transform.Multiply(matrix, MatrixOrder.Prepend);
                            sceneGroup.welding = flag3;
                            sceneGroup.shear = num4;
                            sceneGroup.calcBbox(null);
                            break;
                        }
                    case 16:
                        {
                            float m = float.Parse(array[0]);
                            float m2 = float.Parse(array[1]);
                            float.Parse(array[2]);
                            float.Parse(array[3]);
                            float m3 = float.Parse(array[4]);
                            float m4 = float.Parse(array[5]);
                            float.Parse(array[6]);
                            float.Parse(array[7]);
                            float dx = float.Parse(array[8]);
                            float dy = float.Parse(array[9]);
                            float.Parse(array[10]);
                            float.Parse(array[11]);
                            float.Parse(array[12]);
                            float.Parse(array[13]);
                            float.Parse(array[14]);
                            float.Parse(array[15]);
                            sceneGroup.transform = new Matrix(m, m2, m3, m4, dx, dy);
                            break;
                        }
                }
            }
            while (!flag && !flag2);
            while (!flag)
            {
                string text3 = streamReader.ReadLine();
                if (text3 == "EOF")
                {
                    flag = true;
                }
                else
                {
                    if (!text3.StartsWith("\""))
                    {
                        continue;
                    }
                    int num25 = text3.LastIndexOf("\"");
                    if (num25 <= 0)
                    {
                        continue;
                    }
                    string text7 = text3.Substring(1, num25 - 1);
                    text3 = text3.Remove(0, num25 + 1);
                    array = text3.Split(separator);
                    if (array.Length == 3)
                    {
                        int num24 = int.Parse(array[1]);
                        if (num24 > 0)
                        {
                            num24--;
                        }
                        if (num24 + num < matTabControl.Controls.Count)
                        {
                            tabPage = (TabPage)matTabControl.Controls[num + num24];
                            pictureBox = (PictureBox)tabPage.Tag;
                            canvas = (Canvas)pictureBox.Tag;
                            canvas.layerProperties.layerName = text7;
                            canvas.layerProperties.includeInPreview = array[2] == "1";
                            tabPage.Name = text7;
                            tabPage.Text = text7;
                        }
                    }
                }
            }
            if (12 == num2 && 6 == num3)
            {
                changeMatSize_Click(x6ToolStripMenuItem, null);
            }
            else if (12 == num2 && 12 == num3)
            {
                changeMatSize_Click(x12ToolStripMenuItem, null);
            }
            else if (24 == num2 && 12 == num3)
            {
                changeMatSize_Click(x24ToolStripMenuItem, null);
            }
            streamReader.Close();
            matTabControl.SelectedIndex = 0;
            matTabControl.Show();
            projectProperties.filename = filename;
            saveProjectMenuItem.Enabled = true;
            Text = getAppName(Path.GetFileNameWithoutExtension(projectProperties.filename));
            cleanProject();
        }

        private void openProject(string projectFilename)
        {
            SceneGroup.CutCartRecord.reset();
            Canvas.clearPreview();
            FileStream fileStream = new FileStream(projectFilename, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            try
            {
                binaryReader.ReadString();
                binaryReader.ReadString();
            }
            catch (Exception ex)
            {
                messageBoxOpen = true;
                MessageBox.Show(myRootForm, "Cricut DesignStudio cannot read that file.\nThe file may be corrupt or empty. The error that Windows returned is:\n\n" + ex.Message, "Cannot Read File", MessageBoxButtons.OK);
                messageBoxOpen = false;
                fileStream?.Close();
                binaryReader?.Close();
                return;
            }
            matTabControl.Hide();
            matTabControl.Controls.Clear();
            do
            {
                addEmptyLayer();
                addLayer("untitled page");
                TabPage tabPage = (TabPage)matTabControl.Controls[matTabControl.Controls.Count - 1];
                PictureBox pictureBox = (PictureBox)tabPage.Tag;
                Canvas canvas = (Canvas)pictureBox.Tag;
                canvas.read(binaryReader);
                int num = (int)Math.Round(canvas.matteWidth);
                int num2 = (int)Math.Round(canvas.matteHeight);
                if (14 == num && 7 == num2)
                {
                    changeMatSize_Click(x6ToolStripMenuItem, null);
                }
                else if (14 == num && 13 == num2)
                {
                    changeMatSize_Click(x12ToolStripMenuItem, null);
                }
                else if (26 == num && 13 == num2)
                {
                    changeMatSize_Click(x24ToolStripMenuItem, null);
                }
            }
            while (Canvas.checkNextCanvas(binaryReader));
            binaryReader.Close();
            fileStream.Close();
            matTabControl.SelectedIndex = 0;
            matTabControl.Show();
            projectProperties.filename = projectFilename;
            saveProjectMenuItem.Enabled = true;
            Text = getAppName(Path.GetFileNameWithoutExtension(projectProperties.filename));
            cleanProject();
        }

        private void openProjectMenuItem_Click(object sender, EventArgs e)
        {
            if (isProjectDirty() && !checkClose())
            {
                return;
            }
            _extender.DialogViewType = global::FileDialogExtender.FileDialogExtender.DialogViewTypes.Thumbnails;
            _extender.Enabled = true;
            if (myRootForm.trialMode)
            {
                openProjectDialog.DefaultExt = ".cut";
                openProjectDialog.Filter = "Cricut files (*.cut)|*.cut";
            }
            else
            {
                openProjectDialog.DefaultExt = ".cut";
                openProjectDialog.Filter = "Cricut files (*.cut)|*.cut|Gypsy files (*.gypsy)|*.gypsy";
            }
            openProjectDialog.InitialDirectory = projectFolderPath;
            openProjectDialog.FileName = null;
            if (openProjectDialog.ShowDialog(this) == DialogResult.Cancel)
            {
                _extender.Enabled = false;
                return;
            }
            _extender.Enabled = false;
            string fileName = openProjectDialog.FileName;
            if (string.Compare(Path.GetExtension(fileName).ToLower(), ".gypsy") == 0)
            {
                openGypsyProject(fileName);
            }
            else
            {
                openProject(fileName);
            }
            if (((string)fontsChooseByComboBox.SelectedItem).CompareTo("This Project") == 0)
            {
                createCartridgeLibraryPanel((string)fontsChooseByComboBox.SelectedItem);
            }
        }

        public void anyTagComboBox_Enter(object sender, EventArgs e)
        {
            allowLocalFocus = true;
        }

        public void anyTagComboBox_Leave(object sender, EventArgs e)
        {
            allowLocalFocus = false;
        }

        private void fontsTag1ComboBox_TextChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            searchTag1 = comboBox.Text;
        }

        private void fontsTag2ComboBox_TextChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            searchTag2 = comboBox.Text;
        }

        public void setKeywordSelection()
        {
            fromtagsList = fontLoading.tagDictionary.matchTags(searchTag1, searchTag2);
            fontLoading.showFontList(FontLoading.FontLibraryMode.Keywords, fromtagsList, myRootForm.fontLoading.shapes);
            fromtagsIndex = 0;
            if (fromtagsList != null && fromtagsList.Count > 0)
            {
                nextKeywordButton.Enabled = true;
                prevKeywordButton.Enabled = true;
            }
            else
            {
                nextKeywordButton.Enabled = false;
                prevKeywordButton.Enabled = false;
                focusMattePicBox();
            }
            fromtagsIndex = -1;
        }

        public void anyTagComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender != myRootForm.fontsTag1ComboBox && sender != myRootForm.fontsTag2ComboBox)
            {
                focusMattePicBox();
            }
            else
            {
                setKeywordSelection();
            }
        }

        public void anyTagComboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (Keys.Return == e.KeyCode)
            {
                anyTagComboBox_SelectedIndexChanged(sender, null);
            }
            else if (Keys.Escape == e.KeyCode)
            {
                anyTagComboBox_SelectedIndexChanged(sender, null);
            }
        }

        public bool openCricut(USB_Interface usb, int baud)
        {
            string registryValue = getRegistryValue(userKey, "usbSerNo");
            string registryValue2 = getRegistryValue(userKey, "usbDesc");
            string[] array = usb.Open(baud, registryValue, registryValue2);
            if ((!usb.deviceOpen && array == null) || array.Length < 1)
            {
                return false;
            }
            if (!usb.deviceOpen && array.Length > 0)
            {
                messageBoxOpen = true;
                MessageBox.Show(myRootForm, "Your Cricut was not identified.\n\nPlease disconnect all USB devices from this computer except for the Cricut\nand then click OK.", "Cricut Not Identified", MessageBoxButtons.OK);
                messageBoxOpen = false;
                string[] array2 = array;
                foreach (string text in array2)
                {
                    int num = text.IndexOf(',');
                    registryValue = text.Substring(0, num);
                    registryValue2 = text.Substring(num + 1, text.Length - (num + 1));
                    usb.Open(baud, registryValue, registryValue2);
                    if (usb.deviceOpen)
                    {
                        setRegistryValue(userKey, "usbSerNo", registryValue);
                        setRegistryValue(userKey, "usbDesc", registryValue2);
                        return true;
                    }
                }
            }
            if (array.Length < 1)
            {
                return false;
            }
            return true;
        }

        public string getAppName(string projectName)
        {
            string text = "Cricut DesignStudio";
            if (trialMode)
            {
                text += " Trial Mode";
            }
            if (projectName != null)
            {
                text = text + " - " + projectName;
            }
            return text;
        }

        public void premultImage(Bitmap img)
        {
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    Color pixel = img.GetPixel(j, i);
                    byte b = (byte)((pixel.A < byte.MaxValue) ? 64u : 255u);
                    b = 64;
                    img.SetPixel(j, i, Color.FromArgb(b, pixel.R * b / 255, pixel.G * b / 255, pixel.B * b / 255));
                }
            }
        }

        private static ulong sToUlong(string s)
        {
            if (s.Length > 16)
            {
                s = s.Substring(s.Length - 16, 16);
            }
            if (ulong.TryParse(s, NumberStyles.HexNumber, null, out var result))
            {
                return result;
            }
            result = 0uL;
            string text = s;
            foreach (char c in text)
            {
                result <<= 8;
                result += c;
            }
            return result;
        }

        public string getMACAddress()
        {
            int num = 0;
            int num2 = 0;
            string text = getRegistryValue(progKey, "size");
            if (text == null)
            {
                text = getRegistryValue(userKey, "size");
            }
            if (text != null)
            {
                fake.size = text;
            }
            else if (fake.size != null)
            {
                text = fake.size;
            }
            else
            {
                Random random = new Random();
                do
                {
                    num = random.Next(int.MaxValue);
                }
                while (num < 1000000);
                random = new Random(num);
                num2 = random.Next(int.MaxValue);
                text = num.ToString();
                string text2 = num.ToString();
                if (1 == text2.Length % 2)
                {
                    text2 = text2.PadRight(text2.Length + 1, '0');
                }
                string text3 = RijndaelSimple.Encrypt(text2, "SG8WU1J8e4GZp4F6q8nOi1bC5w2XcR3q5y8P", "0123456789", "SHA1", 2, "trI1lylmsEPWoGTe", 128);
                fake.size = text;
                fake.keyname = "{" + text3.Insert(20, "-").Insert(16, "-").Insert(12, "-")
                    .Insert(8, "-") + "}";
                fake.name = num2.ToString();
                fake.value = Guid.NewGuid().ToString("N");
                setRegistryValue(userKey, "size", fake.size);
                text3 = RijndaelSimple.Encrypt(fake.value, "SG8WU1J8e4GZp4F6q8nOi1bC5w2XcR3q5y8P", "0123456789", "SHA1", 2, "trI1lylmsEPWoGTe", 128);
                fakeMACAddress = text3.Substring(0, 12);
            }
            ManagementObjectSearcher managementObjectSearcher = null;
            ManagementObjectCollection managementObjectCollection = null;
            string text4 = fakeMACAddress;
            try
            {
                ulong num3 = 0uL;
                ulong num4 = ulong.MaxValue;
                num4++;
                managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                managementObjectCollection = managementObjectSearcher.Get();
                foreach (ManagementObject item in managementObjectCollection)
                {
                    if (item["ProcessorID"] != null)
                    {
                        string s = item["ProcessorID"].ToString();
                        num3 += sToUlong(s);
                        break;
                    }
                }
                managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
                managementObjectCollection = managementObjectSearcher.Get();
                foreach (ManagementObject item2 in managementObjectCollection)
                {
                    if (item2["Product"] != null)
                    {
                        string s = item2["Product"].ToString();
                        num3 += sToUlong(s);
                        break;
                    }
                }
                num3 += sToUlong(fake.size.ToString());
                managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
                managementObjectCollection = managementObjectSearcher.Get();
                foreach (ManagementObject item3 in managementObjectCollection)
                {
                    if (item3["SerialNumber"] != null)
                    {
                        string s = item3["SerialNumber"].ToString();
                        num3 += sToUlong(s);
                        break;
                    }
                }
                string text5 = num3.ToString();
                if (1 == text5.Length % 2)
                {
                    text5 = text5.PadRight(text5.Length + 1, '0');
                }
                string text6 = RijndaelSimple.Encrypt(text5.ToString(), "SG8WU1J8e4GZp4F6q8nOi1bC5w2XcR3q5y8P", "0123456789", "SHA1", 2, "trI1lylmsEPWoGTe", 128);
                text6 = text6.Substring(0, 12);
                text4 = text6.Insert(10, ":").Insert(8, ":").Insert(6, ":")
                    .Insert(4, ":")
                    .Insert(2, ":");
            }
            catch (Exception ex)
            {
                trace(ex.Message);
                text4 = null;
            }
            if (text4 == null)
            {
                num = int.Parse(text);
                Random random2 = new Random(num);
                num2 = random2.Next(int.MaxValue);
                string text7 = num.ToString();
                if (1 == text7.Length % 2)
                {
                    text7 = text7.PadRight(text7.Length + 1, '0');
                }
                string text8 = RijndaelSimple.Encrypt(text7, "SG8WU1J8e4GZp4F6q8nOi1bC5w2XcR3q5y8P", "0123456789", "SHA1", 2, "trI1lylmsEPWoGTe", 128);
                fake.addr = "{" + text8.Insert(20, "-").Insert(16, "-").Insert(12, "-")
                    .Insert(8, "-") + "}";
                string text9 = null;
                RegistryKey localMachine = Registry.LocalMachine;
                localMachine = localMachine.OpenSubKey("SOFTWARE", writable: false);
                RegistryKey registryKey = localMachine.OpenSubKey(fake.addr, writable: false);
                if (registryKey == null)
                {
                    text9 = fake.value;
                }
                else
                {
                    text9 = (string)registryKey.GetValue(num2.ToString());
                    if (text9 == null)
                    {
                        return null;
                    }
                }
                text8 = RijndaelSimple.Encrypt(text9, "SG8WU1J8e4GZp4F6q8nOi1bC5w2XcR3q5y8P", "0123456789", "SHA1", 2, "trI1lylmsEPWoGTe", 128);
                fakeMACAddress = text8.Substring(0, 12);
                text4 = fakeMACAddress.Insert(10, ":").Insert(8, ":").Insert(6, ":")
                    .Insert(4, ":")
                    .Insert(2, ":");
            }
            return text4;
        }

        public void enableNormalControls(bool enabled)
        {
            fileToolStripMenuItem.Enabled = enabled;
            editToolStripMenuItem.Enabled = enabled;
            viewToolStripMenuItem.Enabled = enabled;
            fontFeaturesMenuItem.Enabled = enabled;
            prefsToolStripMenuItem.Enabled = enabled;
            helpToolStripMenuItem.Enabled = enabled;
            newPageButton.Enabled = enabled;
            copyGroupButton.Enabled = enabled;
            pasteGroupButton.Enabled = enabled;
            deleteGroupButton.Enabled = enabled;
            previewButton.Enabled = enabled;
            cricutCutButton.Enabled = enabled;
            fitToPageButton.Enabled = enabled;
            percent100Button.Enabled = enabled;
            percent200Button.Enabled = enabled;
            fontsChooseByComboBox.Enabled = enabled;
            prevKeywordButton.Enabled = enabled;
            nextKeywordButton.Enabled = enabled;
            fontsTag1ComboBox.Enabled = enabled;
            fontsTag2ComboBox.Enabled = enabled;
            fontTreeView.Enabled = enabled;
            paperSaverCheckBox.Enabled = enabled;
            realSizeCheckBox.Enabled = enabled;
            applyKerningButton.Enabled = enabled;
            revertShapePropertiesButton.Enabled = enabled;
            rotate90Button.Enabled = enabled;
            kerningTextBox.Enabled = enabled;
            keepAspectRatioButton.Enabled = enabled;
        }

        public void trace(string msg)
        {
            if (traceFilename == null)
            {
                traceFilename = userDataFolderPath + "\\trace.txt";
                FileStream fileStream = File.Create(traceFilename);
                fileStream.Close();
            }
            StreamWriter streamWriter = new StreamWriter(traceFilename, append: true);
            streamWriter.WriteLine(msg);
            streamWriter.Close();
        }

        public Form1()
        {
            Environment.OSVersion.Version.ToString();
            if (SingleApplication.IsAlreadyRunning())
            {
                SingleApplication.SwitchToCurrentInstance();
                Application.Exit();
                return;
            }
            exeFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            fontFolderPath = exeFolderPath + "\\Cricut Fonts";
            userDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal).ToString() + "\\Cricut\\User Data";
            projectFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal).ToString() + "\\Cricut\\Projects";
            imageFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures).ToString();
            if (!Directory.Exists(userDataFolderPath))
            {
                Directory.CreateDirectory(userDataFolderPath);
            }
            if (!File.Exists(userDataFolderPath + "\\CricutFontsMetadata.xml") && File.Exists(exeFolderPath + "\\CricutFontsMetadata.xml"))
            {
                File.Copy(exeFolderPath + "\\CricutFontsMetadata.xml", userDataFolderPath + "\\CricutFontsMetadata.xml");
            }
            if (!Directory.Exists(projectFolderPath))
            {
                Directory.CreateDirectory(projectFolderPath);
                string[] files = Directory.GetFiles(exeFolderPath + "\\User Projects\\");
                foreach (string text in files)
                {
                    File.Copy(text, projectFolderPath + "\\" + Path.GetFileName(text), overwrite: false);
                }
            }
            trace("CP 0001");
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, value: true);
            SplashScreen.ShowSplashScreen();
            trace("CP 0002");
            initRegistry();
            trace("CP 0003");
            Application.AddMessageFilter(this);
            myRootForm = this;
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            executingAssembly.GetManifestResourceNames();
            SplashScreen.SetReferencePoint();
            matTabControlRightMargin = -1;
            matTabControlBottomMargin = -1;
            InitializeComponent();
            matTabControlRightMargin = base.Width - (matTabControl.Left + matTabControl.Size.Width);
            matTabControlBottomMargin = base.Height - (matTabControl.Top + matTabControl.Size.Height);
            hoverTimer.Interval = 125;
            hoverTimer.Stop();
            hoverTimer.Enabled = true;
            hoverBox.Paint += hoverBox_Paint;
            hoverBox.Click += hoverBox_Click;
            hoverBox.MouseEnter += hoverBox_MouseEnter;
            hoverBox.MouseLeave += hoverBox_MouseLeave;
            base.Icon = Icon.FromHandle(MarcusResources.cricut_logo_2_32x32.GetHicon());
            prevKeywordButton.Tag = new ButtonState();
            ((ButtonState)prevKeywordButton.Tag).baselineAdj = -2;
            nextKeywordButton.Tag = new ButtonState();
            ((ButtonState)nextKeywordButton.Tag).baselineAdj = -2;
            nudgeLeftButton.Tag = new ButtonState();
            ((ButtonState)nudgeLeftButton.Tag).baselineAdj = -2;
            nudgeRightButton.Tag = new ButtonState();
            ((ButtonState)nudgeRightButton.Tag).baselineAdj = -2;
            nudgeUpButton.Tag = new ButtonState();
            ((ButtonState)nudgeUpButton.Tag).baselineAdj = -2;
            nudgeDownButton.Tag = new ButtonState();
            ((ButtonState)nudgeDownButton.Tag).baselineAdj = -2;
            newPageButton.pressedImg = MarcusResources.btn_new_page_down;
            newPageButton.hotImg = MarcusResources.btn_new_page_over;
            newPageButton.normalImg = MarcusResources.btn_new_page;
            newPageButton.BackColor = buttonBackColor;
            copyGroupButton.pressedImg = MarcusResources.btn_copy_group_down;
            copyGroupButton.hotImg = MarcusResources.btn_copy_group_over;
            copyGroupButton.normalImg = MarcusResources.btn_copy_group;
            copyGroupButton.BackColor = buttonBackColor;
            pasteGroupButton.pressedImg = MarcusResources.btn_paste_group_down;
            pasteGroupButton.hotImg = MarcusResources.btn_paste_group_over;
            pasteGroupButton.normalImg = MarcusResources.btn_paste_group;
            pasteGroupButton.BackColor = buttonBackColor;
            deleteGroupButton.pressedImg = MarcusResources.btn_delete_group_down;
            deleteGroupButton.hotImg = MarcusResources.btn_delete_group_over;
            deleteGroupButton.normalImg = MarcusResources.btn_delete_group;
            deleteGroupButton.BackColor = buttonBackColor;
            previewButton.pressedImg = MarcusResources.btn_preview_down;
            previewButton.hotImg = MarcusResources.btn_preview_over;
            previewButton.normalImg = MarcusResources.btn_preview;
            previewButton.BackColor = buttonBackColor;
            cricutCutButton.pressedImg = MarcusResources.btn_cut_base_down;
            cricutCutButton.hotImg = MarcusResources.btn_cut_base_over;
            cricutCutButton.normalImg = MarcusResources.btn_cut_base;
            cricutCutButton.BackColor = buttonBackColor;
            fitToPageButton.pressedImg = MarcusResources.base_views_click;
            fitToPageButton.hotImg = MarcusResources.base_views_over;
            fitToPageButton.normalImg = MarcusResources.base_views;
            fitToPageButton.selectedImg = MarcusResources.base_views_up;
            fitToPageButton.selectedDownImg = MarcusResources.base_views_down;
            fitToPageButton.BackColor = buttonBackColor;
            fitToPageButton.bitmapAlignment = CricutButton.BitmapAlignment.AlignTop;
            percent100Button.pressedImg = MarcusResources.base_views_click;
            percent100Button.hotImg = MarcusResources.base_views_over;
            percent100Button.normalImg = MarcusResources.base_views;
            percent100Button.selectedImg = MarcusResources.base_views_up;
            percent100Button.selectedDownImg = MarcusResources.base_views_down;
            percent100Button.BackColor = buttonBackColor;
            percent100Button.bitmapAlignment = CricutButton.BitmapAlignment.AlignCenter;
            percent100Button.drawEdge = true;
            percent200Button.pressedImg = MarcusResources.base_views_click;
            percent200Button.hotImg = MarcusResources.base_views_over;
            percent200Button.normalImg = MarcusResources.base_views;
            percent200Button.selectedImg = MarcusResources.base_views_up;
            percent200Button.selectedDownImg = MarcusResources.base_views_down;
            percent200Button.BackColor = buttonBackColor;
            percent200Button.bitmapAlignment = CricutButton.BitmapAlignment.AlignBottom;
            shiftKeyLabel.BackColor = Color.FromArgb(218, 218, 218);
            spaceKeyLabel.BackColor = Color.FromArgb(218, 218, 218);
            backspaceKeyLabel.BackColor = Color.FromArgb(218, 218, 218);
            paperSaverCheckBox.Checked = paperSaver;
            realSizeCheckBox.Checked = realSize;
            enableBalloonHelpMenuItem.Checked = enableBalloonHelp;
            showRulerToolStripMenuItem.Checked = showRuler;
            keepPreviewAsMatBackgroundToolStripMenuItem.Checked = keepPreview;
            x6ToolStripMenuItem.Checked = false;
            x12ToolStripMenuItem.Checked = false;
            x24ToolStripMenuItem.Checked = false;
            switch (matSize)
            {
                case 0:
                    x6ToolStripMenuItem.Checked = true;
                    matSizeToolStripMenuItem.Text = "Mat Size - 12x6";
                    break;

                case 1:
                    x12ToolStripMenuItem.Checked = true;
                    matSizeToolStripMenuItem.Text = "Mat Size - 12x12";
                    break;

                case 2:
                    x24ToolStripMenuItem.Checked = true;
                    matSizeToolStripMenuItem.Text = "Mat Size - 24x12";
                    break;
            }
            fitToPageToolStripMenuItem.Checked = false;
            percent100ToolStripMenuItem.Checked = false;
            percent200ToolStripMenuItem.Checked = false;
            switch (viewIndex)
            {
                case 0:
                    fitToPageToolStripMenuItem.Checked = true;
                    matViewToolStripMenuItem.Text = "Mat View - Fit to Page";
                    fitToPageButton.selected = true;
                    break;

                case 1:
                    percent100ToolStripMenuItem.Checked = true;
                    matViewToolStripMenuItem.Text = "Mat View - 100%";
                    percent100Button.selected = true;
                    break;

                case 2:
                    percent200ToolStripMenuItem.Checked = true;
                    matViewToolStripMenuItem.Text = "Mat View - 200%";
                    percent200Button.selected = true;
                    break;
            }
            multiCut1ToolStripMenuItem.Checked = false;
            multiCut2ToolStripMenuItem.Checked = false;
            multiCut3ToolStripMenuItem.Checked = false;
            multiCut4ToolStripMenuItem.Checked = false;
            switch (multiCut)
            {
                case 0:
                    multiCut1ToolStripMenuItem.Checked = true;
                    multiCutToolStripMenuItem.Text = "Multi Cut - Off (Default)";
                    break;

                case 1:
                    multiCut2ToolStripMenuItem.Checked = true;
                    multiCutToolStripMenuItem.Text = "Multi Cut - 2 Times";
                    break;

                case 2:
                    multiCut3ToolStripMenuItem.Checked = true;
                    multiCutToolStripMenuItem.Text = "Multi Cut - 3 Times";
                    break;

                case 3:
                    multiCut4ToolStripMenuItem.Checked = true;
                    multiCutToolStripMenuItem.Text = "Multi Cut - 4 Times";
                    break;
            }
            sizeTrackBar = new CustomTrackBar(11, keypadPanel.Width * 264 / 470, keypadPanel.Height * 208 / 240, new Size(120, keypadPanel.Height * 28 / 240));
            sizeTrackBar.BackColor = trackBarBkgnd;
            keypadPanel.Controls.Add(sizeTrackBar);
            cartridgeLibraryPanelLocation = cartridgeLibraryPanel.Location;
            keypadPanelLocation = keypadPanel.Location;
            propertiesPanelLocation = propertiesPanel.Location;
            matTabControlSize = matTabControl.Size;
            matTabControl.backColor = Color.FromArgb(255, 255, 248, 219);
            matTabControl.dimTabColor = Color.FromArgb(255, 152, 197, 116);
            BackColor = Color.FromArgb(255, 176, 211, 147);
            menuStripSeparator.BackColor = lightMenuText;
            sizeLabel.BackColor = Color.FromArgb(255, 248, 219);
            foreach (TabPage tabPage in matTabControl.TabPages)
            {
                tabPage.BackColor = Color.FromArgb(255, 248, 219);
            }
            menuStrip1.Renderer = new CricutMenuStripRenderer();
            glyphContextMenu.Renderer = menuStrip1.Renderer;
            pageContextMenu.Renderer = menuStrip1.Renderer;
            showFontMenuItem.Click += showFont_Click;
            pageNameTextBox.Text = "untitled page";
            pageNameTextBox.KeyPress += pageNameTextBox_KeyPress;
            changePageNameMenuItem.Click += changePageNameMenuItem_Click;
            bringPageForwardMenuItem.Click += bringPageForwardMenuItem_Click;
            sendPageBackwardMenuItem.Click += sendPageBackwardMenuItem_Click;
            bringPagetoFrontMenuItem.Click += bringPagetoFrontMenuItem_Click;
            sendPagetoBackMenuItem.Click += sendPagetoBackMenuItem_Click;
            glyphKeywordComboBox.Leave += anyTagComboBox_Leave;
            glyphKeywordComboBox.Enter += anyTagComboBox_Enter;
            glyphKeywordComboBox.SelectedIndexChanged += anyTagComboBox_SelectedIndexChanged;
            glyphKeywordComboBox.KeyDown += glyphKeywordComboBox_KeyDown;
            addKeywordMenuItem.Click += addKeywordMenuItem_Click;
            trace("CP 0003A");
            fontLoading = new FontLoading();
            trace("CP 0003B");
            fontLoading.loadFonts();
            trace("CP 0003Z");
            try
            {
                SplashScreen.SplashForm.Owner = this;
            }
            catch
            {
                trace("EXP 0001");
            }
            base.TopMost = true;
            Activate();
            SplashScreen.CloseForm();
            base.TopMost = false;
            setupNumTextBox(shearTextBox, 3);
            setupNumTextBox(angleTextBox, 7);
            setupNumTextBox(xPositionTextBox, 8);
            setupNumTextBox(yPositionTextBox, 6);
            setupNumTextBox(widthTextBox, 4);
            setupNumTextBox(heightTextBox, 2);
            SceneGroup.clearNumBoxes();
            trace("CP 0004");
            loadRegistrySettings();
            trace("CP 0005");
            matTabControl.TabPages.Remove(activationCompleteTabPage);
            matTabControl.TabPages.Remove(updateCricutFirmwareTabPage);
            if (trialMode)
            {
                enableNormalControls(enabled: false);
            }
            else
            {
                matTabControl.TabPages.Clear();
                addEmptyLayer();
                addLayer("untitled page");
                cleanProject();
            }
            Text = getAppName(null);
            cuttingBackgroundWorker.WorkerReportsProgress = true;
            cuttingBackgroundWorker.WorkerSupportsCancellation = true;
            cuttingBackgroundWorker.DoWork += cuttingBackgroundWorker_DoWork;
            cuttingBackgroundWorker.ProgressChanged += cuttingBackgroundWorker_ProgressChanged;
            cuttingBackgroundWorker.RunWorkerCompleted += cuttingBackgroundWorker_RunWorkerCompleted;
            firmwareBackgroundWorker.WorkerReportsProgress = true;
            firmwareBackgroundWorker.WorkerSupportsCancellation = true;
            firmwareBackgroundWorker.DoWork += firmwareBackgroundWorker_DoWork;
            firmwareBackgroundWorker.ProgressChanged += firmwareBackgroundWorker_ProgressChanged;
            firmwareBackgroundWorker.RunWorkerCompleted += firmwareBackgroundWorker_RunWorkerCompleted;
        }

        private void setupNumTextBox(TextBox tb, int mode)
        {
            tb.Tag = new FloatParam(0f, mode);
            tb.Enter += anyTextBox_Enter;
            tb.Leave += number_textBox_Leave;
            tb.KeyPress += number_textBox_KeyPress;
        }

        public static void fillRoundBox(Graphics g, float radius, Color col, float ox, float oy, float cx, float cy)
        {
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.Reset();
            graphicsPath.AddArc(ox, oy, radius, radius, 180f, 90f);
            graphicsPath.AddArc(cx - radius, oy, radius, radius, 270f, 90f);
            graphicsPath.AddArc(cx - radius, cy - radius, radius, radius, 0f, 90f);
            graphicsPath.AddArc(ox, cy - radius, radius, radius, 90f, 90f);
            SolidBrush brush = new SolidBrush(col);
            g.FillPath(brush, graphicsPath);
        }

        public static void drawRoundBox(Graphics g, float radius, Color col, float ox, float oy, float cx, float cy)
        {
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddLine(ox, oy + radius, ox, (oy + cy) / 2f);
            graphicsPath.AddArc(ox, oy, radius, radius, 180f, 90f);
            graphicsPath.AddArc(cx - radius, oy, radius, radius, 270f, 90f);
            graphicsPath.AddArc(cx - radius, cy - radius, radius, radius, 0f, 90f);
            graphicsPath.AddArc(ox, cy - radius, radius, radius, 90f, 90f);
            graphicsPath.AddLine(ox, (oy + cy) / 2f, ox, cy - radius);
            Pen pen = new Pen(col);
            g.DrawPath(pen, graphicsPath);
        }

        public static void drawRoundBox(Graphics g, float radius, Pen pen, float ox, float oy, float cx, float cy)
        {
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddLine(ox, oy + radius, ox, (oy + cy) / 2f);
            graphicsPath.AddArc(ox, oy, radius, radius, 180f, 90f);
            graphicsPath.AddArc(cx - radius, oy, radius, radius, 270f, 90f);
            graphicsPath.AddArc(cx - radius, cy - radius, radius, radius, 0f, 90f);
            graphicsPath.AddArc(ox, cy - radius, radius, radius, 90f, 90f);
            graphicsPath.AddLine(ox, (oy + cy) / 2f, ox, cy - radius);
            g.DrawPath(pen, graphicsPath);
        }

        private void panelBackground_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = (Panel)sender;
            e.Graphics.Clear(SystemColors.Control);
            fillRoundBox(e.Graphics, 20f, Color.FromArgb(255, 255, 248, 219), 0f, 0f, panel.Width - 1, panel.Height - 1);
        }

        private void cartridgeLibraryPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = (Panel)sender;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            fillRoundBox(e.Graphics, 20f, Color.FromArgb(255, 255, 248, 219), 0f, 0f, panel.Width - 1, panel.Height - 1);
            drawRoundBox(e.Graphics, 20f, Color.Black, 0f, 0f, panel.Width - 1, panel.Height - 1);
        }

        private void keypadPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = (Panel)sender;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            fillRoundBox(e.Graphics, 20f, Color.FromArgb(255, 255, 248, 219), 0f, 0f, panel.Width - 1, panel.Height - 1);
            drawRoundBox(e.Graphics, 20f, Color.Black, 0f, 0f, panel.Width - 1, panel.Height - 1);
        }

        private void keypadOutlinePanel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = (Panel)sender;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            fillRoundBox(e.Graphics, 20f, Color.FromArgb(255, 227, 213, 158), 0f, 0f, panel.Width - 1, panel.Height - 1);
        }

        private void sliderPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = (Panel)sender;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            fillRoundBox(e.Graphics, 10f, trackBarBkgnd, 0f, 0f, panel.Width - 1, panel.Height - 1);
            drawRoundBox(e.Graphics, 10f, Color.Black, 0f, 0f, panel.Width - 1, panel.Height - 1);
        }

        public void drawDropShadow(Graphics g, Panel p)
        {
            int num = 2;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Pen pen = new Pen(Color.FromArgb(40, 0, 0, 0), 3f);
            drawRoundBox(g, 22f, pen, p.Location.X + num + 2, p.Location.Y + num + 2, p.Location.X + p.Width + num - 2, p.Location.Y + p.Height + num - 2);
            pen.Color = Color.FromArgb(20, 0, 0, 0);
            drawRoundBox(g, 22f, pen, p.Location.X + num + 1, p.Location.Y + num + 1, p.Location.X + p.Width + num - 1, p.Location.Y + p.Height + num - 1);
            pen.Color = Color.FromArgb(10, 0, 0, 0);
            drawRoundBox(g, 22f, pen, p.Location.X + num, p.Location.Y + num, p.Location.X + p.Width + num, p.Location.Y + p.Height + num);
            pen.Dispose();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            ColorBlend colorBlend = new ColorBlend();
            colorBlend.Colors = new Color[3]
            {
                Color.FromArgb(176, 211, 147),
                Color.FromArgb(202, 227, 175),
                Color.FromArgb(176, 211, 147)
            };
            colorBlend.Positions = new float[3] { 0f, 0.5f, 1f };
            Color color = Color.FromArgb(176, 211, 147);
            Color color2 = Color.FromArgb(202, 227, 175);
            Rectangle rect = new Rectangle(0, 0, base.Width, base.Height);
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, color, color2, LinearGradientMode.Horizontal);
            linearGradientBrush.InterpolationColors = colorBlend;
            graphics.FillRectangle(linearGradientBrush, rect);
            new Rectangle(0, 0, e.ClipRectangle.Width, e.ClipRectangle.Height);
            if (58 == e.ClipRectangle.Left && 277 == e.ClipRectangle.Top)
            {
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(64, 0, 0, 0)), 7, 23, matTabControl.Width - 9, matTabControl.Height - 25);
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(32, 0, 0, 0)), 6, 22, matTabControl.Width - 7, matTabControl.Height - 23);
                graphics.FillRectangle(new SolidBrush(Color.FromArgb(16, 0, 0, 0)), 5, 21, matTabControl.Width - 5, matTabControl.Height - 21);
            }
            else
            {
                drawDropShadow(graphics, cartridgeLibraryPanel);
                drawDropShadow(graphics, keypadPanel);
                drawDropShadow(graphics, propertiesPanel);
            }
        }

        private void fontNameLabel_Paint(object sender, PaintEventArgs e)
        {
            Label label = (Label)sender;
            SizeF sizeF = e.Graphics.MeasureString(label.Text, label.Font);
            Color color = (label.Enabled ? darkMenuText : SystemColors.GrayText);
            SolidBrush brush = new SolidBrush(color);
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.Clear(Color.FromArgb(255, 255, 248, 219));
            e.Graphics.DrawString(label.Text, label.Font, brush, ((float)label.Width - sizeF.Width) / 2f, 0f);
        }

        private void labelAA_Paint(object sender, PaintEventArgs e)
        {
            Label label = (Label)sender;
            e.Graphics.MeasureString(label.Text, label.Font);
            Color color = (label.Enabled ? darkMenuText : SystemColors.GrayText);
            SolidBrush brush = new SolidBrush(color);
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.Clear(Color.FromArgb(255, 255, 248, 219));
            e.Graphics.DrawString(label.Text, label.Font, brush, 0f, 0f);
        }

        private void labelBgAA_Paint(object sender, PaintEventArgs e)
        {
            Label label = (Label)sender;
            e.Graphics.MeasureString(label.Text, label.Font);
            Color color = (label.Enabled ? darkMenuText : SystemColors.GrayText);
            SolidBrush brush = new SolidBrush(color);
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.Clear(label.BackColor);
            e.Graphics.DrawString(label.Text, label.Font, brush, 0f, 0f);
        }

        private void keypadButtonLabel_Paint(object sender, PaintEventArgs e)
        {
            Label label = (Label)sender;
            SolidBrush brush = new SolidBrush(Color.Black);
            e.Graphics.MeasureString(label.Text, label.Font);
            Rectangle clientRectangle = label.ClientRectangle;
            clientRectangle.Location = new Point(clientRectangle.Location.X - 1, clientRectangle.Location.Y);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            if (label.Image != null)
            {
                DrawControlBackingStore(e.Graphics, label.Image, label);
            }
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(label.Text, label.Font, brush, clientRectangle, stringFormat);
        }

        private void showRulerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            toolStripMenuItem.Checked = !toolStripMenuItem.Checked;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            Form1 form = (Form1)sender;
            menuStripSeparator.Size = new Size(menuStrip1.Width, 1);
            if (matTabControlRightMargin >= 0 && matTabControlBottomMargin >= 0)
            {
                matTabControl.Size = new Size(base.Width - matTabControl.Left - matTabControlRightMargin, base.Height - matTabControl.Top - matTabControlBottomMargin);
                int num = 8;
                int num2 = cartridgeLibraryPanel.Width + num + keypadPanel.Width + num + propertiesPanel.Width;
                cartridgeLibraryPanel.Left = matTabControl.Left + (matTabControl.Width - num2) / 2;
                keypadPanel.Left = cartridgeLibraryPanel.Left + cartridgeLibraryPanel.Width + num;
                propertiesPanel.Left = keypadPanel.Left + keypadPanel.Width + num;
            }
            resizeLayers();
            form.Refresh();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            if (base.WindowState == FormWindowState.Normal)
            {
                setRegistryValue(userKey, "winSize", base.Size.Width + "," + base.Size.Height);
            }
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            if (base.WindowState == FormWindowState.Normal && loadWasCalled)
            {
                setRegistryValue(userKey, "winLoc", base.Location.X + "," + base.Location.Y);
            }
        }

        private void parseXYstr(string s, ref int x, ref int y)
        {
            int num = 0;
            int num2 = 0;
            try
            {
                num = int.Parse(s.Substring(0, s.IndexOf(",")));
                num2 = int.Parse(s.Substring(s.IndexOf(",") + 1));
            }
            catch
            {
                return;
            }
            x = num;
            y = num2;
        }

        private void usePreviousSizeAndLoc()
        {
            string registryValue = getRegistryValue(userKey, "winLoc");
            string registryValue2 = getRegistryValue(userKey, "winSize");
            string registryValue3 = getRegistryValue(userKey, "winMax");
            if (registryValue2 != null)
            {
                int num = base.Width;
                int num2 = base.Height;
                parseXYstr(registryValue2, ref num, ref num2);
                base.Size = new Size(num, num2);
            }
            if (registryValue != null)
            {
                int num3 = base.Location.X;
                int num4 = base.Location.Y;
                parseXYstr(registryValue, ref num3, ref num4);
                base.Location = new Point(num3, num4);
            }
            if (registryValue3 != null && registryValue3.CompareTo("True") == 0)
            {
                base.WindowState = FormWindowState.Maximized;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            usePreviousSizeAndLoc();
            loadWasCalled = true;
            if (commandLineArgs.Length > 1)
            {
                enableNormalControls(enabled: true);
                openProject(commandLineArgs[1]);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closeProject(force: false))
            {
                e.Cancel = true;
                return;
            }
            if (base.WindowState == FormWindowState.Normal)
            {
                setRegistryValue(userKey, "winMax", "False");
            }
            else if (base.WindowState == FormWindowState.Maximized)
            {
                setRegistryValue(userKey, "winMax", "True");
            }
            foreach (Shape shape in fontLoading.shapes)
            {
                fontLoading.fontMetadata.setData(shape.getFontName(), shape.properties);
            }
            fontLoading.fontMetadata.save();
        }

        public void focusMattePicBox()
        {
            getMattePicBox()?.Focus();
        }

        public void refreshMattePicBox()
        {
            PictureBox mattePicBox = getMattePicBox();
            if (mattePicBox != null)
            {
                if (myRootForm.messageBoxOpen)
                {
                    Canvas.previewFreshness = 0;
                }
                if (!myRootForm.keepPreview && Canvas.previewFreshness > 0)
                {
                    Canvas.clearPreview();
                    Canvas.previewFreshness = 0;
                }
                if (!myRootForm.messageBoxOpen)
                {
                    Canvas.previewFreshness++;
                }
                mattePicBox.Refresh();
            }
        }

        public PictureBox getMattePicBox()
        {
            if (matTabControl == null || matTabControl.TabCount == 0)
            {
                return null;
            }
            TabPage selectedTab = matTabControl.SelectedTab;
            return (PictureBox)selectedTab.Tag;
        }

        public Canvas getCanvas()
        {
            if (getMattePicBox() == null)
            {
                return null;
            }
            return (Canvas)getMattePicBox().Tag;
        }

        public bool isProjectDirty()
        {
            bool result = projectDirty;
            foreach (Control control in matTabControl.Controls)
            {
                TabPage tabPage = (TabPage)control;
                PictureBox pictureBox = (PictureBox)tabPage.Tag;
                if (pictureBox != null)
                {
                    Canvas canvas = (Canvas)pictureBox.Tag;
                    if (canvas.isDirty())
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        public void cleanProject()
        {
            foreach (Control control in matTabControl.Controls)
            {
                TabPage tabPage = (TabPage)control;
                PictureBox pictureBox = (PictureBox)tabPage.Tag;
                if (pictureBox != null)
                {
                    Canvas canvas = (Canvas)pictureBox.Tag;
                    canvas.clean();
                }
            }
            projectDirty = false;
        }

        public void addEmptyLayer()
        {
            TabPage tabPage = new TabPage();
            tabPage.Location = new Point(4, 22);
            tabPage.Name = "emptyLayer";
            tabPage.Size = new Size(768, 392);
            tabPage.TabIndex = 0;
            tabPage.Text = "New Page";
            tabPage.Tag = null;
            matTabControl.Controls.Add(tabPage);
        }

        public void addLayer(string name)
        {
            Panel panel = new CDSPanel();
            PictureBox pictureBox = new PictureBox();
            TabPage tabPage = ((matTabControl.Controls.Count <= 0) ? new TabPage() : ((TabPage)matTabControl.Controls[matTabControl.Controls.Count - 1]));
            pictureBox.BackColor = Color.FromArgb(255, 248, 219);
            pictureBox.BorderStyle = BorderStyle.None;
            pictureBox.Location = new Point(0, 0);
            pictureBox.Name = "mattePicBox";
            pictureBox.TabIndex = 2;
            pictureBox.TabStop = false;
            pictureBox.Click += mattePicBox_Click;
            pictureBox.MouseUp += mattePicBox_MouseUp;
            pictureBox.MouseMove += mattePicBox_MouseMove;
            pictureBox.MouseDown += mattePicBox_MouseDown;
            pictureBox.DoubleClick += mattePicBox_DoubleClick;
            pictureBox.ContextMenuStrip = pageContextMenu;
            Size panelSize = getPanelSize(viewIndex, matSize);
            Size layerSize = getLayerSize(viewIndex, matSize);
            int num = (matTabControl.DisplayRectangle.Width - panelSize.Width) / 2;
            int num2 = (matTabControl.DisplayRectangle.Height - panelSize.Height) / 2;
            pictureBox.Size = layerSize;
            panel.Size = panelSize;
            panel.AutoScroll = true;
            panel.BackColor = Color.FromArgb(255, 248, 219);
            panel.Controls.Add(pictureBox);
            panel.Location = new Point(num, num2);
            panel.Name = "panel5";
            panel.TabIndex = 34;
            panel.Tag = tabPage;
            tabPage.Controls.Add(panel);
            tabPage.BackColor = Color.FromArgb(255, 248, 219);
            tabPage.Location = new Point(0, 0);
            tabPage.Name = name;
            tabPage.Size = panelSize;
            tabPage.TabIndex = 0;
            tabPage.Text = name;
            tabPage.Tag = pictureBox;
            Canvas canvas = new Canvas(pictureBox, matSize);
            canvas.setCursor(0.25f, sizeValue + 0.25f, sizeValue);
            canvas.layerProperties.layerName = name;
            canvas.layerProperties.tag = panel;
            canvas.dirty = true;
            pagePreviewMenuItem.Checked = canvas.layerProperties.includeInPreview;
            if (pageColorMenuItem.Image == null)
            {
                pageColorMenuItem.Image = new Bitmap(16, 16);
                Graphics graphics = Graphics.FromImage(pageColorMenuItem.Image);
                graphics.Clear(canvas.layerProperties.color);
            }
            pictureBox.Tag = canvas;
            matTabControl.SelectTab(matTabControl.Controls.Count - 1);
            pictureBox.Focus();
            refreshMattePicBox();
        }

        public void deleteLayer()
        {
            if (matTabControl.Controls.Count < 2)
            {
                messageBoxOpen = true;
                DialogResult dialogResult = MessageBox.Show(myRootForm, "Can't delete this page because at least one page must always exist.\n\nDo you want to delete everything on this page instead?", "Delete This Page", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                messageBoxOpen = false;
                if (dialogResult == DialogResult.Yes)
                {
                    getCanvas().removeAll();
                    getCanvas().setCursor(0.25f, sizeValue + 0.25f, sizeValue);
                    refreshMattePicBox();
                }
                return;
            }
            TabPage tabPage = (TabPage)matTabControl.Controls[matTabControl.SelectedIndex];
            PictureBox pictureBox = (PictureBox)tabPage.Tag;
            if (pictureBox == null)
            {
                return;
            }
            _ = (Canvas)pictureBox.Tag;
            bool flag = false;
            if (isProjectDirty())
            {
                messageBoxOpen = true;
                DialogResult dialogResult = MessageBox.Show(myRootForm, "Are you sure you want to delete this page and everything on it?", "Delete This Page", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                messageBoxOpen = false;
                if (dialogResult == DialogResult.Yes)
                {
                    flag = true;
                }
            }
            else
            {
                flag = true;
            }
            if (flag)
            {
                int selectedIndex = matTabControl.SelectedIndex;
                matTabControl.Controls.RemoveAt(selectedIndex);
                projectDirty = true;
            }
        }

        public void resizeLayers()
        {
            if (getCanvas() == null)
            {
                return;
            }
            Size panelSize = getPanelSize(viewIndex, matSize);
            getLayerSize(viewIndex, matSize);
            int num = (matTabControl.DisplayRectangle.Width - panelSize.Width) / 2;
            int num2 = (matTabControl.DisplayRectangle.Height - panelSize.Height) / 2;
            foreach (TabPage tabPage in matTabControl.TabPages)
            {
                Panel panel = (Panel)tabPage.Controls[0];
                panel.Location = new Point(num, num2);
                panel.Size = panelSize;
                tabPage.Size = panelSize;
                changeView(panel);
            }
            ((Panel)getCanvas().layerProperties.tag).Refresh();
        }

        public Size getPanelSize(int viewSelectedIndex, int matSizeSelectedIndex)
        {
            int num = matTabControl.DisplayRectangle.Width - 16;
            int num2 = matTabControl.DisplayRectangle.Height - 16;
            Size layerSize = getLayerSize(viewIndex, matSize);
            int num3 = ((viewSelectedIndex != 0) ? 17 : 0);
            if (num > layerSize.Width + num3)
            {
                num = layerSize.Width + num3;
            }
            if (num2 > layerSize.Height + num3)
            {
                num2 = layerSize.Height + num3;
            }
            return new Size(num, num2);
        }

        public Size getLayerSize(int viewSelectedIndex, int matSizeSelectedIndex)
        {
            int num = matTabControl.DisplayRectangle.Width - 34;
            int num2 = matTabControl.DisplayRectangle.Height - 34;
            switch (viewSelectedIndex)
            {
                case 0:
                    switch (matSizeSelectedIndex)
                    {
                        case 0:
                        case 2:
                            if (num > num2 * 2)
                            {
                                num = num2 * 2;
                            }
                            else
                            {
                                num2 = num / 2;
                            }
                            break;

                        case 1:
                            if (num > num2 * 14 / 13)
                            {
                                num = num2 * 14 / 13;
                            }
                            else
                            {
                                num2 = num * 13 / 14;
                            }
                            break;
                    }
                    break;

                case 1:
                case 2:
                    switch (matSizeSelectedIndex)
                    {
                        case 0:
                        case 2:
                            if (num > num2 * 2)
                            {
                                num2 = num / 2;
                            }
                            else
                            {
                                num = num2 * 2;
                            }
                            break;

                        case 1:
                            if (num > num2 * 14 / 13)
                            {
                                num2 = num * 13 / 14;
                            }
                            else
                            {
                                num = num2 * 14 / 13;
                            }
                            break;
                    }
                    if (2 == viewSelectedIndex)
                    {
                        num = 2 * num;
                        num2 = 2 * num2;
                    }
                    break;
            }
            return new Size(num, num2);
        }

        public void sizeTrackBar_ValueChanged(int inc)
        {
            sizeCounter += inc;
            if (sizeCounter < 0)
            {
                sizeCounter = 0;
            }
            if (sizeCounter > 184)
            {
                sizeCounter = 184;
            }
            setRegistryValue(userKey, "shapeSize", sizeCounter.ToString());
            int num = sizeCounter / 8;
            int num2 = sizeCounter % 8;
            string text = "";
            switch (num2)
            {
                case 0:
                    text = "    \"";
                    break;

                case 1:
                    text = " 1/8\"";
                    break;

                case 2:
                    text = " 1/4\"";
                    break;

                case 3:
                    text = " 3/8\"";
                    break;

                case 4:
                    text = " 1/2\"";
                    break;

                case 5:
                    text = " 5/8\"";
                    break;

                case 6:
                    text = " 3/4\"";
                    break;

                case 7:
                    text = " 7/8\"";
                    break;
            }
            sizeLabel.Text = (1 + num).ToString().PadLeft(2) + text;
            sizeValue = (float)sizeCounter / 8f + 1f;
            getCanvas()?.adjCursor(0f, 0f, sizeValue);
        }

        private void ChangeTabColor(TabControl tabControl, DrawItemEventArgs e)
        {
            Font font = e.Font;
            Brush brush = new SolidBrush(Color.Green);
            Brush brush2 = new SolidBrush(Color.Yellow);
            _ = e.Index;
            _ = tabControl.SelectedIndex;
            string s = tabControl.TabPages[e.Index].Text;
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            e.Graphics.FillRectangle(brush, e.Bounds);
            Rectangle bounds = e.Bounds;
            bounds = new Rectangle(bounds.X, bounds.Y + 3, bounds.Width, bounds.Height - 3);
            e.Graphics.DrawString(s, font, brush2, bounds, stringFormat);
            stringFormat.Dispose();
            if (e.Index == tabControl.SelectedIndex)
            {
                brush.Dispose();
                return;
            }
            brush.Dispose();
            brush2.Dispose();
        }

        private void matTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = (TabControl)sender;
            ChangeTabColor(tabControl, e);
        }

        private void showRulerToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            toolStripMenuItem.Checked = !toolStripMenuItem.Checked;
            showRuler = toolStripMenuItem.Checked;
            refreshMattePicBox();
            setRegistryValue(userKey, "showRuler", showRuler.ToString());
        }

        private void changeView(Panel panel)
        {
            Canvas.clearPreview();
            TabPage tabPage = (TabPage)panel.Tag;
            if (tabPage != null)
            {
                PictureBox pictureBox = (PictureBox)tabPage.Tag;
                if (pictureBox != null)
                {
                    Canvas canvas = (Canvas)pictureBox.Tag;
                    panel.HorizontalScroll.Value = 0;
                    panel.VerticalScroll.Value = 0;
                    pictureBox.Size = getLayerSize(viewIndex, matSize);
                    canvas.setupSize(pictureBox, matSize);
                    canvas.adjCursor(0f, 0f, canvas.cursorSize);
                }
            }
        }

        private void changeMatSize_Click(object sender, EventArgs e)
        {
            Canvas.clearPreview();
            string text = "";
            int num = 0;
            foreach (ToolStripMenuItem dropDownItem in matSizeToolStripMenuItem.DropDownItems)
            {
                if (sender == dropDownItem)
                {
                    dropDownItem.Checked = true;
                    text = dropDownItem.Text;
                    matSize = num;
                }
                else
                {
                    dropDownItem.Checked = false;
                }
                num++;
            }
            matSizeToolStripMenuItem.Text = "Mat Size - " + text;
            setRegistryValue(userKey, "matSize", text);
            PictureBox mattePicBox = getMattePicBox();
            if (mattePicBox != null)
            {
                Canvas canvas = (Canvas)mattePicBox.Tag;
                int num2 = 12;
                int num3 = 12;
                switch (matSize)
                {
                    case 0:
                        num2 = 12;
                        num3 = 6;
                        break;

                    case 1:
                        num2 = 12;
                        num3 = 12;
                        break;

                    case 2:
                        num2 = 24;
                        num3 = 12;
                        break;
                }
                if (canvas.layerProperties.paperWidth > canvas.matteWidth - 2f)
                {
                    canvas.layerProperties.paperWidth = canvas.matteWidth - 2f;
                }
                if (canvas.layerProperties.paperHeight > canvas.matteHeight - 1f)
                {
                    canvas.layerProperties.paperHeight = canvas.matteHeight - 1f;
                }
                if (Math.Abs(canvas.layerProperties.paperWidth - (canvas.matteWidth - 2f)) < float.Epsilon)
                {
                    canvas.layerProperties.paperWidth = num2;
                }
                if (Math.Abs(canvas.layerProperties.paperHeight - (canvas.matteHeight - 1f)) < float.Epsilon)
                {
                    canvas.layerProperties.paperHeight = num3;
                }
                canvas.dirty = true;
                Form1_Resize(this, null);
            }
        }

        private void changeView_Click(object sender, EventArgs e)
        {
            Canvas.clearPreview();
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            _ = toolStripMenuItem.Checked;
            string text = "";
            fitToPageToolStripMenuItem.Checked = false;
            percent100ToolStripMenuItem.Checked = false;
            percent200ToolStripMenuItem.Checked = false;
            fitToPageButton.selected = false;
            percent100Button.selected = false;
            percent200Button.selected = false;
            if (fitToPageToolStripMenuItem == toolStripMenuItem)
            {
                viewIndex = 0;
                text = "Fit to Page";
                fitToPageButton.selected = true;
                fitToPageToolStripMenuItem.Checked = true;
            }
            else if (percent100ToolStripMenuItem == toolStripMenuItem)
            {
                viewIndex = 1;
                text = "100%";
                percent100Button.selected = true;
                percent100ToolStripMenuItem.Checked = true;
            }
            else if (percent200ToolStripMenuItem == toolStripMenuItem)
            {
                viewIndex = 2;
                text = "200%";
                percent200Button.selected = true;
                percent200ToolStripMenuItem.Checked = true;
            }
            matViewToolStripMenuItem.Text = "Mat View - " + text;
            setRegistryValue(userKey, "viewSize", text);
            fitToPageButton.Invalidate();
            percent100Button.Invalidate();
            percent200Button.Invalidate();
            Form1_Resize(this, null);
        }

        private void multiCut_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            _ = toolStripMenuItem.Checked;
            multiCut1ToolStripMenuItem.Checked = false;
            multiCut2ToolStripMenuItem.Checked = false;
            multiCut3ToolStripMenuItem.Checked = false;
            multiCut4ToolStripMenuItem.Checked = false;
            toolStripMenuItem.Checked = true;
            if (multiCut1ToolStripMenuItem == toolStripMenuItem)
            {
                multiCut = 0;
                multiCutToolStripMenuItem.Text = "Multi Cut - Off (Default)";
            }
            else if (multiCut2ToolStripMenuItem == toolStripMenuItem)
            {
                multiCut = 1;
                multiCutToolStripMenuItem.Text = "Multi Cut - 2 Times";
            }
            else if (multiCut3ToolStripMenuItem == toolStripMenuItem)
            {
                multiCut = 2;
                multiCutToolStripMenuItem.Text = "Multi Cut - 3 Times";
            }
            else if (multiCut4ToolStripMenuItem == toolStripMenuItem)
            {
                multiCut = 3;
                multiCutToolStripMenuItem.Text = "Multi Cut - 4 Times";
            }
        }

        private void enableSoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            toolStripMenuItem.Checked = !toolStripMenuItem.Checked;
            enableSounds = toolStripMenuItem.Checked;
            setRegistryValue(userKey, "enableSounds", enableSounds.ToString());
        }

        private void keepPreviewAsMatBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            toolStripMenuItem.Checked = !toolStripMenuItem.Checked;
            keepPreview = toolStripMenuItem.Checked;
            setRegistryValue(userKey, "keepPreview", keepPreview.ToString());
        }

        private void realSizeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            realSize = realSizeCheckBox.Checked;
            setRegistryValue(userKey, "realSize", realSize.ToString());
        }

        private void paperSaverCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            paperSaver = paperSaverCheckBox.Checked;
            setRegistryValue(userKey, "paperSaver", paperSaver.ToString());
        }

        private void defaultButton_Paint(object sender, PaintEventArgs e)
        {
            Button button = (Button)sender;
            Rectangle clientRectangle = button.ClientRectangle;
            Font font = button.Font;
            e.Graphics.MeasureString(button.Text, font);
            Color color = (button.Enabled ? darkMenuText : SystemColors.GrayText);
            SolidBrush brush = new SolidBrush(color);
            if (button.Tag != null && ((ButtonState)button.Tag).pressed)
            {
                ButtonRenderer.DrawButton(e.Graphics, clientRectangle, PushButtonState.Pressed);
            }
            else if (button.Tag != null && ((ButtonState)button.Tag).mouseOver)
            {
                ButtonRenderer.DrawButton(e.Graphics, clientRectangle, PushButtonState.Hot);
            }
            else
            {
                ButtonRenderer.DrawButton(e.Graphics, clientRectangle, PushButtonState.Normal);
            }
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            if (button.Tag != null)
            {
                clientRectangle.Location = new Point(clientRectangle.Location.X, clientRectangle.Location.Y + ((ButtonState)button.Tag).baselineAdj);
            }
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(button.Text, font, brush, clientRectangle, stringFormat);
        }

        private void bitmapButton_Paint(object sender, PaintEventArgs e)
        {
            Button button = (Button)sender;
            Font font = button.Font;
            SolidBrush brush = new SolidBrush(darkMenuText);
            e.Graphics.MeasureString(button.Text, font);
            if (button.Tag != null && ((ButtonState)button.Tag).pressed)
            {
                e.Graphics.DrawImageUnscaled(((ButtonState)button.Tag).pressedImg, 0, 0);
            }
            else if (button.Tag != null && ((ButtonState)button.Tag).mouseOver)
            {
                e.Graphics.DrawImageUnscaled(((ButtonState)button.Tag).hotImg, 0, 0);
            }
            else
            {
                e.Graphics.DrawImageUnscaled(((ButtonState)button.Tag).normalImg, 0, 0);
            }
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            if (button.Text != null && button.Text.Length > 0)
            {
                e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                e.Graphics.DrawString(button.Text, font, brush, button.ClientRectangle, stringFormat);
            }
        }

        private void button_MouseDown(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            if (button.Tag == null)
            {
                button.Tag = new ButtonState();
            }
            ((ButtonState)button.Tag).pressed = true;
            button.Refresh();
        }

        private void button_MouseUp(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            if (button.Tag == null)
            {
                button.Tag = new ButtonState();
            }
            ((ButtonState)button.Tag).pressed = false;
            button.Refresh();
        }

        private void button_MouseEnter(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (button.Tag == null)
            {
                button.Tag = new ButtonState();
            }
            ((ButtonState)button.Tag).mouseOver = true;
            button.Refresh();
        }

        private void button_MouseLeave(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (button.Tag == null)
            {
                button.Tag = new ButtonState();
            }
            ((ButtonState)button.Tag).mouseOver = false;
            button.Refresh();
        }

        private void enableBalloonHelpMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            toolStripMenuItem.Checked = !toolStripMenuItem.Checked;
            enableBalloonHelp = toolStripMenuItem.Checked;
            setRegistryValue(userKey, "balloonHelp", enableBalloonHelp.ToString());
            form1ToolTips.Active = enableBalloonHelp;
        }

        private void viewButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (button == fitToPageButton)
            {
                changeView_Click(fitToPageToolStripMenuItem, e);
            }
            else if (button == percent100Button)
            {
                changeView_Click(percent100ToolStripMenuItem, e);
            }
            else if (button == percent200Button)
            {
                changeView_Click(percent200ToolStripMenuItem, e);
            }
        }

        private void fontsChooseByComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            createCartridgeLibraryPanel((string)comboBox.SelectedItem);
            setRegistryValue(userKey, "fontView", (string)comboBox.SelectedItem);
        }

        private void myCartridgeMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            toolStripMenuItem.Checked = !toolStripMenuItem.Checked;
            fontLoading.selectedShape.properties.owned = toolStripMenuItem.Checked;
            if (((string)fontsChooseByComboBox.SelectedItem).CompareTo("My Cartridges") == 0)
            {
                createCartridgeLibraryPanel((string)fontsChooseByComboBox.SelectedItem);
            }
        }

        private void favoriteCartridgeMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            toolStripMenuItem.Checked = !toolStripMenuItem.Checked;
            fontLoading.selectedShape.properties.favorite = toolStripMenuItem.Checked;
            if (((string)fontsChooseByComboBox.SelectedItem).CompareTo("Favorites") == 0)
            {
                createCartridgeLibraryPanel((string)fontsChooseByComboBox.SelectedItem);
            }
        }

        private void mattePicBox_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownPnt.X = e.X;
            mouseDownPnt.Y = e.Y;
            mouseDown = true;
        }

        private void mattePicBox_MouseMove(object sender, MouseEventArgs e)
        {
            mouseMovePnt.X = e.X;
            mouseMovePnt.Y = e.Y;
            if (mouseDown)
            {
                if (!SceneGroup.dragging)
                {
                    getCanvas().dragBgn(mouseMovePnt);
                    SceneGroup.dragging = true;
                }
                else
                {
                    getCanvas().drag(mouseMovePnt, ctrlKeyDown ? SceneGroup.DragRelativeTo.PrimarySelection : SceneGroup.DragRelativeTo.Self);
                }
                refreshMattePicBox();
            }
        }

        private void mattePicBox_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
            SceneGroup.dragging = false;
            SceneUtils.thisHandle = null;
            getCanvas().dragEnd();
            refreshMattePicBox();
        }

        private void mattePicBox_Click(object sender, EventArgs e)
        {
            focusMattePicBox();
            if (!SceneGroup.dragging)
            {
                myRootForm.getCanvas().undoRedo.addAndDo(new UndoRedo.UndoSelectPoint(mouseDownPnt, !ctrlKeyDown));
                refreshMattePicBox();
            }
        }

        private void mattePicBox_DoubleClick(object sender, EventArgs e)
        {
            Canvas canvas = getCanvas();
            focusMattePicBox();
            if (canvas != null && !SceneGroup.dragging)
            {
                if (!canvas.IsAnythingSelected())
                {
                    PointF[] array = new PointF[2] { mouseDownPnt, mouseMovePnt };
                    canvas.canvasToWorld.TransformPoints(array);
                    canvas.setCursor(array[0].X - 1f, array[0].Y - 0.5f, canvas.cursorSize);
                }
                refreshMattePicBox();
            }
        }

        internal int GlyphButtonHeight()
        {
            return fontPicBox.Height / 5;
        }

        internal int GlyphButtonWidth()
        {
            return fontPicBox.Width / 10;
        }

        internal int GlyphButtonRow(int mousey)
        {
            int val = mousey / GlyphButtonHeight();
            val = Math.Min(4, val);
            return Math.Max(0, val);
        }

        internal int GlyphButtonColumn(int mousex)
        {
            int val = mousex / GlyphButtonWidth();
            val = Math.Min(9, val);
            return Math.Max(0, val);
        }

        internal int GlyphKeyID(int mousex, int mousey)
        {
            int num = GlyphButtonRow(mousey);
            int num2 = GlyphButtonColumn(mousex);
            return num * 14 + 2 + num2;
        }

        private void fontPicBox_Click(object sender, EventArgs e)
        {
            int num = GlyphKeyID(fontPicBox_mouseX, fontPicBox_mouseY);
            hoverBox.Tag = num;
            hoverBox_Click(sender, e);
        }

        private void fontPicBox_MouseEnter(object sender, EventArgs e)
        {
            fontPicBox_inside = true;
        }

        private void fontPicBox_MouseLeave(object sender, EventArgs e)
        {
            fontPicBox_inside = false;
            lastKeyId = -1;
        }

        private void fontPicBox_MouseMove(object sender, MouseEventArgs e)
        {
            fontPicBox_mouseX = e.X;
            fontPicBox_mouseY = e.Y;
            int num = GlyphKeyID(fontPicBox_mouseX, fontPicBox_mouseY);
            if (num != lastKeyId)
            {
                fontPicBox.Controls.Clear();
                lastKeyId = num;
                hoverBox.Tag = num;
                hoverTimer.Stop();
                hoverTimer.Start();
            }
        }

        private void featuresPicBox_MouseDown(object sender, MouseEventArgs e)
        {
            fontLoading.GlyphIndex = -1;
            PictureBox pictureBox = (PictureBox)sender;
            int num = 3 * e.Y / pictureBox.Height;
            int num2 = 2 * e.X / pictureBox.Width;
            int num3 = (num * 2 + num2) * 2 + 2 + (shiftLock ? 1 : 0);
            if (num3 == fontLoading.FontId)
            {
                num3 = (shiftLock ? 1 : 0);
            }
            fontLoading.FontId = num3;
            setRegistryValue(userKey, "currentFontFeature", fontLoading.FontId.ToString());
            fontLoading.selectedShape.properties.selectedFeatureId = fontLoading.FontId;
            fontLoading.selectedShape.properties.selectedKeyId = -1;
            featuresPicBox_backStore = null;
            fontPicBox_backStore = null;
            featuresPicBox.Refresh();
            fontPicBox.Refresh();
        }

        private void hoverBox_Paint(object sender, PaintEventArgs e)
        {
            int num = GlyphKeyID(fontPicBox_mouseX, fontPicBox_mouseY);
            Shape selectedShape = fontLoading.selectedShape;
            if (selectedShape != null)
            {
                hoverBox.Tag = num;
                bool flag = fontLoading.drawScaledGlyph(null, selectedShape, fontLoading.FontId, num, 0f, 0f, highlight: false, useScaleArg: false, 0f, drawingFeatures: false, fontLoading.glyphOnKeyFitSize, hoverBox.Width * 5, 32, 32);
                if (fontLoading.matteImage != null && flag)
                {
                    Bitmap image = new Bitmap(hoverBox.Width, hoverBox.Height);
                    Graphics graphics = Graphics.FromImage(image);
                    Matrix matrix = new Matrix();
                    new Pen(Color.FromArgb(255, 64, 64, 64), 1f * FontLoading.penScale);
                    float radius = 20f;
                    graphics.Clear(Color.FromArgb(0, 0, 0, 0));
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    fillRoundBox(graphics, radius, Color.FromArgb(64, 0, 0, 0), 2f, 2f, (float)hoverBox.Width - 0.5f, (float)hoverBox.Height - 0.5f);
                    fillRoundBox(graphics, radius, Color.FromArgb(224, 255, 255, 255), 0.5f, 0.5f, (float)hoverBox.Width - 2.5f, (float)hoverBox.Height - 2.5f);
                    matrix.Reset();
                    graphics.Transform = matrix;
                    graphics.InterpolationMode = InterpolationMode.High;
                    graphics.DrawImage(fontLoading.matteImage, 0, 0, hoverBox.Width - 2, hoverBox.Height - 2);
                    drawRoundBox(graphics, radius, Color.FromArgb(224, 0, 0, 0), 0.5f, 0.5f, (float)hoverBox.Width - 2.5f, (float)hoverBox.Height - 2.5f);
                    e.Graphics.CompositingMode = CompositingMode.SourceOver;
                    e.Graphics.DrawImage(image, 0, 0);
                }
            }
        }

        private void hoverBox_MouseEnter(object sender, EventArgs e)
        {
        }

        private void hoverBox_MouseLeave(object sender, EventArgs e)
        {
            lastKeyId = -1;
            hoverTimer.Stop();
            fontPicBox.Controls.Clear();
        }

        private void hoverBox_Click(object sender, EventArgs e)
        {
            int keyId = (int)hoverBox.Tag;
            float size = (float)sizeCounter / 8f + 1f;
            if (myRootForm.getCanvas() != null)
            {
                myRootForm.getCanvas().AddGlyphWithUndo(fontLoading, keyId, size);
            }
        }

        private void hoverTimer_Tick(object sender, EventArgs e)
        {
            hoverTimer.Stop();
            if (fontPicBox_inside)
            {
                GlyphKeyID(fontPicBox_mouseX, fontPicBox_mouseY);
                int num = GlyphButtonRow(fontPicBox_mouseY);
                int num2 = GlyphButtonColumn(fontPicBox_mouseX);
                int num3 = num2 * GlyphButtonWidth() - GlyphButtonWidth() / 4;
                int num4 = num3 + hoverBox.Width;
                int num5 = num * GlyphButtonHeight() - GlyphButtonHeight() / 4;
                int num6 = num5 + hoverBox.Height;
                if (num3 < 0)
                {
                    num4 += -num3;
                    num3 = 0;
                }
                else if (num4 >= fontPicBox.Width)
                {
                    num3 -= num4 - fontPicBox.Width;
                    num4 = fontPicBox.Width;
                }
                if (num5 < 0)
                {
                    num6 += -num5;
                    num5 = 0;
                }
                else if (num6 >= fontPicBox.Height)
                {
                    num5 -= num6 - fontPicBox.Height;
                    num6 = fontPicBox.Height;
                }
                hoverBox.Location = new Point(num3, num5);
                hoverBox.Size = new Size(50, 50);
                fontPicBox.Controls.Add(hoverBox);
            }
        }

        private void continueTrialButton_Click(object sender, EventArgs e)
        {
            saveActData(!trialMode);
            enableNormalControls(enabled: true);
            matTabControl.TabPages.Clear();
            addEmptyLayer();
            addLayer("untitled page");
            cleanProject();
        }

        private void shiftKeyLabel_MouseDown(object sender, MouseEventArgs e)
        {
            fontLoading.GlyphIndex = -1;
            int fontId = fontLoading.FontId;
            fontId /= 2;
            fontId *= 2;
            shiftLock = !shiftLock;
            if (!shiftLock)
            {
                shiftKeyLabel.Image = MarcusResources.shift_lock;
                fontLoading.FontId = fontId;
            }
            else
            {
                shiftKeyLabel.Image = MarcusResources.shift_lock_hot;
                fontLoading.FontId = fontId + 1;
            }
            shiftKeyLabel.Refresh();
            setRegistryValue(userKey, "currentFontFeature", fontLoading.FontId.ToString());
            fontLoading.selectedShape.properties.selectedFeatureId = fontLoading.FontId;
            fontLoading.selectedShape.properties.selectedKeyId = -1;
            featuresPicBox_backStore = null;
            fontPicBox_backStore = null;
            featuresPicBox.Refresh();
            fontPicBox.Refresh();
        }

        private void newPageButton_Click(object sender, EventArgs e)
        {
            Canvas.clearPreview();
            addEmptyLayer();
            addLayer("untitled page");
        }

        private void deletePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Canvas.clearPreview();
            deleteLayer();
        }

        public bool checkClose()
        {
            messageBoxOpen = true;
            DialogResult dialogResult = MessageBox.Show(myRootForm, "This project has changed.\n\nWould you like to save your changes before continuing?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            messageBoxOpen = false;
            switch (dialogResult)
            {
                case DialogResult.Yes:
                    if (projectProperties.filename != null && File.Exists(projectProperties.filename))
                    {
                        saveProject(projectProperties.filename);
                        break;
                    }
                    if (myRootForm.trialMode)
                    {
                        saveProjectDialog.DefaultExt = ".cut";
                        saveProjectDialog.Filter = "Cricut files (*.cut)|*.cut|All files (*.*)|*.*";
                    }
                    else
                    {
                        saveProjectDialog.DefaultExt = ".cut";
                        saveProjectDialog.Filter = "Cricut files (*.cut)|*.cut|Gypsy files (*.gypsy)|*.gypsy|All files (*.*)|*.*";
                    }
                    saveProjectDialog.InitialDirectory = projectFolderPath;
                    if (saveProjectDialog.ShowDialog() == DialogResult.OK)
                    {
                        string fileName = saveProjectDialog.FileName;
                        saveProject(fileName);
                        break;
                    }
                    return false;

                case DialogResult.Cancel:
                    return false;
            }
            return true;
        }

        public bool closeProject(bool force)
        {
            if (!force && isProjectDirty() && !checkClose())
            {
                return false;
            }
            SceneGroup.CutCartRecord.reset();
            Canvas.clearPreview();
            matTabControl.Controls.Clear();
            addEmptyLayer();
            addLayer("untitled page");
            cleanProject();
            projectProperties.filename = null;
            saveProjectMenuItem.Enabled = false;
            Text = getAppName(null);
            return true;
        }

        private void newProjectMenuItem_Click(object sender, EventArgs e)
        {
            closeProject(force: false);
        }

        private void closeProjectMenuItem_Click(object sender, EventArgs e)
        {
            closeProject(force: false);
        }

        private void saveProjectMenuItem_Click(object sender, EventArgs e)
        {
            if (projectProperties.filename != null && File.Exists(projectProperties.filename))
            {
                saveProject(projectProperties.filename);
            }
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            if (closeProject(force: false))
            {
                Application.Exit();
            }
        }

        private void deleteAllOnThisPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (getCanvas() == null)
            {
                return;
            }
            if (getCanvas().isDirty())
            {
                messageBoxOpen = true;
                DialogResult dialogResult = MessageBox.Show(myRootForm, "Are you sure you want to delete everything on this page?", "Delete This Page", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                messageBoxOpen = false;
                if (dialogResult == DialogResult.No)
                {
                    return;
                }
            }
            Canvas.clearPreview();
            getCanvas().removeAll();
            getCanvas().setCursor(0.25f, sizeValue + 0.25f, sizeValue);
            refreshMattePicBox();
        }

        private void gotoNextGlyph(int direction)
        {
            if (fromtagsList != null && fromtagsList.Count > 0)
            {
                int fontNo = 0;
                int featureId = 0;
                int glyphId = 0;
                if (fromtagsIndex < 0 && direction > 0)
                {
                    fromtagsIndex = 0;
                }
                else if (fromtagsIndex < 0 && direction < 0)
                {
                    fromtagsIndex = fromtagsList.Count - 1;
                }
                else if (direction > 0)
                {
                    fromtagsIndex = (fromtagsIndex + 1) % fromtagsList.Count;
                }
                else
                {
                    fromtagsIndex = ((fromtagsIndex > 0) ? (fromtagsIndex - 1) : (fromtagsList.Count - 1));
                }
                TagDictionary.fromTagRef((int)fromtagsList[fromtagsIndex], ref fontNo, ref featureId, ref glyphId);
                fontLoading.GlyphIndex = glyphId;
                fontLoading.selectFont((Shape)fontLoading.shapes[fontNo], featureId);
                featuresPicBox.Refresh();
                fontPicBox.Refresh();
            }
        }

        private void nextKeywordButton_Click(object sender, EventArgs e)
        {
            gotoNextGlyph(1);
        }

        private void prevKeywordButton_Click(object sender, EventArgs e)
        {
            gotoNextGlyph(-1);
        }

        private void setGlyphPositionX(SceneGlyph glyph, float x)
        {
            glyph.ox = x;
            glyph.bbox.Location = new PointF(glyph.ox, glyph.oy);
            glyph.glyphToWorld = Canvas.glyphToWorld(glyph.ox, glyph.oy, glyph.size);
            glyph.bboxAssigned = false;
        }

        private void setGlyphPositionY(SceneGlyph glyph, float y)
        {
            glyph.oy = y;
            glyph.bbox.Location = new PointF(glyph.ox, glyph.oy);
            glyph.glyphToWorld = Canvas.glyphToWorld(glyph.ox, glyph.oy, glyph.size);
            glyph.bboxAssigned = false;
        }

        private void nudge(float directionHorz, float directionVert)
        {
            ArrayList sceneGroups = myRootForm.getCanvas().sceneGroups;
            foreach (SceneGroup item in sceneGroups)
            {
                if (!item.selected)
                {
                    continue;
                }
                bool flag = false;
                for (int i = 1; i < item.children.Count; i++)
                {
                    SceneGlyph sceneGlyph = (SceneGlyph)item.children[i];
                    if (sceneGlyph.Equals(SceneGlyph.selectedGlyph))
                    {
                        flag = true;
                        if (directionVert != 0f)
                        {
                            setGlyphPositionY(sceneGlyph, sceneGlyph.oy + directionVert);
                        }
                    }
                    if (flag)
                    {
                        item.dirty = true;
                        if (directionHorz != 0f)
                        {
                            setGlyphPositionX(sceneGlyph, sceneGlyph.ox + directionHorz);
                        }
                    }
                }
            }
            refreshMattePicBox();
            focusMattePicBox();
        }

        public void EnableGlyphManipulationControls(bool en)
        {
            showFontMenuItem.Enabled = en;
            nudgeLeftButton.Enabled = en;
            nudgeRightButton.Enabled = en;
            nudgeUpButton.Enabled = en;
            nudgeDownButton.Enabled = en;
        }

        private void nudgeLeftButton_Click(object sender, EventArgs e)
        {
            nudge(0f - nudgeValue, 0f);
        }

        private void setupNudgeAutorepeat(float directionx, float directiony)
        {
            nudgeTimer = new NudgeAutorepeatTimer(this, directionx * nudgeValue, directiony * nudgeValue);
            nudgeTimer.Start();
        }

        private void cleanupNudgeAutorepeat()
        {
            if (nudgeTimer != null)
            {
                nudgeTimer.Stop();
            }
            nudgeTimer = null;
        }

        private void nudgeLeftButton_MouseDown(object sender, MouseEventArgs e)
        {
            setupNudgeAutorepeat(-1f, 0f);
            button_MouseDown(sender, e);
        }

        private void nudgeButton_MouseUp(object sender, MouseEventArgs e)
        {
            cleanupNudgeAutorepeat();
            button_MouseUp(sender, e);
        }

        private void nudgeRightButton_Click(object sender, EventArgs e)
        {
            nudge(nudgeValue, 0f);
        }

        private void nudgeRightButton_MouseDown(object sender, MouseEventArgs e)
        {
            setupNudgeAutorepeat(1f, 0f);
            button_MouseDown(sender, e);
        }

        private void nudgeUpButton_Click(object sender, EventArgs e)
        {
            nudge(0f, 0f - nudgeValue);
        }

        private void nudgeUpButton_MouseDown(object sender, MouseEventArgs e)
        {
            setupNudgeAutorepeat(0f, -1f);
            button_MouseDown(sender, e);
        }

        private void nudgeDownButton_Click(object sender, EventArgs e)
        {
            nudge(0f, nudgeValue);
        }

        private void nudgeDownButton_MouseDown(object sender, MouseEventArgs e)
        {
            setupNudgeAutorepeat(0f, 1f);
            button_MouseDown(sender, e);
        }

        private void rotate90Button_Click(object sender, EventArgs e)
        {
            SceneGroup.doNumBoxTransform(10);
            refreshMattePicBox();
        }

        private void showFont_Click(object sender, EventArgs e)
        {
            if (SceneGlyph.selectedGlyph != null)
            {
                fontLoading.GlyphIndex = FontLoading.keyIdToGlyphIndex(SceneGlyph.selectedGlyph.keyId);
                fontLoading.selectFont(SceneGlyph.selectedGlyph.shape, SceneGlyph.selectedGlyph.fontId);
                featuresPicBox.Refresh();
                fontPicBox.Refresh();
            }
        }

        private void changePageNameMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripTextBox toolStripTextBox = pageNameTextBox;
            Canvas canvas = getCanvas();
            canvas.layerProperties.layerName = toolStripTextBox.Text;
            canvas.dirty = true;
            matTabControl.SelectedTab.Name = toolStripTextBox.Text;
            matTabControl.SelectedTab.Text = toolStripTextBox.Text;
        }

        private void matTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (activateByInternetTabPage == matTabControl.SelectedTab)
            {
                iSerialNum1.Text = pSerialNum1.Text;
                iSerialNum2.Text = pSerialNum2.Text;
                iSerialNum3.Text = pSerialNum3.Text;
                iSerialNum4.Text = pSerialNum4.Text;
                iSerialNum5.Text = pSerialNum5.Text;
                iSerialNum6.Text = pSerialNum6.Text;
                iFirstNameTextBox.Text = pFirstNameTextBox.Text;
                iLastNameTextBox.Text = pLastNameTextBox.Text;
                iEmailTextBox.Text = pEmailTextBox.Text;
            }
            else if (activateByPhoneTabPage == matTabControl.SelectedTab)
            {
                pSerialNum1.Text = iSerialNum1.Text;
                pSerialNum2.Text = iSerialNum2.Text;
                pSerialNum3.Text = iSerialNum3.Text;
                pSerialNum4.Text = iSerialNum4.Text;
                pSerialNum5.Text = iSerialNum5.Text;
                pSerialNum6.Text = iSerialNum6.Text;
                pFirstNameTextBox.Text = iFirstNameTextBox.Text;
                pLastNameTextBox.Text = iLastNameTextBox.Text;
                pEmailTextBox.Text = iEmailTextBox.Text;
            }
            else if (getCanvas() != null)
            {
                pageNameTextBox.Text = getCanvas().layerProperties.layerName;
                pagePreviewMenuItem.Checked = getCanvas().layerProperties.includeInPreview;
                Graphics graphics = Graphics.FromImage(pageColorMenuItem.Image);
                graphics.Clear(getCanvas().layerProperties.color);
            }
        }

        private void glyphContextMenu_Opening(object sender, CancelEventArgs e)
        {
            int num = 18;
            int keyId = (int)hoverBox.Tag;
            bool flag = fontLoading.drawScaledGlyph(null, fontLoading.selectedShape, fontLoading.FontId, keyId, 0f, 0f, highlight: false, useScaleArg: false, 0f, drawingFeatures: false, 0.95f, num * 5, 32, 32);
            if (fontLoading.matteImage == null || !flag)
            {
                e.Cancel = true;
                return;
            }
            Bitmap image = new Bitmap(num, num);
            Graphics graphics = Graphics.FromImage(image);
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.High;
            graphics.DrawImage(fontLoading.matteImage, 0, 0, num, num);
            addKeywordMenuItem.Image = image;
            addKeywordMenuItem.ImageScaling = ToolStripItemImageScaling.SizeToFit;
            string[] tags = fontLoading.fontMetadata.getTags(fontLoading.selectedShape.getFontName(), fontLoading.FontId, FontLoading.keyIdToGlyphIndex(keyId));
            removeKeywordMenuItem.DropDownItems.Clear();
            if (tags != null)
            {
                string[] array = tags;
                foreach (string text in array)
                {
                    removeKeywordMenuItem.DropDownItems.Add(text, null, removeKeyword_Click);
                }
            }
            if (removeKeywordMenuItem.DropDownItems.Count > 0)
            {
                removeKeywordMenuItem.Enabled = true;
            }
            else
            {
                removeKeywordMenuItem.Enabled = false;
            }
        }

        private void removeKeyword_Click(object sender, EventArgs e)
        {
            int keyId = (int)hoverBox.Tag;
            ToolStripDropDownItem toolStripDropDownItem = (ToolStripDropDownItem)sender;
            string[] array = new string[removeKeywordMenuItem.DropDownItems.Count - 1];
            string text = null;
            int i = 0;
            int num = 0;
            for (; i < removeKeywordMenuItem.DropDownItems.Count; i++)
            {
                if (toolStripDropDownItem == removeKeywordMenuItem.DropDownItems[i])
                {
                    text = removeKeywordMenuItem.DropDownItems[i].Text;
                }
                else
                {
                    array[num++] = removeKeywordMenuItem.DropDownItems[i].Text;
                }
            }
            fontLoading.fontMetadata.setTags(fontLoading.selectedShape.getFontName(), fontLoading.FontId, FontLoading.keyIdToGlyphIndex(keyId), array);
            if (text != null)
            {
                fontLoading.tagDictionary.removeTagString(text, fontLoading.getFontIndex(fontLoading.selectedShape), fontLoading.FontId, FontLoading.keyIdToGlyphIndex(keyId));
            }
        }

        private void addKeywordMenuItem_Click(object sender, EventArgs e)
        {
            int keyId = (int)hoverBox.Tag;
            string text = glyphKeywordComboBox.Text;
            if (text != null && text.Length > 0)
            {
                fontLoading.fontMetadata.addTag(fontLoading.selectedShape.getFontName(), fontLoading.FontId, FontLoading.keyIdToGlyphIndex(keyId), text);
                string[] tags = new string[1] { text };
                fontLoading.tagDictionary.addTagStrings(tags, fontLoading.getFontIndex(fontLoading.selectedShape), fontLoading.FontId, FontLoading.keyIdToGlyphIndex(keyId));
                myRootForm.fontsTag1ComboBox.Items.AddRange(fontLoading.tagDictionary.getKeywordsStringList());
                myRootForm.fontsTag2ComboBox.Items.AddRange(fontLoading.tagDictionary.getKeywordsStringList());
                myRootForm.glyphKeywordComboBox.Items.AddRange(fontLoading.tagDictionary.getKeywordsStringList());
            }
        }

        private void swapTabPages(TabControl tc, int p1, int p2)
        {
            int tabCount = tc.TabCount;
            if (p1 != p2 && p1 >= 0 && p2 >= 0 && p1 < tabCount && p2 < tabCount)
            {
                TabPage value = matTabControl.TabPages[p1];
                matTabControl.TabPages[p1] = matTabControl.TabPages[p2];
                matTabControl.TabPages[p2] = value;
                matTabControl.SelectedTab = matTabControl.TabPages[p1];
                matTabControl.Refresh();
                projectDirty = true;
            }
        }

        private void sendPagetoBackMenuItem_Click(object sender, EventArgs e)
        {
            TabPage value = matTabControl.TabPages[matTabControl.SelectedIndex];
            matTabControl.TabPages.Remove(value);
            matTabControl.TabPages.Add(value);
            matTabControl.SelectedTab = matTabControl.TabPages[matTabControl.TabPages.Count - 1];
            matTabControl.Refresh();
            projectDirty = true;
        }

        private void bringPagetoFrontMenuItem_Click(object sender, EventArgs e)
        {
            TabPage tabPage = matTabControl.TabPages[matTabControl.SelectedIndex];
            matTabControl.TabPages.Remove(tabPage);
            matTabControl.TabPages.Insert(0, tabPage);
            matTabControl.SelectedTab = matTabControl.TabPages[0];
            matTabControl.Refresh();
            projectDirty = true;
        }

        private void sendPageBackwardMenuItem_Click(object sender, EventArgs e)
        {
            swapTabPages(matTabControl, matTabControl.SelectedIndex + 1, matTabControl.SelectedIndex);
        }

        private void bringPageForwardMenuItem_Click(object sender, EventArgs e)
        {
            swapTabPages(matTabControl, matTabControl.SelectedIndex - 1, matTabControl.SelectedIndex);
        }

        private void glyphKeywordComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keys.Return == e.KeyCode)
            {
                anyTagComboBox_SelectedIndexChanged(sender, null);
            }
            else if (Keys.Escape == e.KeyCode)
            {
                anyTagComboBox_SelectedIndexChanged(sender, null);
            }
        }

        private void cutWithCricutMenuItem_Click(object sender, EventArgs e)
        {
            cutWithCricut();
        }

        private void reverShapePropertiesButton_Click(object sender, EventArgs e)
        {
            ArrayList sceneGroups = myRootForm.getCanvas().sceneGroups;
            foreach (SceneGroup item in sceneGroups)
            {
                if (item.selected)
                {
                    item.resetTransform();
                }
            }
            myRootForm.refreshMattePicBox();
        }

        private void activateByPhoneButton_Click(object sender, EventArgs e)
        {
            matTabControl.SelectedIndex = 1;
        }

        private void goBackToInternetActivationButton_Click(object sender, EventArgs e)
        {
            matTabControl.SelectedIndex = 0;
        }

        private void verifyActKeyButton_Click(object sender, EventArgs e)
        {
            verifyActivation(getRegNumFromTextBoxes(), getActNumFromTextBoxes());
            if (!trialMode)
            {
                activationKeyStatusLabel.ForeColor = Color.Green;
                activationKeyStatusLabel.Text = "Activation OK";
                activationKeyStatusLabel.Update();
                utilityTimer.Interval = 3000;
                utilityTimer.Tick += utilityTimer_Tick;
                utilityTimer.Start();
            }
            else
            {
                activationKeyStatusLabel.ForeColor = Color.DarkGray;
                activationKeyStatusLabel.Text = "Not Activated";
            }
            activationKeyStatusLabel.Visible = true;
            Text = getAppName(null);
        }

        private static string activateByInternet(string macAddr, string regNum, string firstName, string lastName, string email)
        {
            string text = null;
            Service service = null;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                service = new Service();
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                myRootForm.messageBoxOpen = true;
                MessageBox.Show(myRootForm, ex.Message, "Not Activated", MessageBoxButtons.OK);
                myRootForm.messageBoxOpen = false;
                return null;
            }
            try
            {
                text = service.Register(firstName, lastName, email, regNum, macAddr);
            }
            catch
            {
                Cursor.Current = Cursors.Default;
                myRootForm.messageBoxOpen = true;
                MessageBox.Show(myRootForm, "License activation did not succeed because the Internet was not found.\n\nPlease make sure that your computer has a valid Internet connection and try again.", "Not Activated", MessageBoxButtons.OK);
                myRootForm.messageBoxOpen = false;
                text = null;
            }
            service.Dispose();
            Cursor.Current = Cursors.Default;
            if (text != null)
            {
                Regex regex = new Regex("[^a-fA-F0-9-]");
                if (regex.IsMatch(text))
                {
                    myRootForm.messageBoxOpen = true;
                    MessageBox.Show(myRootForm, text, "Not Activated - Server Error", MessageBoxButtons.OK);
                    myRootForm.messageBoxOpen = false;
                    return null;
                }
            }
            return text;
        }

        private void activateByInternetButton_Click(object sender, EventArgs e)
        {
            string mACAddress = getMACAddress();
            string regNumFromTextBoxes = getRegNumFromTextBoxes();
            string text = iFirstNameTextBox.Text;
            string text2 = iLastNameTextBox.Text;
            string text3 = iEmailTextBox.Text;
            string text4 = null;
            setRegistryValue(userKey, "firstName", text);
            setRegistryValue(userKey, "lastName", text2);
            setRegistryValue(userKey, "email", text3);
            setRegistryValue(userKey, "regNum", regNumFromTextBoxes);
            text4 = activateByInternet(mACAddress.Replace(":", ""), regNumFromTextBoxes.Replace("-", ""), text, text2, text3);
            if (text4 != null)
            {
                verifyActivation(getRegNumFromTextBoxes(), text4);
            }
            if (trialMode)
            {
                messageBoxOpen = true;
                MessageBox.Show(myRootForm, "License activation did not succeed.\n\nPlease make sure that your serial number was correctly entered and try again.", "Not Activated", MessageBoxButtons.OK);
                messageBoxOpen = false;
            }
            else
            {
                setActNum(text4);
                utilityTimer.Interval = 500;
                utilityTimer.Tick += utilityTimer_Tick;
                utilityTimer.Start();
            }
            Text = getAppName(null);
        }

        private void utilityTimer_Tick(object sender, EventArgs e)
        {
            utilityTimer.Stop();
            saveActData(actOkay: true);
            if (utilityTimerMode == 0)
            {
                setRegNum(getRegNumFromTextBoxes());
                setActNum(getActNumFromTextBoxes());
                if (activateByInternetTabPage == matTabControl.SelectedTab)
                {
                    aFirstNameTextBox.Text = iFirstNameTextBox.Text;
                    aLastNameTextBox.Text = iLastNameTextBox.Text;
                    aEmailTextBox.Text = iEmailTextBox.Text;
                }
                else if (activateByPhoneTabPage == matTabControl.SelectedTab)
                {
                    aFirstNameTextBox.Text = pFirstNameTextBox.Text;
                    aLastNameTextBox.Text = pLastNameTextBox.Text;
                    aEmailTextBox.Text = pEmailTextBox.Text;
                }
                activationCompleteOKButton.Focus();
                matTabControl.TabPages.Add(activationCompleteTabPage);
                matTabControl.TabPages.Remove(activateByInternetTabPage);
                matTabControl.TabPages.Remove(activateByPhoneTabPage);
                utilityTimerMode++;
            }
        }

        private void anyTextBox_Enter(object sender, EventArgs e)
        {
            allowLocalFocus = true;
        }

        private void anyTextBox_Leave(object sender, EventArgs e)
        {
            allowLocalFocus = false;
        }

        private void hex_textBox_Leave(object sender, EventArgs e)
        {
            allowLocalFocus = false;
            TextBox textBox = (TextBox)sender;
            string text = textBox.Text;
            string text2 = "";
            text = text.ToUpper();
            string text3 = text;
            foreach (char c in text3)
            {
                text2 = (('O' != c) ? (('L' != c) ? (('Z' != c) ? (('0' != c && '1' != c && '2' != c && '3' != c && '4' != c && '5' != c && '6' != c && '7' != c && '8' != c && '9' != c && 'A' != c && 'B' != c && 'C' != c && 'D' != c && 'E' != c && 'F' != c) ? (text2 + '*') : (text2 + c)) : (text2 + '2')) : (text2 + '1')) : (text2 + '0'));
            }
            textBox.Text = text2.PadLeft(textBox.MaxLength, '*');
        }

        private void activationCompleteOKButton_Click(object sender, EventArgs e)
        {
            matTabControl.TabPages.Clear();
            addEmptyLayer();
            addLayer("untitled page");
            cleanProject();
            enableNormalControls(enabled: true);
        }

        private void flipShapesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ArrayList sceneGroups = myRootForm.getCanvas().sceneGroups;
            foreach (SceneGroup item in sceneGroups)
            {
                if (!item.selected)
                {
                    continue;
                }
                item.dirty = true;
                item.flipShapes = flipShapesCheckBox.Checked;
                foreach (SceneGlyph child in item.children)
                {
                    child.bboxAssigned = false;
                }
            }
            myRootForm.refreshMattePicBox();
        }

        private void weldingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ArrayList sceneGroups = myRootForm.getCanvas().sceneGroups;
            foreach (SceneGroup item in sceneGroups)
            {
                if (item.selected)
                {
                    item.dirty = true;
                    item.welding = weldingCheckBox.Checked;
                }
            }
            myRootForm.refreshMattePicBox();
        }

        private void beginUpdatingButton_Click(object sender, EventArgs e)
        {
            bool flag = true;
            string text = "";
            if (string.Compare(beginUpdatingButton.Text, "Begin 2nd Update Now") == 0)
            {
                flag = false;
            }
            if (v1RadioButton.Checked)
            {
                text = ((!flag) ? "cricut_v1_34b.enc" : "cricut_v1_34.enc");
            }
            else if (v2RadioButton.Checked)
            {
                text = ((!flag) ? "cricut_v2_34b.enc" : "cricut_v2_34.enc");
            }
            else
            {
                if (!v3RadioButton.Checked)
                {
                    messageBoxOpen = true;
                    MessageBox.Show(myRootForm, "Please choose the correct model of your Cricut machine. (See Step 3.)\n\nThe Cricut Personal Cutter has a one line display located on the flip-up lid.\nThe Cricut Create has a multi-line line display located on the flip-up lid.\nThe Cricut Expression has a multi-line display located on the keypad panel.", "Cricut Model Not Selected", MessageBoxButtons.OK);
                    messageBoxOpen = false;
                    return;
                }
                text = "cricut_v15_4.enc";
            }
            USB_Interface uSB_Interface = new USB_Interface();
            if (uSB_Interface.driverOkay && openCricut(uSB_Interface, 14400))
            {
                uSB_Interface.Close();
                updatingProgress = new UpdatingProgressForm();
                updatingProgress.Text = "Updating Firmware";
                updatingProgress.updatingProgressBar.Value = 0;
                text = exeFolderPath + "\\Firmware Releases\\" + text;
                firmwareBackgroundWorker.RunWorkerAsync(text);
                updatingProgress.ShowDialog(this);
                if (firmwareBackgroundWorker.IsBusy)
                {
                    firmwareBackgroundWorker.CancelAsync();
                }
                updatingProgress = null;
                PcControl pcControl = new PcControl();
                bool flag2 = true;
                if (pcControl.bgn())
                {
                    USB_Interface.setTimeout(4000);
                    int machine = 0;
                    int major = 0;
                    int minor = 0;
                    if (pcControl.getVersion(ref machine, ref major, ref minor))
                    {
                        if (20 == machine && major >= 2 && minor >= 34)
                        {
                            flag2 = false;
                        }
                        else if (10 == machine && major >= 1 && minor >= 34)
                        {
                            flag2 = false;
                        }
                        else if (15 == machine && major >= 3 && minor >= 4)
                        {
                            flag2 = false;
                        }
                    }
                    pcControl.end();
                }
                if (updateStatus && flag2)
                {
                    successfulUpdateLabel.Text = "Your Cricut requires a 2nd update pass. Please start over at STEP 1 to perform the 2nd update.";
                    successfulUpdateLabel.Visible = true;
                    beginUpdatingButton.Text = "Begin 2nd Update Now";
                    beginUpdatingButton.Enabled = true;
                }
                else if (updateStatus)
                {
                    successfulUpdateLabel.Text = "The Cricut firmware was successfully updated.  Please click the \"OK\" button to continue.";
                    successfulUpdateLabel.Visible = true;
                    beginUpdatingButton.Text = "Begin Updating Now";
                    beginUpdatingButton.Enabled = false;
                    updateLaterButton.Text = "OK";
                }
                else
                {
                    successfulUpdateLabel.Text = "Sorry, the Cricut firmware update failed.   Please start over at STEP 1 to try again.";
                    successfulUpdateLabel.Visible = true;
                    beginUpdatingButton.Text = "Begin Updating Now";
                    beginUpdatingButton.Enabled = true;
                }
            }
            else if (!uSB_Interface.driverOkay)
            {
                messageBoxOpen = true;
                MessageBox.Show(myRootForm, "Your Cricut was not found.\n\nThe USB drivers that this computer uses to communicate with the Cricut\nmay be out of date or not properly installed. Please refer to the \"Getting Started\"\ndocumentation for help on properly connecting this computer to your Cricut machine.", "Cricut Not Found", MessageBoxButtons.OK);
                messageBoxOpen = false;
            }
            else
            {
                messageBoxOpen = true;
                MessageBox.Show(myRootForm, "Your Cricut was not found.\n\nPlease follow the instructions carefully starting with Step 1.", "Cricut Not Found", MessageBoxButtons.OK);
                messageBoxOpen = false;
            }
        }

        private void updateLaterButton_Click(object sender, EventArgs e)
        {
            foreach (TabPage tabPage in matTabControl.TabPages)
            {
                if (tabPage == updateCricutFirmwareTabPage)
                {
                    matTabControl.TabPages.Remove(tabPage);
                    break;
                }
            }
            enableNormalControls(enabled: true);
        }

        private void updateFirmwareMenuItem_Click(object sender, EventArgs e)
        {
            enableNormalControls(enabled: false);
            successfulUpdateLabel.Text = "The Cricut firmware was successfully updated.  Please click the \"OK\" button to continue.";
            successfulUpdateLabel.Visible = false;
            beginUpdatingButton.Text = "Begin Updating Now";
            beginUpdatingButton.Enabled = true;
            updateLaterButton.Text = "Update Later";
            v1RadioButton.Checked = false;
            v2RadioButton.Checked = false;
            matTabControl.TabPages.Add(updateCricutFirmwareTabPage);
            matTabControl.SelectedTab = updateCricutFirmwareTabPage;
        }

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private void firmwareBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(500);
            _ = (BackgroundWorker)sender;
            string filename = (string)e.Argument;
            UpdateCricutFirmware updateCricutFirmware = new UpdateCricutFirmware();
            updateCricutFirmware.read(filename);
            updateStartCounter = 0L;
            updateFreq = 0L;
            lastSecs = int.MaxValue;
            updateStatus = updateCricutFirmware.update(updateCricutFirmware.bytes);
        }

        private void firmwareBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _ = (BackgroundWorker)sender;
            if (0 == updateStartCounter)
            {
                QueryPerformanceFrequency(out updateFreq);
                QueryPerformanceCounter(out updateStartCounter);
            }
            if (updatingProgress == null || updatingProgress.Disposing)
            {
                return;
            }
            if (e.ProgressPercentage > 100)
            {
                long lpPerformanceCount = 0L;
                QueryPerformanceCounter(out lpPerformanceCount);
                double num = (double)(lpPerformanceCount - updateStartCounter) / (double)updateFreq;
                double num2 = num / ((double)e.ProgressPercentage / 10000.0);
                int num3 = (int)Math.Round(num2 - num);
                if (num3 > lastSecs)
                {
                    num3 = lastSecs;
                }
                else
                {
                    lastSecs = num3;
                }
                int num4 = num3 / 60;
                num3 -= num4 * 60;
                updatingProgress.Text = "Updating Firmware - " + num4.ToString().PadLeft(2, ' ') + "m" + num3.ToString().PadLeft(2, ' ') + "s remaining";
            }
            updatingProgress.updatingProgressBar.Value = e.ProgressPercentage / 100;
        }

        private void firmwareBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (updatingProgress != null && !updatingProgress.Disposing)
            {
                updatingProgress.Close();
            }
        }

        public void createPreview(bool drawBkgnd)
        {
            if (getCanvas() == null)
            {
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            Canvas.clearPreview();
            for (int num = matTabControl.TabPages.Count - 1; num >= 0; num--)
            {
                TabPage tabPage = matTabControl.TabPages[num];
                PictureBox pictureBox = (PictureBox)tabPage.Tag;
                Canvas canvas = (Canvas)pictureBox.Tag;
                if (canvas.layerProperties.includeInPreview)
                {
                    if (Canvas.previewBitmap == null)
                    {
                        Canvas.previewFreshness = 0;
                        Canvas.previewBitmap = canvas.drawPreview(null, drawBkgnd);
                    }
                    else
                    {
                        canvas.drawPreview(Canvas.previewBitmap, drawBkgnd);
                    }
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void previewButton_Click(object sender, EventArgs e)
        {
            createPreview(drawBkgnd: false);
            refreshMattePicBox();
        }

        private void clearPreviewMenuItem_Click(object sender, EventArgs e)
        {
            Canvas.clearPreview();
            refreshMattePicBox();
        }

        private void pagePreviewMenuItem_Click(object sender, EventArgs e)
        {
            pagePreviewMenuItem.Checked = !pagePreviewMenuItem.Checked;
            if (getCanvas() != null)
            {
                getCanvas().layerProperties.includeInPreview = pagePreviewMenuItem.Checked;
                getCanvas().dirty = true;
            }
        }

        private void pageColorMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = getCanvas().layerProperties.color;
            colorDialog.ShowDialog(this);
            getCanvas().layerProperties.color = colorDialog.Color;
            getCanvas().dirty = true;
            Graphics graphics = Graphics.FromImage(pageColorMenuItem.Image);
            graphics.Clear(getCanvas().layerProperties.color);
        }

        private void previewButton_MouseDown(object sender, MouseEventArgs e)
        {
            createPreview(drawBkgnd: false);
            refreshMattePicBox();
        }

        private void copyMenuItem_Click(object sender, EventArgs e)
        {
            Canvas canvas = getCanvas();
            if (canvas != null && canvas.selectedGroup != null)
            {
                SceneGroup.copyBuffer = (SceneGroup)canvas.selectedGroup.getCopy();
                refreshMattePicBox();
            }
        }

        private void pasteMenuItem_Click(object sender, EventArgs e)
        {
            Canvas canvas = getCanvas();
            if (canvas != null && SceneGroup.copyBuffer != null)
            {
                SceneGroup sceneGroup = (SceneGroup)SceneGroup.copyBuffer.getCopy();
                canvas.deselectAll();
                SceneGlyph.selectedGlyph = null;
                canvas.sceneGroups.Add(sceneGroup);
                canvas.Select(sceneGroup);
                getMattePicBox().Refresh();
            }
        }

        private void spaceKeyLabel_Click(object sender, EventArgs e)
        {
            spaceButton_Click(null, null);
        }

        private void backspaceKeyLabel_Click(object sender, EventArgs e)
        {
            backSpaceButton_Click(null, null);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog(this);
            aboutBox.Dispose();
        }

        private void revertAllMenuItem_Click(object sender, EventArgs e)
        {
            Canvas.clearPreview();
            ArrayList sceneGroups = myRootForm.getCanvas().sceneGroups;
            foreach (SceneGroup item in sceneGroups)
            {
                item.resetTransform();
            }
            myRootForm.refreshMattePicBox();
        }

        private bool applyKerning(float kerningDist)
        {
            Bitmap bitmap = new Bitmap(100, 100, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            Clipper clipper = Shape.openClipper();
            clipper.kerning = true;
            weAreKerning = true;
            getCanvas().setDrawMode(2);
            getCanvas().draw(graphics);
            getCanvas().setDrawMode(31);
            weAreKerning = false;
            float[] array = clipper.kern(kerningDist);
            Shape.closeClipper();
            graphics.Dispose();
            bitmap.Dispose();
            if (array == null)
            {
                return false;
            }
            ArrayList sceneGroups = myRootForm.getCanvas().sceneGroups;
            foreach (SceneGroup item in sceneGroups)
            {
                if (!item.selected)
                {
                    continue;
                }
                float num = 0f;
                int num2 = 0;
                for (int i = 1; i < item.children.Count; i++)
                {
                    SceneGlyph sceneGlyph = (SceneGlyph)item.children[i];
                    if (!sceneGlyph.isSpace)
                    {
                        num += array[num2++] - kerningDist;
                        setGlyphPositionX(sceneGlyph, sceneGlyph.ox - num);
                        item.dirty = true;
                    }
                }
            }
            return true;
        }

        private void applyKerningButton_Click(object sender, EventArgs e)
        {
            if (myRootForm.getCanvas() == null || myRootForm.getCanvas().sceneGroups == null)
            {
                return;
            }
            ArrayList sceneGroups = myRootForm.getCanvas().sceneGroups;
            foreach (SceneGroup item in sceneGroups)
            {
                if (item.selected && item.children.Count < 2)
                {
                    return;
                }
            }
            float result = 0f;
            if (!float.TryParse(kerningTextBox.Text, out result))
            {
                kerningTextBox.Text = "0.000";
            }
            else if (applyKerning(result))
            {
                refreshMattePicBox();
            }
        }

        public void pageNameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            textBox_KeyPress(sender, e);
            if (13 == textBoxKeyChar)
            {
                Canvas canvas = getCanvas();
                canvas.layerProperties.layerName = pageNameTextBox.Text;
                canvas.dirty = true;
                matTabControl.SelectedTab.Name = pageNameTextBox.Text;
                matTabControl.SelectedTab.Text = pageNameTextBox.Text;
            }
        }

        private void pageContextMenu_Opening(object sender, CancelEventArgs e)
        {
            showFontMenuItem.Text = "Show Cartridge";
            if (SceneGlyph.selectedGlyph != null)
            {
                string fontDisplayName = FontLoading.GetFontDisplayName(SceneGlyph.selectedGlyph.shape.getFontName());
                ToolStripMenuItem toolStripMenuItem = showFontMenuItem;
                toolStripMenuItem.Text = toolStripMenuItem.Text + " \"" + fontDisplayName + "\"";
            }
            pageNameTextBox.Text = matTabControl.SelectedTab.Text;
            if (SceneGlyph.selectedGlyph != null && SceneGlyph.selectedGlyph.contourInvis != null)
            {
                int selectedContour = SceneGlyph.selectedGlyph.selectedContour;
                if (selectedContour != -1)
                {
                    if (SceneGlyph.selectedGlyph.contourInvis[selectedContour])
                    {
                        invisContourMenuItem.Text = "Show Selected Contour";
                    }
                    else
                    {
                        invisContourMenuItem.Text = "Hide Selected Contour";
                    }
                    invisContourMenuItem.Enabled = true;
                }
                else
                {
                    invisContourMenuItem.Text = "Hide Selected Contour";
                    invisContourMenuItem.Enabled = false;
                }
            }
            else
            {
                invisContourMenuItem.Text = "Hide Selected Contour";
                invisContourMenuItem.Enabled = false;
            }
        }

        private string getDefaultBrowser()
        {
            string empty = string.Empty;
            RegistryKey registryKey = null;
            try
            {
                registryKey = Registry.ClassesRoot.OpenSubKey("HTTP\\shell\\open\\command", writable: false);
                empty = registryKey.GetValue(null).ToString().ToLower()
                    .Replace("\"", "");
                if (!empty.EndsWith("exe"))
                {
                    return empty.Substring(0, empty.LastIndexOf(".exe") + 4);
                }
                return empty;
            }
            finally
            {
                registryKey?.Close();
            }
        }

        private void launchDefaultBrowser(string url)
        {
            Process process = new Process();
            process.StartInfo.FileName = getDefaultBrowser();
            process.StartInfo.Arguments = url;
            process.Start();
        }

        private void checkForUpdatesMenuItem_Click(object sender, EventArgs e)
        {
            string text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            try
            {
                Cricut_Design_Studio.com.cricut.webservice.Version version = new Cricut_Design_Studio.com.cricut.webservice.Version();
                string currentVersion = version.GetCurrentVersion("29-0500");
                char[] separator = new char[1] { '.' };
                string[] array = currentVersion.Split(separator);
                string[] array2 = text.Split(separator);
                bool flag = false;
                for (int i = 0; i < 4; i++)
                {
                    int num = Convert.ToInt32(array[i]);
                    int num2 = Convert.ToInt32(array2[i]);
                    if (num < num2)
                    {
                        break;
                    }
                    if (num > num2)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    if (MessageBox.Show(myRootForm, "A newer version of Cricut Design Studio is available.\n\nYour current version is " + text + ",\nthe new downloadable version is " + currentVersion + "\n\nWould you like to open a web browser to the download site?", "Cricut DesignStudio Update", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        launchDefaultBrowser("http://www.cricut.com/updates/designstudio.aspx");
                    }
                }
                else
                {
                    MessageBox.Show(myRootForm, "Your copy of the Cricut DesignStudio is up to date.", "Cricut DesignStudio Update");
                }
            }
            catch (Exception)
            {
                if (MessageBox.Show(myRootForm, "Unable to automatically check the Cricut web site for\nthe latest version of Cricut DesignStudio.\n\nYour version is " + text + "\n\nWould you like to open a web browser to check manually?", "Cricut DesignStudio Update", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    launchDefaultBrowser("http://www.cricut.com/updates/designstudio.aspx");
                }
            }
        }

        private void storeLocatorMenuItem_Click(object sender, EventArgs e)
        {
            launchDefaultBrowser("http://www.cricut.com/storelocator.aspx");
        }

        private void helpMenuItem_Click(object sender, EventArgs e)
        {
            string url = "file://" + exeFolderPath + "\\CDS_Help\\CDS_Help.htm";
            launchDefaultBrowser(url);
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (getCanvas() != null)
            {
                getCanvas().undoRedo.undo();
            }
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (getCanvas() != null)
            {
                getCanvas().undoRedo.redo();
            }
        }

        private void keepAspectRatioButton_Click(object sender, EventArgs e)
        {
            if (keepShapeAspectRatio)
            {
                keepAspectRatioButton.Text = "-";
                keepShapeAspectRatio = false;
            }
            else
            {
                keepAspectRatioButton.Text = "+";
                keepShapeAspectRatio = true;
            }
        }

        private void widthTextBox_Leave(object sender, EventArgs e)
        {
            if (shapeAspectRatio != 0f && keepShapeAspectRatio && float.TryParse(widthTextBox.Text, out var result))
            {
                float f = result / shapeAspectRatio;
                heightTextBox.Text = f.ToString();
                FloatParam floatParam = (FloatParam)heightTextBox.Tag;
                floatParam.f = f;
                SceneGroup.doNumBoxTransform(floatParam.mode);
                widthTextBox.Text = result.ToString();
            }
        }

        private void heightTextBox_Leave(object sender, EventArgs e)
        {
            if (shapeAspectRatio != 0f && keepShapeAspectRatio && float.TryParse(heightTextBox.Text, out var result))
            {
                float f = result * shapeAspectRatio;
                widthTextBox.Text = f.ToString();
                FloatParam floatParam = (FloatParam)widthTextBox.Tag;
                floatParam.f = f;
                SceneGroup.doNumBoxTransform(floatParam.mode);
                heightTextBox.Text = result.ToString();
            }
        }

        private void widthHeightTextBox_Enter(object sender, EventArgs e)
        {
            shapeAspectRatio = 0f;
            if (float.TryParse(widthTextBox.Text, out var result) && float.TryParse(heightTextBox.Text, out var result2) && result2 != 0f)
            {
                shapeAspectRatio = result / result2;
            }
        }

        private void nudgeButton_MouseLeave(object sender, EventArgs e)
        {
            cleanupNudgeAutorepeat();
            Button button = (Button)sender;
            if (button.Tag != null)
            {
                ButtonState buttonState = (ButtonState)button.Tag;
                buttonState.mouseOver = false;
                buttonState.pressed = false;
            }
            button_MouseLeave(sender, e);
        }

        private void invisContourMenuItem_Click(object sender, EventArgs e)
        {
            if (SceneGlyph.selectedGlyph == null || SceneGlyph.selectedGlyph.contourInvis == null)
            {
                return;
            }
            int selectedContour = SceneGlyph.selectedGlyph.selectedContour;
            if (selectedContour != -1)
            {
                if (SceneGlyph.selectedGlyph.contourInvis[selectedContour])
                {
                    SceneGlyph.selectedGlyph.contourInvis[selectedContour] = false;
                    invisContourMenuItem.Text = "Show Selected Contour";
                }
                else
                {
                    SceneGlyph.selectedGlyph.contourInvis[selectedContour] = true;
                    invisContourMenuItem.Text = "Hide Selected Contour";
                }
                refreshMattePicBox();
            }
        }

        private void nextShapeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SceneGroup sceneGroup = (SceneGroup)SceneGlyph.selectedGlyph.parent;
            int i;
            for (i = 0; i < sceneGroup.children.Count && SceneGlyph.selectedGlyph != sceneGroup.children[i]; i++)
            {
            }
            if (i < sceneGroup.children.Count)
            {
                i = (i + 1) % sceneGroup.children.Count;
                SceneGlyph.selectedGlyph = (SceneGlyph)sceneGroup.children[i];
                if (-1 == SceneGlyph.selectedGlyph.selectedContour)
                {
                    SceneGlyph.selectedGlyph.selectedContour = 0;
                }
                refreshMattePicBox();
            }
        }

        internal void EnableMenuItemsForGroupSelected(bool enable)
        {
            myRootForm.copyMenuItem.Enabled = enable;
            myRootForm.nextShapeToolStripMenuItem.Enabled = enable;
        }

        private void loadPaperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cricut_LoadPaper();
        }

        private void unloadPaperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cricut_UnloadPaper();
        }

        internal void RegainLocalFocus()
        {
            allowLocalFocus = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Cricut_Design_Studio.Form1));
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this.keypadPanel = new System.Windows.Forms.Panel();
            this.label16 = new System.Windows.Forms.Label();
            this.sizeLabel = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.panel67 = new System.Windows.Forms.Panel();
            this.keypadOutlinePanel = new System.Windows.Forms.Panel();
            this.fontPicBox = new System.Windows.Forms.PictureBox();
            this.glyphContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addKeywordMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.glyphKeywordComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.removeKeywordMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.featuresPicBox = new System.Windows.Forms.PictureBox();
            this.panel16 = new System.Windows.Forms.Panel();
            this.shiftKeyLabel = new System.Windows.Forms.Label();
            this.panel15 = new System.Windows.Forms.Panel();
            this.spaceKeyLabel = new System.Windows.Forms.Label();
            this.panel14 = new System.Windows.Forms.Panel();
            this.backspaceKeyLabel = new System.Windows.Forms.Label();
            this.fontNameLabel = new System.Windows.Forms.Label();
            this.realSizeCheckBox = new System.Windows.Forms.CheckBox();
            this.paperSaverCheckBox = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.saveProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.cutWithCricutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.copyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.nextShapeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteShapesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllOnThisPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.revertAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deletePageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.previewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearPreviewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keepPreviewAsMatBackgroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.showRulerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.matSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x6ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x12ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.x24ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.matViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fitToPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.percent100ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.percent200ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fontFeaturesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.prefsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.myCartridgeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.favoriteCartridgeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.multiCutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.multiCut1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.multiCut2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.multiCut3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.multiCut4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableBalloonHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.checkForUpdatesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.storeLocatorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.updateFirmwareMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.helpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.loadPaperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unloadPaperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStripSeparator = new System.Windows.Forms.Panel();
            this.form1ToolTips = new System.Windows.Forms.ToolTip(this.components);
            this.prevKeywordButton = new System.Windows.Forms.Button();
            this.nextKeywordButton = new System.Windows.Forms.Button();
            this.fontTreeView = new System.Windows.Forms.TreeView();
            this.keepAspectRatioButton = new System.Windows.Forms.Button();
            this.nudgeDownButton = new System.Windows.Forms.Button();
            this.nudgeUpButton = new System.Windows.Forms.Button();
            this.rotate90Button = new System.Windows.Forms.Button();
            this.nudgeRightButton = new System.Windows.Forms.Button();
            this.nudgeLeftButton = new System.Windows.Forms.Button();
            this.applyKerningButton = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.revertShapePropertiesButton = new System.Windows.Forms.Button();
            this.percent200Button = new Cricut_Design_Studio.CricutButton();
            this.percent100Button = new Cricut_Design_Studio.CricutButton();
            this.fitToPageButton = new Cricut_Design_Studio.CricutButton();
            this.previewButton = new Cricut_Design_Studio.CricutButton();
            this.deleteGroupButton = new Cricut_Design_Studio.CricutButton();
            this.pasteGroupButton = new Cricut_Design_Studio.CricutButton();
            this.copyGroupButton = new Cricut_Design_Studio.CricutButton();
            this.newPageButton = new Cricut_Design_Studio.CricutButton();
            this.cricutCutButton = new Cricut_Design_Studio.CricutButton();
            this.fontImageList = new System.Windows.Forms.ImageList(this.components);
            this.hoverTimer = new System.Windows.Forms.Timer(this.components);
            this.saveProjectDialog = new System.Windows.Forms.SaveFileDialog();
            this.openProjectDialog = new System.Windows.Forms.OpenFileDialog();
            this.pageContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.changePageNameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pageNameTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.showFontMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.orderSubMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bringPagetoFrontMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendPagetoBackMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bringPageForwardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendPageBackwardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pagePreviewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pageColorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.invisContourMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.utilityTimer = new System.Windows.Forms.Timer(this.components);
            this.cuttingBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.firmwareBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.panel2 = new System.Windows.Forms.Panel();
            this.cartridgeLibraryPanel = new System.Windows.Forms.Panel();
            this.cartridgeLibraryHeaderPanel = new System.Windows.Forms.Panel();
            this.fontsChooseByComboBox = new System.Windows.Forms.ComboBox();
            this.key2Label = new System.Windows.Forms.Label();
            this.viewLabel = new System.Windows.Forms.Label();
            this.fontsTag2ComboBox = new System.Windows.Forms.ComboBox();
            this.fontsTag1ComboBox = new System.Windows.Forms.ComboBox();
            this.key1Label = new System.Windows.Forms.Label();
            this.panel10 = new System.Windows.Forms.Panel();
            this.propertiesPanel = new System.Windows.Forms.Panel();
            this.propertiesHeaderPanel = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label68 = new System.Windows.Forms.Label();
            this.label67 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.kerningTextBox = new System.Windows.Forms.TextBox();
            this.flipShapesCheckBox = new System.Windows.Forms.CheckBox();
            this.weldingCheckBox = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.shearTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.angleTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.heightTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.widthTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.yPositionTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.xPositionTextBox = new System.Windows.Forms.TextBox();
            this.matTabControl = new Cricut_Design_Studio.CustomTabControl();
            this.activateByInternetTabPage = new System.Windows.Forms.TabPage();
            this.trialVersionPanel = new System.Windows.Forms.Panel();
            this.activateByPhoneButton = new System.Windows.Forms.Button();
            this.label20 = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.iEmailTextBox = new System.Windows.Forms.TextBox();
            this.iLastNameTextBox = new System.Windows.Forms.TextBox();
            this.iFirstNameTextBox = new System.Windows.Forms.TextBox();
            this.label28 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.iSerialNum2 = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.iSerialNum1 = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.iSerialNum5 = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.iSerialNum3 = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.iSerialNum4 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.iSerialNum6 = new System.Windows.Forms.TextBox();
            this.continueTrialButton = new System.Windows.Forms.Button();
            this.activateByInternetButton = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.activateByPhoneTabPage = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pContinueTrialButton = new System.Windows.Forms.Button();
            this.goBackToInternetActivationButton = new System.Windows.Forms.Button();
            this.label24 = new System.Windows.Forms.Label();
            this.activationKeyStatusLabel = new System.Windows.Forms.Label();
            this.verifyActKeyButton = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.activationPhoneNumberLabel = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.registrationCodeLabel = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.pEmailTextBox = new System.Windows.Forms.TextBox();
            this.pLastNameTextBox = new System.Windows.Forms.TextBox();
            this.pFirstNameTextBox = new System.Windows.Forms.TextBox();
            this.label37 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label38 = new System.Windows.Forms.Label();
            this.label39 = new System.Windows.Forms.Label();
            this.actKey8 = new System.Windows.Forms.TextBox();
            this.actKey7 = new System.Windows.Forms.TextBox();
            this.actKey6 = new System.Windows.Forms.TextBox();
            this.actKey5 = new System.Windows.Forms.TextBox();
            this.actKey4 = new System.Windows.Forms.TextBox();
            this.actKey3 = new System.Windows.Forms.TextBox();
            this.actKey2 = new System.Windows.Forms.TextBox();
            this.label40 = new System.Windows.Forms.Label();
            this.actKey1 = new System.Windows.Forms.TextBox();
            this.label41 = new System.Windows.Forms.Label();
            this.label42 = new System.Windows.Forms.Label();
            this.label43 = new System.Windows.Forms.Label();
            this.label44 = new System.Windows.Forms.Label();
            this.label46 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.pSerialNum2 = new System.Windows.Forms.TextBox();
            this.label47 = new System.Windows.Forms.Label();
            this.pSerialNum1 = new System.Windows.Forms.TextBox();
            this.label48 = new System.Windows.Forms.Label();
            this.pSerialNum5 = new System.Windows.Forms.TextBox();
            this.label49 = new System.Windows.Forms.Label();
            this.pSerialNum3 = new System.Windows.Forms.TextBox();
            this.label50 = new System.Windows.Forms.Label();
            this.pSerialNum4 = new System.Windows.Forms.TextBox();
            this.label51 = new System.Windows.Forms.Label();
            this.pSerialNum6 = new System.Windows.Forms.TextBox();
            this.label52 = new System.Windows.Forms.Label();
            this.activationCompleteTabPage = new System.Windows.Forms.TabPage();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.aSerialNum2 = new System.Windows.Forms.TextBox();
            this.label58 = new System.Windows.Forms.Label();
            this.aSerialNum1 = new System.Windows.Forms.TextBox();
            this.label59 = new System.Windows.Forms.Label();
            this.aSerialNum5 = new System.Windows.Forms.TextBox();
            this.label60 = new System.Windows.Forms.Label();
            this.aSerialNum3 = new System.Windows.Forms.TextBox();
            this.label61 = new System.Windows.Forms.Label();
            this.aSerialNum4 = new System.Windows.Forms.TextBox();
            this.label62 = new System.Windows.Forms.Label();
            this.aSerialNum6 = new System.Windows.Forms.TextBox();
            this.label57 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label27 = new System.Windows.Forms.Label();
            this.label33 = new System.Windows.Forms.Label();
            this.aActKey8 = new System.Windows.Forms.TextBox();
            this.aActKey7 = new System.Windows.Forms.TextBox();
            this.aActKey6 = new System.Windows.Forms.TextBox();
            this.aActKey5 = new System.Windows.Forms.TextBox();
            this.aActKey4 = new System.Windows.Forms.TextBox();
            this.aActKey3 = new System.Windows.Forms.TextBox();
            this.aActKey2 = new System.Windows.Forms.TextBox();
            this.label45 = new System.Windows.Forms.Label();
            this.aActKey1 = new System.Windows.Forms.TextBox();
            this.label53 = new System.Windows.Forms.Label();
            this.label54 = new System.Windows.Forms.Label();
            this.label55 = new System.Windows.Forms.Label();
            this.label56 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.aEmailTextBox = new System.Windows.Forms.TextBox();
            this.aLastNameTextBox = new System.Windows.Forms.TextBox();
            this.aFirstNameTextBox = new System.Windows.Forms.TextBox();
            this.activationCompleteOKButton = new System.Windows.Forms.Button();
            this.label22 = new System.Windows.Forms.Label();
            this.updateCricutFirmwareTabPage = new System.Windows.Forms.TabPage();
            this.label71 = new System.Windows.Forms.Label();
            this.label69 = new System.Windows.Forms.Label();
            this.label70 = new System.Windows.Forms.Label();
            this.v3RadioButton = new System.Windows.Forms.RadioButton();
            this.successfulUpdateLabel = new System.Windows.Forms.Label();
            this.updateLaterButton = new System.Windows.Forms.Button();
            this.beginUpdatingButton = new System.Windows.Forms.Button();
            this.label66 = new System.Windows.Forms.Label();
            this.v2RadioButton = new System.Windows.Forms.RadioButton();
            this.v1RadioButton = new System.Windows.Forms.RadioButton();
            this.label65 = new System.Windows.Forms.Label();
            this.label63 = new System.Windows.Forms.Label();
            this.label64 = new System.Windows.Forms.Label();
            this.keypadPanel.SuspendLayout();
            this.keypadOutlinePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.fontPicBox).BeginInit();
            this.glyphContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.featuresPicBox).BeginInit();
            this.panel16.SuspendLayout();
            this.panel15.SuspendLayout();
            this.panel14.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.pageContextMenu.SuspendLayout();
            this.cartridgeLibraryPanel.SuspendLayout();
            this.propertiesPanel.SuspendLayout();
            this.matTabControl.SuspendLayout();
            this.activateByInternetTabPage.SuspendLayout();
            this.trialVersionPanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.activateByPhoneTabPage.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.activationCompleteTabPage.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.updateCricutFirmwareTabPage.SuspendLayout();
            base.SuspendLayout();
            this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
            this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
            this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
            this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
            this.ContentPanel.Size = new System.Drawing.Size(100, 175);
            this.keypadPanel.BackColor = System.Drawing.Color.Transparent;
            this.keypadPanel.Controls.Add(this.label16);
            this.keypadPanel.Controls.Add(this.sizeLabel);
            this.keypadPanel.Controls.Add(this.label13);
            this.keypadPanel.Controls.Add(this.label7);
            this.keypadPanel.Controls.Add(this.panel67);
            this.keypadPanel.Controls.Add(this.keypadOutlinePanel);
            this.keypadPanel.Controls.Add(this.fontNameLabel);
            this.keypadPanel.Controls.Add(this.realSizeCheckBox);
            this.keypadPanel.Controls.Add(this.paperSaverCheckBox);
            this.keypadPanel.Location = new System.Drawing.Point(296, 31);
            this.keypadPanel.Name = "keypadPanel";
            this.keypadPanel.Size = new System.Drawing.Size(470, 240);
            this.keypadPanel.TabIndex = 6;
            this.keypadPanel.Paint += new System.Windows.Forms.PaintEventHandler(keypadPanel_Paint);
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(194, 212);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(64, 13);
            this.label16.TabIndex = 72;
            this.label16.Text = "Default Size";
            this.form1ToolTips.SetToolTip(this.label16, resources.GetString("label16.ToolTip"));
            this.label16.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.sizeLabel.AutoSize = true;
            this.sizeLabel.BackColor = System.Drawing.Color.Transparent;
            this.sizeLabel.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.sizeLabel.Location = new System.Drawing.Point(390, 211);
            this.sizeLabel.Name = "sizeLabel";
            this.sizeLabel.Size = new System.Drawing.Size(64, 16);
            this.sizeLabel.TabIndex = 71;
            this.sizeLabel.Text = "20 1/4\"";
            this.sizeLabel.Paint += new System.Windows.Forms.PaintEventHandler(labelBgAA_Paint);
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(16, 212);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(66, 13);
            this.label13.TabIndex = 64;
            this.label13.Text = "Paper Saver";
            this.label13.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(109, 212);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(52, 13);
            this.label7.TabIndex = 63;
            this.label7.Text = "Real Size";
            this.label7.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.panel67.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.panel67.Location = new System.Drawing.Point(15, 203);
            this.panel67.Name = "panel67";
            this.panel67.Size = new System.Drawing.Size(440, 1);
            this.panel67.TabIndex = 46;
            this.keypadOutlinePanel.Controls.Add(this.fontPicBox);
            this.keypadOutlinePanel.Controls.Add(this.featuresPicBox);
            this.keypadOutlinePanel.Controls.Add(this.panel16);
            this.keypadOutlinePanel.Controls.Add(this.panel15);
            this.keypadOutlinePanel.Controls.Add(this.panel14);
            this.keypadOutlinePanel.Location = new System.Drawing.Point(39, 29);
            this.keypadOutlinePanel.Name = "keypadOutlinePanel";
            this.keypadOutlinePanel.Size = new System.Drawing.Size(392, 168);
            this.keypadOutlinePanel.TabIndex = 62;
            this.keypadOutlinePanel.Paint += new System.Windows.Forms.PaintEventHandler(keypadOutlinePanel_Paint);
            this.fontPicBox.ContextMenuStrip = this.glyphContextMenu;
            this.fontPicBox.Location = new System.Drawing.Point(67, 4);
            this.fontPicBox.Name = "fontPicBox";
            this.fontPicBox.Size = new System.Drawing.Size(320, 160);
            this.fontPicBox.TabIndex = 63;
            this.fontPicBox.TabStop = false;
            this.fontPicBox.MouseLeave += new System.EventHandler(fontPicBox_MouseLeave);
            this.fontPicBox.MouseMove += new System.Windows.Forms.MouseEventHandler(fontPicBox_MouseMove);
            this.fontPicBox.Click += new System.EventHandler(fontPicBox_Click);
            this.fontPicBox.MouseDown += new System.Windows.Forms.MouseEventHandler(mattePicBox_MouseDown);
            this.fontPicBox.Paint += new System.Windows.Forms.PaintEventHandler(fontPicBox_Paint);
            this.fontPicBox.MouseEnter += new System.EventHandler(fontPicBox_MouseEnter);
            this.glyphContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.addKeywordMenuItem, this.glyphKeywordComboBox, this.toolStripSeparator17, this.removeKeywordMenuItem });
            this.glyphContextMenu.Name = "glyphContextMenu";
            this.glyphContextMenu.Size = new System.Drawing.Size(182, 129);
            this.glyphContextMenu.Opening += new System.ComponentModel.CancelEventHandler(glyphContextMenu_Opening);
            this.addKeywordMenuItem.Name = "addKeywordMenuItem";
            this.addKeywordMenuItem.Size = new System.Drawing.Size(181, 22);
            this.addKeywordMenuItem.Text = "Add Keyword";
            this.glyphKeywordComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.glyphKeywordComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.glyphKeywordComboBox.Name = "glyphKeywordComboBox";
            this.glyphKeywordComboBox.Size = new System.Drawing.Size(121, 21);
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(178, 6);
            this.removeKeywordMenuItem.Name = "removeKeywordMenuItem";
            this.removeKeywordMenuItem.Size = new System.Drawing.Size(181, 22);
            this.removeKeywordMenuItem.Text = "Remove Keyword";
            this.featuresPicBox.Location = new System.Drawing.Point(4, 4);
            this.featuresPicBox.Name = "featuresPicBox";
            this.featuresPicBox.Size = new System.Drawing.Size(64, 96);
            this.featuresPicBox.TabIndex = 62;
            this.featuresPicBox.TabStop = false;
            this.featuresPicBox.MouseDown += new System.Windows.Forms.MouseEventHandler(featuresPicBox_MouseDown);
            this.featuresPicBox.Paint += new System.Windows.Forms.PaintEventHandler(featuresPicBox_Paint);
            this.panel16.BackgroundImage = (System.Drawing.Image)resources.GetObject("panel16.BackgroundImage");
            this.panel16.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel16.Controls.Add(this.shiftKeyLabel);
            this.panel16.Location = new System.Drawing.Point(4, 100);
            this.panel16.Name = "panel16";
            this.panel16.Size = new System.Drawing.Size(64, 32);
            this.panel16.TabIndex = 11;
            this.shiftKeyLabel.Image = Cricut_Design_Studio.MarcusResources.shift_lock;
            this.shiftKeyLabel.Location = new System.Drawing.Point(0, 0);
            this.shiftKeyLabel.Name = "shiftKeyLabel";
            this.shiftKeyLabel.Size = new System.Drawing.Size(64, 32);
            this.shiftKeyLabel.TabIndex = 12;
            this.shiftKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.form1ToolTips.SetToolTip(this.shiftKeyLabel, "SHIFT LOCK works with the Feature buttons to\r\nselect a set of shapes, just like on the Cricut.\r\n\r\nClick SHIFT LOCK to show the set of shifted or\r\nnon-shifted shapes available for the chosen\r\nFeature.");
            this.shiftKeyLabel.Paint += new System.Windows.Forms.PaintEventHandler(keypadButtonLabel_Paint);
            this.shiftKeyLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(shiftKeyLabel_MouseDown);
            this.panel15.BackgroundImage = (System.Drawing.Image)resources.GetObject("panel15.BackgroundImage");
            this.panel15.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel15.Controls.Add(this.spaceKeyLabel);
            this.panel15.Location = new System.Drawing.Point(4, 132);
            this.panel15.Name = "panel15";
            this.panel15.Size = new System.Drawing.Size(32, 32);
            this.panel15.TabIndex = 9;
            this.spaceKeyLabel.Image = Cricut_Design_Studio.MarcusResources.space;
            this.spaceKeyLabel.Location = new System.Drawing.Point(0, 0);
            this.spaceKeyLabel.Name = "spaceKeyLabel";
            this.spaceKeyLabel.Size = new System.Drawing.Size(32, 32);
            this.spaceKeyLabel.TabIndex = 13;
            this.spaceKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.spaceKeyLabel.Paint += new System.Windows.Forms.PaintEventHandler(keypadButtonLabel_Paint);
            this.spaceKeyLabel.Click += new System.EventHandler(spaceKeyLabel_Click);
            this.panel14.BackgroundImage = (System.Drawing.Image)resources.GetObject("panel14.BackgroundImage");
            this.panel14.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel14.Controls.Add(this.backspaceKeyLabel);
            this.panel14.Location = new System.Drawing.Point(36, 132);
            this.panel14.Name = "panel14";
            this.panel14.Size = new System.Drawing.Size(32, 32);
            this.panel14.TabIndex = 10;
            this.backspaceKeyLabel.Image = Cricut_Design_Studio.MarcusResources.back_space;
            this.backspaceKeyLabel.Location = new System.Drawing.Point(0, 0);
            this.backspaceKeyLabel.Name = "backspaceKeyLabel";
            this.backspaceKeyLabel.Size = new System.Drawing.Size(32, 32);
            this.backspaceKeyLabel.TabIndex = 14;
            this.backspaceKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.form1ToolTips.SetToolTip(this.backspaceKeyLabel, "BACK SPACE will delete the last Shape from\r\nthe selected group of Shapes.");
            this.backspaceKeyLabel.Paint += new System.Windows.Forms.PaintEventHandler(keypadButtonLabel_Paint);
            this.backspaceKeyLabel.Click += new System.EventHandler(backspaceKeyLabel_Click);
            this.fontNameLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fontNameLabel.Font = new System.Drawing.Font("Franklin Gothic Medium", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.fontNameLabel.Location = new System.Drawing.Point(15, 3);
            this.fontNameLabel.Name = "fontNameLabel";
            this.fontNameLabel.Size = new System.Drawing.Size(440, 23);
            this.fontNameLabel.TabIndex = 42;
            this.fontNameLabel.Text = "George and Basic Shapes";
            this.fontNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.fontNameLabel.Paint += new System.Windows.Forms.PaintEventHandler(fontNameLabel_Paint);
            this.realSizeCheckBox.AutoSize = true;
            this.realSizeCheckBox.Location = new System.Drawing.Point(167, 212);
            this.realSizeCheckBox.Name = "realSizeCheckBox";
            this.realSizeCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.realSizeCheckBox.Size = new System.Drawing.Size(15, 14);
            this.realSizeCheckBox.TabIndex = 41;
            this.realSizeCheckBox.TabStop = false;
            this.realSizeCheckBox.UseVisualStyleBackColor = false;
            this.realSizeCheckBox.CheckedChanged += new System.EventHandler(realSizeCheckBox_CheckedChanged);
            this.paperSaverCheckBox.AutoSize = true;
            this.paperSaverCheckBox.Location = new System.Drawing.Point(88, 212);
            this.paperSaverCheckBox.Name = "paperSaverCheckBox";
            this.paperSaverCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.paperSaverCheckBox.Size = new System.Drawing.Size(15, 14);
            this.paperSaverCheckBox.TabIndex = 40;
            this.paperSaverCheckBox.TabStop = false;
            this.paperSaverCheckBox.UseVisualStyleBackColor = false;
            this.paperSaverCheckBox.CheckedChanged += new System.EventHandler(paperSaverCheckBox_CheckedChanged);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.fileToolStripMenuItem, this.editToolStripMenuItem, this.viewToolStripMenuItem, this.fontFeaturesMenuItem, this.prefsToolStripMenuItem, this.helpToolStripMenuItem });
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1016, 24);
            this.menuStrip1.TabIndex = 16;
            this.menuStrip1.Text = "menuStrip1";
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[11]
            {
                this.newProjectMenuItem, this.openProjectMenuItem, this.closeProjectMenuItem, this.toolStripSeparator, this.saveProjectMenuItem, this.saveAsProjectMenuItem, this.toolStripSeparator1, this.toolStripSeparator20, this.cutWithCricutMenuItem, this.toolStripSeparator6,
                this.exitMenuItem
            });
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            this.newProjectMenuItem.Image = (System.Drawing.Image)resources.GetObject("newProjectMenuItem.Image");
            this.newProjectMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newProjectMenuItem.Name = "newProjectMenuItem";
            this.newProjectMenuItem.Size = new System.Drawing.Size(192, 22);
            this.newProjectMenuItem.Text = "New";
            this.newProjectMenuItem.Click += new System.EventHandler(newProjectMenuItem_Click);
            this.openProjectMenuItem.Image = (System.Drawing.Image)resources.GetObject("openProjectMenuItem.Image");
            this.openProjectMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openProjectMenuItem.Name = "openProjectMenuItem";
            this.openProjectMenuItem.ShortcutKeys = System.Windows.Forms.Keys.O | System.Windows.Forms.Keys.Control;
            this.openProjectMenuItem.ShowShortcutKeys = false;
            this.openProjectMenuItem.Size = new System.Drawing.Size(192, 22);
            this.openProjectMenuItem.Text = "Open...";
            this.openProjectMenuItem.Click += new System.EventHandler(openProjectMenuItem_Click);
            this.closeProjectMenuItem.Name = "closeProjectMenuItem";
            this.closeProjectMenuItem.ShowShortcutKeys = false;
            this.closeProjectMenuItem.Size = new System.Drawing.Size(192, 22);
            this.closeProjectMenuItem.Text = "Close";
            this.closeProjectMenuItem.Click += new System.EventHandler(closeProjectMenuItem_Click);
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(189, 6);
            this.saveProjectMenuItem.Enabled = false;
            this.saveProjectMenuItem.Image = (System.Drawing.Image)resources.GetObject("saveProjectMenuItem.Image");
            this.saveProjectMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveProjectMenuItem.Name = "saveProjectMenuItem";
            this.saveProjectMenuItem.ShortcutKeys = System.Windows.Forms.Keys.S | System.Windows.Forms.Keys.Control;
            this.saveProjectMenuItem.Size = new System.Drawing.Size(192, 22);
            this.saveProjectMenuItem.Text = "Save";
            this.saveProjectMenuItem.Click += new System.EventHandler(saveProjectMenuItem_Click);
            this.saveAsProjectMenuItem.Name = "saveAsProjectMenuItem";
            this.saveAsProjectMenuItem.ShowShortcutKeys = false;
            this.saveAsProjectMenuItem.Size = new System.Drawing.Size(192, 22);
            this.saveAsProjectMenuItem.Text = "Save As...";
            this.saveAsProjectMenuItem.Click += new System.EventHandler(saveAsProjectButton_Click);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(189, 6);
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(189, 6);
            this.cutWithCricutMenuItem.Image = Cricut_Design_Studio.MarcusResources.icn_cut_base;
            this.cutWithCricutMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cutWithCricutMenuItem.Name = "cutWithCricutMenuItem";
            this.cutWithCricutMenuItem.ShortcutKeys = System.Windows.Forms.Keys.P | System.Windows.Forms.Keys.Control;
            this.cutWithCricutMenuItem.ShowShortcutKeys = false;
            this.cutWithCricutMenuItem.Size = new System.Drawing.Size(192, 22);
            this.cutWithCricutMenuItem.Text = "Cut With Cricut...";
            this.cutWithCricutMenuItem.Click += new System.EventHandler(cutWithCricutMenuItem_Click);
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(189, 6);
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.ShowShortcutKeys = false;
            this.exitMenuItem.Size = new System.Drawing.Size(192, 22);
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Click += new System.EventHandler(exitMenuItem_Click);
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[12]
            {
                this.undoToolStripMenuItem, this.redoToolStripMenuItem, this.toolStripSeparator3, this.copyMenuItem, this.pasteMenuItem, this.toolStripSeparator16, this.nextShapeToolStripMenuItem, this.toolStripSeparator10, this.deleteShapesMenuItem, this.deleteAllOnThisPageToolStripMenuItem,
                this.toolStripSeparator13, this.revertAllMenuItem
            });
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            this.undoToolStripMenuItem.Enabled = false;
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Z | System.Windows.Forms.Keys.Control;
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(undoToolStripMenuItem_Click);
            this.redoToolStripMenuItem.Enabled = false;
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Y | System.Windows.Forms.Keys.Control;
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(redoToolStripMenuItem_Click);
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(166, 6);
            this.copyMenuItem.Image = Cricut_Design_Studio.MarcusResources.icn_group_copy;
            this.copyMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyMenuItem.Name = "copyMenuItem";
            this.copyMenuItem.ShortcutKeys = System.Windows.Forms.Keys.C | System.Windows.Forms.Keys.Control;
            this.copyMenuItem.Size = new System.Drawing.Size(169, 22);
            this.copyMenuItem.Text = "&Copy";
            this.copyMenuItem.Click += new System.EventHandler(copyMenuItem_Click);
            this.pasteMenuItem.Image = Cricut_Design_Studio.MarcusResources.icn_group_paste;
            this.pasteMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pasteMenuItem.Name = "pasteMenuItem";
            this.pasteMenuItem.ShortcutKeys = System.Windows.Forms.Keys.V | System.Windows.Forms.Keys.Control;
            this.pasteMenuItem.Size = new System.Drawing.Size(169, 22);
            this.pasteMenuItem.Text = "&Paste";
            this.pasteMenuItem.Click += new System.EventHandler(pasteMenuItem_Click);
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(166, 6);
            this.nextShapeToolStripMenuItem.Name = "nextShapeToolStripMenuItem";
            this.nextShapeToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.N | System.Windows.Forms.Keys.Control;
            this.nextShapeToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.nextShapeToolStripMenuItem.Text = "&Next Shape";
            this.nextShapeToolStripMenuItem.Click += new System.EventHandler(nextShapeToolStripMenuItem_Click);
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(166, 6);
            this.deleteShapesMenuItem.Image = Cricut_Design_Studio.MarcusResources.icn_group_delete;
            this.deleteShapesMenuItem.Name = "deleteShapesMenuItem";
            this.deleteShapesMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteShapesMenuItem.Size = new System.Drawing.Size(169, 22);
            this.deleteShapesMenuItem.Text = "Delete Shapes";
            this.deleteShapesMenuItem.Click += new System.EventHandler(deleteButton_Click);
            this.deleteAllOnThisPageToolStripMenuItem.Name = "deleteAllOnThisPageToolStripMenuItem";
            this.deleteAllOnThisPageToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.deleteAllOnThisPageToolStripMenuItem.Text = "Delete All";
            this.deleteAllOnThisPageToolStripMenuItem.Click += new System.EventHandler(deleteAllOnThisPageToolStripMenuItem_Click);
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(166, 6);
            this.revertAllMenuItem.Name = "revertAllMenuItem";
            this.revertAllMenuItem.Size = new System.Drawing.Size(169, 22);
            this.revertAllMenuItem.Text = "Revert All";
            this.revertAllMenuItem.Click += new System.EventHandler(revertAllMenuItem_Click);
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[11]
            {
                this.newPageToolStripMenuItem, this.deletePageToolStripMenuItem, this.toolStripSeparator9, this.previewMenuItem, this.clearPreviewMenuItem, this.keepPreviewAsMatBackgroundToolStripMenuItem, this.toolStripSeparator8, this.showRulerToolStripMenuItem, this.toolStripSeparator4, this.matSizeToolStripMenuItem,
                this.matViewToolStripMenuItem
            });
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.viewToolStripMenuItem.Text = "View";
            this.newPageToolStripMenuItem.Image = Cricut_Design_Studio.MarcusResources.icn_page_new;
            this.newPageToolStripMenuItem.Name = "newPageToolStripMenuItem";
            this.newPageToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.newPageToolStripMenuItem.Text = "New Page";
            this.newPageToolStripMenuItem.Click += new System.EventHandler(newPageButton_Click);
            this.deletePageToolStripMenuItem.Name = "deletePageToolStripMenuItem";
            this.deletePageToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.D | System.Windows.Forms.Keys.Control;
            this.deletePageToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.deletePageToolStripMenuItem.Text = "Delete Page";
            this.deletePageToolStripMenuItem.Click += new System.EventHandler(deletePageToolStripMenuItem_Click);
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(230, 6);
            this.previewMenuItem.Image = Cricut_Design_Studio.MarcusResources.icn_preview;
            this.previewMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.previewMenuItem.Name = "previewMenuItem";
            this.previewMenuItem.ShortcutKeys = System.Windows.Forms.Keys.A | System.Windows.Forms.Keys.Control;
            this.previewMenuItem.Size = new System.Drawing.Size(233, 22);
            this.previewMenuItem.Text = "Preview";
            this.previewMenuItem.Click += new System.EventHandler(previewButton_Click);
            this.clearPreviewMenuItem.Name = "clearPreviewMenuItem";
            this.clearPreviewMenuItem.ShortcutKeys = System.Windows.Forms.Keys.L | System.Windows.Forms.Keys.Control;
            this.clearPreviewMenuItem.Size = new System.Drawing.Size(233, 22);
            this.clearPreviewMenuItem.Text = "Clear Preview";
            this.clearPreviewMenuItem.Click += new System.EventHandler(clearPreviewMenuItem_Click);
            this.keepPreviewAsMatBackgroundToolStripMenuItem.Checked = true;
            this.keepPreviewAsMatBackgroundToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.keepPreviewAsMatBackgroundToolStripMenuItem.Name = "keepPreviewAsMatBackgroundToolStripMenuItem";
            this.keepPreviewAsMatBackgroundToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.keepPreviewAsMatBackgroundToolStripMenuItem.Text = "Keep Preview as Mat Background";
            this.keepPreviewAsMatBackgroundToolStripMenuItem.Click += new System.EventHandler(keepPreviewAsMatBackgroundToolStripMenuItem_Click);
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(230, 6);
            this.showRulerToolStripMenuItem.Checked = true;
            this.showRulerToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showRulerToolStripMenuItem.Name = "showRulerToolStripMenuItem";
            this.showRulerToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showRulerToolStripMenuItem.Text = "Show Ruler";
            this.showRulerToolStripMenuItem.Click += new System.EventHandler(showRulerToolStripMenuItem_Click_1);
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(230, 6);
            this.matSizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.x6ToolStripMenuItem, this.x12ToolStripMenuItem, this.x24ToolStripMenuItem });
            this.matSizeToolStripMenuItem.Name = "matSizeToolStripMenuItem";
            this.matSizeToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.matSizeToolStripMenuItem.Text = "Mat Size";
            this.x6ToolStripMenuItem.Checked = true;
            this.x6ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.x6ToolStripMenuItem.Name = "x6ToolStripMenuItem";
            this.x6ToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.x6ToolStripMenuItem.Text = "12x6";
            this.x6ToolStripMenuItem.Click += new System.EventHandler(changeMatSize_Click);
            this.x12ToolStripMenuItem.Name = "x12ToolStripMenuItem";
            this.x12ToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.x12ToolStripMenuItem.Text = "12x12";
            this.x12ToolStripMenuItem.Click += new System.EventHandler(changeMatSize_Click);
            this.x24ToolStripMenuItem.Name = "x24ToolStripMenuItem";
            this.x24ToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
            this.x24ToolStripMenuItem.Text = "24x12";
            this.x24ToolStripMenuItem.Click += new System.EventHandler(changeMatSize_Click);
            this.matViewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.fitToPageToolStripMenuItem, this.percent100ToolStripMenuItem, this.percent200ToolStripMenuItem });
            this.matViewToolStripMenuItem.Name = "matViewToolStripMenuItem";
            this.matViewToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.matViewToolStripMenuItem.Text = "Mat View";
            this.fitToPageToolStripMenuItem.Checked = true;
            this.fitToPageToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fitToPageToolStripMenuItem.Name = "fitToPageToolStripMenuItem";
            this.fitToPageToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.fitToPageToolStripMenuItem.Text = "Fit to Page";
            this.fitToPageToolStripMenuItem.Click += new System.EventHandler(changeView_Click);
            this.percent100ToolStripMenuItem.Name = "percent100ToolStripMenuItem";
            this.percent100ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.percent100ToolStripMenuItem.Text = "100%";
            this.percent100ToolStripMenuItem.Click += new System.EventHandler(changeView_Click);
            this.percent200ToolStripMenuItem.Name = "percent200ToolStripMenuItem";
            this.percent200ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.percent200ToolStripMenuItem.Text = "200%";
            this.percent200ToolStripMenuItem.Click += new System.EventHandler(changeView_Click);
            this.fontFeaturesMenuItem.Name = "fontFeaturesMenuItem";
            this.fontFeaturesMenuItem.Size = new System.Drawing.Size(62, 20);
            this.fontFeaturesMenuItem.Text = "Features";
            this.prefsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.myCartridgeMenuItem, this.favoriteCartridgeMenuItem, this.toolStripSeparator12, this.multiCutToolStripMenuItem });
            this.prefsToolStripMenuItem.Name = "prefsToolStripMenuItem";
            this.prefsToolStripMenuItem.Size = new System.Drawing.Size(77, 20);
            this.prefsToolStripMenuItem.Text = "Preferences";
            this.myCartridgeMenuItem.Checked = true;
            this.myCartridgeMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.myCartridgeMenuItem.Name = "myCartridgeMenuItem";
            this.myCartridgeMenuItem.Size = new System.Drawing.Size(162, 22);
            this.myCartridgeMenuItem.Text = "My Cartridge";
            this.myCartridgeMenuItem.Click += new System.EventHandler(myCartridgeMenuItem_Click);
            this.favoriteCartridgeMenuItem.Checked = true;
            this.favoriteCartridgeMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.favoriteCartridgeMenuItem.Name = "favoriteCartridgeMenuItem";
            this.favoriteCartridgeMenuItem.Size = new System.Drawing.Size(162, 22);
            this.favoriteCartridgeMenuItem.Text = "Favorite Cartridge";
            this.favoriteCartridgeMenuItem.Click += new System.EventHandler(favoriteCartridgeMenuItem_Click);
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(159, 6);
            this.multiCutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.multiCut1ToolStripMenuItem, this.multiCut2ToolStripMenuItem, this.multiCut3ToolStripMenuItem, this.multiCut4ToolStripMenuItem });
            this.multiCutToolStripMenuItem.Name = "multiCutToolStripMenuItem";
            this.multiCutToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.multiCutToolStripMenuItem.Text = "Multi Cut";
            this.multiCut1ToolStripMenuItem.Checked = true;
            this.multiCut1ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.multiCut1ToolStripMenuItem.Name = "multiCut1ToolStripMenuItem";
            this.multiCut1ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.multiCut1ToolStripMenuItem.Text = "Off (Default)";
            this.multiCut1ToolStripMenuItem.Click += new System.EventHandler(multiCut_Click);
            this.multiCut2ToolStripMenuItem.Name = "multiCut2ToolStripMenuItem";
            this.multiCut2ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.multiCut2ToolStripMenuItem.Text = "2 Times";
            this.multiCut2ToolStripMenuItem.Click += new System.EventHandler(multiCut_Click);
            this.multiCut3ToolStripMenuItem.Name = "multiCut3ToolStripMenuItem";
            this.multiCut3ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.multiCut3ToolStripMenuItem.Text = "3 Times";
            this.multiCut3ToolStripMenuItem.Click += new System.EventHandler(multiCut_Click);
            this.multiCut4ToolStripMenuItem.Name = "multiCut4ToolStripMenuItem";
            this.multiCut4ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.multiCut4ToolStripMenuItem.Text = "4 Times";
            this.multiCut4ToolStripMenuItem.Click += new System.EventHandler(multiCut_Click);
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[12]
            {
                this.enableBalloonHelpMenuItem, this.toolStripSeparator11, this.checkForUpdatesMenuItem, this.storeLocatorMenuItem, this.toolStripSeparator5, this.updateFirmwareMenuItem, this.toolStripSeparator7, this.helpMenuItem, this.aboutToolStripMenuItem, this.toolStripSeparator18,
                this.loadPaperToolStripMenuItem, this.unloadPaperToolStripMenuItem
            });
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            this.enableBalloonHelpMenuItem.Checked = true;
            this.enableBalloonHelpMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableBalloonHelpMenuItem.Name = "enableBalloonHelpMenuItem";
            this.enableBalloonHelpMenuItem.Size = new System.Drawing.Size(199, 22);
            this.enableBalloonHelpMenuItem.Text = "Enable Balloon Help";
            this.enableBalloonHelpMenuItem.Click += new System.EventHandler(enableBalloonHelpMenuItem_Click);
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(196, 6);
            this.checkForUpdatesMenuItem.Name = "checkForUpdatesMenuItem";
            this.checkForUpdatesMenuItem.Size = new System.Drawing.Size(199, 22);
            this.checkForUpdatesMenuItem.Text = "Check for Updates...";
            this.checkForUpdatesMenuItem.Click += new System.EventHandler(checkForUpdatesMenuItem_Click);
            this.storeLocatorMenuItem.Name = "storeLocatorMenuItem";
            this.storeLocatorMenuItem.Size = new System.Drawing.Size(199, 22);
            this.storeLocatorMenuItem.Text = "Store Locator...";
            this.storeLocatorMenuItem.Click += new System.EventHandler(storeLocatorMenuItem_Click);
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(196, 6);
            this.updateFirmwareMenuItem.Name = "updateFirmwareMenuItem";
            this.updateFirmwareMenuItem.Size = new System.Drawing.Size(199, 22);
            this.updateFirmwareMenuItem.Text = "Update Cricut Firmware...";
            this.updateFirmwareMenuItem.Click += new System.EventHandler(updateFirmwareMenuItem_Click);
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(196, 6);
            this.helpMenuItem.Name = "helpMenuItem";
            this.helpMenuItem.Size = new System.Drawing.Size(199, 22);
            this.helpMenuItem.Text = "Help...";
            this.helpMenuItem.Click += new System.EventHandler(helpMenuItem_Click);
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.aboutToolStripMenuItem.Text = "About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(aboutToolStripMenuItem_Click);
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(196, 6);
            this.toolStripSeparator18.Visible = false;
            this.loadPaperToolStripMenuItem.Name = "loadPaperToolStripMenuItem";
            this.loadPaperToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.loadPaperToolStripMenuItem.Text = "Load Paper";
            this.loadPaperToolStripMenuItem.Visible = false;
            this.loadPaperToolStripMenuItem.Click += new System.EventHandler(loadPaperToolStripMenuItem_Click);
            this.unloadPaperToolStripMenuItem.Name = "unloadPaperToolStripMenuItem";
            this.unloadPaperToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.unloadPaperToolStripMenuItem.Text = "Unload Paper";
            this.unloadPaperToolStripMenuItem.Visible = false;
            this.unloadPaperToolStripMenuItem.Click += new System.EventHandler(unloadPaperToolStripMenuItem_Click);
            this.menuStripSeparator.BackColor = System.Drawing.Color.Black;
            this.menuStripSeparator.Location = new System.Drawing.Point(0, 23);
            this.menuStripSeparator.Name = "menuStripSeparator";
            this.menuStripSeparator.Size = new System.Drawing.Size(1016, 1);
            this.menuStripSeparator.TabIndex = 47;
            this.form1ToolTips.AutomaticDelay = 1000;
            this.form1ToolTips.IsBalloon = true;
            this.prevKeywordButton.Font = new System.Drawing.Font("Webdings", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 2);
            this.prevKeywordButton.Location = new System.Drawing.Point(161, 33);
            this.prevKeywordButton.Name = "prevKeywordButton";
            this.prevKeywordButton.Size = new System.Drawing.Size(24, 24);
            this.prevKeywordButton.TabIndex = 39;
            this.prevKeywordButton.TabStop = false;
            this.prevKeywordButton.Text = "9";
            this.form1ToolTips.SetToolTip(this.prevKeywordButton, "Show Previous Key Shape");
            this.prevKeywordButton.UseVisualStyleBackColor = true;
            this.prevKeywordButton.MouseLeave += new System.EventHandler(button_MouseLeave);
            this.prevKeywordButton.Paint += new System.Windows.Forms.PaintEventHandler(defaultButton_Paint);
            this.prevKeywordButton.Click += new System.EventHandler(prevKeywordButton_Click);
            this.prevKeywordButton.MouseDown += new System.Windows.Forms.MouseEventHandler(button_MouseDown);
            this.prevKeywordButton.MouseUp += new System.Windows.Forms.MouseEventHandler(button_MouseUp);
            this.prevKeywordButton.MouseEnter += new System.EventHandler(button_MouseEnter);
            this.nextKeywordButton.Enabled = false;
            this.nextKeywordButton.Font = new System.Drawing.Font("Webdings", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 2);
            this.nextKeywordButton.Location = new System.Drawing.Point(191, 33);
            this.nextKeywordButton.Name = "nextKeywordButton";
            this.nextKeywordButton.Size = new System.Drawing.Size(24, 24);
            this.nextKeywordButton.TabIndex = 38;
            this.nextKeywordButton.TabStop = false;
            this.nextKeywordButton.Text = ":";
            this.form1ToolTips.SetToolTip(this.nextKeywordButton, "Show Next Key Shape");
            this.nextKeywordButton.UseVisualStyleBackColor = true;
            this.nextKeywordButton.MouseLeave += new System.EventHandler(button_MouseLeave);
            this.nextKeywordButton.Paint += new System.Windows.Forms.PaintEventHandler(defaultButton_Paint);
            this.nextKeywordButton.Click += new System.EventHandler(nextKeywordButton_Click);
            this.nextKeywordButton.MouseDown += new System.Windows.Forms.MouseEventHandler(button_MouseDown);
            this.nextKeywordButton.MouseUp += new System.Windows.Forms.MouseEventHandler(button_MouseUp);
            this.nextKeywordButton.MouseEnter += new System.EventHandler(button_MouseEnter);
            this.fontTreeView.Location = new System.Drawing.Point(15, 117);
            this.fontTreeView.Name = "fontTreeView";
            this.fontTreeView.ShowLines = false;
            this.fontTreeView.ShowPlusMinus = false;
            this.fontTreeView.ShowRootLines = false;
            this.fontTreeView.Size = new System.Drawing.Size(200, 113);
            this.fontTreeView.TabIndex = 37;
            this.fontTreeView.TabStop = false;
            this.form1ToolTips.SetToolTip(this.fontTreeView, "Double-click on a cartridge\r\nname to select it");
            this.keepAspectRatioButton.Location = new System.Drawing.Point(3, 59);
            this.keepAspectRatioButton.Name = "keepAspectRatioButton";
            this.keepAspectRatioButton.Size = new System.Drawing.Size(18, 23);
            this.keepAspectRatioButton.TabIndex = 29;
            this.keepAspectRatioButton.Text = "-";
            this.form1ToolTips.SetToolTip(this.keepAspectRatioButton, "Keep the aspect ratio when Width or Height are changed");
            this.keepAspectRatioButton.UseVisualStyleBackColor = true;
            this.keepAspectRatioButton.Visible = false;
            this.keepAspectRatioButton.MouseLeave += new System.EventHandler(button_MouseLeave);
            this.keepAspectRatioButton.Paint += new System.Windows.Forms.PaintEventHandler(defaultButton_Paint);
            this.keepAspectRatioButton.MouseMove += new System.Windows.Forms.MouseEventHandler(button_MouseUp);
            this.keepAspectRatioButton.Click += new System.EventHandler(keepAspectRatioButton_Click);
            this.keepAspectRatioButton.MouseDown += new System.Windows.Forms.MouseEventHandler(button_MouseDown);
            this.keepAspectRatioButton.MouseEnter += new System.EventHandler(button_MouseEnter);
            this.nudgeDownButton.Enabled = false;
            this.nudgeDownButton.Font = new System.Drawing.Font("Webdings", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 2);
            this.nudgeDownButton.Location = new System.Drawing.Point(151, 178);
            this.nudgeDownButton.Name = "nudgeDownButton";
            this.nudgeDownButton.Size = new System.Drawing.Size(24, 24);
            this.nudgeDownButton.TabIndex = 22;
            this.nudgeDownButton.Text = "6";
            this.form1ToolTips.SetToolTip(this.nudgeDownButton, "Nudge Down");
            this.nudgeDownButton.UseVisualStyleBackColor = true;
            this.nudgeDownButton.MouseLeave += new System.EventHandler(nudgeButton_MouseLeave);
            this.nudgeDownButton.Paint += new System.Windows.Forms.PaintEventHandler(defaultButton_Paint);
            this.nudgeDownButton.Click += new System.EventHandler(nudgeDownButton_Click);
            this.nudgeDownButton.MouseDown += new System.Windows.Forms.MouseEventHandler(nudgeDownButton_MouseDown);
            this.nudgeDownButton.MouseUp += new System.Windows.Forms.MouseEventHandler(nudgeButton_MouseUp);
            this.nudgeDownButton.MouseEnter += new System.EventHandler(button_MouseEnter);
            this.nudgeUpButton.Enabled = false;
            this.nudgeUpButton.Font = new System.Drawing.Font("Webdings", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 2);
            this.nudgeUpButton.Location = new System.Drawing.Point(121, 178);
            this.nudgeUpButton.Name = "nudgeUpButton";
            this.nudgeUpButton.Size = new System.Drawing.Size(24, 24);
            this.nudgeUpButton.TabIndex = 21;
            this.nudgeUpButton.Text = "5";
            this.form1ToolTips.SetToolTip(this.nudgeUpButton, "Nudge Up");
            this.nudgeUpButton.UseVisualStyleBackColor = true;
            this.nudgeUpButton.MouseLeave += new System.EventHandler(nudgeButton_MouseLeave);
            this.nudgeUpButton.Paint += new System.Windows.Forms.PaintEventHandler(defaultButton_Paint);
            this.nudgeUpButton.Click += new System.EventHandler(nudgeUpButton_Click);
            this.nudgeUpButton.MouseDown += new System.Windows.Forms.MouseEventHandler(nudgeUpButton_MouseDown);
            this.nudgeUpButton.MouseUp += new System.Windows.Forms.MouseEventHandler(nudgeButton_MouseUp);
            this.nudgeUpButton.MouseEnter += new System.EventHandler(button_MouseEnter);
            this.rotate90Button.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.rotate90Button.Location = new System.Drawing.Point(61, 113);
            this.rotate90Button.Name = "rotate90Button";
            this.rotate90Button.Size = new System.Drawing.Size(24, 24);
            this.rotate90Button.TabIndex = 12;
            this.rotate90Button.Text = "90";
            this.form1ToolTips.SetToolTip(this.rotate90Button, "Rotate 90");
            this.rotate90Button.UseVisualStyleBackColor = true;
            this.rotate90Button.MouseLeave += new System.EventHandler(button_MouseLeave);
            this.rotate90Button.Paint += new System.Windows.Forms.PaintEventHandler(defaultButton_Paint);
            this.rotate90Button.Click += new System.EventHandler(rotate90Button_Click);
            this.rotate90Button.MouseDown += new System.Windows.Forms.MouseEventHandler(button_MouseDown);
            this.rotate90Button.MouseUp += new System.Windows.Forms.MouseEventHandler(button_MouseUp);
            this.rotate90Button.MouseEnter += new System.EventHandler(button_MouseEnter);
            this.nudgeRightButton.Enabled = false;
            this.nudgeRightButton.Font = new System.Drawing.Font("Webdings", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 2);
            this.nudgeRightButton.Location = new System.Drawing.Point(91, 178);
            this.nudgeRightButton.Name = "nudgeRightButton";
            this.nudgeRightButton.Size = new System.Drawing.Size(24, 24);
            this.nudgeRightButton.TabIndex = 20;
            this.nudgeRightButton.Text = "4";
            this.form1ToolTips.SetToolTip(this.nudgeRightButton, "Nudge Right");
            this.nudgeRightButton.UseVisualStyleBackColor = true;
            this.nudgeRightButton.MouseLeave += new System.EventHandler(nudgeButton_MouseLeave);
            this.nudgeRightButton.Paint += new System.Windows.Forms.PaintEventHandler(defaultButton_Paint);
            this.nudgeRightButton.Click += new System.EventHandler(nudgeRightButton_Click);
            this.nudgeRightButton.MouseDown += new System.Windows.Forms.MouseEventHandler(nudgeRightButton_MouseDown);
            this.nudgeRightButton.MouseUp += new System.Windows.Forms.MouseEventHandler(nudgeButton_MouseUp);
            this.nudgeRightButton.MouseEnter += new System.EventHandler(button_MouseEnter);
            this.nudgeLeftButton.Enabled = false;
            this.nudgeLeftButton.Font = new System.Drawing.Font("Webdings", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 2);
            this.nudgeLeftButton.Location = new System.Drawing.Point(61, 178);
            this.nudgeLeftButton.Name = "nudgeLeftButton";
            this.nudgeLeftButton.Size = new System.Drawing.Size(24, 24);
            this.nudgeLeftButton.TabIndex = 19;
            this.nudgeLeftButton.Text = "3";
            this.form1ToolTips.SetToolTip(this.nudgeLeftButton, "Nudge Left");
            this.nudgeLeftButton.UseVisualStyleBackColor = true;
            this.nudgeLeftButton.MouseLeave += new System.EventHandler(nudgeButton_MouseLeave);
            this.nudgeLeftButton.Paint += new System.Windows.Forms.PaintEventHandler(defaultButton_Paint);
            this.nudgeLeftButton.Click += new System.EventHandler(nudgeLeftButton_Click);
            this.nudgeLeftButton.MouseDown += new System.Windows.Forms.MouseEventHandler(nudgeLeftButton_MouseDown);
            this.nudgeLeftButton.MouseUp += new System.Windows.Forms.MouseEventHandler(nudgeButton_MouseUp);
            this.nudgeLeftButton.MouseEnter += new System.EventHandler(button_MouseEnter);
            this.applyKerningButton.AutoSize = true;
            this.applyKerningButton.Location = new System.Drawing.Point(137, 150);
            this.applyKerningButton.Name = "applyKerningButton";
            this.applyKerningButton.Size = new System.Drawing.Size(78, 27);
            this.applyKerningButton.TabIndex = 18;
            this.applyKerningButton.Text = "Apply";
            this.form1ToolTips.SetToolTip(this.applyKerningButton, "Apply Kerning");
            this.applyKerningButton.UseVisualStyleBackColor = true;
            this.applyKerningButton.MouseLeave += new System.EventHandler(button_MouseLeave);
            this.applyKerningButton.Paint += new System.Windows.Forms.PaintEventHandler(defaultButton_Paint);
            this.applyKerningButton.Click += new System.EventHandler(applyKerningButton_Click);
            this.applyKerningButton.MouseDown += new System.Windows.Forms.MouseEventHandler(button_MouseDown);
            this.applyKerningButton.MouseUp += new System.Windows.Forms.MouseEventHandler(button_MouseUp);
            this.applyKerningButton.MouseEnter += new System.EventHandler(button_MouseEnter);
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 155);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(43, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "Kerning";
            this.form1ToolTips.SetToolTip(this.label9, "Kerning controls spacing\r\nbetween characters\r\nin a shape");
            this.label9.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.revertShapePropertiesButton.AutoSize = true;
            this.revertShapePropertiesButton.Location = new System.Drawing.Point(137, 114);
            this.revertShapePropertiesButton.Name = "revertShapePropertiesButton";
            this.revertShapePropertiesButton.Size = new System.Drawing.Size(78, 27);
            this.revertShapePropertiesButton.TabIndex = 14;
            this.revertShapePropertiesButton.Text = "Revert";
            this.form1ToolTips.SetToolTip(this.revertShapePropertiesButton, "Revert Shape Properties");
            this.revertShapePropertiesButton.UseVisualStyleBackColor = true;
            this.revertShapePropertiesButton.MouseLeave += new System.EventHandler(button_MouseLeave);
            this.revertShapePropertiesButton.Paint += new System.Windows.Forms.PaintEventHandler(defaultButton_Paint);
            this.revertShapePropertiesButton.Click += new System.EventHandler(reverShapePropertiesButton_Click);
            this.revertShapePropertiesButton.MouseDown += new System.Windows.Forms.MouseEventHandler(button_MouseDown);
            this.revertShapePropertiesButton.MouseUp += new System.Windows.Forms.MouseEventHandler(button_MouseUp);
            this.revertShapePropertiesButton.MouseEnter += new System.EventHandler(button_MouseEnter);
            this.percent200Button.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.percent200Button.Font = new System.Drawing.Font("Microsoft Sans Serif", 7f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.percent200Button.Location = new System.Drawing.Point(6, 676);
            this.percent200Button.Name = "percent200Button";
            this.percent200Button.Size = new System.Drawing.Size(48, 32);
            this.percent200Button.TabIndex = 78;
            this.percent200Button.Text = "200%";
            this.form1ToolTips.SetToolTip(this.percent200Button, "Mat View 200%");
            this.percent200Button.UseVisualStyleBackColor = false;
            this.percent200Button.Click += new System.EventHandler(viewButton_Click);
            this.percent100Button.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.percent100Button.Font = new System.Drawing.Font("Microsoft Sans Serif", 7f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.percent100Button.Location = new System.Drawing.Point(6, 650);
            this.percent100Button.Name = "percent100Button";
            this.percent100Button.Size = new System.Drawing.Size(48, 26);
            this.percent100Button.TabIndex = 77;
            this.percent100Button.Text = "100%";
            this.form1ToolTips.SetToolTip(this.percent100Button, "Mat View - 100%");
            this.percent100Button.UseVisualStyleBackColor = false;
            this.percent100Button.Click += new System.EventHandler(viewButton_Click);
            this.fitToPageButton.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.fitToPageButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.fitToPageButton.Location = new System.Drawing.Point(6, 618);
            this.fitToPageButton.Name = "fitToPageButton";
            this.fitToPageButton.Size = new System.Drawing.Size(48, 32);
            this.fitToPageButton.TabIndex = 76;
            this.fitToPageButton.Text = "FIT\r\nVIEW";
            this.form1ToolTips.SetToolTip(this.fitToPageButton, "Mat View - Fit View");
            this.fitToPageButton.UseVisualStyleBackColor = false;
            this.fitToPageButton.Click += new System.EventHandler(viewButton_Click);
            this.previewButton.BackColor = System.Drawing.Color.Transparent;
            this.previewButton.Location = new System.Drawing.Point(6, 503);
            this.previewButton.Name = "previewButton";
            this.previewButton.Size = new System.Drawing.Size(48, 48);
            this.previewButton.TabIndex = 75;
            this.form1ToolTips.SetToolTip(this.previewButton, "Preview");
            this.previewButton.UseVisualStyleBackColor = false;
            this.previewButton.Click += new System.EventHandler(previewButton_Click);
            this.deleteGroupButton.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.deleteGroupButton.Location = new System.Drawing.Point(6, 441);
            this.deleteGroupButton.Name = "deleteGroupButton";
            this.deleteGroupButton.Size = new System.Drawing.Size(48, 48);
            this.deleteGroupButton.TabIndex = 74;
            this.form1ToolTips.SetToolTip(this.deleteGroupButton, "Delete Shapes");
            this.deleteGroupButton.UseVisualStyleBackColor = false;
            this.deleteGroupButton.Click += new System.EventHandler(deleteButton_Click);
            this.pasteGroupButton.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.pasteGroupButton.Location = new System.Drawing.Point(6, 393);
            this.pasteGroupButton.Name = "pasteGroupButton";
            this.pasteGroupButton.Size = new System.Drawing.Size(48, 48);
            this.pasteGroupButton.TabIndex = 73;
            this.form1ToolTips.SetToolTip(this.pasteGroupButton, "Paste Shapes");
            this.pasteGroupButton.UseVisualStyleBackColor = false;
            this.pasteGroupButton.Click += new System.EventHandler(pasteMenuItem_Click);
            this.copyGroupButton.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.copyGroupButton.Location = new System.Drawing.Point(6, 345);
            this.copyGroupButton.Name = "copyGroupButton";
            this.copyGroupButton.Size = new System.Drawing.Size(48, 48);
            this.copyGroupButton.TabIndex = 72;
            this.form1ToolTips.SetToolTip(this.copyGroupButton, "Copy Shapes");
            this.copyGroupButton.UseVisualStyleBackColor = false;
            this.copyGroupButton.Click += new System.EventHandler(copyMenuItem_Click);
            this.newPageButton.BackColor = System.Drawing.Color.Transparent;
            this.newPageButton.Location = new System.Drawing.Point(6, 283);
            this.newPageButton.Name = "newPageButton";
            this.newPageButton.Size = new System.Drawing.Size(48, 48);
            this.newPageButton.TabIndex = 71;
            this.form1ToolTips.SetToolTip(this.newPageButton, "New Page");
            this.newPageButton.UseVisualStyleBackColor = false;
            this.newPageButton.Click += new System.EventHandler(newPageButton_Click);
            this.cricutCutButton.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.cricutCutButton.Location = new System.Drawing.Point(6, 551);
            this.cricutCutButton.Name = "cricutCutButton";
            this.cricutCutButton.Size = new System.Drawing.Size(48, 48);
            this.cricutCutButton.TabIndex = 70;
            this.form1ToolTips.SetToolTip(this.cricutCutButton, "Cut With Cricut");
            this.cricutCutButton.UseVisualStyleBackColor = false;
            this.cricutCutButton.Paint += new System.Windows.Forms.PaintEventHandler(bitmapButton_Paint);
            this.cricutCutButton.Click += new System.EventHandler(cutWithCricutMenuItem_Click);
            this.fontImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.fontImageList.ImageSize = new System.Drawing.Size(16, 16);
            this.fontImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.hoverTimer.Tick += new System.EventHandler(hoverTimer_Tick);
            this.pageContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[10] { this.changePageNameMenuItem, this.pageNameTextBox, this.toolStripSeparator14, this.showFontMenuItem, this.toolStripSeparator15, this.orderSubMenuItem, this.pagePreviewMenuItem, this.pageColorMenuItem, this.toolStripSeparator2, this.invisContourMenuItem });
            this.pageContextMenu.Name = "glyphContextMenu";
            this.pageContextMenu.Size = new System.Drawing.Size(189, 177);
            this.pageContextMenu.Opening += new System.ComponentModel.CancelEventHandler(pageContextMenu_Opening);
            this.changePageNameMenuItem.Font = new System.Drawing.Font("Tahoma", 8.25f);
            this.changePageNameMenuItem.Name = "changePageNameMenuItem";
            this.changePageNameMenuItem.Size = new System.Drawing.Size(188, 22);
            this.changePageNameMenuItem.Text = "Change Page Name";
            this.pageNameTextBox.Name = "pageNameTextBox";
            this.pageNameTextBox.Size = new System.Drawing.Size(100, 21);
            this.pageNameTextBox.Text = "Page Name";
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(185, 6);
            this.showFontMenuItem.Enabled = false;
            this.showFontMenuItem.Name = "showFontMenuItem";
            this.showFontMenuItem.Size = new System.Drawing.Size(188, 22);
            this.showFontMenuItem.Text = "Show Cartridge";
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(185, 6);
            this.orderSubMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.bringPagetoFrontMenuItem, this.sendPagetoBackMenuItem, this.bringPageForwardMenuItem, this.sendPageBackwardMenuItem });
            this.orderSubMenuItem.Name = "orderSubMenuItem";
            this.orderSubMenuItem.Size = new System.Drawing.Size(188, 22);
            this.orderSubMenuItem.Text = "Page Order";
            this.bringPagetoFrontMenuItem.Name = "bringPagetoFrontMenuItem";
            this.bringPagetoFrontMenuItem.Size = new System.Drawing.Size(174, 22);
            this.bringPagetoFrontMenuItem.Text = "Bring Page to Front";
            this.sendPagetoBackMenuItem.Name = "sendPagetoBackMenuItem";
            this.sendPagetoBackMenuItem.Size = new System.Drawing.Size(174, 22);
            this.sendPagetoBackMenuItem.Text = "Send Page to Back";
            this.bringPageForwardMenuItem.Name = "bringPageForwardMenuItem";
            this.bringPageForwardMenuItem.Size = new System.Drawing.Size(174, 22);
            this.bringPageForwardMenuItem.Text = "Bring Page Forward";
            this.sendPageBackwardMenuItem.Name = "sendPageBackwardMenuItem";
            this.sendPageBackwardMenuItem.Size = new System.Drawing.Size(174, 22);
            this.sendPageBackwardMenuItem.Text = "Send Page Backward";
            this.pagePreviewMenuItem.Checked = true;
            this.pagePreviewMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.pagePreviewMenuItem.Name = "pagePreviewMenuItem";
            this.pagePreviewMenuItem.Size = new System.Drawing.Size(188, 22);
            this.pagePreviewMenuItem.Text = "Include Page in Preview";
            this.pagePreviewMenuItem.Click += new System.EventHandler(pagePreviewMenuItem_Click);
            this.pageColorMenuItem.Name = "pageColorMenuItem";
            this.pageColorMenuItem.Size = new System.Drawing.Size(188, 22);
            this.pageColorMenuItem.Text = "Set Preview Color";
            this.pageColorMenuItem.Click += new System.EventHandler(pageColorMenuItem_Click);
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(185, 6);
            this.invisContourMenuItem.Name = "invisContourMenuItem";
            this.invisContourMenuItem.Size = new System.Drawing.Size(188, 22);
            this.invisContourMenuItem.Text = "Invis Selected Contour";
            this.invisContourMenuItem.Click += new System.EventHandler(invisContourMenuItem_Click);
            this.utilityTimer.Interval = 1000;
            this.cuttingBackgroundWorker.WorkerReportsProgress = true;
            this.cuttingBackgroundWorker.WorkerSupportsCancellation = true;
            this.firmwareBackgroundWorker.WorkerReportsProgress = true;
            this.firmwareBackgroundWorker.WorkerSupportsCancellation = true;
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.BackgroundImage = Cricut_Design_Studio.MarcusResources.cricut_logo_2;
            this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel2.Location = new System.Drawing.Point(4, 31);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(48, 50);
            this.panel2.TabIndex = 0;
            this.cartridgeLibraryPanel.BackColor = System.Drawing.Color.Transparent;
            this.cartridgeLibraryPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.cartridgeLibraryPanel.Controls.Add(this.cartridgeLibraryHeaderPanel);
            this.cartridgeLibraryPanel.Controls.Add(this.prevKeywordButton);
            this.cartridgeLibraryPanel.Controls.Add(this.nextKeywordButton);
            this.cartridgeLibraryPanel.Controls.Add(this.fontTreeView);
            this.cartridgeLibraryPanel.Controls.Add(this.fontsChooseByComboBox);
            this.cartridgeLibraryPanel.Controls.Add(this.key2Label);
            this.cartridgeLibraryPanel.Controls.Add(this.viewLabel);
            this.cartridgeLibraryPanel.Controls.Add(this.fontsTag2ComboBox);
            this.cartridgeLibraryPanel.Controls.Add(this.fontsTag1ComboBox);
            this.cartridgeLibraryPanel.Controls.Add(this.key1Label);
            this.cartridgeLibraryPanel.Location = new System.Drawing.Point(58, 31);
            this.cartridgeLibraryPanel.Name = "cartridgeLibraryPanel";
            this.cartridgeLibraryPanel.Size = new System.Drawing.Size(230, 240);
            this.cartridgeLibraryPanel.TabIndex = 15;
            this.cartridgeLibraryPanel.Paint += new System.Windows.Forms.PaintEventHandler(cartridgeLibraryPanel_Paint);
            this.cartridgeLibraryHeaderPanel.BackgroundImage = Cricut_Design_Studio.MarcusResources.header_library;
            this.cartridgeLibraryHeaderPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cartridgeLibraryHeaderPanel.Location = new System.Drawing.Point(0, 0);
            this.cartridgeLibraryHeaderPanel.Name = "cartridgeLibraryHeaderPanel";
            this.cartridgeLibraryHeaderPanel.Size = new System.Drawing.Size(230, 27);
            this.cartridgeLibraryHeaderPanel.TabIndex = 40;
            this.fontsChooseByComboBox.FormattingEnabled = true;
            this.fontsChooseByComboBox.Items.AddRange(new object[5] { "Favorites", "All by Category", "My Cartridges", "Keywords", "This Project" });
            this.fontsChooseByComboBox.Location = new System.Drawing.Point(51, 36);
            this.fontsChooseByComboBox.Name = "fontsChooseByComboBox";
            this.fontsChooseByComboBox.Size = new System.Drawing.Size(104, 21);
            this.fontsChooseByComboBox.TabIndex = 0;
            this.fontsChooseByComboBox.TabStop = false;
            this.fontsChooseByComboBox.SelectedIndexChanged += new System.EventHandler(fontsChooseByComboBox_SelectedIndexChanged);
            this.key2Label.AutoSize = true;
            this.key2Label.Location = new System.Drawing.Point(12, 93);
            this.key2Label.Name = "key2Label";
            this.key2Label.Size = new System.Drawing.Size(34, 13);
            this.key2Label.TabIndex = 7;
            this.key2Label.Text = "Key 2";
            this.key2Label.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.viewLabel.AutoSize = true;
            this.viewLabel.Location = new System.Drawing.Point(12, 39);
            this.viewLabel.Name = "viewLabel";
            this.viewLabel.Size = new System.Drawing.Size(30, 13);
            this.viewLabel.TabIndex = 1;
            this.viewLabel.Text = "View";
            this.viewLabel.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.fontsTag2ComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.fontsTag2ComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.fontsTag2ComboBox.FormattingEnabled = true;
            this.fontsTag2ComboBox.Location = new System.Drawing.Point(51, 90);
            this.fontsTag2ComboBox.Name = "fontsTag2ComboBox";
            this.fontsTag2ComboBox.Size = new System.Drawing.Size(164, 21);
            this.fontsTag2ComboBox.TabIndex = 6;
            this.fontsTag2ComboBox.TabStop = false;
            this.fontsTag2ComboBox.SelectedIndexChanged += new System.EventHandler(anyTagComboBox_SelectedIndexChanged);
            this.fontsTag2ComboBox.Leave += new System.EventHandler(anyTagComboBox_Leave);
            this.fontsTag2ComboBox.Enter += new System.EventHandler(anyTagComboBox_Enter);
            this.fontsTag2ComboBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(anyTagComboBox_PreviewKeyDown);
            this.fontsTag2ComboBox.TextChanged += new System.EventHandler(fontsTag2ComboBox_TextChanged);
            this.fontsTag1ComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.fontsTag1ComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.fontsTag1ComboBox.FormattingEnabled = true;
            this.fontsTag1ComboBox.Location = new System.Drawing.Point(51, 63);
            this.fontsTag1ComboBox.Name = "fontsTag1ComboBox";
            this.fontsTag1ComboBox.Size = new System.Drawing.Size(164, 21);
            this.fontsTag1ComboBox.TabIndex = 2;
            this.fontsTag1ComboBox.TabStop = false;
            this.fontsTag1ComboBox.SelectedIndexChanged += new System.EventHandler(anyTagComboBox_SelectedIndexChanged);
            this.fontsTag1ComboBox.Leave += new System.EventHandler(anyTagComboBox_Leave);
            this.fontsTag1ComboBox.Enter += new System.EventHandler(anyTagComboBox_Enter);
            this.fontsTag1ComboBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(anyTagComboBox_PreviewKeyDown);
            this.fontsTag1ComboBox.TextChanged += new System.EventHandler(fontsTag1ComboBox_TextChanged);
            this.key1Label.AutoSize = true;
            this.key1Label.Location = new System.Drawing.Point(12, 66);
            this.key1Label.Name = "key1Label";
            this.key1Label.Size = new System.Drawing.Size(34, 13);
            this.key1Label.TabIndex = 5;
            this.key1Label.Text = "Key 1";
            this.key1Label.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.panel10.BackColor = System.Drawing.Color.Transparent;
            this.panel10.BackgroundImage = Cricut_Design_Studio.MarcusResources.cricut_type_2;
            this.panel10.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel10.Location = new System.Drawing.Point(4, 87);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(48, 95);
            this.panel10.TabIndex = 18;
            this.propertiesPanel.BackColor = System.Drawing.Color.Transparent;
            this.propertiesPanel.BackgroundImage = Cricut_Design_Studio.MarcusResources.header_properties;
            this.propertiesPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.propertiesPanel.Controls.Add(this.propertiesHeaderPanel);
            this.propertiesPanel.Controls.Add(this.keepAspectRatioButton);
            this.propertiesPanel.Controls.Add(this.nudgeDownButton);
            this.propertiesPanel.Controls.Add(this.nudgeUpButton);
            this.propertiesPanel.Controls.Add(this.panel4);
            this.propertiesPanel.Controls.Add(this.panel3);
            this.propertiesPanel.Controls.Add(this.label68);
            this.propertiesPanel.Controls.Add(this.label67);
            this.propertiesPanel.Controls.Add(this.rotate90Button);
            this.propertiesPanel.Controls.Add(this.label15);
            this.propertiesPanel.Controls.Add(this.label14);
            this.propertiesPanel.Controls.Add(this.nudgeRightButton);
            this.propertiesPanel.Controls.Add(this.nudgeLeftButton);
            this.propertiesPanel.Controls.Add(this.applyKerningButton);
            this.propertiesPanel.Controls.Add(this.label9);
            this.propertiesPanel.Controls.Add(this.kerningTextBox);
            this.propertiesPanel.Controls.Add(this.flipShapesCheckBox);
            this.propertiesPanel.Controls.Add(this.weldingCheckBox);
            this.propertiesPanel.Controls.Add(this.revertShapePropertiesButton);
            this.propertiesPanel.Controls.Add(this.label6);
            this.propertiesPanel.Controls.Add(this.shearTextBox);
            this.propertiesPanel.Controls.Add(this.label5);
            this.propertiesPanel.Controls.Add(this.angleTextBox);
            this.propertiesPanel.Controls.Add(this.label4);
            this.propertiesPanel.Controls.Add(this.heightTextBox);
            this.propertiesPanel.Controls.Add(this.label3);
            this.propertiesPanel.Controls.Add(this.widthTextBox);
            this.propertiesPanel.Controls.Add(this.label2);
            this.propertiesPanel.Controls.Add(this.yPositionTextBox);
            this.propertiesPanel.Controls.Add(this.label1);
            this.propertiesPanel.Controls.Add(this.xPositionTextBox);
            this.propertiesPanel.Location = new System.Drawing.Point(774, 31);
            this.propertiesPanel.Name = "propertiesPanel";
            this.propertiesPanel.Size = new System.Drawing.Size(230, 240);
            this.propertiesPanel.TabIndex = 8;
            this.propertiesPanel.Paint += new System.Windows.Forms.PaintEventHandler(cartridgeLibraryPanel_Paint);
            this.propertiesHeaderPanel.BackgroundImage = Cricut_Design_Studio.MarcusResources.header_properties;
            this.propertiesHeaderPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.propertiesHeaderPanel.Location = new System.Drawing.Point(0, 0);
            this.propertiesHeaderPanel.Name = "propertiesHeaderPanel";
            this.propertiesHeaderPanel.Size = new System.Drawing.Size(230, 27);
            this.propertiesHeaderPanel.TabIndex = 41;
            this.panel4.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.panel4.Location = new System.Drawing.Point(15, 208);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(200, 1);
            this.panel4.TabIndex = 24;
            this.panel3.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.panel3.Location = new System.Drawing.Point(15, 143);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(200, 1);
            this.panel3.TabIndex = 15;
            this.label68.AutoSize = true;
            this.label68.Location = new System.Drawing.Point(16, 185);
            this.label68.Name = "label68";
            this.label68.Size = new System.Drawing.Size(39, 13);
            this.label68.TabIndex = 23;
            this.label68.Text = "Nudge";
            this.label68.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.label67.AutoSize = true;
            this.label67.Location = new System.Drawing.Point(26, 119);
            this.label67.Name = "label67";
            this.label67.Size = new System.Drawing.Size(29, 13);
            this.label67.TabIndex = 13;
            this.label67.Text = "Turn";
            this.label67.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(117, 215);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(62, 13);
            this.label15.TabIndex = 27;
            this.label15.Text = "Flip Shapes";
            this.label15.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(29, 215);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(46, 13);
            this.label14.TabIndex = 25;
            this.label14.Text = "Welding";
            this.label14.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.kerningTextBox.Font = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.kerningTextBox.Location = new System.Drawing.Point(61, 152);
            this.kerningTextBox.Name = "kerningTextBox";
            this.kerningTextBox.Size = new System.Drawing.Size(50, 20);
            this.kerningTextBox.TabIndex = 16;
            this.kerningTextBox.Text = "0.000";
            this.flipShapesCheckBox.AutoSize = true;
            this.flipShapesCheckBox.Location = new System.Drawing.Point(185, 215);
            this.flipShapesCheckBox.Name = "flipShapesCheckBox";
            this.flipShapesCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.flipShapesCheckBox.Size = new System.Drawing.Size(15, 14);
            this.flipShapesCheckBox.TabIndex = 28;
            this.flipShapesCheckBox.TabStop = false;
            this.flipShapesCheckBox.UseVisualStyleBackColor = true;
            this.flipShapesCheckBox.CheckedChanged += new System.EventHandler(flipShapesCheckBox_CheckedChanged);
            this.weldingCheckBox.AutoSize = true;
            this.weldingCheckBox.Location = new System.Drawing.Point(81, 215);
            this.weldingCheckBox.Name = "weldingCheckBox";
            this.weldingCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.weldingCheckBox.Size = new System.Drawing.Size(15, 14);
            this.weldingCheckBox.TabIndex = 26;
            this.weldingCheckBox.TabStop = false;
            this.weldingCheckBox.UseVisualStyleBackColor = true;
            this.weldingCheckBox.CheckedChanged += new System.EventHandler(weldingCheckBox_CheckedChanged);
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(128, 91);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Slant";
            this.label6.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.shearTextBox.Font = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.shearTextBox.Location = new System.Drawing.Point(165, 88);
            this.shearTextBox.Name = "shearTextBox";
            this.shearTextBox.Size = new System.Drawing.Size(50, 20);
            this.shearTextBox.TabIndex = 10;
            this.shearTextBox.Text = "0.000";
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 91);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Rotate";
            this.label5.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.angleTextBox.Font = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.angleTextBox.Location = new System.Drawing.Point(61, 88);
            this.angleTextBox.Name = "angleTextBox";
            this.angleTextBox.Size = new System.Drawing.Size(50, 20);
            this.angleTextBox.TabIndex = 8;
            this.angleTextBox.Text = "0.000";
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(121, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Height";
            this.label4.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.heightTextBox.Font = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.heightTextBox.Location = new System.Drawing.Point(165, 62);
            this.heightTextBox.Name = "heightTextBox";
            this.heightTextBox.Size = new System.Drawing.Size(50, 20);
            this.heightTextBox.TabIndex = 6;
            this.heightTextBox.Text = "0.000";
            this.heightTextBox.Leave += new System.EventHandler(heightTextBox_Leave);
            this.heightTextBox.Enter += new System.EventHandler(widthHeightTextBox_Enter);
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Width";
            this.label3.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.widthTextBox.Font = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.widthTextBox.Location = new System.Drawing.Point(61, 62);
            this.widthTextBox.Name = "widthTextBox";
            this.widthTextBox.Size = new System.Drawing.Size(50, 20);
            this.widthTextBox.TabIndex = 4;
            this.widthTextBox.Text = "0.000";
            this.widthTextBox.Leave += new System.EventHandler(widthTextBox_Leave);
            this.widthTextBox.Enter += new System.EventHandler(widthHeightTextBox_Enter);
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(145, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Y";
            this.label2.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.yPositionTextBox.Font = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.yPositionTextBox.Location = new System.Drawing.Point(165, 36);
            this.yPositionTextBox.Name = "yPositionTextBox";
            this.yPositionTextBox.Size = new System.Drawing.Size(50, 20);
            this.yPositionTextBox.TabIndex = 2;
            this.yPositionTextBox.Text = "0.000";
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "X";
            this.label1.Paint += new System.Windows.Forms.PaintEventHandler(labelAA_Paint);
            this.xPositionTextBox.Font = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.xPositionTextBox.Location = new System.Drawing.Point(61, 36);
            this.xPositionTextBox.Name = "xPositionTextBox";
            this.xPositionTextBox.Size = new System.Drawing.Size(50, 20);
            this.xPositionTextBox.TabIndex = 0;
            this.xPositionTextBox.Text = "0.000";
            this.matTabControl.Controls.Add(this.activateByInternetTabPage);
            this.matTabControl.Controls.Add(this.activateByPhoneTabPage);
            this.matTabControl.Controls.Add(this.activationCompleteTabPage);
            this.matTabControl.Controls.Add(this.updateCricutFirmwareTabPage);
            this.matTabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.matTabControl.ItemSize = new System.Drawing.Size(0, 15);
            this.matTabControl.Location = new System.Drawing.Point(58, 277);
            this.matTabControl.Name = "matTabControl";
            this.matTabControl.SelectedIndex = 0;
            this.matTabControl.Size = new System.Drawing.Size(946, 445);
            this.matTabControl.TabIndex = 48;
            this.matTabControl.SelectedIndexChanged += new System.EventHandler(matTabControl_SelectedIndexChanged);
            this.activateByInternetTabPage.Controls.Add(this.trialVersionPanel);
            this.activateByInternetTabPage.Location = new System.Drawing.Point(4, 19);
            this.activateByInternetTabPage.Name = "activateByInternetTabPage";
            this.activateByInternetTabPage.Size = new System.Drawing.Size(938, 422);
            this.activateByInternetTabPage.TabIndex = 0;
            this.activateByInternetTabPage.Text = "Activate using the Internet";
            this.activateByInternetTabPage.UseVisualStyleBackColor = true;
            this.trialVersionPanel.Controls.Add(this.activateByPhoneButton);
            this.trialVersionPanel.Controls.Add(this.label20);
            this.trialVersionPanel.Controls.Add(this.label32);
            this.trialVersionPanel.Controls.Add(this.label31);
            this.trialVersionPanel.Controls.Add(this.label30);
            this.trialVersionPanel.Controls.Add(this.label29);
            this.trialVersionPanel.Controls.Add(this.iEmailTextBox);
            this.trialVersionPanel.Controls.Add(this.iLastNameTextBox);
            this.trialVersionPanel.Controls.Add(this.iFirstNameTextBox);
            this.trialVersionPanel.Controls.Add(this.label28);
            this.trialVersionPanel.Controls.Add(this.label19);
            this.trialVersionPanel.Controls.Add(this.groupBox1);
            this.trialVersionPanel.Controls.Add(this.continueTrialButton);
            this.trialVersionPanel.Controls.Add(this.activateByInternetButton);
            this.trialVersionPanel.Controls.Add(this.label8);
            this.trialVersionPanel.Location = new System.Drawing.Point(3, 3);
            this.trialVersionPanel.Name = "trialVersionPanel";
            this.trialVersionPanel.Size = new System.Drawing.Size(932, 416);
            this.trialVersionPanel.TabIndex = 0;
            this.activateByPhoneButton.AutoSize = true;
            this.activateByPhoneButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.activateByPhoneButton.Location = new System.Drawing.Point(671, 257);
            this.activateByPhoneButton.Name = "activateByPhoneButton";
            this.activateByPhoneButton.Size = new System.Drawing.Size(205, 35);
            this.activateByPhoneButton.TabIndex = 11;
            this.activateByPhoneButton.Text = "Activate by Phone";
            this.activateByPhoneButton.UseVisualStyleBackColor = true;
            this.activateByPhoneButton.Click += new System.EventHandler(activateByPhoneButton_Click);
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label20.Location = new System.Drawing.Point(11, 320);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(613, 85);
            this.label20.TabIndex = 27;
            this.label20.Text = resources.GetString("label20.Text");
            this.label32.AutoSize = true;
            this.label32.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label32.Location = new System.Drawing.Point(12, 257);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(633, 51);
            this.label32.TabIndex = 26;
            this.label32.Text = resources.GetString("label32.Text");
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(279, 208);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(73, 13);
            this.label31.TabIndex = 25;
            this.label31.Text = "Email Address";
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(121, 208);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(58, 13);
            this.label30.TabIndex = 24;
            this.label30.Text = "Last Name";
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(43, 208);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(57, 13);
            this.label29.TabIndex = 23;
            this.label29.Text = "First Name";
            this.iEmailTextBox.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.iEmailTextBox.Location = new System.Drawing.Point(282, 224);
            this.iEmailTextBox.Name = "iEmailTextBox";
            this.iEmailTextBox.Size = new System.Drawing.Size(280, 22);
            this.iEmailTextBox.TabIndex = 9;
            this.iEmailTextBox.Leave += new System.EventHandler(anyTextBox_Leave);
            this.iEmailTextBox.Enter += new System.EventHandler(anyTextBox_Enter);
            this.iLastNameTextBox.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.iLastNameTextBox.Location = new System.Drawing.Point(124, 224);
            this.iLastNameTextBox.Name = "iLastNameTextBox";
            this.iLastNameTextBox.Size = new System.Drawing.Size(152, 22);
            this.iLastNameTextBox.TabIndex = 8;
            this.iLastNameTextBox.Leave += new System.EventHandler(anyTextBox_Leave);
            this.iLastNameTextBox.Enter += new System.EventHandler(anyTextBox_Enter);
            this.iFirstNameTextBox.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.iFirstNameTextBox.Location = new System.Drawing.Point(46, 224);
            this.iFirstNameTextBox.Name = "iFirstNameTextBox";
            this.iFirstNameTextBox.Size = new System.Drawing.Size(72, 22);
            this.iFirstNameTextBox.TabIndex = 7;
            this.iFirstNameTextBox.Leave += new System.EventHandler(anyTextBox_Leave);
            this.iFirstNameTextBox.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label28.AutoSize = true;
            this.label28.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label28.Location = new System.Drawing.Point(11, 113);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(873, 34);
            this.label28.TabIndex = 19;
            this.label28.Text = "If this computer is connected to the internet you can click the \"Activate using the Internet\" button to automatically upgrade from Trial Mode\r\nto the Licensed Mode of the Cricut DesignStudio.";
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label19.Location = new System.Drawing.Point(11, 8);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(340, 20);
            this.label19.TabIndex = 16;
            this.label19.Text = "Activate Cricut DesignStudio using the Internet";
            this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.groupBox1.Controls.Add(this.iSerialNum2);
            this.groupBox1.Controls.Add(this.label18);
            this.groupBox1.Controls.Add(this.iSerialNum1);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Controls.Add(this.iSerialNum5);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.iSerialNum3);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.iSerialNum4);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.iSerialNum6);
            this.groupBox1.Location = new System.Drawing.Point(46, 154);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(448, 47);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Serial Number";
            this.iSerialNum2.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.iSerialNum2.Location = new System.Drawing.Point(82, 19);
            this.iSerialNum2.MaxLength = 6;
            this.iSerialNum2.Name = "iSerialNum2";
            this.iSerialNum2.Size = new System.Drawing.Size(56, 22);
            this.iSerialNum2.TabIndex = 2;
            this.iSerialNum2.Text = "000000";
            this.iSerialNum2.Leave += new System.EventHandler(hex_textBox_Leave);
            this.iSerialNum2.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label18.Location = new System.Drawing.Point(368, 22);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(16, 16);
            this.label18.TabIndex = 14;
            this.label18.Text = "-";
            this.iSerialNum1.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.iSerialNum1.Location = new System.Drawing.Point(6, 19);
            this.iSerialNum1.MaxLength = 6;
            this.iSerialNum1.Name = "iSerialNum1";
            this.iSerialNum1.Size = new System.Drawing.Size(56, 22);
            this.iSerialNum1.TabIndex = 1;
            this.iSerialNum1.Text = "000000";
            this.iSerialNum1.Leave += new System.EventHandler(hex_textBox_Leave);
            this.iSerialNum1.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label17.Location = new System.Drawing.Point(292, 22);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(16, 16);
            this.label17.TabIndex = 13;
            this.label17.Text = "-";
            this.iSerialNum5.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.iSerialNum5.Location = new System.Drawing.Point(310, 19);
            this.iSerialNum5.MaxLength = 6;
            this.iSerialNum5.Name = "iSerialNum5";
            this.iSerialNum5.Size = new System.Drawing.Size(56, 22);
            this.iSerialNum5.TabIndex = 5;
            this.iSerialNum5.Text = "000000";
            this.iSerialNum5.Leave += new System.EventHandler(hex_textBox_Leave);
            this.iSerialNum5.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label12.Location = new System.Drawing.Point(216, 22);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(16, 16);
            this.label12.TabIndex = 12;
            this.label12.Text = "-";
            this.iSerialNum3.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.iSerialNum3.Location = new System.Drawing.Point(158, 19);
            this.iSerialNum3.MaxLength = 6;
            this.iSerialNum3.Name = "iSerialNum3";
            this.iSerialNum3.Size = new System.Drawing.Size(56, 22);
            this.iSerialNum3.TabIndex = 3;
            this.iSerialNum3.Text = "000000";
            this.iSerialNum3.Leave += new System.EventHandler(hex_textBox_Leave);
            this.iSerialNum3.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label11.Location = new System.Drawing.Point(140, 22);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(16, 16);
            this.label11.TabIndex = 11;
            this.label11.Text = "-";
            this.iSerialNum4.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.iSerialNum4.Location = new System.Drawing.Point(234, 19);
            this.iSerialNum4.MaxLength = 6;
            this.iSerialNum4.Name = "iSerialNum4";
            this.iSerialNum4.Size = new System.Drawing.Size(56, 22);
            this.iSerialNum4.TabIndex = 4;
            this.iSerialNum4.Text = "000000";
            this.iSerialNum4.Leave += new System.EventHandler(hex_textBox_Leave);
            this.iSerialNum4.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label10.Location = new System.Drawing.Point(64, 22);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(16, 16);
            this.label10.TabIndex = 10;
            this.label10.Text = "-";
            this.iSerialNum6.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.iSerialNum6.Location = new System.Drawing.Point(386, 19);
            this.iSerialNum6.MaxLength = 6;
            this.iSerialNum6.Name = "iSerialNum6";
            this.iSerialNum6.Size = new System.Drawing.Size(56, 22);
            this.iSerialNum6.TabIndex = 6;
            this.iSerialNum6.Text = "000000";
            this.iSerialNum6.Leave += new System.EventHandler(hex_textBox_Leave);
            this.iSerialNum6.Enter += new System.EventHandler(anyTextBox_Enter);
            this.continueTrialButton.AutoSize = true;
            this.continueTrialButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.continueTrialButton.Location = new System.Drawing.Point(671, 337);
            this.continueTrialButton.Name = "continueTrialButton";
            this.continueTrialButton.Size = new System.Drawing.Size(251, 35);
            this.continueTrialButton.TabIndex = 12;
            this.continueTrialButton.Text = "Continue using Trial Mode";
            this.continueTrialButton.UseVisualStyleBackColor = true;
            this.continueTrialButton.Click += new System.EventHandler(continueTrialButton_Click);
            this.activateByInternetButton.AutoSize = true;
            this.activateByInternetButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.activateByInternetButton.Location = new System.Drawing.Point(671, 177);
            this.activateByInternetButton.Name = "activateByInternetButton";
            this.activateByInternetButton.Size = new System.Drawing.Size(246, 35);
            this.activateByInternetButton.TabIndex = 10;
            this.activateByInternetButton.Text = "Activate using the Internet";
            this.activateByInternetButton.UseVisualStyleBackColor = true;
            this.activateByInternetButton.Click += new System.EventHandler(activateByInternetButton_Click);
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label8.Location = new System.Drawing.Point(11, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(865, 68);
            this.label8.TabIndex = 0;
            this.label8.Text = resources.GetString("label8.Text");
            this.activateByPhoneTabPage.Controls.Add(this.panel1);
            this.activateByPhoneTabPage.Location = new System.Drawing.Point(4, 19);
            this.activateByPhoneTabPage.Name = "activateByPhoneTabPage";
            this.activateByPhoneTabPage.Size = new System.Drawing.Size(938, 422);
            this.activateByPhoneTabPage.TabIndex = 1;
            this.activateByPhoneTabPage.Text = "Activate by Phone";
            this.activateByPhoneTabPage.UseVisualStyleBackColor = true;
            this.panel1.Controls.Add(this.pContinueTrialButton);
            this.panel1.Controls.Add(this.goBackToInternetActivationButton);
            this.panel1.Controls.Add(this.label24);
            this.panel1.Controls.Add(this.activationKeyStatusLabel);
            this.panel1.Controls.Add(this.verifyActKeyButton);
            this.panel1.Controls.Add(this.groupBox5);
            this.panel1.Controls.Add(this.label21);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.label34);
            this.panel1.Controls.Add(this.label35);
            this.panel1.Controls.Add(this.label36);
            this.panel1.Controls.Add(this.pEmailTextBox);
            this.panel1.Controls.Add(this.pLastNameTextBox);
            this.panel1.Controls.Add(this.pFirstNameTextBox);
            this.panel1.Controls.Add(this.label37);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.label46);
            this.panel1.Controls.Add(this.groupBox4);
            this.panel1.Controls.Add(this.label52);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(932, 416);
            this.panel1.TabIndex = 1;
            this.pContinueTrialButton.AutoSize = true;
            this.pContinueTrialButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.pContinueTrialButton.Location = new System.Drawing.Point(673, 361);
            this.pContinueTrialButton.Name = "pContinueTrialButton";
            this.pContinueTrialButton.Size = new System.Drawing.Size(251, 35);
            this.pContinueTrialButton.TabIndex = 20;
            this.pContinueTrialButton.Text = "Continue using Trial Mode";
            this.pContinueTrialButton.UseVisualStyleBackColor = true;
            this.pContinueTrialButton.Click += new System.EventHandler(continueTrialButton_Click);
            this.goBackToInternetActivationButton.AutoSize = true;
            this.goBackToInternetActivationButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.goBackToInternetActivationButton.Location = new System.Drawing.Point(673, 313);
            this.goBackToInternetActivationButton.Name = "goBackToInternetActivationButton";
            this.goBackToInternetActivationButton.Size = new System.Drawing.Size(276, 35);
            this.goBackToInternetActivationButton.TabIndex = 19;
            this.goBackToInternetActivationButton.Text = "Go back to Internet Activation";
            this.goBackToInternetActivationButton.UseVisualStyleBackColor = true;
            this.goBackToInternetActivationButton.Click += new System.EventHandler(goBackToInternetActivationButton_Click);
            this.label24.AutoSize = true;
            this.label24.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label24.Location = new System.Drawing.Point(12, 325);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(594, 68);
            this.label24.TabIndex = 33;
            this.label24.Text = resources.GetString("label24.Text");
            this.activationKeyStatusLabel.AutoSize = true;
            this.activationKeyStatusLabel.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.activationKeyStatusLabel.Location = new System.Drawing.Point(524, 289);
            this.activationKeyStatusLabel.Name = "activationKeyStatusLabel";
            this.activationKeyStatusLabel.Size = new System.Drawing.Size(184, 16);
            this.activationKeyStatusLabel.TabIndex = 32;
            this.activationKeyStatusLabel.Text = "Activation Key Invalid";
            this.verifyActKeyButton.AutoSize = true;
            this.verifyActKeyButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.verifyActKeyButton.Location = new System.Drawing.Point(673, 220);
            this.verifyActKeyButton.Name = "verifyActKeyButton";
            this.verifyActKeyButton.Size = new System.Drawing.Size(230, 35);
            this.verifyActKeyButton.TabIndex = 18;
            this.verifyActKeyButton.Text = "Verify Activation Key";
            this.verifyActKeyButton.UseVisualStyleBackColor = true;
            this.verifyActKeyButton.Click += new System.EventHandler(verifyActKeyButton_Click);
            this.groupBox5.Controls.Add(this.activationPhoneNumberLabel);
            this.groupBox5.Location = new System.Drawing.Point(715, 140);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(152, 47);
            this.groupBox5.TabIndex = 30;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Activation Phone Number";
            this.activationPhoneNumberLabel.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.activationPhoneNumberLabel.Location = new System.Drawing.Point(6, 22);
            this.activationPhoneNumberLabel.Name = "activationPhoneNumberLabel";
            this.activationPhoneNumberLabel.Size = new System.Drawing.Size(140, 16);
            this.activationPhoneNumberLabel.TabIndex = 31;
            this.activationPhoneNumberLabel.Text = "877-727-4288";
            this.activationPhoneNumberLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label21.Location = new System.Drawing.Point(13, 243);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(497, 17);
            this.label21.TabIndex = 29;
            this.label21.Text = "When you are ready to call, dial the \"Activation Phone Number\" shown above.";
            this.groupBox2.Controls.Add(this.registrationCodeLabel);
            this.groupBox2.Location = new System.Drawing.Point(532, 140);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(152, 47);
            this.groupBox2.TabIndex = 28;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Registration Code";
            this.registrationCodeLabel.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.registrationCodeLabel.Location = new System.Drawing.Point(6, 22);
            this.registrationCodeLabel.Name = "registrationCodeLabel";
            this.registrationCodeLabel.Size = new System.Drawing.Size(140, 16);
            this.registrationCodeLabel.TabIndex = 32;
            this.registrationCodeLabel.Text = "0000000000000000";
            this.registrationCodeLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(279, 194);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(73, 13);
            this.label34.TabIndex = 25;
            this.label34.Text = "Email Address";
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(121, 194);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(58, 13);
            this.label35.TabIndex = 24;
            this.label35.Text = "Last Name";
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(43, 194);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(57, 13);
            this.label36.TabIndex = 23;
            this.label36.Text = "First Name";
            this.pEmailTextBox.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.pEmailTextBox.Location = new System.Drawing.Point(282, 210);
            this.pEmailTextBox.Name = "pEmailTextBox";
            this.pEmailTextBox.Size = new System.Drawing.Size(280, 22);
            this.pEmailTextBox.TabIndex = 9;
            this.pEmailTextBox.Leave += new System.EventHandler(anyTextBox_Leave);
            this.pEmailTextBox.Enter += new System.EventHandler(anyTextBox_Enter);
            this.pLastNameTextBox.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.pLastNameTextBox.Location = new System.Drawing.Point(124, 210);
            this.pLastNameTextBox.Name = "pLastNameTextBox";
            this.pLastNameTextBox.Size = new System.Drawing.Size(152, 22);
            this.pLastNameTextBox.TabIndex = 8;
            this.pLastNameTextBox.Leave += new System.EventHandler(anyTextBox_Leave);
            this.pLastNameTextBox.Enter += new System.EventHandler(anyTextBox_Enter);
            this.pFirstNameTextBox.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.pFirstNameTextBox.Location = new System.Drawing.Point(46, 210);
            this.pFirstNameTextBox.Name = "pFirstNameTextBox";
            this.pFirstNameTextBox.Size = new System.Drawing.Size(72, 22);
            this.pFirstNameTextBox.TabIndex = 7;
            this.pFirstNameTextBox.Leave += new System.EventHandler(anyTextBox_Leave);
            this.pFirstNameTextBox.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label37.AutoSize = true;
            this.label37.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label37.Location = new System.Drawing.Point(11, 94);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(858, 34);
            this.label37.TabIndex = 19;
            this.label37.Text = "The technician will confirm that the serial number was properly entered and then guide you as you input the activation key. Finally he or\r\nshe will verify that the activation process was successful.";
            this.groupBox3.Controls.Add(this.label38);
            this.groupBox3.Controls.Add(this.label39);
            this.groupBox3.Controls.Add(this.actKey8);
            this.groupBox3.Controls.Add(this.actKey7);
            this.groupBox3.Controls.Add(this.actKey6);
            this.groupBox3.Controls.Add(this.actKey5);
            this.groupBox3.Controls.Add(this.actKey4);
            this.groupBox3.Controls.Add(this.actKey3);
            this.groupBox3.Controls.Add(this.actKey2);
            this.groupBox3.Controls.Add(this.label40);
            this.groupBox3.Controls.Add(this.actKey1);
            this.groupBox3.Controls.Add(this.label41);
            this.groupBox3.Controls.Add(this.label42);
            this.groupBox3.Controls.Add(this.label43);
            this.groupBox3.Controls.Add(this.label44);
            this.groupBox3.Location = new System.Drawing.Point(46, 267);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(472, 47);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Activation Key";
            this.label38.AutoSize = true;
            this.label38.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label38.Location = new System.Drawing.Point(408, 22);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(16, 16);
            this.label38.TabIndex = 23;
            this.label38.Text = "-";
            this.label39.AutoSize = true;
            this.label39.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label39.Location = new System.Drawing.Point(348, 22);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(16, 16);
            this.label39.TabIndex = 22;
            this.label39.Text = "-";
            this.actKey8.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.actKey8.Location = new System.Drawing.Point(426, 19);
            this.actKey8.MaxLength = 4;
            this.actKey8.Name = "actKey8";
            this.actKey8.Size = new System.Drawing.Size(40, 22);
            this.actKey8.TabIndex = 17;
            this.actKey8.Text = "0000";
            this.actKey8.Leave += new System.EventHandler(hex_textBox_Leave);
            this.actKey8.Enter += new System.EventHandler(anyTextBox_Enter);
            this.actKey7.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.actKey7.Location = new System.Drawing.Point(366, 19);
            this.actKey7.MaxLength = 4;
            this.actKey7.Name = "actKey7";
            this.actKey7.Size = new System.Drawing.Size(40, 22);
            this.actKey7.TabIndex = 16;
            this.actKey7.Text = "0000";
            this.actKey7.Leave += new System.EventHandler(hex_textBox_Leave);
            this.actKey7.Enter += new System.EventHandler(anyTextBox_Enter);
            this.actKey6.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.actKey6.Location = new System.Drawing.Point(306, 19);
            this.actKey6.MaxLength = 4;
            this.actKey6.Name = "actKey6";
            this.actKey6.Size = new System.Drawing.Size(40, 22);
            this.actKey6.TabIndex = 15;
            this.actKey6.Text = "0000";
            this.actKey6.Leave += new System.EventHandler(hex_textBox_Leave);
            this.actKey6.Enter += new System.EventHandler(anyTextBox_Enter);
            this.actKey5.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.actKey5.Location = new System.Drawing.Point(246, 19);
            this.actKey5.MaxLength = 4;
            this.actKey5.Name = "actKey5";
            this.actKey5.Size = new System.Drawing.Size(40, 22);
            this.actKey5.TabIndex = 14;
            this.actKey5.Text = "0000";
            this.actKey5.Leave += new System.EventHandler(hex_textBox_Leave);
            this.actKey5.Enter += new System.EventHandler(anyTextBox_Enter);
            this.actKey4.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.actKey4.Location = new System.Drawing.Point(186, 19);
            this.actKey4.MaxLength = 4;
            this.actKey4.Name = "actKey4";
            this.actKey4.Size = new System.Drawing.Size(40, 22);
            this.actKey4.TabIndex = 13;
            this.actKey4.Text = "0000";
            this.actKey4.Leave += new System.EventHandler(hex_textBox_Leave);
            this.actKey4.Enter += new System.EventHandler(anyTextBox_Enter);
            this.actKey3.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.actKey3.Location = new System.Drawing.Point(126, 19);
            this.actKey3.MaxLength = 4;
            this.actKey3.Name = "actKey3";
            this.actKey3.Size = new System.Drawing.Size(40, 22);
            this.actKey3.TabIndex = 12;
            this.actKey3.Text = "0000";
            this.actKey3.Leave += new System.EventHandler(hex_textBox_Leave);
            this.actKey3.Enter += new System.EventHandler(anyTextBox_Enter);
            this.actKey2.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.actKey2.Location = new System.Drawing.Point(66, 19);
            this.actKey2.MaxLength = 4;
            this.actKey2.Name = "actKey2";
            this.actKey2.Size = new System.Drawing.Size(40, 22);
            this.actKey2.TabIndex = 11;
            this.actKey2.Text = "0000";
            this.actKey2.Leave += new System.EventHandler(hex_textBox_Leave);
            this.actKey2.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label40.AutoSize = true;
            this.label40.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label40.Location = new System.Drawing.Point(288, 22);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(16, 16);
            this.label40.TabIndex = 14;
            this.label40.Text = "-";
            this.actKey1.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.actKey1.Location = new System.Drawing.Point(6, 19);
            this.actKey1.MaxLength = 4;
            this.actKey1.Name = "actKey1";
            this.actKey1.Size = new System.Drawing.Size(40, 22);
            this.actKey1.TabIndex = 10;
            this.actKey1.Text = "0000";
            this.actKey1.Leave += new System.EventHandler(hex_textBox_Leave);
            this.actKey1.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label41.AutoSize = true;
            this.label41.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label41.Location = new System.Drawing.Point(228, 22);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(16, 16);
            this.label41.TabIndex = 13;
            this.label41.Text = "-";
            this.label42.AutoSize = true;
            this.label42.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label42.Location = new System.Drawing.Point(168, 22);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(16, 16);
            this.label42.TabIndex = 12;
            this.label42.Text = "-";
            this.label43.AutoSize = true;
            this.label43.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label43.Location = new System.Drawing.Point(108, 22);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(16, 16);
            this.label43.TabIndex = 11;
            this.label43.Text = "-";
            this.label44.AutoSize = true;
            this.label44.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label44.Location = new System.Drawing.Point(48, 22);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(16, 16);
            this.label44.TabIndex = 10;
            this.label44.Text = "-";
            this.label46.AutoSize = true;
            this.label46.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label46.Location = new System.Drawing.Point(11, 8);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(281, 20);
            this.label46.TabIndex = 16;
            this.label46.Text = "Activate Cricut DesignStudio by Phone";
            this.label46.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.groupBox4.Controls.Add(this.pSerialNum2);
            this.groupBox4.Controls.Add(this.label47);
            this.groupBox4.Controls.Add(this.pSerialNum1);
            this.groupBox4.Controls.Add(this.label48);
            this.groupBox4.Controls.Add(this.pSerialNum5);
            this.groupBox4.Controls.Add(this.label49);
            this.groupBox4.Controls.Add(this.pSerialNum3);
            this.groupBox4.Controls.Add(this.label50);
            this.groupBox4.Controls.Add(this.pSerialNum4);
            this.groupBox4.Controls.Add(this.label51);
            this.groupBox4.Controls.Add(this.pSerialNum6);
            this.groupBox4.Location = new System.Drawing.Point(46, 140);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(448, 47);
            this.groupBox4.TabIndex = 15;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Serial Number";
            this.pSerialNum2.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.pSerialNum2.Location = new System.Drawing.Point(82, 19);
            this.pSerialNum2.MaxLength = 6;
            this.pSerialNum2.Name = "pSerialNum2";
            this.pSerialNum2.Size = new System.Drawing.Size(56, 22);
            this.pSerialNum2.TabIndex = 2;
            this.pSerialNum2.Text = "000000";
            this.pSerialNum2.Leave += new System.EventHandler(hex_textBox_Leave);
            this.pSerialNum2.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label47.AutoSize = true;
            this.label47.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label47.Location = new System.Drawing.Point(368, 22);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(16, 16);
            this.label47.TabIndex = 14;
            this.label47.Text = "-";
            this.pSerialNum1.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.pSerialNum1.Location = new System.Drawing.Point(6, 19);
            this.pSerialNum1.MaxLength = 6;
            this.pSerialNum1.Name = "pSerialNum1";
            this.pSerialNum1.Size = new System.Drawing.Size(56, 22);
            this.pSerialNum1.TabIndex = 1;
            this.pSerialNum1.Text = "000000";
            this.pSerialNum1.Leave += new System.EventHandler(hex_textBox_Leave);
            this.pSerialNum1.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label48.AutoSize = true;
            this.label48.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label48.Location = new System.Drawing.Point(292, 22);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(16, 16);
            this.label48.TabIndex = 13;
            this.label48.Text = "-";
            this.pSerialNum5.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.pSerialNum5.Location = new System.Drawing.Point(310, 19);
            this.pSerialNum5.MaxLength = 6;
            this.pSerialNum5.Name = "pSerialNum5";
            this.pSerialNum5.Size = new System.Drawing.Size(56, 22);
            this.pSerialNum5.TabIndex = 5;
            this.pSerialNum5.Text = "000000";
            this.pSerialNum5.Leave += new System.EventHandler(hex_textBox_Leave);
            this.pSerialNum5.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label49.AutoSize = true;
            this.label49.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label49.Location = new System.Drawing.Point(216, 22);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(16, 16);
            this.label49.TabIndex = 12;
            this.label49.Text = "-";
            this.pSerialNum3.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.pSerialNum3.Location = new System.Drawing.Point(158, 19);
            this.pSerialNum3.MaxLength = 6;
            this.pSerialNum3.Name = "pSerialNum3";
            this.pSerialNum3.Size = new System.Drawing.Size(56, 22);
            this.pSerialNum3.TabIndex = 3;
            this.pSerialNum3.Text = "000000";
            this.pSerialNum3.Leave += new System.EventHandler(hex_textBox_Leave);
            this.pSerialNum3.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label50.AutoSize = true;
            this.label50.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label50.Location = new System.Drawing.Point(140, 22);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(16, 16);
            this.label50.TabIndex = 11;
            this.label50.Text = "-";
            this.pSerialNum4.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.pSerialNum4.Location = new System.Drawing.Point(234, 19);
            this.pSerialNum4.MaxLength = 6;
            this.pSerialNum4.Name = "pSerialNum4";
            this.pSerialNum4.Size = new System.Drawing.Size(56, 22);
            this.pSerialNum4.TabIndex = 4;
            this.pSerialNum4.Text = "000000";
            this.pSerialNum4.Leave += new System.EventHandler(hex_textBox_Leave);
            this.pSerialNum4.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label51.AutoSize = true;
            this.label51.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label51.Location = new System.Drawing.Point(64, 22);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(16, 16);
            this.label51.TabIndex = 10;
            this.label51.Text = "-";
            this.pSerialNum6.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.pSerialNum6.Location = new System.Drawing.Point(386, 19);
            this.pSerialNum6.MaxLength = 6;
            this.pSerialNum6.Name = "pSerialNum6";
            this.pSerialNum6.Size = new System.Drawing.Size(56, 22);
            this.pSerialNum6.TabIndex = 6;
            this.pSerialNum6.Text = "000000";
            this.pSerialNum6.Leave += new System.EventHandler(hex_textBox_Leave);
            this.pSerialNum6.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label52.AutoSize = true;
            this.label52.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label52.Location = new System.Drawing.Point(11, 35);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(905, 51);
            this.label52.TabIndex = 0;
            this.label52.Text = resources.GetString("label52.Text");
            this.activationCompleteTabPage.Controls.Add(this.groupBox7);
            this.activationCompleteTabPage.Controls.Add(this.label57);
            this.activationCompleteTabPage.Controls.Add(this.groupBox6);
            this.activationCompleteTabPage.Controls.Add(this.label23);
            this.activationCompleteTabPage.Controls.Add(this.label25);
            this.activationCompleteTabPage.Controls.Add(this.label26);
            this.activationCompleteTabPage.Controls.Add(this.aEmailTextBox);
            this.activationCompleteTabPage.Controls.Add(this.aLastNameTextBox);
            this.activationCompleteTabPage.Controls.Add(this.aFirstNameTextBox);
            this.activationCompleteTabPage.Controls.Add(this.activationCompleteOKButton);
            this.activationCompleteTabPage.Controls.Add(this.label22);
            this.activationCompleteTabPage.Location = new System.Drawing.Point(4, 19);
            this.activationCompleteTabPage.Name = "activationCompleteTabPage";
            this.activationCompleteTabPage.Size = new System.Drawing.Size(938, 422);
            this.activationCompleteTabPage.TabIndex = 2;
            this.activationCompleteTabPage.Text = "Activation Completed";
            this.activationCompleteTabPage.UseVisualStyleBackColor = true;
            this.groupBox7.Controls.Add(this.aSerialNum2);
            this.groupBox7.Controls.Add(this.label58);
            this.groupBox7.Controls.Add(this.aSerialNum1);
            this.groupBox7.Controls.Add(this.label59);
            this.groupBox7.Controls.Add(this.aSerialNum5);
            this.groupBox7.Controls.Add(this.label60);
            this.groupBox7.Controls.Add(this.aSerialNum3);
            this.groupBox7.Controls.Add(this.label61);
            this.groupBox7.Controls.Add(this.aSerialNum4);
            this.groupBox7.Controls.Add(this.label62);
            this.groupBox7.Controls.Add(this.aSerialNum6);
            this.groupBox7.Location = new System.Drawing.Point(49, 85);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(448, 47);
            this.groupBox7.TabIndex = 41;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Serial Number";
            this.aSerialNum2.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aSerialNum2.Location = new System.Drawing.Point(82, 19);
            this.aSerialNum2.Name = "aSerialNum2";
            this.aSerialNum2.ReadOnly = true;
            this.aSerialNum2.Size = new System.Drawing.Size(56, 22);
            this.aSerialNum2.TabIndex = 5;
            this.aSerialNum2.Text = "000000";
            this.label58.AutoSize = true;
            this.label58.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label58.Location = new System.Drawing.Point(368, 22);
            this.label58.Name = "label58";
            this.label58.Size = new System.Drawing.Size(16, 16);
            this.label58.TabIndex = 14;
            this.label58.Text = "-";
            this.aSerialNum1.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aSerialNum1.Location = new System.Drawing.Point(6, 19);
            this.aSerialNum1.Name = "aSerialNum1";
            this.aSerialNum1.ReadOnly = true;
            this.aSerialNum1.Size = new System.Drawing.Size(56, 22);
            this.aSerialNum1.TabIndex = 4;
            this.aSerialNum1.Text = "000000";
            this.aSerialNum1.Leave += new System.EventHandler(anyTextBox_Leave);
            this.aSerialNum1.Enter += new System.EventHandler(anyTextBox_Enter);
            this.label59.AutoSize = true;
            this.label59.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label59.Location = new System.Drawing.Point(292, 22);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(16, 16);
            this.label59.TabIndex = 13;
            this.label59.Text = "-";
            this.aSerialNum5.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aSerialNum5.Location = new System.Drawing.Point(310, 19);
            this.aSerialNum5.Name = "aSerialNum5";
            this.aSerialNum5.ReadOnly = true;
            this.aSerialNum5.Size = new System.Drawing.Size(56, 22);
            this.aSerialNum5.TabIndex = 6;
            this.aSerialNum5.Text = "000000";
            this.label60.AutoSize = true;
            this.label60.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label60.Location = new System.Drawing.Point(216, 22);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(16, 16);
            this.label60.TabIndex = 12;
            this.label60.Text = "-";
            this.aSerialNum3.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aSerialNum3.Location = new System.Drawing.Point(158, 19);
            this.aSerialNum3.Name = "aSerialNum3";
            this.aSerialNum3.ReadOnly = true;
            this.aSerialNum3.Size = new System.Drawing.Size(56, 22);
            this.aSerialNum3.TabIndex = 7;
            this.aSerialNum3.Text = "000000";
            this.label61.AutoSize = true;
            this.label61.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label61.Location = new System.Drawing.Point(140, 22);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(16, 16);
            this.label61.TabIndex = 11;
            this.label61.Text = "-";
            this.aSerialNum4.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aSerialNum4.Location = new System.Drawing.Point(234, 19);
            this.aSerialNum4.Name = "aSerialNum4";
            this.aSerialNum4.ReadOnly = true;
            this.aSerialNum4.Size = new System.Drawing.Size(56, 22);
            this.aSerialNum4.TabIndex = 8;
            this.aSerialNum4.Text = "000000";
            this.label62.AutoSize = true;
            this.label62.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label62.Location = new System.Drawing.Point(64, 22);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(16, 16);
            this.label62.TabIndex = 10;
            this.label62.Text = "-";
            this.aSerialNum6.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aSerialNum6.Location = new System.Drawing.Point(386, 19);
            this.aSerialNum6.Name = "aSerialNum6";
            this.aSerialNum6.ReadOnly = true;
            this.aSerialNum6.Size = new System.Drawing.Size(56, 22);
            this.aSerialNum6.TabIndex = 9;
            this.aSerialNum6.Text = "000000";
            this.label57.AutoSize = true;
            this.label57.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label57.Location = new System.Drawing.Point(45, 263);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(244, 20);
            this.label57.TabIndex = 40;
            this.label57.Text = "Click the \"OK\" button to continue.";
            this.label57.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.groupBox6.Controls.Add(this.label27);
            this.groupBox6.Controls.Add(this.label33);
            this.groupBox6.Controls.Add(this.aActKey8);
            this.groupBox6.Controls.Add(this.aActKey7);
            this.groupBox6.Controls.Add(this.aActKey6);
            this.groupBox6.Controls.Add(this.aActKey5);
            this.groupBox6.Controls.Add(this.aActKey4);
            this.groupBox6.Controls.Add(this.aActKey3);
            this.groupBox6.Controls.Add(this.aActKey2);
            this.groupBox6.Controls.Add(this.label45);
            this.groupBox6.Controls.Add(this.aActKey1);
            this.groupBox6.Controls.Add(this.label53);
            this.groupBox6.Controls.Add(this.label54);
            this.groupBox6.Controls.Add(this.label55);
            this.groupBox6.Controls.Add(this.label56);
            this.groupBox6.Location = new System.Drawing.Point(49, 185);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(472, 47);
            this.groupBox6.TabIndex = 39;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Activation Key";
            this.label27.AutoSize = true;
            this.label27.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label27.Location = new System.Drawing.Point(408, 22);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(16, 16);
            this.label27.TabIndex = 23;
            this.label27.Text = "-";
            this.label33.AutoSize = true;
            this.label33.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label33.Location = new System.Drawing.Point(348, 22);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(16, 16);
            this.label33.TabIndex = 22;
            this.label33.Text = "-";
            this.aActKey8.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aActKey8.Location = new System.Drawing.Point(426, 19);
            this.aActKey8.Name = "aActKey8";
            this.aActKey8.ReadOnly = true;
            this.aActKey8.Size = new System.Drawing.Size(40, 22);
            this.aActKey8.TabIndex = 21;
            this.aActKey8.Text = "0000";
            this.aActKey7.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aActKey7.Location = new System.Drawing.Point(366, 19);
            this.aActKey7.Name = "aActKey7";
            this.aActKey7.ReadOnly = true;
            this.aActKey7.Size = new System.Drawing.Size(40, 22);
            this.aActKey7.TabIndex = 20;
            this.aActKey7.Text = "0000";
            this.aActKey6.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aActKey6.Location = new System.Drawing.Point(306, 19);
            this.aActKey6.Name = "aActKey6";
            this.aActKey6.ReadOnly = true;
            this.aActKey6.Size = new System.Drawing.Size(40, 22);
            this.aActKey6.TabIndex = 19;
            this.aActKey6.Text = "0000";
            this.aActKey5.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aActKey5.Location = new System.Drawing.Point(246, 19);
            this.aActKey5.Name = "aActKey5";
            this.aActKey5.ReadOnly = true;
            this.aActKey5.Size = new System.Drawing.Size(40, 22);
            this.aActKey5.TabIndex = 18;
            this.aActKey5.Text = "0000";
            this.aActKey4.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aActKey4.Location = new System.Drawing.Point(186, 19);
            this.aActKey4.Name = "aActKey4";
            this.aActKey4.ReadOnly = true;
            this.aActKey4.Size = new System.Drawing.Size(40, 22);
            this.aActKey4.TabIndex = 17;
            this.aActKey4.Text = "0000";
            this.aActKey3.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aActKey3.Location = new System.Drawing.Point(126, 19);
            this.aActKey3.Name = "aActKey3";
            this.aActKey3.ReadOnly = true;
            this.aActKey3.Size = new System.Drawing.Size(40, 22);
            this.aActKey3.TabIndex = 16;
            this.aActKey3.Text = "0000";
            this.aActKey2.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aActKey2.Location = new System.Drawing.Point(66, 19);
            this.aActKey2.Name = "aActKey2";
            this.aActKey2.ReadOnly = true;
            this.aActKey2.Size = new System.Drawing.Size(40, 22);
            this.aActKey2.TabIndex = 15;
            this.aActKey2.Text = "0000";
            this.label45.AutoSize = true;
            this.label45.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label45.Location = new System.Drawing.Point(288, 22);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(16, 16);
            this.label45.TabIndex = 14;
            this.label45.Text = "-";
            this.aActKey1.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aActKey1.Location = new System.Drawing.Point(6, 19);
            this.aActKey1.Name = "aActKey1";
            this.aActKey1.ReadOnly = true;
            this.aActKey1.Size = new System.Drawing.Size(40, 22);
            this.aActKey1.TabIndex = 4;
            this.aActKey1.Text = "0000";
            this.label53.AutoSize = true;
            this.label53.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label53.Location = new System.Drawing.Point(228, 22);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(16, 16);
            this.label53.TabIndex = 13;
            this.label53.Text = "-";
            this.label54.AutoSize = true;
            this.label54.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label54.Location = new System.Drawing.Point(168, 22);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(16, 16);
            this.label54.TabIndex = 12;
            this.label54.Text = "-";
            this.label55.AutoSize = true;
            this.label55.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label55.Location = new System.Drawing.Point(108, 22);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(16, 16);
            this.label55.TabIndex = 11;
            this.label55.Text = "-";
            this.label56.AutoSize = true;
            this.label56.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.label56.Location = new System.Drawing.Point(48, 22);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(16, 16);
            this.label56.TabIndex = 10;
            this.label56.Text = "-";
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(282, 139);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(73, 13);
            this.label23.TabIndex = 38;
            this.label23.Text = "Email Address";
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(124, 139);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(58, 13);
            this.label25.TabIndex = 37;
            this.label25.Text = "Last Name";
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(46, 139);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(57, 13);
            this.label26.TabIndex = 36;
            this.label26.Text = "First Name";
            this.aEmailTextBox.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aEmailTextBox.Location = new System.Drawing.Point(285, 155);
            this.aEmailTextBox.Name = "aEmailTextBox";
            this.aEmailTextBox.ReadOnly = true;
            this.aEmailTextBox.Size = new System.Drawing.Size(280, 22);
            this.aEmailTextBox.TabIndex = 3;
            this.aLastNameTextBox.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aLastNameTextBox.Location = new System.Drawing.Point(127, 155);
            this.aLastNameTextBox.Name = "aLastNameTextBox";
            this.aLastNameTextBox.ReadOnly = true;
            this.aLastNameTextBox.Size = new System.Drawing.Size(152, 22);
            this.aLastNameTextBox.TabIndex = 2;
            this.aFirstNameTextBox.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            this.aFirstNameTextBox.Location = new System.Drawing.Point(49, 155);
            this.aFirstNameTextBox.Name = "aFirstNameTextBox";
            this.aFirstNameTextBox.ReadOnly = true;
            this.aFirstNameTextBox.Size = new System.Drawing.Size(72, 22);
            this.aFirstNameTextBox.TabIndex = 1;
            this.activationCompleteOKButton.AutoSize = true;
            this.activationCompleteOKButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.activationCompleteOKButton.Location = new System.Drawing.Point(311, 258);
            this.activationCompleteOKButton.Name = "activationCompleteOKButton";
            this.activationCompleteOKButton.Size = new System.Drawing.Size(80, 35);
            this.activationCompleteOKButton.TabIndex = 4;
            this.activationCompleteOKButton.Text = "OK";
            this.activationCompleteOKButton.UseVisualStyleBackColor = true;
            this.activationCompleteOKButton.Click += new System.EventHandler(activationCompleteOKButton_Click);
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label22.Location = new System.Drawing.Point(14, 11);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(716, 60);
            this.label22.TabIndex = 18;
            this.label22.Text = "Congratulations!\r\n\r\nThis copy of Cricut DesignStudio is fully activated. Here is your registration and activation information.";
            this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.updateCricutFirmwareTabPage.Controls.Add(this.label71);
            this.updateCricutFirmwareTabPage.Controls.Add(this.label69);
            this.updateCricutFirmwareTabPage.Controls.Add(this.label70);
            this.updateCricutFirmwareTabPage.Controls.Add(this.v3RadioButton);
            this.updateCricutFirmwareTabPage.Controls.Add(this.successfulUpdateLabel);
            this.updateCricutFirmwareTabPage.Controls.Add(this.updateLaterButton);
            this.updateCricutFirmwareTabPage.Controls.Add(this.beginUpdatingButton);
            this.updateCricutFirmwareTabPage.Controls.Add(this.label66);
            this.updateCricutFirmwareTabPage.Controls.Add(this.v2RadioButton);
            this.updateCricutFirmwareTabPage.Controls.Add(this.v1RadioButton);
            this.updateCricutFirmwareTabPage.Controls.Add(this.label65);
            this.updateCricutFirmwareTabPage.Controls.Add(this.label63);
            this.updateCricutFirmwareTabPage.Controls.Add(this.label64);
            this.updateCricutFirmwareTabPage.Location = new System.Drawing.Point(4, 19);
            this.updateCricutFirmwareTabPage.Name = "updateCricutFirmwareTabPage";
            this.updateCricutFirmwareTabPage.Size = new System.Drawing.Size(938, 422);
            this.updateCricutFirmwareTabPage.TabIndex = 3;
            this.updateCricutFirmwareTabPage.Text = "Update Cricut Firmware";
            this.updateCricutFirmwareTabPage.UseVisualStyleBackColor = true;
            this.label71.AutoSize = true;
            this.label71.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label71.Location = new System.Drawing.Point(15, 98);
            this.label71.Name = "label71";
            this.label71.Size = new System.Drawing.Size(891, 34);
            this.label71.TabIndex = 34;
            this.label71.Text = resources.GetString("label71.Text");
            this.label69.AutoSize = true;
            this.label69.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label69.Location = new System.Drawing.Point(15, 194);
            this.label69.Name = "label69";
            this.label69.Size = new System.Drawing.Size(863, 34);
            this.label69.TabIndex = 33;
            this.label69.Text = resources.GetString("label69.Text");
            this.label70.AutoSize = true;
            this.label70.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label70.Location = new System.Drawing.Point(15, 148);
            this.label70.Name = "label70";
            this.label70.Size = new System.Drawing.Size(879, 34);
            this.label70.TabIndex = 32;
            this.label70.Text = resources.GetString("label70.Text");
            this.v3RadioButton.AutoSize = true;
            this.v3RadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.v3RadioButton.Location = new System.Drawing.Point(737, 238);
            this.v3RadioButton.Name = "v3RadioButton";
            this.v3RadioButton.Size = new System.Drawing.Size(108, 21);
            this.v3RadioButton.TabIndex = 30;
            this.v3RadioButton.Text = "Cricut Create";
            this.v3RadioButton.UseVisualStyleBackColor = true;
            this.successfulUpdateLabel.AutoSize = true;
            this.successfulUpdateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.successfulUpdateLabel.Location = new System.Drawing.Point(15, 340);
            this.successfulUpdateLabel.Name = "successfulUpdateLabel";
            this.successfulUpdateLabel.Size = new System.Drawing.Size(556, 17);
            this.successfulUpdateLabel.TabIndex = 29;
            this.successfulUpdateLabel.Text = "The Cricut firmware was successfully updated. Please click the \"OK\" button to continue.";
            this.updateLaterButton.AutoSize = true;
            this.updateLaterButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.updateLaterButton.Location = new System.Drawing.Point(673, 361);
            this.updateLaterButton.Name = "updateLaterButton";
            this.updateLaterButton.Size = new System.Drawing.Size(230, 35);
            this.updateLaterButton.TabIndex = 28;
            this.updateLaterButton.Text = "Update Later";
            this.updateLaterButton.UseVisualStyleBackColor = true;
            this.updateLaterButton.Click += new System.EventHandler(updateLaterButton_Click);
            this.beginUpdatingButton.AutoSize = true;
            this.beginUpdatingButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.beginUpdatingButton.Location = new System.Drawing.Point(673, 313);
            this.beginUpdatingButton.Name = "beginUpdatingButton";
            this.beginUpdatingButton.Size = new System.Drawing.Size(230, 35);
            this.beginUpdatingButton.TabIndex = 27;
            this.beginUpdatingButton.Text = "Begin Updating Now";
            this.beginUpdatingButton.UseVisualStyleBackColor = true;
            this.beginUpdatingButton.Click += new System.EventHandler(beginUpdatingButton_Click);
            this.label66.AutoSize = true;
            this.label66.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label66.Location = new System.Drawing.Point(15, 269);
            this.label66.Name = "label66";
            this.label66.Size = new System.Drawing.Size(862, 34);
            this.label66.TabIndex = 22;
            this.label66.Text = resources.GetString("label66.Text");
            this.v2RadioButton.AutoSize = true;
            this.v2RadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.v2RadioButton.Location = new System.Drawing.Point(588, 238);
            this.v2RadioButton.Name = "v2RadioButton";
            this.v2RadioButton.Size = new System.Drawing.Size(135, 21);
            this.v2RadioButton.TabIndex = 21;
            this.v2RadioButton.Text = "Cricut Expression";
            this.v2RadioButton.UseVisualStyleBackColor = true;
            this.v1RadioButton.AutoSize = true;
            this.v1RadioButton.Checked = true;
            this.v1RadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.v1RadioButton.Location = new System.Drawing.Point(410, 238);
            this.v1RadioButton.Name = "v1RadioButton";
            this.v1RadioButton.Size = new System.Drawing.Size(164, 21);
            this.v1RadioButton.TabIndex = 20;
            this.v1RadioButton.TabStop = true;
            this.v1RadioButton.Text = "Cricut Personal Cutter";
            this.v1RadioButton.UseVisualStyleBackColor = true;
            this.label65.AutoSize = true;
            this.label65.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label65.Location = new System.Drawing.Point(15, 240);
            this.label65.Name = "label65";
            this.label65.Size = new System.Drawing.Size(380, 17);
            this.label65.TabIndex = 19;
            this.label65.Text = "STEP 3 - Choose the correct model of your Cricut machine:";
            this.label63.AutoSize = true;
            this.label63.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label63.Location = new System.Drawing.Point(14, 11);
            this.label63.Name = "label63";
            this.label63.Size = new System.Drawing.Size(176, 20);
            this.label63.TabIndex = 18;
            this.label63.Text = "Update Cricut Firmware";
            this.label63.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label64.AutoSize = true;
            this.label64.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.label64.Location = new System.Drawing.Point(15, 39);
            this.label64.Name = "label64";
            this.label64.Size = new System.Drawing.Size(862, 51);
            this.label64.TabIndex = 17;
            this.label64.Text = resources.GetString("label64.Text");
            base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
            base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            base.ClientSize = new System.Drawing.Size(1016, 741);
            base.Controls.Add(this.percent200Button);
            base.Controls.Add(this.percent100Button);
            base.Controls.Add(this.fitToPageButton);
            base.Controls.Add(this.previewButton);
            base.Controls.Add(this.deleteGroupButton);
            base.Controls.Add(this.pasteGroupButton);
            base.Controls.Add(this.copyGroupButton);
            base.Controls.Add(this.newPageButton);
            base.Controls.Add(this.cricutCutButton);
            base.Controls.Add(this.matTabControl);
            base.Controls.Add(this.menuStripSeparator);
            base.Controls.Add(this.panel2);
            base.Controls.Add(this.cartridgeLibraryPanel);
            base.Controls.Add(this.panel10);
            base.Controls.Add(this.propertiesPanel);
            base.Controls.Add(this.keypadPanel);
            base.Controls.Add(this.menuStrip1);
            base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            this.MinimumSize = new System.Drawing.Size(1023, 767);
            base.Name = "Form1";
            base.Load += new System.EventHandler(Form1_Load);
            base.Paint += new System.Windows.Forms.PaintEventHandler(Form1_Paint);
            base.Move += new System.EventHandler(Form1_Move);
            base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(Form1_FormClosing);
            base.Resize += new System.EventHandler(Form1_Resize);
            base.ResizeEnd += new System.EventHandler(Form1_ResizeEnd);
            this.keypadPanel.ResumeLayout(false);
            this.keypadPanel.PerformLayout();
            this.keypadOutlinePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.fontPicBox).EndInit();
            this.glyphContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.featuresPicBox).EndInit();
            this.panel16.ResumeLayout(false);
            this.panel15.ResumeLayout(false);
            this.panel14.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.pageContextMenu.ResumeLayout(false);
            this.pageContextMenu.PerformLayout();
            this.cartridgeLibraryPanel.ResumeLayout(false);
            this.cartridgeLibraryPanel.PerformLayout();
            this.propertiesPanel.ResumeLayout(false);
            this.propertiesPanel.PerformLayout();
            this.matTabControl.ResumeLayout(false);
            this.activateByInternetTabPage.ResumeLayout(false);
            this.trialVersionPanel.ResumeLayout(false);
            this.trialVersionPanel.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.activateByPhoneTabPage.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.activationCompleteTabPage.ResumeLayout(false);
            this.activationCompleteTabPage.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.updateCricutFirmwareTabPage.ResumeLayout(false);
            this.updateCricutFirmwareTabPage.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public int HandleAlphaKeys(Keys keyCode)
        {
            int result = -1;
            switch (keyCode)
            {
                case Keys.D1:
                    result = 2;
                    break;

                case Keys.D2:
                    result = 3;
                    break;

                case Keys.D3:
                    result = 4;
                    break;

                case Keys.D4:
                    result = 5;
                    break;

                case Keys.D5:
                    result = 6;
                    break;

                case Keys.D6:
                    result = 7;
                    break;

                case Keys.D7:
                    result = 8;
                    break;

                case Keys.D8:
                    result = 9;
                    break;

                case Keys.D9:
                    result = 10;
                    break;

                case Keys.D0:
                    result = 11;
                    break;

                case Keys.Q:
                    result = 16;
                    break;

                case Keys.W:
                    result = 17;
                    break;

                case Keys.E:
                    result = 18;
                    break;

                case Keys.R:
                    result = 19;
                    break;

                case Keys.T:
                    result = 20;
                    break;

                case Keys.Y:
                    result = 21;
                    break;

                case Keys.U:
                    result = 22;
                    break;

                case Keys.I:
                    result = 23;
                    break;

                case Keys.O:
                    result = 24;
                    break;

                case Keys.P:
                    result = 25;
                    break;

                case Keys.A:
                    result = 30;
                    break;

                case Keys.S:
                    result = 31;
                    break;

                case Keys.D:
                    result = 32;
                    break;

                case Keys.F:
                    result = 33;
                    break;

                case Keys.G:
                    result = 34;
                    break;

                case Keys.H:
                    result = 35;
                    break;

                case Keys.J:
                    result = 36;
                    break;

                case Keys.K:
                    result = 37;
                    break;

                case Keys.L:
                    result = 38;
                    break;

                case Keys.Z:
                    result = 44;
                    break;

                case Keys.X:
                    result = 45;
                    break;

                case Keys.C:
                    result = 46;
                    break;

                case Keys.V:
                    result = 47;
                    break;

                case Keys.B:
                    result = 48;
                    break;

                case Keys.N:
                    result = 49;
                    break;

                case Keys.M:
                    result = 50;
                    break;
            }
            return result;
        }

        public bool HandleKeysDown(Keys keyCode)
        {
            foreach (Control control in myRootForm.propertiesPanel.Controls)
            {
                if (control.Focused)
                {
                    return false;
                }
            }
            bool result = true;
            int num = 0;
            int num2 = 0;
            int num3 = -1;
            num3 = HandleAlphaKeys(keyCode);
            if (num3 != -1)
            {
                int keyId = num3;
                float size = myRootForm.sizeValue;
                if (myRootForm.getCanvas() != null)
                {
                    myRootForm.getCanvas().AddGlyphWithUndo(fontLoading, keyId, size);
                }
                return true;
            }
            switch (keyCode)
            {
                case Keys.Left:
                    num = -1;
                    break;

                case Keys.Right:
                    num = 1;
                    break;

                case Keys.Up:
                    num2 = -1;
                    break;

                case Keys.Down:
                    num2 = 1;
                    break;

                case Keys.ControlKey:
                    ctrlKeyDown = true;
                    break;

                case Keys.ShiftKey:
                    shiftKeyDown = true;
                    break;

                case Keys.Menu:
                case Keys.Alt:
                    SceneGroup.hideTransformHandles = true;
                    refreshMattePicBox();
                    break;

                case Keys.Return:
                    okButton_Click(null, null);
                    break;

                case Keys.Back:
                    backSpaceButton_Click(null, null);
                    break;

                case Keys.Delete:
                    deleteButton_Click(null, null);
                    break;

                case Keys.Space:
                    spaceButton_Click(null, null);
                    break;

                default:
                    result = false;
                    break;
            }
            if (shiftKeyDown)
            {
                if (1 == num2)
                {
                    myRootForm.sizeTrackBar_ValueChanged(-1);
                    getCanvas().adjCursor(0f, 0f, myRootForm.sizeValue);
                    refreshMattePicBox();
                }
                else if (-1 == num2)
                {
                    myRootForm.sizeTrackBar_ValueChanged(1);
                    getCanvas().adjCursor(0f, 0f, myRootForm.sizeValue);
                    refreshMattePicBox();
                }
            }
            else if (num != 0 || num2 != 0)
            {
                SceneGroup[] selectedGroups = getCanvas().GetSelectedGroups();
                SceneGroup[] array = selectedGroups;
                for (int i = 0; i < array.Length; i++)
                {
                    _ = array[i];
                    if (ctrlKeyDown)
                    {
                        SceneGroup.moveGroups(selectedGroups, (float)num * smallMoveValue, (float)num2 * smallMoveValue);
                    }
                    else
                    {
                        SceneGroup.moveGroups(selectedGroups, (float)num * largeMoveValue, (float)num2 * largeMoveValue);
                    }
                }
                if (selectedGroups.Length == 0)
                {
                    if (ctrlKeyDown)
                    {
                        getCanvas().adjCursor((float)num * smallMoveValue, (float)num2 * smallMoveValue, getCanvas().cursorSize);
                    }
                    else
                    {
                        getCanvas().adjCursor((float)num * largeMoveValue, (float)num2 * largeMoveValue, getCanvas().cursorSize);
                    }
                }
                refreshMattePicBox();
            }
            return result;
        }

        public bool HandleKeysUp(Keys keyCode)
        {
            bool result = false;
            switch (keyCode)
            {
                case Keys.ControlKey:
                    ctrlKeyDown = false;
                    result = true;
                    break;

                case Keys.ShiftKey:
                    shiftKeyDown = false;
                    result = true;
                    break;

                case Keys.Menu:
                case Keys.Alt:
                    result = true;
                    SceneGroup.hideTransformHandles = false;
                    refreshMattePicBox();
                    break;
            }
            return result;
        }

        public bool PreFilterMessage(ref Message m)
        {
            Keys keys = (Keys)((int)m.WParam & 0xFFFF);
            if (allowLocalFocus || pageContextMenu.Visible || glyphContextMenu.Visible)
            {
                return false;
            }
            if (ctrlKeyDown && (Keys.D == keys || Keys.A == keys || Keys.L == keys || Keys.Z == keys || Keys.Y == keys || Keys.C == keys || Keys.V == keys || Keys.S == keys || Keys.N == keys))
            {
                return false;
            }
            if (m.Msg == 256 || m.Msg == 260)
            {
                return HandleKeysDown((Keys)((int)m.WParam & 0xFFFF));
            }
            if (m.Msg == 257 || m.Msg == 261)
            {
                return HandleKeysUp((Keys)((int)m.WParam & 0xFFFF));
            }
            return false;
        }

        private void number_textBox_Leave(object sender, EventArgs e)
        {
            allowLocalFocus = false;
            TextBox textBox = (TextBox)sender;
            FloatParam floatParam = (FloatParam)textBox.Tag;
            if (float.TryParse(textBox.Text, out var result))
            {
                floatParam.f = result;
                SceneGroup.doNumBoxTransform(floatParam.mode);
                refreshMattePicBox();
            }
            else
            {
                textBox.Text = floatParam.f.ToString();
            }
        }

        private void number_textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ('\r' == e.KeyChar)
            {
                focusMattePicBox();
                e.Handled = true;
            }
            else if ('\u001b' == e.KeyChar)
            {
                TextBox textBox = (TextBox)sender;
                FloatParam floatParam = (FloatParam)textBox.Tag;
                textBox.Text = floatParam.f.ToString();
                focusMattePicBox();
                e.Handled = true;
            }
        }

        public void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            textBoxKeyChar = e.KeyChar;
            if ('\r' == e.KeyChar)
            {
                focusMattePicBox();
                e.Handled = true;
            }
            else if ('\u001b' == e.KeyChar)
            {
                focusMattePicBox();
                e.Handled = true;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (getCanvas().selectedGroup != null)
            {
                getCanvas().selectedGroup.selected = false;
            }
            refreshMattePicBox();
        }

        private void backSpaceButton_Click(object sender, EventArgs e)
        {
            if (getCanvas() != null)
            {
                getCanvas().delGlyphAndAddUndo(getCanvas().selectedGroup);
                refreshMattePicBox();
            }
        }

        public void deleteSelectedGroupAndAddUndo()
        {
            SceneGroup[] array = null;
            PCImage[] array2 = null;
            int num = 0;
            foreach (SceneGroup sceneGroup3 in getCanvas().sceneGroups)
            {
                if (sceneGroup3.selected)
                {
                    num++;
                }
            }
            int num2 = 0;
            foreach (PCImage image in getCanvas().images)
            {
                if (image.selected)
                {
                    num2++;
                }
            }
            array = new SceneGroup[num];
            num = 0;
            foreach (SceneGroup sceneGroup4 in getCanvas().sceneGroups)
            {
                if (sceneGroup4.selected)
                {
                    array[num++] = sceneGroup4;
                }
            }
            array2 = new PCImage[num2];
            foreach (PCImage image2 in getCanvas().images)
            {
                if (image2.selected)
                {
                    array2[num2++] = image2;
                }
            }
            if (num > 0 || num2 > 0)
            {
                getCanvas().undoRedo.addAndDo(new UndoRedo.UndoDeleteShapes(array, array2));
                refreshMattePicBox();
            }
        }

        public void deleteSelectedGroup()
        {
            bool flag = false;
            foreach (SceneGroup sceneGroup in getCanvas().sceneGroups)
            {
                if (sceneGroup.selected)
                {
                    getCanvas().removeGroup(sceneGroup);
                    flag = true;
                    SceneGlyph.selectedGlyph = null;
                    break;
                }
            }
            foreach (PCImage image in getCanvas().images)
            {
                if (image.selected)
                {
                    getCanvas().removeImage(image);
                    flag = true;
                    break;
                }
            }
            myRootForm.EnableGlyphManipulationControls(en: true);
            SceneGroup.clearNumBoxes();
            if (flag)
            {
                refreshMattePicBox();
            }
        }

        public void deleteButton_Click(object sender, EventArgs e)
        {
            if (getCanvas() != null && !allowLocalFocus)
            {
                deleteSelectedGroupAndAddUndo();
            }
        }

        private void spaceButton_Click(object sender, EventArgs e)
        {
            if (getCanvas() != null)
            {
                getCanvas().addSpace(getCanvas().cursorSize, getCanvas().selectedGroup);
                getCanvas().adjCursor(getCanvas().cursorSize / 2f, 0f, getCanvas().cursorSize);
                refreshMattePicBox();
            }
        }
    }
}
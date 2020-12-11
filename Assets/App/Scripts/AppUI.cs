using UnityEngine;
using UnityEngine.UI;

namespace NRatel.TextureUnpacker
{
    public class AppUI : MonoBehaviour
    {
        public GameObject m_Go_BigImageBg;
        public Image m_Image_BigImage;
        public Text m_Text_Tip;
        public InputField m_InputField;
        public Button m_Btn_Excute;
        public Button m_Btn_Stop;
        public Dropdown m_Dropdown_SelectMode;
        public Button m_Btn_ContactMe;
        public Scrollbar m_Scrollbar;
        public Transform m_EditorPanel;

        public bool isSplitTip { set => isSplit = value; }

        private bool isSplit = false;

        public AppUI Init()
        {
            this.m_Go_BigImageBg.gameObject.SetActive(false);
            //this.m_Text_Tip.gameObject.SetActive(false);
            this.m_Text_Tip.text = this.m_Text_Tip.text = string.Empty;
            this.RegisterEvents();
            return this;
        }

        private void RegisterEvents()
        {
            var imageParent = m_Image_BigImage.transform.parent.GetComponent<Image>();
            this.m_EditorPanel.Find("R_Btn").GetComponent<Button>().onClick.AddListener(() => imageParent.color = Color.red);
            this.m_EditorPanel.Find("G_Btn").GetComponent<Button>().onClick.AddListener(() => imageParent.color = Color.green);
            this.m_EditorPanel.Find("B_Btn").GetComponent<Button>().onClick.AddListener(() => imageParent.color = Color.blue);
            this.m_EditorPanel.Find("A_Btn").GetComponent<Button>().onClick.AddListener(() => imageParent.color = Color.white);
            this.m_EditorPanel.Find("BLACK_Btn").GetComponent<Button>().onClick.AddListener(() => imageParent.color = Color.black);

            this.m_Btn_ContactMe.onClick.AddListener(() =>
            {
                Application.OpenURL("https://github.com/real-re/TextureUnpacker");
            });
        }

        public void SetTip(string str)
        {
            this.m_Text_Tip.text += $"{str}\n";
            this.m_Scrollbar.value = 0;
        }

        public void SetTipProcess(string str)
        {
            if (isSplit)
            {
                int tail = this.m_Text_Tip.text.LastIndexOf('\n');
                int index = this.m_Text_Tip.text.LastIndexOf('\n', tail - 1);
                if (index != -1)
                    this.m_Text_Tip.text = this.m_Text_Tip.text.Remove(index, tail - index);
            }
            else
                isSplit = true;
            this.m_Text_Tip.text += $"{str}\n";
            this.m_Scrollbar.value = 0;
        }

        public void SetTipInfo(string str)
        {
            this.m_Text_Tip.text += $"{LOG_INFO}{str}</color>\n";
            this.m_Scrollbar.value = 0;
        }

        public void SetTipErr(string str)
        {
            this.m_Text_Tip.text += $"{LOG_ERR}{str}</color>\n";
            this.m_Scrollbar.value = 0;
        }

        public void SetTipWarn(string str)
        {
            this.m_Text_Tip.text += $"{LOG_WARN}{str}</color>\n";
            this.m_Scrollbar.value = 0;
        }

        public void SetImage(Texture2D texture)
        {
            this.m_Go_BigImageBg.gameObject.SetActive(true);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            this.m_Image_BigImage.sprite = sprite;
            RectTransform rt = this.m_Image_BigImage.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(texture.width, texture.height);

            //缩放至屏幕可直接显示的大小
            float minRate = Mathf.Min(800.0f / texture.width, 800.0f / texture.height);
            rt.localScale = new Vector2(minRate, minRate);
        }

        private const string LOG_INFO = "<color=green>";
        private const string LOG_ERR = "<color=red>";
        private const string LOG_WARN = "<color=#ff8a2b>";
    }
}


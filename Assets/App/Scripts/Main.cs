using UnityEngine;

namespace NRatel.TextureUnpacker
{
    //入口类，兼职处理App的协程
    public class Main : MonoBehaviour
    {
        void Start()
        {
            //Screen.SetResolution(1280, 800, false);
            new App(this);
        }
    }
}
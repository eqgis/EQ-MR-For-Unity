using UnityEditor;
namespace Holo.XR.Editor.UX
{
    public class UxProgressBar
    {
        private float maxValue;
        private float presentValue;
        private string title;
        private string info;

        /// <summary>
        /// ¹¹Ôìº¯Êý
        /// </summary>
        /// <param name="maxValue"></param>
        /// <param name="presentValue"></param>
        /// <param name="title"></param>
        /// <param name="info"></param>
        public UxProgressBar(float maxValue, float presentValue, string title, string info)
        {
            this.maxValue = maxValue;
            this.presentValue = presentValue;
            this.title = title;
            this.info = info;
        }

        public float MaxValue { get => maxValue; set => maxValue = value; }
        public float PresentValue { get => presentValue; set => presentValue = value; }

        public void update()
        {
            bool isCancel = EditorUtility.DisplayCancelableProgressBar(title, info, presentValue / maxValue);
            if (isCancel || presentValue >= maxValue)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
            }

        }
    }
}
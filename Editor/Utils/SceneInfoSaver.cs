using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine;

namespace Holo.XR.Editor.Utils
{
    /// <summary>
    /// 场景信息储存
    /// </summary>
    public class SceneInfoSaver
    {
        public static void ExportSceneJson(UnityEngine.SceneManagement.Scene scene, string directory)
        {
            string filePath = directory + "/" + "SceneInfo.json";
            FileInfo t = new FileInfo(filePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            StreamWriter sw = t.CreateText();
            JsonTextWriter writer = new JsonTextWriter(sw);

            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(scene.name);

            writer.WritePropertyName("models");
            writer.WriteStartArray();
            GameObject[] sceneGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            List<GameObject> rootGameObjects = new List<GameObject>();

            foreach (GameObject go in sceneGameObjects)
            {
                if (go.transform.parent != null)
                    continue;
                rootGameObjects.Add(go);
            }

            foreach (GameObject go in rootGameObjects)
            {
                ExportGameObject(directory, writer, go);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();

            sw.Close();
            sw.Dispose();
        }

        public static void ExportGameObject(string directory, JsonWriter writer, GameObject go)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("name");
            //写入层级全称
            writer.WriteValue(GetHierarchyName(go));

            writer.WritePropertyName("enable");
            writer.WriteValue(go.activeSelf ? 1 : 0);

        writer.WritePropertyName("t");
        writer.WriteStartArray();
        writer.WriteValue(go.transform.localPosition.x);
        writer.WriteValue(go.transform.localPosition.y);
        writer.WriteValue(go.transform.localPosition.z);
        writer.WriteEndArray();

        writer.WritePropertyName("r");
        writer.WriteStartArray(); 
        writer.WriteValue(go.transform.localRotation.x);
        writer.WriteValue(go.transform.localRotation.y); 
        writer.WriteValue(go.transform.localRotation.z);
        writer.WriteValue(go.transform.localRotation.w);
        writer.WriteEndArray();

        writer.WritePropertyName("s");
        writer.WriteStartArray(); 
        writer.WriteValue(go.transform.localScale.x);
        writer.WriteValue(go.transform.localScale.y);
        writer.WriteValue(go.transform.localScale.z);
        writer.WriteEndArray();

            writer.WritePropertyName("tag");
            writer.WriteValue(go.tag);

            writer.WritePropertyName("layer");
            writer.WriteValue(LayerMask.LayerToName(go.layer));

#if false
            // lua path
            ScriptRunner[] scriptRunners = go.GetComponents<ScriptRunner>();
            writer.WritePropertyName("luapath");
            writer.WriteStartArray();
            for (int i = 0; i < scriptRunners.Length; i++)
            {
                string luaPath = scriptRunners[i] == null ? "" : AssetDatabase.GetAssetPath(scriptRunners[i].ScriptSource);
                writer.WriteValue(luaPath);

                if (!string.IsNullOrEmpty(luaPath))
                {
                    string destFilePath = Path.Combine(directory, luaPath);
                    string destFileDirectory = Path.GetDirectoryName(destFilePath);
                    if (!Directory.Exists(destFileDirectory)) Directory.CreateDirectory(destFileDirectory);
                    File.Copy(luaPath, destFilePath, true);
                }
            }
            writer.WriteEndArray();
#endif


            // export video
            VideoPlayer[] videoPlayers = go.GetComponents<VideoPlayer>();
            writer.WritePropertyName("videopath");
            writer.WriteStartArray();

            if (videoPlayers != null && videoPlayers.Length > 0)
            {
                for (int i = 0; i < videoPlayers.Length; i++)
                {
                    if (videoPlayers[i].clip != null)
                    {
                        string videoClipPath = AssetDatabase.GetAssetPath(videoPlayers[i].clip);
                        writer.WriteValue(videoClipPath);
                        if (!string.IsNullOrEmpty(videoClipPath))
                        {
                            string destFilePath = Path.Combine(directory, videoClipPath);
                            string destFileDirectory = Path.GetDirectoryName(destFilePath);
                            if (!Directory.Exists(destFileDirectory)) Directory.CreateDirectory(destFileDirectory);
                            File.Copy(videoClipPath, destFilePath, true);
                        }

                        if (videoPlayers[i].clip != null) videoPlayers[i].clip = null;
                        //videoPlayers[i].audioOutputMode = VideoAudioOutputMode.Direct;
                        /* VideoPlayer videoPlayer = videoPlayers[i];
                        if (videoPlayer.audioOutputMode != VideoAudioOutputMode.AudioSource)
                        {
                            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                            AudioSource audioSource = videoPlayer.GetComponent<AudioSource>();
                            if (audioSource == null)
                            {
                                audioSource = videoPlayer.gameObject.AddComponent<AudioSource>();
                            }
                            videoPlayer.SetTargetAudioSource(0, audioSource);
                        }*/
                    }
                }
            }
            writer.WriteEndArray();

            writer.WritePropertyName("children");
            writer.WriteStartArray();

            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform child = go.transform.GetChild(i);
                ExportGameObject(directory, writer, child.gameObject);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();

        }

        /// <summary>
        /// 场景打完包之后需要还原视频素材 
        /// </summary>
        public static void RecoverGameObjectVideoPlayers(string sceneDescPath)
        {
            if (!File.Exists(sceneDescPath))
                return;

            string sceneContent = File.ReadAllText(sceneDescPath);
            JObject json_obj = JObject.Parse(sceneContent);
            JArray json_models = (JArray)json_obj.SelectToken("models");
            for (int i = 0; i < json_models.Count; i++)
            {
                JObject json_model = (JObject)json_models[i];
                string obj_name = (string)json_model.SelectToken("name");
                GameObject go = HoloGameObject.Find(obj_name);
                RecoverVideoPlayerFromJson(json_model, go);
            }
        }

        /// <summary>
        /// 还原视频素材 
        /// </summary>
        /// <param name="go">Go.</param>
        private static void RecoverVideoPlayerFromJson(JObject json_model, GameObject go)
        {
            if (go == null)
                return;

            VideoPlayer[] videoPlayers = go.GetComponents<VideoPlayer>();

            if (videoPlayers != null && videoPlayers.Length > 0)
            {
                for (int i = 0; i < videoPlayers.Length; i++)
                {
                    VideoPlayer videoPlayer = videoPlayers[i];
                    string[] video_paths = ParseVideoPath((JArray)json_model.SelectToken("videopath"));
                    string video_path = video_paths != null && video_paths.Length > i ? video_paths[i] : "";
                    if (video_path.Length > i && !string.IsNullOrEmpty(video_path))
                    {
                        VideoClip videoClip = AssetDatabase.LoadAssetAtPath<VideoClip>(video_path);
                        if (videoClip != null)
                        {
                            videoPlayer.clip = videoClip;
                            //videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
                            //AudioSource audioSource = videoPlayer.GetComponent<AudioSource>();
                            //if (audioSource != null)
                            //{
                            //    DestroyImmediate(audioSource);
                            //}
                        }
                    }
                }
            }

            JArray json_children = (JArray)json_model.SelectToken("children");
            if (json_children == null || json_children.Count == 0)
                return;

            for (int i = 0; i < json_children.Count; i++)
            {
                JObject json_child = (JObject)json_children[i];
                string obj_name = (string)json_child.SelectToken("name");
                GameObject childObj = HoloGameObject.Find(obj_name);
                Transform trans = childObj == null ? null : childObj.transform;
                if (trans == null)
                    continue;
                GameObject child = trans.gameObject;
                RecoverVideoPlayerFromJson(json_child, child);
            }
        }

        private static string[] ParseVideoPath(JArray jArray)
        {
            if (jArray == null || jArray.Count == 0) return null;
            List<string> list = new List<string>();
            for (int i = 0; i < jArray.Count; i++)
            {
                string videoFileName = (string)jArray[i];
                string luaFilePath = videoFileName;
                list.Add(luaFilePath);
            }
            return list.ToArray();
        }

        /// <summary>
        /// 返回全称
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static string GetHierarchyName(GameObject go)
        {
            string object_string = go.name;
            Transform parent = go.transform.parent;
            while (parent != null)
            {
                object_string = parent.name + "/" + object_string;
                parent = parent.parent;
            }
            return object_string;
        }
    }


}
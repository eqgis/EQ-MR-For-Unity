###  说明

- 先将“StartScene_Default”场景导出打包成APK，
- 再通过“Holo-XR->HotUpdate->Build Android”打包“HotUpdate/Main”场景
- 最后，将编辑器中StreamingAssets/output目录下的“hur”
和“HotUpdate.dll.bytes”拷贝至安卓手机data目录（/Android/com.xxx.xx/file/data/）
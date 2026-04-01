using UnityEngine;
using UnityEditor;
using System.IO;

public class SaveDataEditorTool : MonoBehaviour
{
    // Tạo một menu item trên thanh công cụ Unity (Phía trên cùng)
    [MenuItem("Tools/Dọn dẹp dữ liệu/Xóa hết Save và PlayerPrefs")]
    public static void DeleteAllSaveData()
    {
        // 1. Xóa file save vật lý
        string savePath = Path.Combine(Application.persistentDataPath, "autosave_data.txt");
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("<color=green>Đã xóa file autosave_data.txt thành công!</color>");
        }
        else
        {
            Debug.Log("Không tìm thấy file save nào để xóa.");
        }

        // 2. Xóa PlayerPrefs (Âm lượng, Fullscreen, v.v.)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=yellow>Đã xóa toàn bộ PlayerPrefs (Cài đặt âm thanh/hình ảnh).</color>");

        // Hiển thị thông báo nhỏ
        EditorUtility.DisplayDialog("Thông báo", "Toàn bộ dữ liệu save và cài đặt đã được xóa sạch!", "OK");
    }
}